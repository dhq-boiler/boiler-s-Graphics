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
    private bool _isActive;
    private bool _isEditing;

    public CanvasPage(string name)
    {
        _name = name;
    }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }

    public bool IsEditing
    {
        get => _isEditing;
        set => SetProperty(ref _isEditing, value);
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
