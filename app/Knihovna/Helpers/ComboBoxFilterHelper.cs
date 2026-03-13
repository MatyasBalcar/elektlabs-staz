using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Knihovna.Helpers
{        
    /*
      * Had do write this, default combobox search was instant (so searchign would change the table a lot), and didnt show selected thing up top, THis intercepts that
      * and makes it so its easily searchable without the table changing while looking for the right filter.
      */
    public static class ComboBoxFilterHelper
    {
        public static readonly DependencyProperty IsSearchableProperty =
            DependencyProperty.RegisterAttached(
                "IsSearchable",
                typeof(bool),
                typeof(ComboBoxFilterHelper),
                new UIPropertyMetadata(false, OnIsSearchableChanged));

        public static bool GetIsSearchable(DependencyObject obj) => (bool)obj.GetValue(IsSearchableProperty);
        public static void SetIsSearchable(DependencyObject obj, bool value) => obj.SetValue(IsSearchableProperty, value);

        private static void OnIsSearchableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ComboBox comboBox)
            {
                if ((bool)e.NewValue)
                {
                    comboBox.IsEditable = true;
                    comboBox.IsTextSearchEnabled = false;
                    comboBox.StaysOpenOnEdit = true;

                    comboBox.AddHandler(System.Windows.Controls.Primitives.TextBoxBase.TextChangedEvent, new TextChangedEventHandler(OnTextChanged));
                    comboBox.DropDownClosed += OnDropDownClosed;
                }
                else
                {
                    comboBox.RemoveHandler(System.Windows.Controls.Primitives.TextBoxBase.TextChangedEvent, new TextChangedEventHandler(OnTextChanged));
                    comboBox.DropDownClosed -= OnDropDownClosed;
                }
            }
        }

        private static void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.IsEditable && comboBox.IsKeyboardFocusWithin)
            {
                var textBox = comboBox.Template.FindName("PART_EditableTextBox", comboBox) as TextBox;
                int caretPosition = textBox?.CaretIndex ?? 0;
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

                    if (string.IsNullOrEmpty(searchText))
                        comboBox.IsDropDownOpen = false;
                    else
                        comboBox.IsDropDownOpen = !view.IsEmpty;
                }

                if (textBox != null)
                {
                    textBox.SelectionLength = 0;
                    textBox.CaretIndex = caretPosition;
                }
            }
        }

        private static void OnDropDownClosed(object sender, EventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                var view = CollectionViewSource.GetDefaultView(comboBox.ItemsSource);
                if (view != null) view.Filter = null;
            }
        }


    }
}