using System;
using System.Globalization;
using System.Windows.Data;

namespace boilersGraphics.Converters;

/// <summary>
/// (time, duration, canvasWidth) → Canvas.Left.
/// time / duration の比率で width にスケールし、ダイヤの中心 (◆ 4px) ぶんだけ左にオフセット。
/// duration が 0 以下の場合は 0 を返す。
/// </summary>
public class TimeToXConverter : IMultiValueConverter
{
    private const double DiamondHalfWidth = 4.0;

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values is null || values.Length < 3) return 0.0;
        if (values[0] is not double time) return 0.0;
        if (values[1] is not double duration || duration <= 0.0) return 0.0;
        if (values[2] is not double width || width <= 0.0) return 0.0;

        var ratio = Math.Max(0.0, Math.Min(1.0, time / duration));
        var x = ratio * width - DiamondHalfWidth;
        return Math.Max(-DiamondHalfWidth, Math.Min(width - DiamondHalfWidth, x));
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
