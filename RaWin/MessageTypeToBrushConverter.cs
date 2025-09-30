using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace RaWin
{
    public class MessageTypeToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value?.ToString()?.ToLowerInvariant())
            {
                case "log":
                    return Brushes.DeepSkyBlue;
                case "error":
                    return Brushes.Red;
                case "success":
                    return Brushes.LimeGreen;
                default:
                    return Brushes.White;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
