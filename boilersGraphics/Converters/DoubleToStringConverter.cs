using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace boilersGraphics.Converters
{
    class DoubleToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = value as string;
            if (string.IsNullOrEmpty(str))
                return 0;
            try
            {
                return double.Parse(str);
            }
            catch (FormatException)
            {
                if (str.Last() == '.')
                    return double.Parse(str + "0"); //最後に"."が入力された場合
                return 0;
            }
        }
    }
}
