using boilersGraphics.ViewModels;
using System;
using System.Globalization;
using System.Windows.Data;

namespace boilersGraphics.Converters;

internal class ToFileNameOrBitmapImage : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var viewModel = value as IEmbeddedImage;
        return viewModel.EmbeddedImage.Value != null ? viewModel.EmbeddedImage.Value : viewModel.FileName;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}