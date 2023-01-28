using System;
using System.Xml.Serialization;
using Prism.Mvvm;

namespace boilersGraphics.Models;

[Serializable]
[XmlInclude(typeof(Rectangle))]
[XmlInclude(typeof(StraightLine))]
[XmlInclude(typeof(Ellipse))]
public class RenderItem : BindableBase
{
    private double _Height;
    private bool _IsSelected;
    private double _Width;
    private double _X;
    private double _Y;

    public double X
    {
        get => _X;
        set => SetProperty(ref _X, value);
    }

    public double Y
    {
        get => _Y;
        set => SetProperty(ref _Y, value);
    }

    public double Width
    {
        get => _Width;
        set => SetProperty(ref _Width, value);
    }

    public double Height
    {
        get => _Height;
        set => SetProperty(ref _Height, value);
    }

    [XmlIgnore]
    public bool IsSelected
    {
        get => _IsSelected;
        set => SetProperty(ref _IsSelected, value);
    }
}