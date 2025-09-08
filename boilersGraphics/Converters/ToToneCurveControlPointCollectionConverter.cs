using boilersGraphics.ViewModels.ColorCorrect;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using ObservableCollections;
using ZLinq;

namespace boilersGraphics.Converters;

public class ToToneCurveControlPointCollectionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var oc = (NotifyCollectionChangedSynchronizedViewList<ToneCurveViewModel.Point>)value; 
        return new PointCollection(oc.AsValueEnumerable().Select(x => new Point(x.X.Value, x.Y.Value)).ToArray());
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}