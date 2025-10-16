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

    public interface ISizeRps : ISize<BindableReactiveProperty<double>>
    {
        BindableReactiveProperty<double> Left { get; }

        BindableReactiveProperty<double> Top { get; }
        IReadOnlyBindableReactiveProperty<Rect> Rect { get; }
    }

    public interface ISizeReadOnlyRps : ISize<IReadOnlyBindableReactiveProperty<double>>
    {
        BindableReactiveProperty<Point> LeftTop { get; }
    }
}
