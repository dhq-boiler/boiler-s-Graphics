using System;
using System.Globalization;
using System.Windows.Data;
using boilersGraphics.Extensions;

namespace boilersGraphics.Converters;

internal class TimeSpanRoundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var timeSpan = (TimeSpan)value;
        var result = timeSpan.Truncate(TimeSpan.FromSeconds(1));
        return result;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}