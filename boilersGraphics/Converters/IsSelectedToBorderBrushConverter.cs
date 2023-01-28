using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace boilersGraphics.Converters;

public class IsSelectedToBorderBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if ((bool)value)
            return Brushes.Red;
        return Brushes.Black;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}