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
            var secondAuthor = new Author { FirstName = "Karel", LastName = "Capek", Nationality = nationality };

            context.Authors.AddRange(firstAuthor, secondAuthor);
            context.SaveChanges();

            return (firstAuthor.AuthorId, secondAuthor.AuthorId);
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
    }
}
