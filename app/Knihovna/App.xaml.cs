using System.Windows;
using Knihovna.Models; 

namespace Knihovna
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            if (!DatabaseManager.TestConnection(out string errorMessage))
            {
                MessageBox.Show(
                    errorMessage,
                    "Chyba databáze",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Environment.Exit(1);
            }

            base.OnStartup(e);
        }
    }
}