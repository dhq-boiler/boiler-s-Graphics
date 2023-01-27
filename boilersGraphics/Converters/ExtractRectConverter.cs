using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using boilersGraphics.ViewModels;

namespace boilersGraphics.Converters;

internal class ExtractRectConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var viewModel = value as DesignerItemViewModelBase;
        return new Rect(viewModel.Left.Value, viewModel.Top.Value, viewModel.Width.Value, viewModel.Height.Value);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}