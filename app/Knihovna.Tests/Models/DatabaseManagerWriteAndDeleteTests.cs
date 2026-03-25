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
        public void SaveBook_UpdatesExistingBook_ReplacesAuthors()
        {
            int authorToKeepId;
            using (var context = new AppDbContext(_options))
            {
                var authorToKeep = new Author { FirstName = "Keep", LastName = "Author", Nationality = DefaultNationality };
                context.Authors.Add(authorToKeep);
                context.SaveChanges();
                authorToKeepId = authorToKeep.AuthorId;
            }

            var manager = new DatabaseManager(_options);
            var original = CreateValidBook("Original", "1111111111");
            manager.SaveBook(original);

            int bookId;
            using (var context = new AppDbContext(_options))
            {
                bookId = context.Books.AsNoTracking().Select(b => b.BookId).First();
            }

            var updated = new Book
            {
                BookId = bookId,
                Name = "Updated",
                ISBN = "1111111111",
                Authors = new List<Author>
                {
                    new Author { AuthorId = authorToKeepId, Nationality = DefaultNationality }
                },
                Language = new Language { Name = "UpdatedLang" },
                Publisher = new Publisher { Name = "UpdatedPub" }
            };

            manager.SaveBook(updated);

            using var verifyContext = new AppDbContext(_options);
            var saved = verifyContext.Books
                .Include(b => b.Authors)
                .Include(b => b.Language)
                .Include(b => b.Publisher)
                .First(b => b.BookId == bookId);

            Assert.AreEqual("Updated", saved.Name);
            Assert.AreEqual("UpdatedLang", saved.Language?.Name);
            Assert.AreEqual("UpdatedPub", saved.Publisher?.Name);
            Assert.AreEqual(1, saved.Authors.Count);
            Assert.AreEqual(authorToKeepId, saved.Authors.First().AuthorId);
        }

        [TestMethod]
        public void SaveBook_UpdateWithDuplicateISBN_Throws()
        {
            var manager = new DatabaseManager(_options);

            manager.SaveBook(CreateValidBook("First", "4444444444"));
            manager.SaveBook(CreateValidBook("Second", "5555555555"));

            int secondBookId;
            using (var context = new AppDbContext(_options))
            {
                secondBookId = context.Books.AsNoTracking()
                    .Where(b => b.Name == "Second")
                    .Select(b => b.BookId)
                    .First();
            }

            var updated = CreateValidBook("Second", "4444444444");
            updated.BookId = secondBookId;

            try
            {
                manager.SaveBook(updated);
                Assert.Fail("Expected an InvalidOperationException due to duplicate ISBN, but it was not thrown.");
            }
            catch (InvalidOperationException ex)
            {
                bool containsIsbn = ex.Message.Contains("ISBN", StringComparison.OrdinalIgnoreCase);
                Assert.IsTrue(containsIsbn, $"Exception message should mention 'ISBN', but was: {ex.Message}");
            }
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
        public void DeleteBook_NonExisting_DoesNotThrow()
        {
            new DatabaseManager(_options).DeleteBook(999);

            using var context = new AppDbContext(_options);
            Assert.AreEqual(0, context.Books.Count());
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
        public void SaveAuthor_NewAuthor_ReusesExistingNationality()
        {
            int nationalityId;
            using (var context = new AppDbContext(_options))
            {
                var nationality = new Nationality { Name = "Czech" };
                context.Nationalities.Add(nationality);
                context.SaveChanges();
                nationalityId = nationality.NationalityID;
            }

            var author = new Author
            {
                FirstName = "Reuse",
                LastName = "Nationality",
                Nationality = new Nationality { NationalityID = nationalityId, Name = "Czech" }
            };

            new DatabaseManager(_options).SaveAuthor(author);

            using var verifyContext = new AppDbContext(_options);
            Assert.AreEqual(1, verifyContext.Nationalities.Count());
            var saved = verifyContext.Authors.Include(a => a.Nationality).First();
            Assert.AreEqual(nationalityId, saved.NationalityId);
        }

        [TestMethod]
        public void SaveAuthor_Update_ReusesExistingNationalityByName()
        {
            int nationalityId;
            int authorId;
            using (var context = new AppDbContext(_options))
            {
                var nationality = new Nationality { Name = "Czech" };
                var author = new Author { FirstName = "Reuse", LastName = "Update", Nationality = nationality };
                context.Nationalities.Add(nationality);
                context.Authors.Add(author);
                context.SaveChanges();
                nationalityId = nationality.NationalityID;
                authorId = author.AuthorId;
            }

            var updated = new Author
            {
                AuthorId = authorId,
                FirstName = "Reuse",
                LastName = "Update",
                Nationality = new Nationality { Name = "Czech" }
            };

            new DatabaseManager(_options).SaveAuthor(updated);

            using var verifyContext = new AppDbContext(_options);
            Assert.AreEqual(1, verifyContext.Nationalities.Count());
            var saved = verifyContext.Authors.AsNoTracking().First(a => a.AuthorId == authorId);
            Assert.AreEqual(nationalityId, saved.NationalityId);
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
        public void DeleteAuthor_NoBooks_RemovesAuthorOnly()
        {
            int authorToDeleteId;
            int bookId;
            using (var context = new AppDbContext(_options))
            {
                var authorToDelete = new Author { FirstName = "Delete", LastName = "Me", Nationality = DefaultNationality };
                var authorToKeep = new Author { FirstName = "Keep", LastName = "Author", Nationality = DefaultNationality };
                var book = new Book { Name = "Keep Book", Authors = new List<Author> { authorToKeep } };
                context.Authors.AddRange(authorToDelete, authorToKeep);
                context.Books.Add(book);
                context.SaveChanges();
                authorToDeleteId = authorToDelete.AuthorId;
                bookId = book.BookId;
            }

            new DatabaseManager(_options).DeleteAuthor(authorToDeleteId);

            using var verifyContext = new AppDbContext(_options);
            Assert.IsNull(verifyContext.Authors.Find(authorToDeleteId));
            Assert.IsNotNull(verifyContext.Books.Find(bookId));
        }

        [TestMethod]
        public void DeleteAuthor_FromCoauthoredBook_KeepsBookWithRemainingAuthor()
        {
            int removedAuthorId;
            int remainingAuthorId;
            int bookId;

            using (var context = new AppDbContext(_options))
            {
                var removedAuthor = new Author { FirstName = "Remove", LastName = "Me", Nationality = DefaultNationality };
                var remainingAuthor = new Author { FirstName = "Keep", LastName = "Me", Nationality = DefaultNationality };

                var book = new Book
                {
                    Name = "Coauthored Book",
                    Authors = new List<Author> { removedAuthor, remainingAuthor }
                };

                context.Books.Add(book);
                context.SaveChanges();

                removedAuthorId = removedAuthor.AuthorId;
                remainingAuthorId = remainingAuthor.AuthorId;
                bookId = book.BookId;
            }

            new DatabaseManager(_options).DeleteAuthor(removedAuthorId);

            using var verifyContext = new AppDbContext(_options);
            var savedBook = verifyContext.Books.Include(b => b.Authors).FirstOrDefault(b => b.BookId == bookId);

            Assert.IsNull(verifyContext.Authors.Find(removedAuthorId));
            Assert.IsNotNull(savedBook);
            Assert.AreEqual(1, savedBook.Authors.Count);
            Assert.AreEqual(remainingAuthorId, savedBook.Authors.First().AuthorId);
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
        public void SaveBook_WithMultipleExistingAuthors_AssociatesAllAuthors()
        {
            const int firstAuthorId = 100;
            const int secondAuthorId = 101;

            using (var dbcontext = new AppDbContext(_options))
            {
                dbcontext.Authors.AddRange(
                    new Author { AuthorId = firstAuthorId, FirstName = "First", LastName = "Author", Nationality = DefaultNationality },
                    new Author { AuthorId = secondAuthorId, FirstName = "Second", LastName = "Author", Nationality = DefaultNationality });
                dbcontext.SaveChanges();
            }

            var book = CreateValidBook("Book With Multiple Authors", "8888888888");
            book.Authors = new List<Author>
            {
                new Author { AuthorId = firstAuthorId, Nationality = DefaultNationality },
                new Author { AuthorId = secondAuthorId, Nationality = DefaultNationality }
            };

            new DatabaseManager(_options).SaveBook(book);

            using var verifyContext = new AppDbContext(_options);
            var dbBook = verifyContext.Books.Include(b => b.Authors).First();
            Assert.AreEqual(2, dbBook.Authors.Count);
            CollectionAssert.AreEquivalent(
                new[] { firstAuthorId, secondAuthorId },
                dbBook.Authors.Select(a => a.AuthorId).ToArray());
        }

        [TestMethod]
        public void SaveBook_UsesExistingLanguageAndPublisher()
        {
            int languageId;
            int publisherId;
            int authorId;
            using (var context = new AppDbContext(_options))
            {
                var language = new Language { Name = "Czech" };
                var publisher = new Publisher { Name = "TestPub" };
                var author = new Author { FirstName = "Lang", LastName = "Author", Nationality = DefaultNationality };
                context.Languages.Add(language);
                context.Publishers.Add(publisher);
                context.Authors.Add(author);
                context.SaveChanges();
                languageId = language.LanguageID;
                publisherId = publisher.PublisherID;
                authorId = author.AuthorId;
            }

            var book = new Book
            {
                Name = "Reuse",
                ISBN = "2222222222",
                Authors = new List<Author> { new Author { AuthorId = authorId, Nationality = DefaultNationality } },
                Language = new Language { LanguageID = languageId, Name = "Czech" },
                Publisher = new Publisher { PublisherID = publisherId, Name = "TestPub" }
            };

            new DatabaseManager(_options).SaveBook(book);

            using var verifyContext = new AppDbContext(_options);
            Assert.AreEqual(1, verifyContext.Languages.Count());
            Assert.AreEqual(1, verifyContext.Publishers.Count());
            var saved = verifyContext.Books.AsNoTracking().First();
            Assert.AreEqual(languageId, saved.LanguageId);
            Assert.AreEqual(publisherId, saved.PublisherId);
        }

        [TestMethod]
        public void SaveBook_Update_RemovesAllAuthors()
        {
            var manager = new DatabaseManager(_options);
            manager.SaveBook(CreateValidBook("With Authors", "6666666666"));

            int bookId;
            using (var context = new AppDbContext(_options))
            {
                bookId = context.Books.AsNoTracking().Select(b => b.BookId).First();
            }

            var updated = new Book
            {
                BookId = bookId,
                Name = "With Authors",
                ISBN = "6666666666",
                Authors = new List<Author>(),
                Language = new Language { Name = "Czech" },
                Publisher = new Publisher { Name = "TestPub" }
            };

            manager.SaveBook(updated);

            using var verifyContext = new AppDbContext(_options);
            var saved = verifyContext.Books.Include(b => b.Authors).First(b => b.BookId == bookId);
            Assert.AreEqual(0, saved.Authors.Count);
        }

        [TestMethod]
        public void SaveBook_NullLanguageAndPublisher_SetsNullIds()
        {
            var book = new Book
            {
                Name = "No Lang Pub",
                ISBN = "3333333333",
                Authors = new List<Author> { DefaultAuthor },
                Language = null,
                Publisher = null
            };

            new DatabaseManager(_options).SaveBook(book);

            using var verifyContext = new AppDbContext(_options);
            var saved = verifyContext.Books.AsNoTracking().First();
            Assert.IsNull(saved.LanguageId);
            Assert.IsNull(saved.PublisherId);
            Assert.AreEqual(0, verifyContext.Languages.Count());
            Assert.AreEqual(0, verifyContext.Publishers.Count());
        }

        [TestMethod]
        public void SaveBook_LanguageIdWithoutLanguage_SetsNullLanguageId()
        {
            var book = new Book
            {
                Name = "Language Id",
                ISBN = "7777777777",
                Authors = new List<Author> { DefaultAuthor },
                LanguageId = 5,
                Publisher = new Publisher { Name = "Pub" }
            };

            new DatabaseManager(_options).SaveBook(book);

            using var verifyContext = new AppDbContext(_options);
            var saved = verifyContext.Books.AsNoTracking().First();
            Assert.IsNull(saved.LanguageId);
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