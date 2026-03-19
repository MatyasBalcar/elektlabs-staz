using Knihovna.Models;
using Microsoft.EntityFrameworkCore;

namespace Knihovna.Tests.Models
{
    [TestClass]
    public class DatabaseManagerReadTests
    {
        private DbContextOptions<AppDbContext>? _options;

        [TestInitialize]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new AppDbContext(_options);
            SeedDatabase(context);
        }

        /*
         * Seed the DB
         */
        private void SeedDatabase(AppDbContext context)
        {
            var langCze = new Language { LanguageID = 1, Name = "Čeština" };
            var langEng = new Language { LanguageID = 2, Name = "English" };
            var pub1 = new Publisher { PublisherID = 1, Name = "Argo" };
            var pub2 = new Publisher { PublisherID = 2, Name = "Penguin Books" };
            var natCze = new Nationality { NationalityID = 1, Name = "Czech" };
            var author1 = new Author { AuthorId = 1, FirstName = "Karel", LastName = "Čapek", Nationality = natCze };
            var author2 = new Author { AuthorId = 2, FirstName = "Jaroslav", LastName = "Foglar", Nationality = natCze };


            context.Languages.AddRange(langCze, langEng);
            context.Publishers.AddRange(pub1, pub2);
            context.Nationalities.Add(natCze);
            context.Authors.Add(author1);
            context.Authors.Add(author2);

            context.Books.Add(new Book
            {
                BookId = 1,
                Name = "R.U.R.",
                Language = langCze,
                Publisher = pub1,
                Authors = new List<Author> { author1 }
            });

            context.Books.Add(new Book
            {
                BookId = 2,
                Name = "Válka s mloky",
                Language = langCze,
                Publisher = pub1,
                Authors = new List<Author> { author1 }
            });

            context.Books.Add(new Book
            {
                BookId = 3,
                Name = "Rychle sipy",
                Language = langCze,
                Publisher = pub2,
                Authors = new List<Author> { author2 }
            });

            context.SaveChanges();
        }
        /*
         * BOOKS
         *
         * No filters
         */
        [TestMethod]
        public void GetBooks_NoFilters_ReturnsAllBooksSorted()
        {
            var manager = new DatabaseManager(_options!);
            var result = manager.GetBooks();
            Assert.HasCount(3, result);
            Assert.AreEqual("R.U.R.", result[0].Name);
            Assert.AreEqual("Rychle sipy", result[1].Name);
            Assert.AreEqual("Válka s mloky", result[2].Name);
        }
        /*
         * Filters and search
         */
        [TestMethod]
        public void GetBooks_FilterByName_ReturnsMatch()
        {
            var manager = new DatabaseManager(_options!);
            var result = manager.GetBooks(name: "Válka");
            Assert.HasCount(1, result);
            Assert.AreEqual("Válka s mloky", result[0].Name);
        }

        [TestMethod]
        public void GetBooks_FilteredByAuthor_ReturnsMatch()
        {
            var manager = new DatabaseManager(_options!);
            var result = manager.GetBooks(author: "Čapek");
            Assert.HasCount(2, result);
        }

        [TestMethod]
        public void GetBooks_FilteredByLanguage_ReturnsMatch()
        {
            var manager = new DatabaseManager(_options!);
            var result = manager.GetBooks(language: "Čeština");
            Assert.HasCount(3, result);
        }

        [TestMethod]
        public void GetBooks_FilteredByPublisher_ReturnsMatch()
        {
            var manager = new DatabaseManager(_options!);
            var result = manager.GetBooks(publisher: "Argo");
            Assert.HasCount(2, result);
        }

        /*
         * AUTHORS
         * No filters
         */
        [TestMethod]
        public void GetAuthors_NoFilter_ReturnsAllSortedAuthors()
        {
            var manager = new DatabaseManager(_options!);
            var result = manager.GetAuthors();
            Assert.HasCount(2, result);
            Assert.AreEqual("Jaroslav Foglar", result[0].FullName);
            Assert.AreEqual("Karel Čapek", result[1].FullName);
        }
    }
}