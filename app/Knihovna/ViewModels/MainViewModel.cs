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
            var savedAuthorId = BooksListVm.SelectedAuthor?.AuthorId;
            var savedLangId = BooksListVm.SelectedLanguage?.LanguageID; 
            var savedPubId = BooksListVm.SelectedPublisher?.PublisherID;

            CurrentView = BooksListVm;
            BooksListVm.RefreshData();
            BooksListVm.LoadFilterData();

            if (savedAuthorId != null)
                BooksListVm.SelectedAuthor = BooksListVm.AllAuthors.FirstOrDefault(a => a.AuthorId == savedAuthorId);

            if (savedLangId != null)
                BooksListVm.SelectedLanguage = BooksListVm.AllLanguages.FirstOrDefault(l => l.LanguageID == savedLangId);

            if (savedPubId != null)
                BooksListVm.SelectedPublisher = BooksListVm.AllPublishers.FirstOrDefault(p => p.PublisherID == savedPubId);
        }

        [RelayCommand]
        public void ShowAuthors()
        {
            var savedNatId = AuthorsListVm.SelectedNationality?.NationalityID;

            CurrentView = AuthorsListVm;
            AuthorsListVm.RefreshData();
            AuthorsListVm.LoadFilterData();

            if (savedNatId != null)
            {
                AuthorsListVm.SelectedNationality =
                    AuthorsListVm.AllNationalities.FirstOrDefault(n => n.NationalityID == savedNatId);
            }
        }


    }
}