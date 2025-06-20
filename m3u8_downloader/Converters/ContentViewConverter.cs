using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;

namespace m3u8_downloader.Converters
{
    public class ContentViewConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Debug.Assert(value != null, nameof(value) + " != null");
            return (int)value == 0 ? "Collapsed" : "Visible";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}