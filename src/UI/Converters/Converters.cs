using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UI.Converters
{
    /// <summary>
    /// Contains converters used in the XAML code.
    /// </summary>
    public static class Converters
    {
        public static readonly LogConverter LogConverter = new LogConverter();
        public static readonly NullableIntConverter NullableIntConverter = new NullableIntConverter();
        public static readonly RadioButton_BoolToStringConverter RadioButton_BoolToStringConverter = new RadioButton_BoolToStringConverter();
    }
}
