﻿using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using boilersGraphics.ViewModels;

namespace boilersGraphics.Converters;

internal class StraightLineBaseShiftConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var x = (double)values.ElementAt(0);
        var viewModel = (ConnectorBaseViewModel)values.ElementAt(1);
        double temp = 0;
        if (parameter.ToString().StartsWith("X"))
            temp = viewModel.LeftTop.Value.X;
        else if (parameter.ToString().StartsWith("Y"))
            temp = viewModel.LeftTop.Value.Y;
        return x - temp + 50;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}