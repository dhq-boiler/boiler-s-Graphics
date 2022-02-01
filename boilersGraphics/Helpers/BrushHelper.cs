using boilersGraphics.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace boilersGraphics.Helpers
{
    internal class BrushHelper
    {
        public static Color ExtractColor(Brush brush)
        {
            if (brush is SolidColorBrush scb)
            {
                return scb.Color;
            }
            else if (brush is LinearGradientBrush lgb)
            {
                return lgb.GradientStops.FirstOrDefault().Color;
            }
            else if (brush is RadialGradientBrush rgb)
            {
                return rgb.GradientStops.FirstOrDefault().Color;
            }
            throw new UnexpectedException("No brush");
        }
    }
}
