using boilersGraphics.ViewModels.ColorCorrect;
using Reactive.Bindings;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using ZLinq;

namespace boilersGraphics.Converters;

public class ToToneCurveControlPointCollectionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var oc = (ReactiveCollection<ToneCurveViewModel.Point>)value; 
        return new PointCollection(oc.AsValueEnumerable().Select(x => new Point(x.X.Value, x.Y.Value)).ToArray());
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}