using Prism.Mvvm;
using System.Windows;
using R3;

namespace boilersGraphics.Models;

public class Corner : BindableBase
{
    public BindableReactiveProperty<int> Number { get; set; } = new();

    public BindableReactiveProperty<double> Radius { get; set; } = new();

    public BindableReactiveProperty<double> Angle { get; set; } = new();

    public BindableReactiveProperty<Point> Point { get; set; } = new();

    public override bool Equals(object obj)
    {
        if (!(obj is Corner))
            return false;
        var corner = obj as Corner;
        return Number.Value.Equals(corner.Number.Value)
               && Radius.Value.Equals(corner.Radius.Value)
               && Angle.Value.Equals(corner.Angle.Value)
               && Point.Value.Equals(corner.Point.Value);
    }

    public override int GetHashCode()
    {
        return Number.GetHashCode()
               ^ Radius.GetHashCode()
               ^ Angle.GetHashCode()
               ^ Point.GetHashCode();
    }
}