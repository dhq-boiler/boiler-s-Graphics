using boilersGraphics.ViewModels.ColorCorrect;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace boilersGraphics.Converters
{
    public class ToneCurvePointConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var toneCurveViewModelPoint = value as ToneCurveViewModel.Point;
            return new Point(toneCurveViewModelPoint.X.Value, toneCurveViewModelPoint.Y.Value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
