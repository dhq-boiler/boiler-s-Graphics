using boilersGraphics.Properties;
using System.Collections.Generic;

namespace boilersGraphics.ViewModels
{
    public class Channel
    {
        public static readonly Channel Red = new RedChannel();
        public static readonly Channel Green = new GreenChannel();
        public static readonly Channel Blue = new BlueChannel();
        public static readonly Channel GrayScale = new GrayScaleChannel();


        public static IEnumerable<Channel> GetValues()
        {
            yield return GrayScale;
            yield return Blue;
            yield return Green;
            yield return Red;
        }
    }

    public class RedChannel : Channel
    {
        public override string ToString()
        {
            return Resources.String_R;
        }
    }

    public class GreenChannel : Channel
    {
        public override string ToString()
        {
            return Resources.String_G;
        }
    }

    public class BlueChannel : Channel
    {
        public override string ToString()
        {
            return Resources.String_B;
        }
    }

    public class GrayScaleChannel : Channel
    {
        public override string ToString()
        {
            return Resources.String_GrayScale;
        }
    }
}
