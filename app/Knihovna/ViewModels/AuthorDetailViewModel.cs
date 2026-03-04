using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Knihovna.Models;
using System.Windows;


namespace Knihovna.ViewModels
{
    public partial class AuthorDetailViewModel : ObservableObject
    {
        [ObservableProperty]
        private Author _currentAuthor;

        public AuthorDetailViewModel(Author author)
        {
            CurrentAuthor = author;
        }

        [RelayCommand]
        public void Close(Window window)
        {
            window?.Close();
        }

        // TODO: Edit a delete
    }
}
