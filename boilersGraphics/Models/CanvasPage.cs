using Prism.Mvvm;
using R3;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace boilersGraphics.Models;

public class CanvasPage : BindableBase
{
    private string _name;
    private XElement _serializedData;
    private BitmapSource _thumbnail;

    public CanvasPage(string name)
    {
        _name = name;
    }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public XElement SerializedData
    {
        get => _serializedData;
        set => SetProperty(ref _serializedData, value);
    }

    public BitmapSource Thumbnail
    {
        get => _thumbnail;
        set => SetProperty(ref _thumbnail, value);
    }
}
