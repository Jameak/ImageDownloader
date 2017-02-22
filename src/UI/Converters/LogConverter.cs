using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using UI.ViewModels;

namespace UI.Converters
{
    /// <summary>
    /// Converts the contents of the binding-target collection to a string that the element can display
    /// </summary>
    public class LogConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var log = values[0] as ThreadsafeObservableStringCollection;

            if (log != null && log.Count > 0)
            {
                return log.ToString();
            }
            
            return String.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
