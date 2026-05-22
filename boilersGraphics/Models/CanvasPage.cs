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
    private int _durationMs = 100;

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

    public int DurationMs
    {
        get => _durationMs;
        set => SetProperty(ref _durationMs, value);
    }

    public BitmapSource Thumbnail
    {
        get => _thumbnail;
        set => SetProperty(ref _thumbnail, value);
    }

    // M-2 修正: UIA ツリー上で DataItem の AutomationName が型名 ("boilersGraphics.Models.CanvasPage")
    // のまま流れていたため、スクリーンリーダーが上位 DataItem を読み上げると意味不明になっていた。
    // Name を返すことで「Canvas 1」等が正しく読み上げられる。
    public override string ToString() => _name;
}
