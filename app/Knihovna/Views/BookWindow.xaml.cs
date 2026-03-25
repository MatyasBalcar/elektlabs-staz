using Knihovna.Models;
using Knihovna.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Knihovna.Views
{

    public partial class BookWindow : Window
    {
        public BookWindow()
        {
            InitializeComponent();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is BookFormViewModel vm)
            {
                if(vm.Save())
                {
                    this.DialogResult = true;
                }

            }
        }
        
        //Auto-complete logic
        private void Lang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is Language selected)
                ((BookFormViewModel)DataContext).SelectLanguageCommand.Execute(selected);
        }

        private void Pub_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is Publisher selected)
                ((BookFormViewModel)DataContext).SelectPublisherCommand.Execute(selected);
        }

        private void Author_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is Author selected)
                ((BookFormViewModel)DataContext).AddSelectedAuthorCommand.Execute(selected);
        }

        private void RemoveAuthor_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.DataContext is not Author author || DataContext is not BookFormViewModel vm)
            {
                return;
            }

            var chipBorder = FindAncestor<Border>(button);
            if (chipBorder == null)
            {
                vm.RemoveSelectedAuthorCommand.Execute(author);
                return;
            }

            var animation = new DoubleAnimation
            {
                From = chipBorder.Opacity,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(150)
            };

            animation.Completed += (_, _) => vm.RemoveSelectedAuthorCommand.Execute(author);
            chipBorder.BeginAnimation(UIElement.OpacityProperty, animation);
        }

        private static T? FindAncestor<T>(DependencyObject? child) where T : DependencyObject
        {
            while (child != null)
            {
                if (child is T typed)
                {
                    return typed;
                }

                child = VisualTreeHelper.GetParent(child);
            }

            return null;
        }
    }
}