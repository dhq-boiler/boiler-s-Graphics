using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace boilersGraphics.Converters
{
    public class ComplementaryBrushConverter : IValueConverter
    {
        private static Color GetComplementaryColor(Color color)
        {
            var r = (byte)~color.R;
            var g = (byte)~color.G;
            var b = (byte)~color.B;

            return Color.FromArgb(color.A, r, g, b);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return GetComplementaryBrush(value);
        }

        private static object GetComplementaryBrush(object value)
        {
            switch (value)
            {
                case SolidColorBrush solid:
                {
                    var ret = solid.Clone();
                    ret.Color = GetComplementaryColor(ret.Color);
                    return ret;
                }
                case LinearGradientBrush linear:
                {
                    var ret = linear.Clone();
                    foreach (var gradientStop in ret.GradientStops)
                    {
                        gradientStop.Color = GetComplementaryColor(gradientStop.Color);
                    }

                    return ret;
                }
                case RadialGradientBrush radial:
                {
                    var ret = radial.Clone();
                    foreach (var gradientStop in ret.GradientStops)
                    {
                        gradientStop.Color = GetComplementaryColor(gradientStop.Color);
                    }

                    return ret;
                }
                default:
                    return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var color = (Color)value;
            return GetComplementaryColor(color);
        }
    }
}
