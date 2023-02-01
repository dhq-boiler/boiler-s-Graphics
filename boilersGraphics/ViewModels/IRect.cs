using Reactive.Bindings;
using System.Windows;

namespace boilersGraphics.ViewModels
{
    public interface IRect
    {
        ReadOnlyReactivePropertySlim<Rect> Rect { get; set; }
    }
}