using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace UI.Converters
{
    public class PercentConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(string)) throw new NotImplementedException();
            Debug.Assert(values.Length == 2);

            var progress = int.Parse(values[0].ToString());
            var total = int.Parse(values[1].ToString());

            if (total == 0) return ""; //Avoid dividing by zero

            double division = (double) progress / total;
            int percent = (int)(division * 100);
            if (percent > 100) percent = 100; //Cap at 100% due to how we populate progressbar values.
            return $"{percent}%";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
