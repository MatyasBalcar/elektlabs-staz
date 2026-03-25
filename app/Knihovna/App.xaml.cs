using Knihovna.Models;
using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

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

            string languageCode = Knihovna.Properties.Settings.Default.AppLanguage;

            SetAppCulture(languageCode);


            base.OnStartup(e);
        }

        public static void SetAppCulture(string langCode)
        {
            try
            {
                var culture = new CultureInfo(langCode);

                CultureInfo.DefaultThreadCurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentUICulture = culture;
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
            }
            catch (CultureNotFoundException)
            {
                var fallbackCulture = CultureInfo.GetCultureInfo("en");

                CultureInfo.DefaultThreadCurrentCulture = fallbackCulture;
                CultureInfo.DefaultThreadCurrentUICulture = fallbackCulture;
                Thread.CurrentThread.CurrentCulture = fallbackCulture;
                Thread.CurrentThread.CurrentUICulture = fallbackCulture;
            }
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