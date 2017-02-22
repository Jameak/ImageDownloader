using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace UI.Converters
{
    /// <summary>
    /// Provides a mechanism for converting between a string-field in XAML to a nullable-int.
    /// </summary>
    public class NullableIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int i;
            if (int.TryParse(value?.ToString(), out i))
            {
                return i;
            }
            return null;
        }
    }
}
