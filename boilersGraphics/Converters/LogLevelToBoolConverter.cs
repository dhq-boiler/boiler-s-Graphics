using NLog;
using System;
using System.Globalization;
using System.Windows.Data;

namespace boilersGraphics.Converters;

internal class LogLevelToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var logLevel = (LogLevel)value;
        return logLevel.ToString() == (string)parameter;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}