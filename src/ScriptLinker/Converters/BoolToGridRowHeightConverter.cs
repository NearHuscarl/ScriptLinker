using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ScriptLinker.Converters
{
    [ValueConversion(typeof(bool), typeof(GridLength))]
    public class BoolToGridRowHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((bool)value == true) ? new GridLength((double)parameter) : new GridLength(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Don't need any convert back
            return null;
        }
    }
}
