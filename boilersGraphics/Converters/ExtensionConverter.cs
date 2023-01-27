using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace boilersGraphics.Converters;

public class ExtensionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Path.GetExtension(value as string);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}