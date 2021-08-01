using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace boilersGraphics.Helpers
{
    public static class Randomizer
    {
        public static Color RandomColor(Random rand)
        {
            return Color.FromRgb((byte)rand.Next(), (byte)rand.Next(), (byte)rand.Next());
        }

        public static Brush RandomColorBrush(Random rand)
        {
            var brush = new SolidColorBrush(RandomColor(rand));
            return brush;
        }
    }
}
