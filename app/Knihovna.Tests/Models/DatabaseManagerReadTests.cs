using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.EntityFrameworkCore;
using Knihovna.Models;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Knihovna.Tests
{
    [TestClass]
    public class DatabaseManagerReadTests
    {
        private DbContextOptions<AppDbContext> _options;

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
            var natCze = new Nationality { NationalityID = 1, Name = "Czech" };
            var author1 = new Author { AuthorId = 1, FirstName = "Karel", LastName = "Čapek", Nationality = natCze };

            context.Languages.AddRange(langCze, langEng);
            context.Publishers.Add(pub1);
            context.Nationalities.Add(natCze);
            context.Authors.Add(author1);

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

            context.SaveChanges();
        }

        [TestMethod]
        public void GetBooks_NoFilters_ReturnsAllBooksSorted()
        {
            var manager = new DatabaseManager(_options);
            var result = manager.GetBooks();
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("R.U.R.", result[0].Name);
        }

        [TestMethod]
        public void GetBooks_FilterByName_ReturnsMatch()
        {
            var manager = new DatabaseManager(_options);
            var result = manager.GetBooks(name: "Válka");
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Válka s mloky", result[0].Name);
        }

        [TestMethod]
        public void GetAuthors_ReturnsSortedAuthors()
        {
            var manager = new DatabaseManager(_options);
            var result = manager.GetAuthors();
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Karel Čapek", result[0].FullName);
        }
    }
}