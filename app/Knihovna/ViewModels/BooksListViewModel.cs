using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Knihovna.Models;
using System.Collections.ObjectModel;

namespace Knihovna.ViewModels
{
    public partial class BooksListViewModel : ObservableObject
    {
        private readonly DatabaseManager _dbManager;

        [ObservableProperty]
        private ObservableCollection<Book> _books;

        [ObservableProperty]
        private string _searchText = string.Empty;
        [ObservableProperty]
        private ObservableCollection<Author> _allAuthors;

        [ObservableProperty]
        private ObservableCollection<Language> _allLanguages;

        [ObservableProperty]
        private ObservableCollection<Publisher> _allPublishers;

        [ObservableProperty]
        private Author _selectedAuthor;

        [ObservableProperty]
        private Language _selectedLanguage;

        [ObservableProperty]
        private Publisher _selectedPublisher;
        public BooksListViewModel(DatabaseManager dbManager)
        {
            _dbManager = dbManager;
            RefreshData();
            LoadFilterData();
        }

        partial void OnSelectedAuthorChanged(Author value) => RefreshData();
        partial void OnSelectedLanguageChanged(Language value) => RefreshData();
        partial void OnSelectedPublisherChanged(Publisher value) => RefreshData();
        partial void OnSearchTextChanged(string value) => RefreshData();

        public void LoadFilterData()
        {
            AllAuthors = new ObservableCollection<Author>(_dbManager.GetAuthors());
            AllLanguages = new ObservableCollection<Language>(_dbManager.GetAllLanguages());
            AllPublishers = new ObservableCollection<Publisher>(_dbManager.GetAllPublishers());
        }

        [RelayCommand]
        public void ClearFilters()
        {
            SelectedAuthor = null;
            SelectedLanguage = null;
            SelectedPublisher = null;
        }

        [RelayCommand]
        public void RefreshData()
        {
            var data = _dbManager.GetBooks(
                SearchText,
                SelectedAuthor?.FullName,
                SelectedLanguage?.Name,
                SelectedPublisher?.Name);
            Books = new ObservableCollection<Book>(data);
        }


        //[RelayCommand]
        //public void AddBook()
        //{
        //    var formVM = new BookFormViewModel(_dbManager);
        //    var window = new Views.BookWindow { DataContext = formVM };

        //    if (window.ShowDialog() == true)
        //    {
        //        formVM.Save();
        //        RefreshData();
        //    }
        //}

        [RelayCommand]
        public void Delete(Book book)
        {
            if (book == null) return;

            var result = System.Windows.MessageBox.Show($"Smazat '{book.Name}'?", "Potvrzení",
                System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                try
                {
                    _dbManager.DeleteBook(book.BookID);
                    RefreshData();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(
                        $"Knihu se nepodařilo smazat.\nDetail: {ex.Message}",
                        "Chyba při mazání",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        public void OpenForm(Book book)
        {
            BookFormViewModel formVM;

            if (book == null)
            {
                formVM = new BookFormViewModel(_dbManager);
            }
            else
            {
                var bookCopy = (Book)book.Clone();
                formVM = new BookFormViewModel(_dbManager, bookCopy);
            }

            var window = new Views.BookWindow();
            window.DataContext = formVM;

            if (window.ShowDialog() == true)
            {
                RefreshData();
            }
        }
        [RelayCommand]
        public void OpenDetail(Book book)
        {
            if (book == null) return;

            var window = new Views.BookDetailWindow();
            window.DataContext = new BookDetailViewModel(book);

            window.ShowDialog();

            RefreshData();
        }
    }
}