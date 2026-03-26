using Knihovna.Models;
using Knihovna.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Knihovna.Tests.ViewModels
{
    [TestClass]
    public class BooksFormViewModelTests
    {
        private static DbContextOptions<AppDbContext> CreateOptions()
        {
            return new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        private static (int firstAuthorId, int secondAuthorId) SeedAuthors(DbContextOptions<AppDbContext> options)
        {
            using var context = new AppDbContext(options);

            var nationality = new Nationality { Name = "Test" };
            var firstAuthor = new Author { FirstName = "George", LastName = "Orwell", Nationality = nationality };
            var secondAuthor = new Author { FirstName = "Karel", LastName = "Čapek", Nationality = nationality };

            context.Authors.AddRange(firstAuthor, secondAuthor);
            context.SaveChanges();

            return (firstAuthor.AuthorId, secondAuthor.AuthorId);
        }

        private static void SeedLanguagesAndPublishers(DbContextOptions<AppDbContext> options)
        {
            using var context = new AppDbContext(options);

            context.Languages.AddRange(
                new Language { Name = "English" },
                new Language { Name = "Esperanto" },
                new Language { Name = "Estonian" },
                new Language { Name = "Ewe" },
                new Language { Name = "Czech" });

            context.Publishers.AddRange(
                new Publisher { Name = "Pearson" },
                new Publisher { Name = "Penguin" },
                new Publisher { Name = "Planeta" },
                new Publisher { Name = "Pragocon" },
                new Publisher { Name = "Albatros" });

            context.SaveChanges();
        }

        [TestMethod]
        public void AvailableAuthors_WhenEditingBook_ExcludesAlreadySelectedAuthor()
        {
            var options = CreateOptions();
            var (firstAuthorId, secondAuthorId) = SeedAuthors(options);

            var book = new Book
            {
                Name = "Test Book",
                Authors = new List<Author>
                {
                    new Author { AuthorId = firstAuthorId }
                }
            };

            var vm = new BookFormViewModel(new DatabaseManager(options), book);

            var availableIds = vm.AvailableAuthors.Select(a => a.AuthorId).ToList();

            CollectionAssert.DoesNotContain(availableIds, firstAuthorId);
            CollectionAssert.Contains(availableIds, secondAuthorId);
        }

        [TestMethod]
        public void AvailableAuthors_WhenAuthorAddedAndRemoved_UpdatesCorrectly()
        {
            var options = CreateOptions();
            SeedAuthors(options);

            var vm = new BookFormViewModel(new DatabaseManager(options));
            var authorToToggle = vm.AllAuthors.First();

            vm.AddSelectedAuthorCommand.Execute(authorToToggle);
            Assert.IsFalse(vm.AvailableAuthors.Any(a => a.AuthorId == authorToToggle.AuthorId));

            vm.RemoveSelectedAuthorCommand.Execute(authorToToggle);
            Assert.IsTrue(vm.AvailableAuthors.Any(a => a.AuthorId == authorToToggle.AuthorId));
        }

        [TestMethod]
        public void AddSelectedAuthor_WhenExecutedTwice_DoesNotDuplicateAuthor()
        {
            var options = CreateOptions();
            SeedAuthors(options);

            var vm = new BookFormViewModel(new DatabaseManager(options));
            var authorToAdd = vm.AllAuthors.First();

            vm.AddSelectedAuthorCommand.Execute(authorToAdd);
            vm.AddSelectedAuthorCommand.Execute(authorToAdd);

            Assert.AreEqual(1, vm.SelectedAuthors.Count(a => a.AuthorId == authorToAdd.AuthorId));
        }

        [TestMethod]
        public void LanguageSuggestions_WhenTypingPrefix_ReturnsTopThreeSortedAndVisible()
        {
            var options = CreateOptions();
            SeedLanguagesAndPublishers(options);

            var vm = new BookFormViewModel(new DatabaseManager(options));
            vm.LanguageText = "e";

            var languageNames = vm.SuggestedLanguages.Select(l => l.Name).ToList();

            CollectionAssert.AreEqual(new List<string> { "English", "Esperanto", "Estonian" }, languageNames);
            Assert.IsTrue(vm.IsLangSuggestionsVisible);
        }

        [TestMethod]
        public void LanguageSuggestions_WhenTextCleared_HidesAndClearsSuggestions()
        {
            var options = CreateOptions();
            SeedLanguagesAndPublishers(options);

            var vm = new BookFormViewModel(new DatabaseManager(options));
            vm.LanguageText = "En";
            Assert.IsTrue(vm.SuggestedLanguages.Count > 0);

            vm.LanguageText = " ";

            Assert.AreEqual(0, vm.SuggestedLanguages.Count);
            Assert.IsFalse(vm.IsLangSuggestionsVisible);
        }
    }
}
