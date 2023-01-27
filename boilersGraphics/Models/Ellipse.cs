using System;
using System.Windows.Media;
using System.Xml.Serialization;

namespace boilersGraphics.Models;

[Serializable]
public class Ellipse : RenderItem
{
    [NonSerialized] private readonly BrushConverter _brushConverter = new();

    [NonSerialized] private Brush _Fill;

    [XmlElement("StrokeString")] private string _strokeString;

    [XmlElement("FillString")] private string _fillString;

    [NonSerialized] private Brush _Stroke;


    public string StrokeString
    {
        get => _strokeString;
        set
        {
            _strokeString = value;
            Stroke = (Brush)_brushConverter.ConvertFrom(value);
        }
    }

    public Brush Stroke
    {
        get => _Stroke;
        set => SetProperty(ref _Stroke, value);
    }

    public string FillString
    {
        get => _fillString;
        set
        {
            _fillString = value;
            Fill = (Brush)_brushConverter.ConvertFrom(value);
        }
    }

    public Brush Fill
    {
        get => _Fill;
        set => SetProperty(ref _Fill, value);
    }
}