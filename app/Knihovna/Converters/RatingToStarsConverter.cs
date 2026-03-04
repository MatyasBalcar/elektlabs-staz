using System.Globalization;
using System.Windows.Data;

namespace Knihovna.Converters
{
    public class RatingToStarsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value == DBNull.Value)
                return "—";


            int rating = System.Convert.ToInt32(value);

            if (rating <= 0) return "—";

            return new string('*', Math.Clamp(rating, 0, 5));
            

        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}