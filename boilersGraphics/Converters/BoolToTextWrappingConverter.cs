using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace boilersGraphics.Converters;

/// <summary>
/// Phase 2-b-2: TextElementBase.IsWordWrap を WPF TextBlock.TextWrapping に変換する。
/// true → Wrap, false → NoWrap。
/// </summary>
public class BoolToTextWrappingConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool b && b ? TextWrapping.Wrap : TextWrapping.NoWrap;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is TextWrapping w && w == TextWrapping.Wrap;
    }
}
