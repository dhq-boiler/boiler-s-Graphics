using R3;
using System.Windows;

namespace boilersGraphics.ViewModels
{
    public interface IRect
    {
        ReadOnlyReactiveProperty<Rect> Rect { get; set; }
    }
}