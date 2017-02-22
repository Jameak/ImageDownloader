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
    /// Provides a mechanism for automatically toggling RadioButtons in XAML
    /// when the binding-target changes to the value specified in the
    /// RadioButtons ConverterParameter.
    /// </summary>
    public class RadioButton_BoolToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is string && (string)value == parameter.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter;
        }
    }
}
