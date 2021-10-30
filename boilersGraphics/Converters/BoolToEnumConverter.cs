using System;
using System.Globalization;
using System.Windows.Data;

namespace boilersGraphics.Converters
{
    public class BoolToEnumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            var ParameterString = parameter as string;
            if (ParameterString == null)
            {
                return System.Windows.DependencyProperty.UnsetValue;
            }

            if (Enum.IsDefined(value.GetType(), value) == false)
            {
                return System.Windows.DependencyProperty.UnsetValue;
            }

            object paramvalue = Enum.Parse(value.GetType(), ParameterString);

            return (int)paramvalue == (int)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var ParameterString = parameter as string;
            return ParameterString == null ? System.Windows.DependencyProperty.UnsetValue : Enum.Parse(targetType, ParameterString);
        }
    }
}
