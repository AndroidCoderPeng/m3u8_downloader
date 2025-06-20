using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;

namespace m3u8_downloader.Converters
{
    public class LoadingStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Debug.Assert(value != null, nameof(value) + " != null");
            if ((bool)value)
            {
                return "Collapsed";
            }

            return "Visible";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}