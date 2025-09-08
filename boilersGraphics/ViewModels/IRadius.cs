using R3;

namespace boilersGraphics.ViewModels;

public interface IRadius
{
    public BindableReactiveProperty<double> RadiusX { get; }
    public BindableReactiveProperty<double> RadiusY { get; }
}