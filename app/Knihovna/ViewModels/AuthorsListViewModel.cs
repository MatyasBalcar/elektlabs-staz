using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Knihovna.Models;
using Knihovna.Properties;
using System.Collections.ObjectModel;
using System.Windows;

namespace Knihovna.ViewModels
{
    public partial class AuthorsListViewModel : ObservableObject
    {
        private readonly DatabaseManager _dbManager;

        [ObservableProperty]
        private ObservableCollection<Author>? _authors;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private ObservableCollection<Nationality>? _allNationalities;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasActiveFilters))] 
        private Nationality? _selectedNationality;

        public string EditLabel => Knihovna.Properties.Resources.Edit;
        public string DeleteLabel => Knihovna.Properties.Resources.Delete;
        public AuthorsListViewModel(DatabaseManager dbManager)
        {
            _dbManager = dbManager;
            RefreshData();
            LoadFilterData();
        }

        partial void OnSelectedNationalityChanged(Nationality? value) => RefreshData();
        partial void OnSearchTextChanged(string value) => RefreshData();

        public void LoadFilterData()
        {
            AllNationalities = new ObservableCollection<Nationality>(_dbManager.GetAllNationalities());
        }

        [RelayCommand]
        public void ClearFilters()
        {
            SelectedNationality = null;
        }

        [RelayCommand]
        public void RefreshData()
        {
            var seznamZDb = _dbManager.GetAuthors(SearchText, SelectedNationality?.Name);
            Authors = new ObservableCollection<Author>(seznamZDb);
        }

        [RelayCommand]
        public void Delete(Author author)
        {

            var result = System.Windows.MessageBox.Show(
                string.Format(Resources.ConfirmDeleteAuthor, author.FullName),
                Resources.ConfirmAuthorDeleteTitle,
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                try
                {
                    _dbManager.DeleteAuthor(author.AuthorId);
                    RefreshData();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(
                        string.Format(Resources.AuthorDeleteError, ex.Message),
                        Resources.DeleteErrorTitle,
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        public void OpenForm(Author? author)
        {
            AuthorFormViewModel formVm;

            if (author == null)
            {
                formVm = new AuthorFormViewModel(_dbManager);
            }
            else
            {
                var authorCopy = (Author)author.Clone();
                formVm = new AuthorFormViewModel(_dbManager, authorCopy);
            }

            var window = new Views.AuthorWindow();
            window.DataContext = formVm;

                if (window.ShowDialog() ?? false)
            {
                RefreshData();
                LoadFilterData();
                if (Application.Current.MainWindow is MainWindow mainWindow)
                {
                    mainWindow.ShowToast(Resources.AuthorSavedToast);
                }
            }
        }

        [RelayCommand]
        public void OpenDetail(Author author)
        {

            var window = new Views.AuthorDetailWindow();
            window.DataContext = new AuthorDetailViewModel(author);

            window.ShowDialog();

            RefreshData();
        }

        public bool HasActiveFilters =>
            SelectedNationality != null;
    }
}