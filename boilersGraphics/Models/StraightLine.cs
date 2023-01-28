using System;
using System.Windows.Media;
using System.Xml.Serialization;

namespace boilersGraphics.Models;

[Serializable]
public class StraightLine : RenderItem
{
    [NonSerialized] private readonly BrushConverter _brushConverter = new();

    [NonSerialized] private Brush _Brush;

    [XmlElement("BrushString")] private string _brushString;


    private double _X2;

    private double _Y2;

    public double X2
    {
        get => _X2;
        set => SetProperty(ref _X2, value);
    }

    public double Y2
    {
        get => _Y2;
        set => SetProperty(ref _Y2, value);
    }

    public string BrushString
    {
        get => _brushString;
        set
        {
            _brushString = value;
            Brush = (Brush)_brushConverter.ConvertFrom(value);
        }
    }

    public Brush Brush
    {
        get => _Brush;
        set => SetProperty(ref _Brush, value);
    }
}