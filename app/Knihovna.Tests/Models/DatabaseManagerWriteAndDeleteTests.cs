using Knihovna.Models;
using Microsoft.EntityFrameworkCore;

namespace Knihovna.Tests.Models
{
    [TestClass]
    public class DatabaseManagerWriteAndDeleteTests
    {
        private DbContextOptions<AppDbContext> _options;

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
            var manager = new DatabaseManager(_options);
            var book = new Book { Name = "Test Book", Authors = new List<Author>() };

            manager.SaveBook(book);

            using var context = new AppDbContext(_options);
            Assert.AreEqual(1, context.Books.Count());
            Assert.AreEqual("Test Book", context.Books.First().Name);
        }

        [TestMethod]
        public void DeleteBook_RemovesBook()
        {
            using (var context = new AppDbContext(_options))
            {
                context.Books.Add(new Book { BookId = 99, Name = "Delete Me" });
                context.SaveChanges();
            }

            var manager = new DatabaseManager(_options);
            manager.DeleteBook(99);

            using (var context = new AppDbContext(_options))
            {
                Assert.IsNull(context.Books.Find(99));
            }
        }

        [TestMethod]
        public void SaveAuthor_UpdatesExistingAuthor()
        {
            using (var context = new AppDbContext(_options))
            {
                context.Authors.Add(new Author { AuthorId = 1, FirstName = "Old", LastName = "Name" });
                context.SaveChanges();
            }

            var manager = new DatabaseManager(_options);
            var updated = new Author { AuthorId = 1, FirstName = "New", LastName = "Name" };

            manager.SaveAuthor(updated);

            using (var context = new AppDbContext(_options))
            {
                Assert.AreEqual("New", context.Authors.Find(1).FirstName);
            }
        }

        [TestMethod]
        public void DeleteAuthor_RemovesAuthorAndBooks()
        {
            using (var context = new AppDbContext(_options))
            {
                var author = new Author { AuthorId = 5, FirstName = "A", LastName = "B" };
                var book = new Book { BookId = 10, Name = "Book", Authors = new List<Author> { author } };
                context.Authors.Add(author);
                context.Books.Add(book);
                context.SaveChanges();
            }

            var manager = new DatabaseManager(_options);
            manager.DeleteAuthor(5);

            using (var context = new AppDbContext(_options))
            {
                Assert.AreEqual(0, context.Authors.Count());
                Assert.AreEqual(0, context.Books.Count());
            }
        }

        [TestMethod]
        public void GetAllNationalities_ReturnsSorted()
        {
            using (var context = new AppDbContext(_options))
            {
                context.Nationalities.Add(new Nationality { Name = "USA" });
                context.Nationalities.Add(new Nationality { Name = "Albania" });
                context.SaveChanges();
            }

            var manager = new DatabaseManager(_options);
            var result = manager.GetAllNationalities();

            Assert.AreEqual("Albania", result[0].Name);
        }
    }
}