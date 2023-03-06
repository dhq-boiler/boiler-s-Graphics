using System;
using System.Windows;
using System.Windows.Media;
using boilersGraphics.Controls;
using boilersGraphics.Helpers;
using boilersGraphics.Views;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Prism.Unity;
using Reactive.Bindings;
using Path = System.Windows.Shapes.Path;

namespace boilersGraphics.ViewModels;

public class NPolygonViewModel : DesignerItemViewModelBase
{
    public NPolygonViewModel()
    {
        Init();
    }

    public NPolygonViewModel(double left, double top, double width, double height)
    {
        Init();
        Left.Value = left;
        Top.Value = top;
        Width.Value = width;
        Height.Value = height;
    }

    public NPolygonViewModel(double left, double top, double width, double height, double angleInDegrees)
        : this(left, top, width, height)
    {
        RotationAngle.Value = angleInDegrees;
        Matrix.Value.RotateAt(angleInDegrees, 0, 0);
    }

    public NPolygonViewModel(int id, IDiagramViewModel parent, double left, double top)
        : base(id, parent, left, top)
    {
        Init();
    }

    public ReactiveCollection<SnapPoint> SnapPoints { get; } = new();

    public ReactivePropertySlim<string> Data { get; set; } = new();

    public override bool SupportsPropertyDialog => true;

    private void Init()
    {
        ShowConnectors = false;
        UpdatingStrategy.Value = PathGeometryUpdatingStrategy.Initial;
    }

    public override void UpdatePathGeometryIfEnable(string propertyName, object oldValue, object newValue,
        bool flag = false)
    {
        if (UpdatingStrategy.Value == PathGeometryUpdatingStrategy.Initial)
        {
            if (!flag)
            {
                if (Left.Value != 0 && Top.Value != 0 && Width.Value != 0 && Height.Value != 0)
                {
                    PathGeometryNoRotate.Value = CreateGeometry(flag);
                    if (Width.Value != PathGeometryNoRotate.Value.Bounds.Width ||
                        Height.Value != PathGeometryNoRotate.Value.Bounds.Height)
                    {
                        var lhs = PathGeometryNoRotate.Value.Clone();
                        var coefficientWidth = Width.Value / lhs.Bounds.Width;
                        var coefficientHeight = Height.Value / lhs.Bounds.Height;
                        if (coefficientWidth == 0)
                            coefficientWidth = 1;
                        if (coefficientHeight == 0)
                            coefficientHeight = 1;
                        var newlhs = GeometryCreator.Scale(lhs, coefficientWidth, coefficientHeight);
                        newlhs = GeometryCreator.Translate(newlhs, -newlhs.Bounds.Left, -newlhs.Bounds.Top);
                        PathGeometryNoRotate.Value = newlhs;
                    }
                }

                if (!(PathGeometryNoRotate.Value is null)) Data.Value = PathGeometryNoRotate.Value.ToString();
            }

            if (RotationAngle.Value != 0d) PathGeometryRotate.Value = GeometryCreator.Rotate(PathGeometryNoRotate.Value, RotationAngle.Value, CenterPoint.Value);
        }
    }

    public override PathGeometry CreateGeometry(bool flag = false)
    {
        return GeometryCreator.CreatePolygon(this, Data.Value, flag);
    }


    public override Type GetViewType()
    {
        return typeof(Path);
    }

    #region IClonable

    public override object Clone()
    {
        var clone = new NPolygonViewModel();
        clone.Owner = Owner;
        clone.Data.Value = Data.Value;
        clone.Left.Value = Left.Value;
        clone.Top.Value = Top.Value;
        clone.Width.Value = Width.Value;
        clone.Height.Value = Height.Value;
        clone.EdgeBrush.Value = EdgeBrush.Value;
        clone.FillBrush.Value = FillBrush.Value;
        clone.EdgeThickness.Value = EdgeThickness.Value;
        clone.RotationAngle.Value = RotationAngle.Value;
        clone.StrokeLineJoin.Value = StrokeLineJoin.Value;
        clone.StrokeDashArray.Value = StrokeDashArray.Value;
        clone.StrokeMiterLimit.Value = StrokeMiterLimit.Value;
        clone.PathGeometryNoRotate.Value = PathGeometryNoRotate.Value;
        clone.PathGeometryRotate.Value = PathGeometryRotate.Value;
        return clone;
    }

    #endregion //IClonable

    public override void OpenPropertyDialog()
    {
        var dialogService =
            new DialogService((Application.Current as PrismApplication).Container as IContainerExtension);
        IDialogResult result = null;
        dialogService.Show(nameof(DetailPolygon), new DialogParameters { { "ViewModel", this } }, ret => result = ret);
    }
}