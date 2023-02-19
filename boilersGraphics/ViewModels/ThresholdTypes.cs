using boilersGraphics.Properties;
using System.Collections.Generic;

namespace boilersGraphics.ViewModels;

public abstract class ThresholdTypes
{
    public static readonly ThresholdTypes Binary = new Binary();
    public static readonly ThresholdTypes BinaryInv = new BinaryInv();
    public static readonly ThresholdTypes Trunc = new Trunc();
    public static readonly ThresholdTypes Tozero = new Tozero();
    public static readonly ThresholdTypes TozeroInv = new TozeroInv();
    public static readonly ThresholdTypes Mask = new Mask();
    public static readonly ThresholdTypes Otsu = new Otsu();
    public static readonly ThresholdTypes Triangle = new Triangle();

    public abstract OpenCvSharp.ThresholdTypes ToOpenCvValue();

    public static IEnumerable<ThresholdTypes> GetValues()
    {
        yield return Binary;
        yield return BinaryInv;
        yield return Trunc;
        yield return Tozero;
        yield return TozeroInv;
        yield return Mask;
        //yield return Otsu; 大津の2値化は別途チェックボックスを設けるため除外
        yield return Triangle;
    }
}

public class Binary : ThresholdTypes
{
    public override OpenCvSharp.ThresholdTypes ToOpenCvValue()
    {
        return OpenCvSharp.ThresholdTypes.Binary;
    }

    public override string ToString()
    {
        return Resources.String_Binary;
    }
}


public class BinaryInv : ThresholdTypes
{
    public override OpenCvSharp.ThresholdTypes ToOpenCvValue()
    {
        return OpenCvSharp.ThresholdTypes.BinaryInv;
    }

    public override string ToString()
    {
        return Resources.String_BinaryInv;
    }
}


public class Trunc : ThresholdTypes
{
    public override OpenCvSharp.ThresholdTypes ToOpenCvValue()
    {
        return OpenCvSharp.ThresholdTypes.Trunc;
    }

    public override string ToString()
    {
        return Resources.String_Trunc;
    }
}


public class Tozero : ThresholdTypes
{
    public override OpenCvSharp.ThresholdTypes ToOpenCvValue()
    {
        return OpenCvSharp.ThresholdTypes.Tozero;
    }

    public override string ToString()
    {
        return Resources.String_Tozero;
    }
}



public class TozeroInv : ThresholdTypes
{
    public override OpenCvSharp.ThresholdTypes ToOpenCvValue()
    {
        return OpenCvSharp.ThresholdTypes.TozeroInv;
    }

    public override string ToString()
    {
        return Resources.String_TozeroInv;
    }
}



public class Mask : ThresholdTypes
{
    public override OpenCvSharp.ThresholdTypes ToOpenCvValue()
    {
        return OpenCvSharp.ThresholdTypes.Mask;
    }

    public override string ToString()
    {
        return Resources.String_Mask;
    }
}

public class Otsu : ThresholdTypes
{
    public override OpenCvSharp.ThresholdTypes ToOpenCvValue()
    {
        return OpenCvSharp.ThresholdTypes.Otsu;
    }

    public override string ToString()
    {
        return Resources.String_Otsu;
    }
}

public class Triangle : ThresholdTypes
{
    public override OpenCvSharp.ThresholdTypes ToOpenCvValue()
    {
        return OpenCvSharp.ThresholdTypes.Triangle;
    }

    public override string ToString()
    {
        return Resources.String_Triangle;
    }
}
