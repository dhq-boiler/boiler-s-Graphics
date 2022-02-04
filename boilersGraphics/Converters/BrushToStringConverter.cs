using boilersGraphics.Exceptions;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace boilersGraphics.Converters
{
    public class BrushToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return DependencyProperty.UnsetValue;
            var color = (Brush)value;
            if (color is SolidColorBrush)
                return color.ToString();
            else if (color is LinearGradientBrush lgb)
            {
                var ret = $"Linear{{";
                foreach (var gs in lgb.GradientStops)
                {
                    ret += $"[{gs.Color.ToString()}, {gs.Offset}]";
                    if (lgb.GradientStops.Last() != gs)
                        ret += ", ";
                }
                ret += $"}}";
                return ret;
            }
            else if (color is RadialGradientBrush rgb)
            {
                var ret = $"Radial{{";
                foreach (var gs in rgb.GradientStops)
                {
                    ret += $"[{gs.Color.ToString()}, {gs.Offset}]";
                    if (rgb.GradientStops.Last() != gs)
                        ret += ", ";
                }
                ret += $"}}";
                return ret;
            }
            throw new UnexpectedException($"Not supported Brush type:{color}");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
