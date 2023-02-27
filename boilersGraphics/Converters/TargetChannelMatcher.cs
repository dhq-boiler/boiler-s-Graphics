using boilersGraphics.ViewModels;
using System;
using System.Globalization;
using System.Windows.Data;

namespace boilersGraphics.Converters
{
    public class TargetChannelMatcher : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var first = values[0] as Channel;
            var second = values[1] as Channel;
            return first.GetType().Name.Equals(second.GetType().Name);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
