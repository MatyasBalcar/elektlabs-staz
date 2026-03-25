using Knihovna.Models;
using Knihovna.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Knihovna.Tests.ViewModels
{
    [TestClass]
    public class AuthorsListViewModelTests
    {
        private DbContextOptions<AppDbContext>? _options;

        [TestInitialize]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new AppDbContext(_options);
            SeedDatabase(context);
        }

        private static void SeedDatabase(AppDbContext context)
        {
            var czech = new Nationality { Name = "Czech" };
            var polish = new Nationality { Name = "Polish" };

            context.Nationalities.AddRange(czech, polish);
            context.Authors.AddRange(
                new Author { FirstName = "Karel", LastName = "Capek", Nationality = czech },
                new Author { FirstName = "Jaroslav", LastName = "Foglar", Nationality = czech },
                new Author { FirstName = "Adam", LastName = "Mickiewicz", Nationality = polish });

            context.SaveChanges();
        }

        [TestMethod]
        public void Constructor_LoadsAuthorsAndNationalities()
        {
            var vm = new AuthorsListViewModel(new DatabaseManager(_options!));

            Assert.IsNotNull(vm.Authors);
            Assert.IsNotNull(vm.AllNationalities);
            Assert.AreEqual(3, vm.Authors.Count);
            Assert.AreEqual(2, vm.AllNationalities.Count);
            Assert.IsFalse(vm.HasActiveFilters);
        }

        [TestMethod]
        public void SearchText_WhenChanged_RefreshesAuthors()
        {
            var vm = new AuthorsListViewModel(new DatabaseManager(_options!));

            vm.SearchText = "Karel";

            Assert.AreEqual(1, vm.Authors!.Count);
            Assert.AreEqual("Karel Capek", vm.Authors[0].FullName);
        }
    }
}
