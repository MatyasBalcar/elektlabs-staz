using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Knihovna.Models;
using System.Collections.ObjectModel;

namespace Knihovna.ViewModels
{
    public partial class AuthorsListViewModel : ObservableObject
    {
        private readonly DatabaseManager _dbManager;

        [ObservableProperty]
        private ObservableCollection<Author> _authors;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private ObservableCollection<Nationality> _allNationalities;

        [ObservableProperty]
        private Nationality _selectedNationality;

        public AuthorsListViewModel(DatabaseManager dbManager)
        {
            _dbManager = dbManager;
            RefreshData();
            LoadFilterData();
        }

        partial void OnSelectedNationalityChanged(Nationality value) => RefreshData();
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
            if (author == null) return;

            var result = System.Windows.MessageBox.Show(
                $"Opravdu chcete smazat autora '{author.FullName}'? \n\n" +
                "VAROVÁNÍ: Tato akce odstraní autora a jeho knihy!",
                "Potvrzení smazání",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                _dbManager.DeleteAuthor(author.AuthorID);
                RefreshData();
            }
        }

        [RelayCommand]
        public void OpenForm(Author author)
        {
            AuthorFormViewModel formVm;

            if (author == null)
            {
                formVm = new AuthorFormViewModel(_dbManager);
            }
            else
            {
                var authorCopy = author.Clone();
                formVm = new AuthorFormViewModel(_dbManager, authorCopy);
            }

            var window = new Views.AuthorWindow();
            window.DataContext = formVm;

            if (window.ShowDialog() == true)
            {
                RefreshData();
            }
        }

        [RelayCommand]
        public void OpenDetail(Author author)
        {
            if (author == null) return;

            var detailVM = new AuthorDetailViewModel(author);
            var window = new Views.AuthorDetailWindow();
            window.DataContext = detailVM;

            window.ShowDialog();

            RefreshData();
        }
    }
}