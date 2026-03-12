using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Knihovna.Models;
using Knihovna.Services;
using System.Collections.ObjectModel;

namespace Knihovna.ViewModels
{
    public partial class BookFormViewModel : ObservableObject
    {
        private readonly DatabaseManager _dbManager;

        private const int ShownResultsCount = 3;
        
        [ObservableProperty]
        private Book _editingBook;

        [ObservableProperty]
        private ObservableCollection<Author> _allAuthors;


        [ObservableProperty]
        private ObservableCollection<Publisher> _allPublishers;
        [ObservableProperty]
        private ObservableCollection<Language> _allLanguages;

        [ObservableProperty]
        private Author _selectedAuthor;

        [ObservableProperty]
        private string _languageText = string.Empty;

        [ObservableProperty]
        private string _publisherText = string.Empty;

        [ObservableProperty]
        private ObservableCollection<Language> _suggestedLanguages = new();

        [ObservableProperty]
        private ObservableCollection<Publisher> _suggestedPublishers = new();

        [ObservableProperty]
        private bool _isLangSuggestionsVisible;

        [ObservableProperty]
        private bool _isPubSuggestionsVisible;



        public BookFormViewModel(DatabaseManager dbManager, Book book = null)
        {
            _dbManager = dbManager;

            AllAuthors = new ObservableCollection<Author>(_dbManager.GetAuthors());

            var languages = _dbManager.GetAllLanguages();
            var publishers = _dbManager.GetAllPublishers();

            AllPublishers = new ObservableCollection<Publisher>(publishers);
            AllLanguages = new ObservableCollection<Language>(languages);

            if (book == null)
            {
                EditingBook = new Book { PublishDate = DateTime.Now, HaveRead = false, Rating = 1 };
            }
            else
            {
                EditingBook = book;

                if (book.Language != null)
                {
                    LanguageText = book.Language.Name;
                }


                if (book.Publisher != null)
                {
                    PublisherText = book.Publisher.Name;
                }
                IsLangSuggestionsVisible = false;
                IsPubSuggestionsVisible= false;

                SelectedAuthor = AllAuthors.FirstOrDefault(a => book.Authors.Any(ba => ba.AuthorId == a.AuthorId));
            }
        }

        public bool Save()
        {
            if (!string.IsNullOrWhiteSpace(LanguageText))
            {
                string langName = LanguageText.Trim();

                var existingLang = AllLanguages.FirstOrDefault(l =>
                    l.Name.Equals(langName, StringComparison.OrdinalIgnoreCase));

                if (existingLang != null)
                {
                    EditingBook.Language = existingLang;
                    EditingBook.LanguageID = existingLang.LanguageID;
                }
                else
                {
                    EditingBook.Language = new Language { Name = langName };
                    EditingBook.LanguageID = null;
                }
            }
            else
            {
                EditingBook.Language = null;
                EditingBook.LanguageID = null;
            }

            if (!string.IsNullOrWhiteSpace(PublisherText))
            {
                string pubName = PublisherText.Trim();

                var existingPub = AllPublishers.FirstOrDefault(p =>
                    p.Name.Equals(pubName, StringComparison.OrdinalIgnoreCase));

                if (existingPub != null)
                {
                    EditingBook.Publisher = existingPub;
                    EditingBook.PublisherID = existingPub.PublisherID;
                }
                else
                {
                    EditingBook.Publisher = new Publisher { Name = pubName };
                    EditingBook.PublisherID = null;
                }
            }
            else
            {
                EditingBook.Publisher = null;
                EditingBook.PublisherID = null;
            }

            if (SelectedAuthor != null)
            {
                EditingBook.Authors.Clear();
                EditingBook.Authors.Add(SelectedAuthor);
            }

            string validationError = Validator.ValidateBook(EditingBook);
            if (!string.IsNullOrEmpty(validationError))
            {
                System.Windows.MessageBox.Show(
                    validationError,
                    "Chyba při ukládání",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);

                return false;
            }

            _dbManager.SaveBook(EditingBook);
            return true;
        }
        partial void OnLanguageTextChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                SuggestedLanguages.Clear();
                IsLangSuggestionsVisible = false;
                return;
            }

            var filtered = AllLanguages 
                .Where(l => l.Name.StartsWith(value, StringComparison.OrdinalIgnoreCase))
                .OrderBy(l => l.Name).Take(ShownResultsCount).ToList();

            SuggestedLanguages = new ObservableCollection<Language>(filtered);
            IsLangSuggestionsVisible = SuggestedLanguages.Any();
        }
        partial void OnPublisherTextChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                SuggestedPublishers.Clear();
                IsPubSuggestionsVisible = false;
                return;
            }

            var filtered = AllPublishers
                .Where(p => p.Name.StartsWith(value, StringComparison.OrdinalIgnoreCase))
                .OrderBy(p => p.Name).Take(ShownResultsCount).ToList();

            SuggestedPublishers = new ObservableCollection<Publisher>(filtered);
            IsPubSuggestionsVisible = SuggestedPublishers.Any();
        }
        [RelayCommand]
        private void SelectLanguage(Language selected)
        {
            if (selected == null) return;
            LanguageText = selected.Name; 

            EditingBook.Language = selected;
            EditingBook.LanguageID = selected.LanguageID;

            IsLangSuggestionsVisible = false;
        }

        [RelayCommand]
        private void SelectPublisher(Publisher selected)
        {
            if (selected == null) return;
            PublisherText = selected.Name;

            EditingBook.Publisher = selected;
            EditingBook.PublisherID = selected.PublisherID;

            IsPubSuggestionsVisible = false;
        }
        [RelayCommand]
        public void AddAuthor()
        {
            var formVM = new AuthorFormViewModel(_dbManager);
            var window = new Views.AuthorWindow();
            window.DataContext = formVM;

            //window.Owner = System.Windows.Application.Current.Windows.OfType<Views.BookWindow>().FirstOrDefault();

            if (window.ShowDialog() == true)
            {
                formVM.Save();


                var newAuthors = _dbManager.GetAuthors();
                AllAuthors = new ObservableCollection<Author>(newAuthors);

                SelectedAuthor = AllAuthors.FirstOrDefault(a => a.AuthorId == formVM.CurrentAuthor.AuthorId);
            }
        }

        [RelayCommand]
        private void SetRating(object parameter)
        {
            if (parameter != null && int.TryParse(parameter.ToString(), out int newRating))
            {
                EditingBook.Rating = (short)newRating;
                OnPropertyChanged(nameof(EditingBook));
            }
        }


    }
}