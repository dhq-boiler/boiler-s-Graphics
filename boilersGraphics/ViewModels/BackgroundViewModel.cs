using boilersGraphics.Helpers;
using System;
using Path = System.Windows.Shapes.Path;

namespace boilersGraphics.ViewModels;

public class BackgroundViewModel : NRectangleViewModel
{
    private readonly DiagramViewModel diagramViewModel;

    public BackgroundViewModel(DiagramViewModel diagramViewModel)
    {
        this.diagramViewModel = diagramViewModel;
        Width.Value = 1000;
        PathGeometryNoRotate.Value = null;
        Height.Value = 1000;
    }

    public override object Clone()
    {
        var clone = new BackgroundViewModel(diagramViewModel);
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
        clone.RadiusX.Value = RadiusX.Value;
        clone.RadiusY.Value = RadiusY.Value;
        clone.PathGeometryNoRotate.Value = GeometryCreator.CreateRectangleWithOffset(clone, 0d, 0d);
        return clone;
    }

    public override Type GetViewType()
    {
        return typeof(Path);
    }
}