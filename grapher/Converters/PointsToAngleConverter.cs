using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace grapher.Converters
{
    class PointsToAngleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var points = value as List<Point>;
            var first = points[0];
            var second = points[1];

            if (second.X - first.X == 0)
            {
                if (second.Y > first.Y)
                    return 180d;
                else
                    return 0d;
            }

            if (first.X < second.X)
            {
                return Math.Atan((second.Y - first.Y) / (second.X - first.X)) * (180d / Math.PI) + 90d;
            }
            else
            {
                return Math.Atan((second.Y - first.Y) / (second.X - first.X)) * (180d / Math.PI) + 90d + 180d;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
