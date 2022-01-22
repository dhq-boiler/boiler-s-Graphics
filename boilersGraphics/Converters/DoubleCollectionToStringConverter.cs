using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace boilersGraphics.Converters
{
    internal class DoubleCollectionToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var split = value.ToString().Split(' ');
            foreach (var item in split)
            {
                if (item.EndsWith("."))
                {
                    return DependencyProperty.UnsetValue;
                }
                else if (double.TryParse(item, out double d))
                {

                }
                else
                {
                    return DependencyProperty.UnsetValue;
                }
            }
            return DoubleCollection.Parse(value.ToString());
        }
    }
}
