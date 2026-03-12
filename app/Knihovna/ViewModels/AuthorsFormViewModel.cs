using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Knihovna.Models;
using System.Collections.ObjectModel;

public partial class AuthorFormViewModel : ObservableObject
{
    private readonly DatabaseManager _dbManager;

    private const int ShownResultsCount = 3;

    [ObservableProperty]
    private Author _currentAuthor;

    [ObservableProperty]
    private ObservableCollection<Nationality> _allNationalities;

    [ObservableProperty]
    private Nationality? _selectedNationality;
    [ObservableProperty]
    private string _nationalityText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Nationality>? _suggestedNationalities;
    [ObservableProperty]
    private bool _isSuggestionsVisible;

    public AuthorFormViewModel(DatabaseManager dbManager, Author? author = null)
    {
        _dbManager = dbManager;

        var nationalities = DatabaseManager.GetAllNationalities();
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
            string natName = NationalityText.Trim();
            var existingNat = AllNationalities.FirstOrDefault(n => n.Name.Equals(natName, StringComparison.OrdinalIgnoreCase));

            if (existingNat != null)
            {
                CurrentAuthor.Nationality = existingNat;
                CurrentAuthor.NationalityId = existingNat.NationalityID;
            }
            else
            {
                CurrentAuthor.Nationality = new Nationality { Name = natName };
                CurrentAuthor.NationalityId = null;
            }
        }
        else
        {
            CurrentAuthor.Nationality = null;
            CurrentAuthor.NationalityId = null;
        }

        string validationError = CurrentAuthor.Validate();
        if (!string.IsNullOrEmpty(validationError))
        {
            System.Windows.MessageBox.Show(validationError, "Chyba při ukládání",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return false;
        }

        try
        {
            DatabaseManager.SaveAuthor(CurrentAuthor);
            return true;
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            System.Windows.MessageBox.Show(
                "Autora se nepodařilo uložit do databáze",
                "Chyba databáze",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
            return false;
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"Došlo k neočekávané chybě:\n{ex.Message}",
                "Kritická chyba",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
            return false;
        }
    }

    partial void OnNationalityTextChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            SuggestedNationalities = new ObservableCollection<Nationality>();
            return;
        }

        var filtered = DatabaseManager.GetAllNationalities()
            .Where(n => n.Name.StartsWith(value, StringComparison.OrdinalIgnoreCase))
            .OrderBy(n => n.Name)
            .Take(ShownResultsCount)
            .ToList();

        SuggestedNationalities = new ObservableCollection<Nationality>(filtered);
        IsSuggestionsVisible = SuggestedNationalities.Count > 0;
    }

    [RelayCommand]
    private void SelectNationality(Nationality? selected)
    {
        if (selected == null) return;

        NationalityText = selected.Name;

        SuggestedNationalities?.Clear();
        IsSuggestionsVisible = false;
    }
}