using DotNetKit.Windows.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Data;

namespace Knihovna.Helpers
{
    public static class AutoCompleteHelper
    {
        public static readonly DependencyProperty SelectFirstOnEnterProperty =
            DependencyProperty.RegisterAttached(
                "SelectFirstOnEnter",
                typeof(bool),
                typeof(AutoCompleteHelper),
                new UIPropertyMetadata(false, OnSelectFirstOnEnterChanged));

        public static bool GetSelectFirstOnEnter(DependencyObject obj) => (bool)obj.GetValue(SelectFirstOnEnterProperty);
        public static void SetSelectFirstOnEnter(DependencyObject obj, bool value) => obj.SetValue(SelectFirstOnEnterProperty, value);

        private static void OnSelectFirstOnEnterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ComboBox comboBox)
            {
                if ((bool)e.NewValue)
                {
                    //also handle breakpoints that have been handled, as is the first enter key press
                    comboBox.AddHandler(UIElement.PreviewKeyDownEvent, new KeyEventHandler(ComboBox_PreviewKeyDown), true);
                }
                else
                {
                    comboBox.RemoveHandler(UIElement.PreviewKeyDownEvent, new KeyEventHandler(ComboBox_PreviewKeyDown));
                }
            }
        }

        private static void ComboBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Enter || e.Key == Key.Tab) && sender is AutoCompleteComboBox comboBox)
            {
                var itemToSelect = comboBox.Items.CurrentItem;

                if (itemToSelect == null && comboBox.HasItems)
                {
                    itemToSelect = comboBox.Items[0];
                }

                if (itemToSelect != null)
                {
                    e.Handled = true;

                    comboBox.SelectedItem = itemToSelect;
                    comboBox.IsDropDownOpen = false;

                    var binding = BindingOperations.GetBindingExpression(comboBox, ComboBox.SelectedItemProperty);
                    binding?.UpdateSource();

                    comboBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                }
            }
        }
    }
}