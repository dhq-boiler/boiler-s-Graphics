using System;
using System.Globalization;
using System.Windows.Data;

namespace boilersGraphics.Converters;

public class QuadConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var d = (double)value;
        return d * 4;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}