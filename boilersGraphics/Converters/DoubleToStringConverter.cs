using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace boilersGraphics.Converters
{
    public class DoubleToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = value as string;
            if (string.IsNullOrEmpty(str))
                return Binding.DoNothing;
            if (str.Last() == '.' || str.Last() == '-')
                return Binding.DoNothing;
            try
            {
                return double.Parse(str);
            }
            catch (FormatException)
            {
                return 0;
            }
        }
    }
}
