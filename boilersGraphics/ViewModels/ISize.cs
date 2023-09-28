using Reactive.Bindings;
using System.Windows;

namespace boilersGraphics.ViewModels
{
    public interface ISize
    {
    }

    public interface ISize<out T> : ISize where T : class
    {
        T Width { get; }
        T Height { get; }
    }

    public interface ISizeRps : ISize<ReactivePropertySlim<double>>
    {
        ReactivePropertySlim<double> Left { get; } 

        ReactivePropertySlim<double> Top { get; }
        ReadOnlyReactivePropertySlim<Rect> Rect { get; }
    }

    public interface ISizeReadOnlyRps : ISize<ReadOnlyReactivePropertySlim<double>>
    {
        ReactiveProperty<Point> LeftTop { get; }
    }
}
