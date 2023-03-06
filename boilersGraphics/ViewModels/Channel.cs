using boilersGraphics.Properties;
using System.Collections.Generic;
using System.Windows.Media;

namespace boilersGraphics.ViewModels
{
    public abstract class Channel
    {
        public static readonly Channel Red = new RedChannel();
        public static readonly Channel Green = new GreenChannel();
        public static readonly Channel Blue = new BlueChannel();
        public static readonly Channel RGB = new RGBChannel();

        public abstract Brush Brush { get; }

        public static IEnumerable<Channel> GetValues()
        {
            yield return RGB;
            yield return Blue;
            yield return Green;
            yield return Red;
        }
    }

    public class RedChannel : Channel
    {
        public override Brush Brush => Brushes.Red;

        public override string ToString()
        {
            return Resources.String_R;
        }
    }

    public class GreenChannel : Channel
    {
        public override Brush Brush => Brushes.Green;

        public override string ToString()
        {
            return Resources.String_G;
        }
    }

    public class BlueChannel : Channel
    {
        public override Brush Brush => Brushes.Blue;

        public override string ToString()
        {
            return Resources.String_B;
        }
    }

    public class RGBChannel : Channel
    {
        public override Brush Brush => Brushes.White;

        public override string ToString()
        {
            return Resources.String_RGB;
        }
    }
}
