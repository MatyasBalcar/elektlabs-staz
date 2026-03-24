using Knihovna.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Knihovna.Tests.Models
{
    [TestClass]
    public class DatabaseManagerWriteAndDeleteTests
    {
        private DbContextOptions<AppDbContext> _options = null!;

        private Nationality DefaultNationality => new() { Name = "Czech" };
        private Author DefaultAuthor => new() { FirstName = "A", LastName = "B", Nationality = DefaultNationality };

        [TestInitialize]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        //factory
        private Book CreateValidBook(string name = "Test Book", string isbn = "1234567890")
        {
            return new Book
            {
                Name = name,
                ISBN = isbn,
                Authors = new List<Author> { DefaultAuthor },
                Language = new Language { Name = "Czech" },
                Publisher = new Publisher { Name = "TestPub" }
            };
        }

        private void AssertDatabaseEmpty()
        {
            using var context = new AppDbContext(_options);
            Assert.AreEqual(0, context.Books.Count(), "Database should be empty of books.");
            Assert.AreEqual(0, context.Authors.Count(), "Database should be empty of authors.");
        }

        

        [TestMethod]
        public void SaveBook_NewBook_InsertsCorrectly()
        {
            var manager = new DatabaseManager(_options);
            var book = CreateValidBook();

            manager.SaveBook(book);

            using var context = new AppDbContext(_options);
            var savedBook = context.Books.FirstOrDefault();
            Assert.IsNotNull(savedBook);
            Assert.AreEqual(book.Name, savedBook.Name);
        }

        [TestMethod]
        public void DeleteBook_RemovesBook()
        {
            const int testId = 99;
            using (var context = new AppDbContext(_options))
            {
                context.Books.Add(new Book { BookId = testId, Name = "Delete Me" });
                context.SaveChanges();
            }

            new DatabaseManager(_options).DeleteBook(testId);

            using (var context = new AppDbContext(_options))
            {
                Assert.IsNull(context.Books.Find(testId));
            }
        }

        [TestMethod]
        public void SaveAuthor_UpdatesExistingAuthor()
        {
            const int authorId = 1;
            using (var dbcontext = new AppDbContext(_options))
            {
                dbcontext.Authors.Add(new Author { AuthorId = authorId, FirstName = "Old", LastName = "Name", Nationality = DefaultNationality });
                dbcontext.SaveChanges();
            }

            var updated = new Author { AuthorId = authorId, FirstName = "New", LastName = "Name", Nationality = DefaultNationality };
            new DatabaseManager(_options).SaveAuthor(updated);

            using var checkContext = new AppDbContext(_options);
            var author = checkContext.Authors.AsNoTracking().First(a => a.AuthorId == authorId);
            Assert.AreEqual("New", author.FirstName);
        }

        [TestMethod]
        public void DeleteAuthor_RemovesAuthorAndBooks()
        {
            using (var context = new AppDbContext(_options))
            {
                var author = new Author { AuthorId = 5, FirstName = "A", LastName = "B", Nationality = DefaultNationality };
                context.Books.Add(new Book { BookId = 10, Name = "Book", Authors = new List<Author> { author } });
                context.SaveChanges();
            }

            new DatabaseManager(_options).DeleteAuthor(5);
            AssertDatabaseEmpty();
        }

        [TestMethod]
        public void SaveBook_WithExistingAuthor_AssociatesAuthor()
        {
            const int existingAuthorId = 42;
            using (var dbcontext = new AppDbContext(_options))
            {
                dbcontext.Authors.Add(new Author { AuthorId = existingAuthorId, FirstName = "Linked", LastName = "Author", Nationality = DefaultNationality });
                dbcontext.SaveChanges();
            }

            var book = CreateValidBook("Book With Author");
            book.Authors = new List<Author> { new Author { AuthorId = existingAuthorId, Nationality = DefaultNationality } };

            new DatabaseManager(_options).SaveBook(book);

            using var verifyContext = new AppDbContext(_options);
            var dbBook = verifyContext.Books.Include(b => b.Authors).First();
            Assert.AreEqual(existingAuthorId, dbBook.Authors.First().AuthorId);
        }


        [TestMethod]
        [DataRow("No Authors", false, true, true, "1234567890")] 
        [DataRow("No Language", true, false, true, "1234567890")]
        [DataRow("No Publisher", true, true, false, "1234567890")] 
        [DataRow("Bad ISBN", true, true, true, "abc123")]         
        public void SaveBook_InvalidData_DoesNotSave(string name, bool hasAuthor, bool hasLang, bool hasPub, string isbn)
        {
            var book = new Book { Name = name, ISBN = isbn };
            if (hasAuthor) book.Authors.Add(DefaultAuthor);
            if (hasLang) book.Language = new Language { Name = "CZ" };
            if (hasPub) book.Publisher = new Publisher { Name = "Pub" };

            var validation = book.Validate();

            Assert.IsFalse(string.IsNullOrWhiteSpace(validation), $"Book '{name}' should fail validation.");
            AssertDatabaseEmpty();
        }

        [TestMethod]
        public void SaveAuthor_InvalidData_DoesNotSave()
        {
            var author = new Author { FirstName = "NoNat", LastName = "Author", Nationality = null };

            Assert.IsFalse(string.IsNullOrWhiteSpace(author.Validate()));
            AssertDatabaseEmpty();
        }
        [TestMethod]
        public void SaveBook_DuplicateISBN_DoesntSave()
        {
            var manager = new DatabaseManager(_options);
            const string duplicatedIsbn = "12345678911";

            manager.SaveBook(CreateValidBook("Original", duplicatedIsbn));

            var copy = CreateValidBook("Copy", duplicatedIsbn);

            try
            {
                manager.SaveBook(copy);
                Assert.Fail("Expected an InvalidOperationException due to duplicate ISBN, but it was not thrown.");
            }
            catch (InvalidOperationException ex)
            {
                bool containsIsbn = ex.Message.Contains("ISBN", StringComparison.OrdinalIgnoreCase);
                Assert.IsTrue(containsIsbn, $"Exception message should mention 'ISBN', but was: {ex.Message}");
            }
        }
    }
}