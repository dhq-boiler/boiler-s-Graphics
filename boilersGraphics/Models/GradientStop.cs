using System.Windows.Media;
using Prism.Mvvm;
using R3;

namespace boilersGraphics.Models;

public class GradientStop : BindableBase
{
    public GradientStop(Color color, double offset)
    {
        Color.Value = color;
        Offset.Value = offset;
    }

    public BindableReactiveProperty<Color> Color { get; } = new();
    public BindableReactiveProperty<double> Offset { get; } = new();

    public System.Windows.Media.GradientStop ConvertToGradientStop()
    {
        return new System.Windows.Media.GradientStop(Color.Value, Offset.Value);
    }

    internal R3.Observable<R3.Unit> GradientStopChangedAsObservable()
    {
        return this.ObservePropertyChanged(x => x.Color)
            .Select(_ => R3.Unit.Default)
            .Merge(this.ObservePropertyChanged(x => x.Offset).Select(_ => R3.Unit.Default));
    }
}