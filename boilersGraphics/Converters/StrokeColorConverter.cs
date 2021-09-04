using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace boilersGraphics.Converters
{
    public class StrokeColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var v = value as ObservableCollection<Color>;
            if (v.Count > 1)
                return new SolidColorBrush(Colors.Transparent); //複数選択中で色一括変更可
            else if (v.Count == 1)
                return new SolidColorBrush(v[0]); //単一選択中で色変更可
            else
                return new SolidColorBrush(Colors.Transparent); //未選択で色変更不可
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
