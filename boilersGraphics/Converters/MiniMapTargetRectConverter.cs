using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace boilersGraphics.Converters;

internal class MiniMapTargetRectConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values[0] == null || values[0] == DependencyProperty.UnsetValue)
            return DependencyProperty.UnsetValue;
        if (values[1] == null || values[1] == DependencyProperty.UnsetValue)
            return DependencyProperty.UnsetValue;
        if (values[2] == null || values[2] == DependencyProperty.UnsetValue)
            return DependencyProperty.UnsetValue;
        if (double.IsNaN((double)values[0]))
            return DependencyProperty.UnsetValue;
        if ((double)values[1] == 0d)
            return DependencyProperty.UnsetValue;
        return ((double)values[0] / (double)values[1] * (double)values[2]).ToString();
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}