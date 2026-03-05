using System.Windows;

namespace Knihovna
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new ViewModels.MainViewModel();
        }
    }
}