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
            if (d is UIElement element)
            {
                if ((bool)e.NewValue)
                {
                    element.AddHandler(UIElement.PreviewKeyDownEvent, new KeyEventHandler(Element_PreviewKeyDown), true);
                }
                else
                {
                    element.RemoveHandler(UIElement.PreviewKeyDownEvent, new KeyEventHandler(Element_PreviewKeyDown));
                }
            }
        }

        private static void Element_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter && e.Key != Key.Tab) return;

            if (sender is AutoCompleteComboBox comboBox)
            {
                var itemToSelect = comboBox.Items.CurrentItem;
                if (itemToSelect == null && comboBox.HasItems) itemToSelect = comboBox.Items[0];

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
            else if (sender is TextBox textBox)
            {
                if (e.Key != Key.Enter) return;

                if (textBox.Parent is Panel parentPanel)
                {
                    ListBox listBox = null;
                    System.Windows.Controls.Primitives.Popup popup = null;

                    foreach (UIElement child in parentPanel.Children)
                    {
                        if (child is System.Windows.Controls.Primitives.Popup p)
                        {
                            popup = p;
                            listBox = p.Child as ListBox;
                            break;
                        }
                    }

                    if (listBox != null)
                    {
                        var itemToSelect = listBox.Items.CurrentItem;

                        if (itemToSelect == null && listBox.HasItems)
                        {
                            itemToSelect = listBox.Items[0];
                        }

                        if (itemToSelect != null)
                        {
                            e.Handled = true;

                            listBox.SelectedItem = itemToSelect;

                            if (popup != null) popup.IsOpen = false;
                            textBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                        }
                    }
                }
            }
        }
    }
}