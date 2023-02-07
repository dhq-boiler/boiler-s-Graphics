using boilersGraphics.Helpers;
using System;
using System.Windows.Media;

namespace boilersGraphics.ViewModels;

public class CombineGeometryViewModel : DesignerItemViewModelBase
{
    public CombineGeometryViewModel()
    {
        Init();
    }

    public CombineGeometryViewModel(double left, double top, double width, double height)
    {
        Init();
        Left.Value = left;
        Top.Value = top;
        Width.Value = width;
        Height.Value = height;
    }

    public CombineGeometryViewModel(double left, double top, double width, double height, double angleInDegrees)
        : this(left, top, width, height)
    {
        RotationAngle.Value = angleInDegrees;
        Matrix.Value.RotateAt(angleInDegrees, 0, 0);
    }

    public CombineGeometryViewModel(int id, IDiagramViewModel parent, double left, double top)
        : base(id, parent, left, top)
    {
        Init();
    }

    public override bool SupportsPropertyDialog => false;

    private void Init()
    {
        UpdatingStrategy.Value = PathGeometryUpdatingStrategy.Fixed;
        ShowConnectors = false;
    }

    public override PathGeometry CreateGeometry(bool flag = false)
    {
        switch (UpdatingStrategy.Value)
        {
            case PathGeometryUpdatingStrategy.Initial:
                return GeometryCreator.CreateRectangle(this, 0, 0, flag);
            case PathGeometryUpdatingStrategy.ResizeWhilePreservingOriginalShape:
                return GeometryCreator.Scale(this.PathGeometryNoRotate.Value, this.Width.Value / this.PathGeometryNoRotate.Value.Bounds.Width, this.Height.Value / this.PathGeometryNoRotate.Value.Bounds.Height);
            case PathGeometryUpdatingStrategy.Fixed:
                return this.PathGeometryNoRotate.Value;
            default:
                throw new NotSupportedException();
        }
    }


    public override Type GetViewType()
    {
        return typeof(System.Windows.Shapes.Path);
    }

    #region IClonable

    public override object Clone()
    {
        var clone = new CombineGeometryViewModel();
        clone.Owner = Owner;
        clone.Left.Value = Left.Value;
        clone.Top.Value = Top.Value;
        clone.Width.Value = Width.Value;
        clone.Height.Value = Height.Value;
        clone.EdgeBrush.Value = EdgeBrush.Value;
        clone.FillBrush.Value = FillBrush.Value;
        clone.EdgeThickness.Value = EdgeThickness.Value;
        clone.RotationAngle.Value = RotationAngle.Value;
        clone.PathGeometryNoRotate.Value = PathGeometryNoRotate.Value;
        clone.StrokeLineJoin.Value = StrokeLineJoin.Value;
        return clone;
    }

    public override void OpenPropertyDialog()
    {
        throw new NotImplementedException();
    }

    #endregion //IClonable
}