using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Knihovna.Models;
using System.Collections.ObjectModel;

public partial class AuthorFormViewModel : ObservableObject
{
    private readonly DatabaseManager _dbManager;

    private const int shownResultsCount = 3;

    [ObservableProperty]
    private Author _currentAuthor;

    [ObservableProperty]
    private ObservableCollection<Nationality> _allNationalities;

    [ObservableProperty]
    private Nationality _selectedNationality;
    [ObservableProperty]
    private string _nationalityText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Nationality> _suggestedNationalities;
    [ObservableProperty]
    private bool _isSuggestionsVisible;

    public AuthorFormViewModel(DatabaseManager dbManager, Author author = null)
    {
        _dbManager = dbManager;

        var nationalities = _dbManager.GetAllNationalities();
        AllNationalities = new ObservableCollection<Nationality>(nationalities);

        if (author == null)
        {
            CurrentAuthor = new Author();
        }
        else
        {
            CurrentAuthor = author;
            NationalityText = author.Nationality.Name;
            IsSuggestionsVisible = false;
        }
    }



    public bool Save()
    {

        if (!string.IsNullOrWhiteSpace(NationalityText))
        {
            CurrentAuthor.Nationality = new Nationality { Name = NationalityText.Trim() };
        }
        else
        {
            CurrentAuthor.Nationality = null;
            CurrentAuthor.NationalityID = null;
        }

        string validationError = Knihovna.Services.Validator.ValidateAuthor(CurrentAuthor);
        if (!string.IsNullOrEmpty(validationError))
        {
            System.Windows.MessageBox.Show(
                validationError,
                "Chyba při ukládání",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);

            return false;
        }

        _dbManager.SaveAuthor(CurrentAuthor);

        return true;

    }


    partial void OnNationalityTextChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            SuggestedNationalities = new ObservableCollection<Nationality>();
            return;
        }

        var filtered = _dbManager.GetAllNationalities()
            .Where(n => n.Name.StartsWith(value, StringComparison.OrdinalIgnoreCase))
            .OrderBy(n => n.Name)
            .Take(shownResultsCount)
            .ToList();

        SuggestedNationalities = new ObservableCollection<Nationality>(filtered);
        IsSuggestionsVisible = SuggestedNationalities.Count > 0;
    }

    [RelayCommand]
    private void SelectNationality(Nationality selected)
    {
        if (selected == null) return;

        NationalityText = selected.Name;

        SuggestedNationalities.Clear();
        IsSuggestionsVisible = false;
    }
}