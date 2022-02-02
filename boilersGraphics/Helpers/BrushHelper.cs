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
                throw new NotSupportedException();
            }
            else if (brush is RadialGradientBrush rgb)
            {
                throw new NotSupportedException();
            }
            throw new UnexpectedException("No brush");
        }
    }
}
