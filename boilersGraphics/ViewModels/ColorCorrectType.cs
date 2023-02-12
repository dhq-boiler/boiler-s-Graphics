using System.Collections;
using System.Collections.Generic;
using boilersGraphics.Helpers;
using boilersGraphics.Properties;

namespace boilersGraphics.ViewModels;

public class ColorCorrectType
{
    public static readonly ColorCorrectType HSV = new Hsv();
    public static readonly ColorCorrectType ToneCurve = new ToneCurve();
    
    public static IEnumerable<ColorCorrectType> GetValues()
    {
        yield return HSV;
        yield return ToneCurve;
    }
}

public class Hsv : ColorCorrectType
{
    public override string ToString()
    {
        return Resources.String_Hsv;
    }
}

public class ToneCurve : ColorCorrectType
{
    public override string ToString()
    {
        return Resources.String_ToneCurve;
    }
}