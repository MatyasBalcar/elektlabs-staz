using System.Globalization;
using System.Windows.Data;

namespace Knihovna.Converters
{
    public class RatingToStarsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == DBNull.Value)
                return Enumerable.Empty<int>();

            try
            {
                int rating = System.Convert.ToInt32(value);

                if (rating <= 0)
                    return Enumerable.Empty<int>();

                return Enumerable.Range(1, Math.Clamp(rating, 0, 5)).ToList();
                //list of length of stars
            }
            catch
            {
                //catch all returns empty list
                return Enumerable.Empty<int>();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}