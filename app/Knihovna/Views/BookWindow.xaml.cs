using Knihovna.Models;
using Knihovna.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Knihovna.Views
{

    public partial class BookWindow : Window
    {
        public BookWindow()
        {
            InitializeComponent();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is BookFormViewModel vm)
            {
                if(vm.Save())
                {
                    this.DialogResult = true;
                }

            }
        }
        private void Lang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is Language selected)
                ((BookFormViewModel)DataContext).SelectLanguageCommand.Execute(selected);
        }

        private void Pub_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is Publisher selected)
                ((BookFormViewModel)DataContext).SelectPublisherCommand.Execute(selected);
        }
    }
}