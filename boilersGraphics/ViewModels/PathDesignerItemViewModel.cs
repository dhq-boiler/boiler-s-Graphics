using Reactive.Bindings;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace boilersGraphics.ViewModels;

public class PathDesignerItemViewModel : DesignerItemViewModelBase
{
    public ReactiveCollection<Point> Points { get; } = new();

    public override bool SupportsPropertyDialog => false;

    public override PathGeometry CreateGeometry(bool flag = false)
    {
        throw new NotSupportedException();
    }


    public override object Clone()
    {
        var clone = new PathDesignerItemViewModel();
        clone.Owner = Owner;
        clone.Left.Value = Left.Value;
        clone.Top.Value = Top.Value;
        clone.Width.Value = Width.Value;
        clone.Height.Value = Height.Value;
        clone.EdgeBrush.Value = EdgeBrush.Value;
        clone.FillBrush.Value = FillBrush.Value;
        clone.EdgeThickness.Value = EdgeThickness.Value;
        clone.Matrix.Value = Matrix.Value;
        clone.RotationAngle.Value = RotationAngle.Value;
        return clone;
    }

    public override Type GetViewType()
    {
        return typeof(Path);
    }

    public override void OpenPropertyDialog()
    {
        throw new NotImplementedException();
    }
}