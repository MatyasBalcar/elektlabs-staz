using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Knihovna
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new ViewModels.MainViewModel();
        }
        public void ShowToast(string message)
        {
            ToastText.Text = message;

            Storyboard sb = (Storyboard)FindResource("ToastAnimation");
            sb.Begin();
        }


    }
}