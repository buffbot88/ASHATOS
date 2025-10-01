using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace RaWin.Converters
{
    public class MessageTypeToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value?.ToString()?.ToLowerInvariant())
            {
                case "log": return Brushes.DeepSkyBlue;
                case "error": return Brushes.OrangeRed;
                case "success": return Brushes.LimeGreen;
                case "warning": return Brushes.Gold;
                case "info": return Brushes.LightGreen;
                default: return Brushes.Gray;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
