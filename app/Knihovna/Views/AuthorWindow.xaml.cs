using Knihovna.Models;
using System.Windows;
using System.Windows.Controls;

namespace Knihovna.Views
{
    public partial class AuthorWindow : Window
    {
        public AuthorWindow()
        {
            InitializeComponent();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is AuthorFormViewModel vm)
            {
                if (vm.Save())
                {
                    this.DialogResult = true;
                }

            }
        }

        //Auto complete logic
        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is Nationality selected)
            {
                if (this.DataContext is AuthorFormViewModel vm)
                {
                    vm.SelectNationalityCommand.Execute(selected);
                }
            }
        }
    }

}