using Knihovna.ViewModels;
using Knihovna.Localization;
using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace Knihovna
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            InitializeDataContext(new MainViewModel());
        }

        private MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            InitializeDataContext(viewModel);
        }

        private void InitializeDataContext(MainViewModel viewModel)
        {
            DataContext = viewModel;
            SubscribeToLanguageChanges(viewModel);
        }

        private void SubscribeToLanguageChanges(MainViewModel vm)
        {
            vm.RequestLanguageChange -= OnLanguageChanged;
            vm.RequestLanguageChange += OnLanguageChanged;
        }

        private void OnLanguageChanged(string langCode)
        {
            App.SetAppCulture(langCode);
            LocalizationManager.Instance.NotifyLanguageChanged();
        }

        protected override void OnClosed(EventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.RequestLanguageChange -= OnLanguageChanged;
            }

            base.OnClosed(e);
        }

        public void ShowToast(string message)
        {
            ToastText.Text = message;
            Storyboard sb = (Storyboard)FindResource("ToastAnimation");
            sb.Begin();
        }
    }
}