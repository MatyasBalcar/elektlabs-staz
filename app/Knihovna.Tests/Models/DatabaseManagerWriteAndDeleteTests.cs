using Knihovna.Models;
using Microsoft.EntityFrameworkCore;

namespace Knihovna.Tests.Models
{
    [TestClass]
    public class DatabaseManagerWriteAndDeleteTests
    {
        private DbContextOptions<AppDbContext>? _options;

        [TestInitialize]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [TestMethod]
        public void SaveBook_NewBook_InsertsCorrectly()
        {
            var manager = new DatabaseManager(_options!);
            var book = new Book
            {
                Name = "Test Book",
                Authors = new List<Author> { new Author { FirstName = "A", LastName = "B" } },
                Language = new Language { Name = "Czech" },
                Publisher = new Publisher { Name = "TestPub" }
            };

            var validation = book.Validate();
            Assert.IsTrue(string.IsNullOrWhiteSpace(validation), "Book should be valid before saving: " + validation);

            manager.SaveBook(book);

            using var context = new AppDbContext(_options!);
            Assert.AreEqual(1, context.Books.Count());
            Assert.AreEqual("Test Book", context.Books.First().Name);
        }

        [TestMethod]
        public void DeleteBook_RemovesBook()
        {
            using (var context = new AppDbContext(_options!))
            {
                context.Books.Add(new Book { BookId = 99, Name = "Delete Me" });
                context.SaveChanges();
            }

            var manager = new DatabaseManager(_options!);
            manager.DeleteBook(99);

            using (var context = new AppDbContext(_options!))
            {
                Assert.IsNull(context.Books.Find(99));
            }
        }

        [TestMethod]
        public void SaveAuthor_UpdatesExistingAuthor()
        {
            using (var context = new AppDbContext(_options!))
            {
                context.Authors.Add(new Author { AuthorId = 1, FirstName = "Old", LastName = "Name" });
                context.SaveChanges();
            }

            var manager = new DatabaseManager(_options!);
            var updated = new Author { AuthorId = 1, FirstName = "New", LastName = "Name", Nationality = new Nationality { Name = "Czech" } };

            manager.SaveAuthor(updated);

            using (var context = new AppDbContext(_options!))
            {
                var author = context.Authors.Find(1);
                Assert.IsNotNull(author);
                Assert.AreEqual("New", author.FirstName);
            }
        }

        [TestMethod]
        public void DeleteAuthor_RemovesAuthorAndBooks()
        {
            using (var context = new AppDbContext(_options!))
            {
                var author = new Author { AuthorId = 5, FirstName = "A", LastName = "B" };
                var book = new Book { BookId = 10, Name = "Book", Authors = new List<Author> { author } };
                context.Authors.Add(author);
                context.Books.Add(book);
                context.SaveChanges();
            }

            var manager = new DatabaseManager(_options!);
            manager.DeleteAuthor(5);

            using (var context = new AppDbContext(_options!))
            {
                Assert.AreEqual(0, context.Authors.Count());
                Assert.AreEqual(0, context.Books.Count());
            }
        }

        [TestMethod]
        public void SaveAuthor_NewAuthor_Inserts()
        {
            var manager = new DatabaseManager(_options!);
            var author = new Author { FirstName = "New", LastName = "Author", Nationality = new Nationality { Name = "Czech" } };

            var validation = author.Validate();
            Assert.IsTrue(string.IsNullOrWhiteSpace(validation), "Author should be valid before saving: " + validation);

            manager.SaveAuthor(author);

            using var context = new AppDbContext(_options!);
            Assert.AreEqual(1, context.Authors.Count());
            Assert.AreEqual("New", context.Authors.First().FirstName);
        }

        [TestMethod]
        public void SaveBook_WithExistingAuthor_AssociatesAuthor()
        {
            using (var context = new AppDbContext(_options!))
            {
                context.Authors.Add(new Author { AuthorId = 42, FirstName = "Linked", LastName = "Author" });
                context.SaveChanges();
            }

            var manager = new DatabaseManager(_options!);
            var book = new Book { Name = "Book With Author", Authors = new List<Author> { new Author { AuthorId = 42 } }, Language = new Language { Name = "Czech" }, Publisher = new Publisher { Name = "Pub" } };

            manager.SaveBook(book);

            using (var context = new AppDbContext(_options!))
            {
                var dbBook = context.Books.Include(b => b.Authors).FirstOrDefault(b => b.Name == "Book With Author");
                Assert.IsNotNull(dbBook);
                Assert.AreEqual(1, dbBook.Authors.Count);
                Assert.AreEqual(42, dbBook.Authors.First().AuthorId);
            }
        }

        [TestMethod]
        public void SaveBook_NoAuthors_DoesNotSave()
        {
            var book = new Book { Name = "No Authors", Authors = new List<Author>() };

            var validation = book.Validate();
            Assert.IsFalse(string.IsNullOrWhiteSpace(validation));

            using var context = new AppDbContext(_options!);
            Assert.AreEqual(0, context.Books.Count());
        }

        [TestMethod]
        public void SaveBook_NoLanguage_DoesNotSave()
        {
            var book = new Book { Name = "No Language", Authors = new List<Author> { new Author { FirstName = "A", LastName = "B" } }, Language = null, LanguageId = null };

            var validation = book.Validate();
            Assert.IsFalse(string.IsNullOrWhiteSpace(validation));

            using var context = new AppDbContext(_options!);
            Assert.AreEqual(0, context.Books.Count());
        }

        [TestMethod]
        public void SaveBook_NoPublisher_DoesNotSave()
        {
            var book = new Book { Name = "No Publisher", Authors = new List<Author> { new Author { FirstName = "A", LastName = "B" } }, Publisher = null, PublisherId = null };

            var validation = book.Validate();
            Assert.IsFalse(string.IsNullOrWhiteSpace(validation));

            using var context = new AppDbContext(_options!);
            Assert.AreEqual(0, context.Books.Count());
        }

        [TestMethod]
        public void SaveBook_InvalidISBN_DoesNotSave()
        {
            var book = new Book { Name = "Bad ISBN", ISBN = "abc123", Authors = new List<Author> { new Author { FirstName = "A", LastName = "B" } } };

            var validation = book.Validate();
            Assert.IsFalse(string.IsNullOrWhiteSpace(validation));

            using var context = new AppDbContext(_options!);
            Assert.AreEqual(0, context.Books.Count());
        }

        [TestMethod]
        public void SaveBook_DuplicateISBN_DoesntSave()
        {
            var manager = new DatabaseManager(_options!);

            var original = new Book { Name = "Original", ISBN = "12345678911", Authors = new List<Author> { new Author { FirstName = "A", LastName = "B" } } };
            manager.SaveBook(original);

            var copy = new Book { Name = "Copy", ISBN = "12345678911", Authors = new List<Author> { new Author { FirstName = "A", LastName = "B" } } };

            try
            {
                manager.SaveBook(copy);

                Assert.Fail("Expected an InvalidOperationException, but the duplicate book saved successfully.");
            }
            catch (InvalidOperationException ex)
            { 
                Assert.IsTrue(ex.Message.Contains("ISBN"));
            }
        }

        [TestMethod]
        public void SaveAuthor_MissingNationality_DoesNotSave()
        {
            var author = new Author { FirstName = "NoNat", LastName = "Author", Nationality = null, NationalityId = null };

            var validation = author.Validate();
            Assert.IsFalse(string.IsNullOrWhiteSpace(validation));

            using var context = new AppDbContext(_options!);
            Assert.AreEqual(0, context.Authors.Count());
        }
    }
}