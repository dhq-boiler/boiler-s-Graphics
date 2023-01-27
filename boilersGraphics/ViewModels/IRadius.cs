using Reactive.Bindings;

namespace boilersGraphics.ViewModels;

public interface IRadius
{
    public ReactivePropertySlim<double> RadiusX { get; }
    public ReactivePropertySlim<double> RadiusY { get; }
}