﻿using boilersGraphics.Properties;
using System.Collections.Generic;

namespace boilersGraphics.ViewModels;

public class ColorCorrectType
{
    public static readonly ColorCorrectType HSV = new Hsv();
    public static readonly ColorCorrectType ToneCurve = new ToneCurve();
    public static readonly ColorCorrectType NegativePositiveConversion = new NegativePositiveConversion();
    public static readonly ColorCorrectType Binarization = new Binarization();

    public static IEnumerable<ColorCorrectType> GetValues()
    {
        yield return HSV;
        yield return ToneCurve;
        yield return NegativePositiveConversion;
        yield return Binarization;
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

public class NegativePositiveConversion : ColorCorrectType
{
    public override string ToString()
    {
        return Resources.String_NegativePositiveConversion;
    }
}

public class Binarization : ColorCorrectType
{
    public override string ToString()
    {
        return Resources.String_Binarization;
    }
}