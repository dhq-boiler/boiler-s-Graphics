using boilersGraphics.Exceptions;
using System;
using System.Windows.Media;

namespace boilersGraphics.Helpers;

internal class BrushHelper
{
    public static Color ExtractColor(Brush brush)
    {
        if (brush is SolidColorBrush scb)
            return scb.Color;
        if (brush is LinearGradientBrush lgb)
            throw new NotSupportedException();
        if (brush is RadialGradientBrush rgb) throw new NotSupportedException();
        throw new UnexpectedException("No brush");
    }
}