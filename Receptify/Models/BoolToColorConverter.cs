using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Receptify.Models
{
    public class BoolToColorConverter : IValueConverter
    {
        public Color TrueColor { get; set; } = Colors.LightBlue;
        public Color FalseColor { get; set; } = Colors.LightGray;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSelected)
            {
                return isSelected ? TrueColor : FalseColor;
            }
            return FalseColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
