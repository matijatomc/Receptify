using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Receptify.Converters
{
    public class TimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int minutes)
            {
                if (minutes < 60)
                    return $"{minutes} min";

                int hours = minutes / 60;
                int remainingMinutes = minutes % 60;

                if (remainingMinutes == 0)
                    return $"{hours} h";
                else
                    return $"{hours} h {remainingMinutes} min";
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public static string ConvertToTimeString(int minutes)
        {
            if (minutes < 60)
                return $"{minutes} min";

            int hours = minutes / 60;
            int remainingMinutes = minutes % 60;

            if (remainingMinutes == 0)
                return $"{hours} h";
            else
                return $"{hours} h {remainingMinutes} min";
        }
    }
}
