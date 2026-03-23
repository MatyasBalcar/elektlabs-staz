using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Knihovna.Models;

namespace Knihovna
{
    public partial class App : Application
    {
        public App()
        {
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException!;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            if (!DatabaseManager.TestConnection(out string errorMessage))
            {
                MessageBox.Show(
                    errorMessage,
                    "Kritická chyba databáze",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Environment.Exit(1);
            }

            base.OnStartup(e);
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            LogError(e.Exception);
            ShowFriendlyMessage();

            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                LogError(ex);
            }
            ShowFriendlyMessage();
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            LogError(e.Exception);
            ShowFriendlyMessage();
            e.SetObserved(); 
        }

        private void LogError(Exception ex)
        {
            try
            {
                string logFilePath = "error.log";
                string message = $"[{DateTime.Now:dd.MM.yyyy HH:mm:ss}] {ex.GetType().Name}\nDetail: {ex.Message}\nStack Trace:\n{ex.StackTrace}\n" + new string('-', 50) + "\n";
                File.AppendAllText(logFilePath, message);
            }
            catch
            {
                // ignored
            }
        }

        private void ShowFriendlyMessage()
        {
            MessageBox.Show(
                "Jejda, v aplikaci došlo k neočekávané chybě. Omlouváme se za komplikace..",
                "Neočekávaná chyba",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }
}