using System;
using System.Globalization;
using System.Windows.Data;

namespace boilersGraphics.Converters;

public class ShiftCoordinateConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var v = double.Parse(value.ToString());
        var p = double.Parse(parameter.ToString());
        return v - p / 2d;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}