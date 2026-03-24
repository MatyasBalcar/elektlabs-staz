using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Media.Animation;
using Knihovna.ViewModels;

namespace Knihovna
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            if (this.DataContext == null)
            {
                var vm = new MainViewModel();
                this.DataContext = vm;
                SubscribeToLanguageChanges(vm);
            }
            //force reload
            this.Loaded += OnWindowLoaded;
        }

        public MainWindow(object dataContext)
        {
            InitializeComponent();
            this.DataContext = dataContext ?? new MainViewModel();

            if (this.DataContext is MainViewModel vm)
            {
                SubscribeToLanguageChanges(vm);
            }

            this.Loaded += OnWindowLoaded;
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            this.Width += 10;


            this.Width -= 10;
        }


        private void SubscribeToLanguageChanges(MainViewModel vm)
        {
            vm.RequestLanguageChange -= OnLanguageChanged;
            vm.RequestLanguageChange += OnLanguageChanged;
        }

        private void OnLanguageChanged(string langCode)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(langCode);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(langCode);

            if (this.DataContext is MainViewModel vm)
            {
                vm.RequestLanguageChange -= OnLanguageChanged;
            }

            var newWindow = new MainWindow(this.DataContext)
            {
                Left = this.Left,
                Top = this.Top,
                Width = this.Width,
                Height = this.Height,
                WindowState = this.WindowState
            };

            Application.Current.MainWindow = newWindow;
            newWindow.Show();
            this.Close();
        }

        public void ShowToast(string message)
        {
            ToastText.Text = message;
            Storyboard sb = (Storyboard)FindResource("ToastAnimation");
            sb.Begin();
        }
    }
}