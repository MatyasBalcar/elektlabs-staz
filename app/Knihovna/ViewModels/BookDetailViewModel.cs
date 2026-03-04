using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Knihovna.Models;
using System.Windows;

namespace Knihovna.ViewModels
{
    public partial class BookDetailViewModel : ObservableObject
    {
        [ObservableProperty]
        private Book _currentBook;

        public BookDetailViewModel(Book book)
        {
            CurrentBook = book;
        }

        [RelayCommand]
        public static void Close(Window window)
        {
            window?.Close();
        }
        
        // Todo, edit a delete
    }
}