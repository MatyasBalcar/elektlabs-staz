using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Knihovna.Models;
using Knihovna.Properties;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading;
using System.Windows;

namespace Knihovna.ViewModels
{
    public partial class 
        MainViewModel : ObservableObject
    {
        private readonly DatabaseManager _dbManager;

        [ObservableProperty]
        private object _currentView;

        public BooksListViewModel BooksListVm { get; }
        public AuthorsListViewModel AuthorsListVm { get; }


        public Dictionary<string, string> AvailableLanguages { get; } = new Dictionary<string, string>
        {
            { "cs", "Čeština" },
            { "en", "English" }
        };
        private string _selectedLanguage = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;

        public string SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                if (SetProperty(ref _selectedLanguage, value))
                {
                    ChangeLanguage(value);
                }
            }
        }



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

        private void ChangeLanguage(string langCode)
        {
            Knihovna.Properties.Settings.Default.AppLanguage = langCode;
            Knihovna.Properties.Settings.Default.Save();

            Thread.CurrentThread.CurrentCulture = new CultureInfo(langCode);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(langCode);

            MessageBox.Show(Resources.LanguageChangedMessage,
                Resources.LanguageChangedTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

    }
}