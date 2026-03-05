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

        public BooksListViewModel BooksListVM { get; }
        public AuthorsListViewModel AuthorsListVM { get; }

        public MainViewModel()
        {
            _dbManager = new DatabaseManager();

            BooksListVM = new BooksListViewModel(_dbManager);
            AuthorsListVM = new AuthorsListViewModel(_dbManager);

            CurrentView = BooksListVM;
        }

        [RelayCommand]
        public void ShowBooks() {
            CurrentView = BooksListVM;
            BooksListVM.RefreshData();
            BooksListVM.LoadFilterData();
        } 

        [RelayCommand]
        public void ShowAuthors()
        {
            CurrentView = AuthorsListVM;
            AuthorsListVM.RefreshData();
            AuthorsListVM.LoadFilterData();
        }


    }
}