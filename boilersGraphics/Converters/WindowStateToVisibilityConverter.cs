﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace boilersGraphics.Converters;

public class WindowStateToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var state = (WindowState)value;
        if (parameter.ToString() == "元に戻す")
        {
            if (state == WindowState.Maximized || state == WindowState.Minimized)
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        if (parameter.ToString() == "最大化")
        {
            if (state == WindowState.Maximized)
                return Visibility.Collapsed;
            return Visibility.Visible;
        }

        throw new Exception("parameter is not set");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}