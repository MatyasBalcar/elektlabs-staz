using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Knihovna.Models;

namespace Knihovna.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly DatabaseManager _dbManager;

        [ObservableProperty]
        private object _currentView;

        public BooksListViewModel BooksListVm { get; }
        public AuthorsListViewModel AuthorsListVm { get; }

        public MainViewModel()
        {
            _dbManager = new DatabaseManager();

            BooksListVm = new BooksListViewModel(_dbManager);
            AuthorsListVm = new AuthorsListViewModel(_dbManager);

            CurrentView = BooksListVm;
        }

        [RelayCommand]
        public void ShowBooks()
        {


            CurrentView = BooksListVm;
            BooksListVm.RefreshData();
            BooksListVm.LoadFilterData();
            BooksListVm.ClearFilters();

        }

        [RelayCommand]
        public void ShowAuthors()
        {

            CurrentView = AuthorsListVm;
            AuthorsListVm.RefreshData();
            AuthorsListVm.LoadFilterData();
            AuthorsListVm.ClearFilters();

        }


    }
}