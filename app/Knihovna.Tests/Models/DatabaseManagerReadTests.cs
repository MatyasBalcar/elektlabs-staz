using Knihovna.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

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

        private void SeedDatabase(AppDbContext context)
        {
            var langCze = new Language { LanguageID = 1, Name = "Čeština" };
            var langEng = new Language { LanguageID = 2, Name = "English" };

            var pub1 = new Publisher { PublisherID = 1, Name = "Argo" };
            var pub2 = new Publisher { PublisherID = 2, Name = "Penguin Books" };

            var natCze = new Nationality { NationalityID = 1, Name = "Czech" };
            var natPol = new Nationality { NationalityID = 2, Name = "Poland" };

            var author1 = new Author { AuthorId = 1, FirstName = "Karel", LastName = "Čapek", Nationality = natCze };
            var author2 = new Author { AuthorId = 2, FirstName = "Jaroslav", LastName = "Foglar", Nationality = natCze };
            var author3 = new Author { AuthorId = 3, FirstName = "Neznámý", LastName = "Autor", Nationality = natPol };

            context.Languages.AddRange(langCze, langEng);
            context.Publishers.AddRange(pub1, pub2);
            context.Nationalities.AddRange(natCze, natPol);
            context.Authors.AddRange(author1, author2, author3);

            context.Books.AddRange(
                new Book
                {
                    BookId = 1,
                    Name = "R.U.R.",
                    Language = langCze,
                    Publisher = pub1,
                    Authors = new List<Author> { author1 }
                },
                new Book
                {
                    BookId = 2,
                    Name = "Válka s mloky",
                    Language = langCze,
                    Publisher = pub1,
                    Authors = new List<Author> { author1 }
                },
                new Book
                {
                    BookId = 3,
                    Name = "Rychlé šípy",
                    Language = langCze,
                    Publisher = pub2,
                    Authors = new List<Author> { author2 }
                },
                new Book
                {
                    BookId = 4,
                    Name = "English Book",
                    Language = langEng,
                    Publisher = pub2,
                    Authors = new List<Author> { author3 }
                }
            );

            context.SaveChanges();
        }

        // -------------------
        // BOOKS
        // -------------------

        [TestMethod]
        public void GetBooks_NoFilters_ReturnsAllBooksSorted()
        {
            var manager = new DatabaseManager(_options!);
            var result = manager.GetBooks();
            Assert.HasCount(4, result);
            Assert.AreEqual("English Book", result[0].Name);
            Assert.AreEqual("R.U.R.", result[1].Name);
            Assert.AreEqual("Rychlé šípy", result[2].Name);
            Assert.AreEqual("Válka s mloky", result[3].Name);
        }

        [TestMethod]
        public void GetBooks_FilterByName_ReturnsMatch()
        {
            var manager = new DatabaseManager(_options!);
            var result = manager.GetBooks(name: "Válka");
            Assert.HasCount(1, result);
            Assert.AreEqual("Válka s mloky", result[0].Name);
        }

        [TestMethod]
        public void GetBooks_FilterByName_ReturnsNothing()
        {
            var manager = new DatabaseManager(_options!);
            var result = manager.GetBooks(name: "Nonexistent Book");
            Assert.HasCount(0, result);
        }

        [TestMethod]
        public void GetBooks_FilterByAuthor_ReturnsMatch()
        {
            var manager = new DatabaseManager(_options!);
            var result = manager.GetBooks(author: "Čapek");
            Assert.HasCount(2, result);
        }

        [TestMethod]
        public void GetBooks_FilterByAuthor_ReturnsNothing()
        {
            var manager = new DatabaseManager(_options!);
            var result = manager.GetBooks(author: "Nonexistent Author");
            Assert.HasCount(0, result);
        }

        [TestMethod]
        public void GetBooks_FilterByLanguage_ReturnsMatch()
        {
            var manager = new DatabaseManager(_options!);
            var result = manager.GetBooks(language: "Čeština");
            Assert.HasCount(3, result);
        }

        [TestMethod]
        public void GetBooks_FilterByLanguage_ReturnsNothing()
        {
            var manager = new DatabaseManager(_options!);
            var result = manager.GetBooks(language: "Nonexistent Language");
            Assert.HasCount(0, result);
        }

        [TestMethod]
        public void GetBooks_FilterByPublisher_ReturnsMatch()
        {
            var manager = new DatabaseManager(_options!);
            var result = manager.GetBooks(publisher: "Argo");
            Assert.HasCount(2, result);
        }

        [TestMethod]
        public void GetBooks_FilterByPublisher_ReturnsNothing()
        {
            var manager = new DatabaseManager(_options!);
            var result = manager.GetBooks(publisher: "Nonexistent Publisher");
            Assert.HasCount(0, result);
        }

        [TestMethod]
        public void GetBooks_MultipleFilters_ReturnsCorrectBook()
        {
            var manager = new DatabaseManager(_options!);
            var result = manager.GetBooks(author: "Čapek", publisher: "Argo");
            Assert.HasCount(2, result);
        }

        [TestMethod]
        public void GetBooks_CaseInsensitiveAndPartialMatch()
        {
            var manager = new DatabaseManager(_options!);
            var result1 = manager.GetBooks(name: "r.u.r.");
            var result2 = manager.GetBooks(author: "čapek");
            Assert.HasCount(1, result1);
            Assert.AreEqual("R.U.R.", result1[0].Name);
            Assert.HasCount(2, result2);
        }

        // -------------------
        // AUTHORS
        // -------------------

        [TestMethod]
        public void GetAuthors_NoFilter_ReturnsAllSortedAuthors()
        {
            var manager = new DatabaseManager(_options!);
            var result = manager.GetAuthors();
            Assert.HasCount(3, result);
            Assert.AreEqual("Jaroslav Foglar", result[0].FullName);
            Assert.AreEqual("Karel Čapek", result[1].FullName);
            Assert.AreEqual("Neznámý Autor", result[2].FullName);
        }

        [TestMethod]
        public void GetAuthors_FilterByName_ReturnsMatch()
        {
            var manager = new DatabaseManager(_options!);
            var result = manager.GetAuthors(searchTerm: "Karel");
            Assert.HasCount(1, result);
            Assert.AreEqual("Karel Čapek", result[0].FullName);
        }

        [TestMethod]
        public void GetAuthors_FilterByName_ReturnsNothing()
        {
            var manager = new DatabaseManager(_options!);
            var result = manager.GetAuthors(searchTerm: "Nonexistent Name");
            Assert.HasCount(0, result);
        }

        [TestMethod]
        public void GetAuthors_FilterByNationality_ReturnsMatch()
        {
            var manager = new DatabaseManager(_options!);
            var result = manager.GetAuthors(nationality: "Czech");
            Assert.HasCount(2, result);
        }

        [TestMethod]
        public void GetAuthors_FilterByNationality_ReturnsNothing()
        {
            var manager = new DatabaseManager(_options!);
            var result = manager.GetAuthors(nationality: "Nonexistent Nationality");
            Assert.HasCount(0, result);
        }

        [TestMethod]
        public void GetBooks_NullFilters_ReturnsAllBooks()
        {
            var manager = new DatabaseManager(_options!);
            var result = manager.GetBooks(author: null, language: null, publisher: null);
            Assert.HasCount(4, result); 
            Assert.AreEqual("English Book", result[0].Name);
            Assert.AreEqual("R.U.R.", result[1].Name);
            Assert.AreEqual("Rychlé šípy", result[2].Name);
            Assert.AreEqual("Válka s mloky", result[3].Name);
        }

        [TestMethod]
        public void GetBooks_EmptyStringName_ReturnsAllBooks()
        {
            var manager = new DatabaseManager(_options!);
            var result = manager.GetBooks(name: "");
            Assert.HasCount(4, result);
            Assert.AreEqual("English Book", result[0].Name);
            Assert.AreEqual("R.U.R.", result[1].Name);
            Assert.AreEqual("Rychlé šípy", result[2].Name);
            Assert.AreEqual("Válka s mloky", result[3].Name);
        }

        [TestMethod]
        public void GetAllNationalities_ReturnsSorted()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using (var context = new AppDbContext(options))
            {
                context.Nationalities.Add(new Nationality { Name = "USA" });
                context.Nationalities.Add(new Nationality { Name = "Albania" });
                context.SaveChanges();
            }

            var manager = new DatabaseManager(options);
            var result = manager.GetAllNationalities();

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("Albania", result[0].Name);
        }

        [TestMethod]
        public void GetAllNationalities_ReturnsNothing()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var manager = new DatabaseManager(options);
            var result = manager.GetAllNationalities();

            Assert.AreEqual(0, result.Count);
        }
    }
}
