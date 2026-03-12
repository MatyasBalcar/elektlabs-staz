using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Knihovna.Converters
{
    public class RatingToColorConverter : IValueConverter
    {
        private readonly SolidColorBrush _selectedColor = Brushes.Gold;
        private readonly SolidColorBrush _notSelectedColor = Brushes.LightGray;
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is short rating)
            {
                if (int.TryParse(parameter?.ToString(), out int starLevel))
                {
                  /*
                     This returns correct color, based on if the star should be selected
                    */
                    return rating >= starLevel ? _selectedColor : _notSelectedColor;
                }
            }

            return _notSelectedColor;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}