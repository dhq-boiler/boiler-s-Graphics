using R3;
using System.Windows;

namespace boilersGraphics.ViewModels
{
    public interface IRect
    {
        IReadOnlyBindableReactiveProperty<Rect> Rect { get; set; }
    }
}