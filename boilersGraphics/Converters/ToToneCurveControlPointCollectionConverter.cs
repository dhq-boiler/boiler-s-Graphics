using boilersGraphics.ViewModels.ColorCorrect;
using Reactive.Bindings;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace boilersGraphics.Converters;

public class ToToneCurveControlPointCollectionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var oc = (ReactiveCollection<ToneCurveViewModel.Point>)value; 
        return new PointCollection(oc.Select(x => new Point(x.X.Value, x.Y.Value)));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}