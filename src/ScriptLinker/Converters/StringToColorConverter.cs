using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ScriptLinker.Converters
{
    class StringToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var color = new Color();
            var hexColor = ((string)value)?.Trim('#');

            if (!string.IsNullOrEmpty(hexColor))
            {
                if (hexColor.Length == 6)
                {
                    color = Color.FromRgb(
                       System.Convert.ToByte(hexColor.Substring(0, 2), 16),
                       System.Convert.ToByte(hexColor.Substring(2, 2), 16),
                       System.Convert.ToByte(hexColor.Substring(4, 2), 16)
                   );
                }
                if (hexColor.Length == 8)
                {
                    color = Color.FromArgb(
                       System.Convert.ToByte(hexColor.Substring(0, 2), 16),
                       System.Convert.ToByte(hexColor.Substring(2, 2), 16),
                       System.Convert.ToByte(hexColor.Substring(4, 2), 16),
                       System.Convert.ToByte(hexColor.Substring(6, 2), 16)
                    );
                }
            }

            return new SolidColorBrush(color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
