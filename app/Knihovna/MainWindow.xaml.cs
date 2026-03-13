using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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
        /*
         * Had do write this, default combobox search was instant (so searchign would change the table a lot), and didnt show selected thing up top, THis intercepts that
         * and makes it so its easily searchable without the table changing while looking for the right filter.
         * TODO add enter and highlighting
         */
        private void FilterComboBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.IsEditable && comboBox.IsKeyboardFocusWithin)
            {
                var textBox = comboBox.Template.FindName("PART_EditableTextBox", comboBox) as TextBox;

                int caretPosition = textBox?.CaretIndex ?? 0;

                comboBox.IsDropDownOpen = true;

                if (textBox != null)
                {
                    textBox.SelectionLength = 0;
                    textBox.CaretIndex = caretPosition;
                }

                string searchText = comboBox.Text;

                var view = CollectionViewSource.GetDefaultView(comboBox.ItemsSource);
                if (view != null)
                {
                    view.Filter = item =>
                    {
                        if (string.IsNullOrEmpty(searchText)) return true;

                        var property = item.GetType().GetProperty(comboBox.DisplayMemberPath);
                        var value = property?.GetValue(item)?.ToString();

                        return value != null && value.Contains(searchText, StringComparison.OrdinalIgnoreCase);
                    };
                }
            }
        }

        private void FilterComboBox_DropDownClosed(object sender, EventArgs e)
        {

            if (sender is ComboBox comboBox)
            {
                var view = CollectionViewSource.GetDefaultView(comboBox.ItemsSource);
                if (view != null)
                {
                    view.Filter = null;
                }
            }
        }
    }
}