using R3;
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

    public interface ISizeRps : ISize<ReactiveProperty<double>>
    {
        ReactiveProperty<double> Left { get; } 

        ReactiveProperty<double> Top { get; }
        ReadOnlyReactiveProperty<Rect> Rect { get; }
    }

    public interface ISizeReadOnlyRps : ISize<ReadOnlyReactiveProperty<double>>
    {
        ReactiveProperty<Point> LeftTop { get; }
    }
}
