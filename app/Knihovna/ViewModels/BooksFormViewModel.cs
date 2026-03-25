using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Knihovna.Models;
using Knihovna.Properties; 
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows;

namespace Knihovna.ViewModels
{
    public partial class BookFormViewModel : ObservableValidator
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

        public IEnumerable<Author> AvailableAuthors =>
            AllAuthors.Where(a => !SelectedAuthors.Any(s => s.AuthorId == a.AuthorId));

        private ObservableCollection<Author> _selectedAuthors = new();
        public ObservableCollection<Author> SelectedAuthors
        {
            get => _selectedAuthors;
            set
            {
                if (SetProperty(ref _selectedAuthors, value))
                {
                    OnPropertyChanged(nameof(AvailableAuthors));
                    RefreshAuthorsValidation();
                }
            }
        }

        [ObservableProperty]
        private bool _isAuthorsInvalid;

        private bool _showAuthorsValidation;

        private Author? _selectedAuthorToAdd;
        public Author? SelectedAuthorToAdd
        {
            get => _selectedAuthorToAdd;
            set => SetProperty(ref _selectedAuthorToAdd, value);
        }

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = nameof(Resources.LanguageRequired))]
        private string _languageText = string.Empty;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = nameof(Resources.PublisherRequired))]
        private string _publisherText = string.Empty;

        [ObservableProperty]
        private ObservableCollection<Language> _suggestedLanguages = new();

        [ObservableProperty]
        private ObservableCollection<Publisher> _suggestedPublishers = new();

        [ObservableProperty]
        private bool _isLangSuggestionsVisible;

        [ObservableProperty]
        private bool _isPubSuggestionsVisible;

        public BookFormViewModel(DatabaseManager dbManager, Book? book = null)
        {
            _dbManager = dbManager;
            AllAuthors = new ObservableCollection<Author>(_dbManager.GetAuthors());

            AllPublishers = new ObservableCollection<Publisher>(_dbManager.GetAllPublishers());
            AllLanguages = new ObservableCollection<Language>(_dbManager.GetAllLanguages());

            if (book == null)
            {
                EditingBook = new Book { PublishDate = DateTime.Now, HaveRead = false, Rating = 1 };
            }
            else
            {
                EditingBook = book;
                LanguageText = book.Language?.Name ?? string.Empty;
                PublisherText = book.Publisher?.Name ?? string.Empty;
                IsLangSuggestionsVisible = false;
                IsPubSuggestionsVisible = false;

                var selected = AllAuthors
                    .Where(a => book.Authors.Any(ba => ba.AuthorId == a.AuthorId))
                    .ToList();

                SelectedAuthors = new ObservableCollection<Author>(selected);
            }
        }

        public bool Save()
        {
            _showAuthorsValidation = true;
            RefreshAuthorsValidation();
            ValidateAllProperties();

            if (!string.IsNullOrWhiteSpace(LanguageText))
            {
                string langName = LanguageText.Trim();
                var existingLang = AllLanguages?.FirstOrDefault(l =>
                    l.Name.Equals(langName, StringComparison.OrdinalIgnoreCase));

                if (existingLang != null)
                {
                    EditingBook.Language = existingLang;
                    EditingBook.LanguageId = existingLang.LanguageID;
                }
                else
                {
                    EditingBook.Language = new Language { Name = langName };
                    EditingBook.LanguageId = null;
                }
            }
            else
            {
                EditingBook.Language = null;
                EditingBook.LanguageId = null;
            }

            if (!string.IsNullOrWhiteSpace(PublisherText))
            {
                string pubName = PublisherText.Trim();
                var existingPub = AllPublishers?.FirstOrDefault(p =>
                    p.Name.Equals(pubName, StringComparison.OrdinalIgnoreCase));

                if (existingPub != null)
                {
                    EditingBook.Publisher = existingPub;
                    EditingBook.PublisherId = existingPub.PublisherID;
                }
                else
                {
                    EditingBook.Publisher = new Publisher { Name = pubName };
                    EditingBook.PublisherId = null;
                }
            }
            else
            {
                EditingBook.Publisher = null;
                EditingBook.PublisherId = null;
            }

            EditingBook.Authors.Clear();
            foreach (var author in SelectedAuthors)
            {
                EditingBook.Authors.Add(author);
            }

            string validationError = EditingBook.Validate();

            if (HasErrors || !string.IsNullOrEmpty(validationError))
            {
                System.Windows.MessageBox.Show(
                    Resources.FillRequiredData + validationError,
                    Resources.ValidationError,
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);
                return false;
            }

            try
            {
                _dbManager.SaveBook(EditingBook);
                return true;
            }
            catch (InvalidOperationException ex)
            {
                System.Windows.MessageBox.Show(ex.Message, Resources.ValidationError, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return false;
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException)
            {
                System.Windows.MessageBox.Show(Resources.UnexpectedErrorMessage, Resources.DBError, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return false;
            }
        }

        partial void OnLanguageTextChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                SuggestedLanguages.Clear();
                IsLangSuggestionsVisible = false;
                return;
            }

            var filtered = AllLanguages?
                .Where(l => l.Name.StartsWith(value, StringComparison.OrdinalIgnoreCase))
                .OrderBy(l => l.Name).Take(ShownResultsCount).ToList() ?? new List<Language>();

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

            var filtered = AllPublishers?
                .Where(p => p.Name.StartsWith(value, StringComparison.OrdinalIgnoreCase))
                .OrderBy(p => p.Name).Take(ShownResultsCount).ToList() ?? new List<Publisher>();

            SuggestedPublishers = new ObservableCollection<Publisher>(filtered);
            IsPubSuggestionsVisible = SuggestedPublishers.Any();
        }

        [RelayCommand]
        private void SelectLanguage(Language? selected)
        {
            if (selected == null) return;
            LanguageText = selected.Name;
            EditingBook.Language = selected;
            EditingBook.LanguageId = selected.LanguageID;
            IsLangSuggestionsVisible = false;
        }

        [RelayCommand]
        private void SelectPublisher(Publisher? selected)
        {
            if (selected == null) return;
            PublisherText = selected.Name;
            EditingBook.Publisher = selected;
            EditingBook.PublisherId = selected.PublisherID;
            IsPubSuggestionsVisible = false;
        }

        [RelayCommand]
        public void AddAuthor()
        {
            var formVm = new AuthorFormViewModel(_dbManager);
            var window = new Views.AuthorWindow { DataContext = formVm };

            if (window.ShowDialog() ?? false)
            {
                var newAuthors = _dbManager.GetAuthors();
                AllAuthors = new ObservableCollection<Author>(newAuthors);
                SelectedAuthorToAdd = AllAuthors.FirstOrDefault(a => a.AuthorId == formVm.CurrentAuthor.AuthorId);

                if (SelectedAuthorToAdd != null)
                {
                    AddSelectedAuthor(SelectedAuthorToAdd);
                }

                if (Application.Current.MainWindow is MainWindow mainWindow)
                {
                    mainWindow.ShowToast(Resources.AuthorSavedToast);
                }
            }
        }

        [RelayCommand]
        private void AddSelectedAuthor(Author? author)
        {
            if (author == null)
            {
                return;
            }

            if (!SelectedAuthors.Any(a => a.AuthorId == author.AuthorId))
            {
                SelectedAuthors.Add(author);
                OnPropertyChanged(nameof(AvailableAuthors));
            }

            SelectedAuthorToAdd = null;
            RefreshAuthorsValidation();
        }

        [RelayCommand]
        private void RemoveSelectedAuthor(Author? author)
        {
            if (author == null)
            {
                return;
            }

            if (SelectedAuthors.Remove(author))
            {
                OnPropertyChanged(nameof(AvailableAuthors));
            }

            RefreshAuthorsValidation();
        }

        partial void OnAllAuthorsChanged(ObservableCollection<Author> value)
        {
            OnPropertyChanged(nameof(AvailableAuthors));
        }

        [RelayCommand]
        private void SetRating(object? parameter)
        {
            if (parameter != null && int.TryParse(parameter.ToString(), out int newRating))
            {
                EditingBook.Rating = (short)newRating;
                OnPropertyChanged(nameof(EditingBook));
            }
        }

        private void RefreshAuthorsValidation()
        {
            IsAuthorsInvalid = _showAuthorsValidation && SelectedAuthors.Count == 0;
        }
    }
}