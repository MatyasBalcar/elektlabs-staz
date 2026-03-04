using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Knihovna.Converters
{
    public class RatingToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is short rating)
            {
                if (int.TryParse(parameter?.ToString(), out int starLevel))
                {
                    return rating >= starLevel ? Brushes.Gold : Brushes.LightGray;
                }
            }

            return Brushes.LightGray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}