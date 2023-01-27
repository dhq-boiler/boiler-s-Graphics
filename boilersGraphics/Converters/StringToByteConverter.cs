using System;
using System.Globalization;
using System.Windows.Data;

namespace boilersGraphics.Converters;

public class StringToByteConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return byte.Parse(value.ToString());
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value.ToString();
    }
}