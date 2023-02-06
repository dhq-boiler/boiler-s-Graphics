using System;
using System.Windows;
using System.Windows.Media;
using boilersGraphics.Helpers;
using boilersGraphics.Views;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Prism.Unity;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Path = System.Windows.Shapes.Path;
using Thickness = System.Windows.Thickness;

namespace boilersGraphics.ViewModels;

public class NPieViewModel : DesignerItemViewModelBase
{
    public NPieViewModel()
    {
        Init();
    }

    public NPieViewModel(double left, double top, double width, double height)
    {
        Init();
        Left.Value = left;
        Top.Value = top;
        Width.Value = width;
        Height.Value = height;
    }

    public NPieViewModel(double left, double top, double width, double height, double angleInDegrees)
        : this(left, top, width, height)
    {
        RotationAngle.Value = angleInDegrees;
        Matrix.Value.RotateAt(angleInDegrees, 0, 0);
    }

    public NPieViewModel(int id, IDiagramViewModel parent, double left, double top)
        : base(id, parent, left, top)
    {
        Init();
    }

    public ReactiveCommand MouseDoubleClickCommand { get; } = new();

    public override bool SupportsPropertyDialog => true;

    public ReactivePropertySlim<Point> PieCenterPoint { get; } = new();

    public ReactivePropertySlim<double> DonutWidth { get; } = new();

    public ReactivePropertySlim<double> Distance { get; } = new();

    public ReactivePropertySlim<double> StartDegree { get; } = new();

    public ReactivePropertySlim<double> EndDegree { get; } = new();

    public ReactivePropertySlim<SweepDirection> SweepDirection { get; } = new();

    private void Init()
    {
        ShowConnectors = false;
        UpdatingStrategy.Value = PathGeometryUpdatingStrategy.Initial;
        MouseDoubleClickCommand.Subscribe(x => { OpenPropertyDialog(); })
            .AddTo(_CompositeDisposable);
    }

    public override void OpenPropertyDialog()
    {
        var dialogService =
            new DialogService((Application.Current as PrismApplication).Container as IContainerExtension);
        IDialogResult result = null;
        dialogService.Show(nameof(DetailPie), new DialogParameters { { "ViewModel", this } }, ret => result = ret);
    }

    public override void UpdatePathGeometryIfEnable(string propertyName, object oldValue, object newValue,
        bool flag = false)
    {
        if (UpdatingStrategy.Value == PathGeometryUpdatingStrategy.Initial)
        {
            if (!flag)
                if (Left.Value != 0 && Top.Value != 0 && Width.Value != 0 && Height.Value != 0)
                {
                    var geometry = CreateGeometry(flag);
                    if (!geometry.IsEmpty()) PathGeometryNoRotate.Value = geometry;
                }

            if (RotationAngle.Value != 0) PathGeometryRotate.Value = CreateGeometry(RotationAngle.Value);
        }
    }

    protected override void AfterUpdatePathGeometry(string propertyName, object oldValue, object newValue)
    {
        if (PathGeometryNoRotate.Value is null)
            return;
        PathGeometryNoRotate.Value = GeometryCreator.Scale(PathGeometryNoRotate.Value,
            PathGeometryNoRotate.Value.Bounds.Width / (Width.Value + EdgeThickness.Value),
            PathGeometryNoRotate.Value.Bounds.Height / (Height.Value + EdgeThickness.Value));
        var geometry = PathGeometryNoRotate.Value;
        geometry.Transform = new TranslateTransform(-geometry.Bounds.Left, -geometry.Bounds.Top);
    }

    public override PathGeometry CreateGeometry(bool flag = false)
    {
        var ret = GeometryCreator.CreateDonut(this, PieCenterPoint.Value, DonutWidth.Value, Distance.Value,
            StartDegree.Value, EndDegree.Value, SweepDirection.Value, flag);
        ret.Transform = new TranslateTransform(-ret.Bounds.Left, -ret.Bounds.Top);
        return ret;
    }

    public override PathGeometry CreateGeometry(double angle)
    {
        var geometry = GeometryCreator.CreateDonut(PieCenterPoint.Value, DonutWidth.Value, Distance.Value,
            StartDegree.Value, EndDegree.Value, SweepDirection.Value);
        geometry.Transform = new RotateTransform(angle, PieCenterPoint.Value.X, PieCenterPoint.Value.Y);
        return geometry;
    }

    public NEllipseViewModel CreateNEllipseViewModelLong()
    {
        var viewModel = new NEllipseViewModel();
        viewModel.Left.Value = PieCenterPoint.Value.X - Distance.Value;
        viewModel.Top.Value = PieCenterPoint.Value.Y - Distance.Value;
        viewModel.Width.Value = Distance.Value * 2;
        viewModel.Height.Value = Distance.Value * 2;
        viewModel.PathGeometryNoRotate.Value = GeometryCreator.CreateEllipse(PieCenterPoint.Value.X,
            PieCenterPoint.Value.Y,
            new Thickness(PieCenterPoint.Value.X - Distance.Value, PieCenterPoint.Value.Y - Distance.Value,
                PieCenterPoint.Value.X + Distance.Value, PieCenterPoint.Value.Y + Distance.Value));
        return viewModel;
    }

    public NEllipseViewModel CreateNEllipseViewModelShort()
    {
        var viewModel = new NEllipseViewModel();
        viewModel.Left.Value = PieCenterPoint.Value.X - (Distance.Value - DonutWidth.Value);
        viewModel.Top.Value = PieCenterPoint.Value.Y - (Distance.Value - DonutWidth.Value);
        viewModel.Width.Value = (Distance.Value - DonutWidth.Value) * 2;
        viewModel.Height.Value = (Distance.Value - DonutWidth.Value) * 2;
        viewModel.PathGeometryNoRotate.Value = GeometryCreator.CreateEllipse(PieCenterPoint.Value.X,
            PieCenterPoint.Value.Y,
            new Thickness(PieCenterPoint.Value.X - Distance.Value, PieCenterPoint.Value.Y - Distance.Value,
                PieCenterPoint.Value.X + Distance.Value, PieCenterPoint.Value.Y + Distance.Value));
        return viewModel;
    }

    public override Type GetViewType()
    {
        return typeof(Path);
    }

    #region IClonable

    public override object Clone()
    {
        var clone = new NPieViewModel();
        clone.Owner = Owner;
        clone.Left.Value = Left.Value;
        clone.Top.Value = Top.Value;
        clone.Width.Value = Width.Value;
        clone.Height.Value = Height.Value;
        clone.EdgeBrush.Value = EdgeBrush.Value;
        clone.FillBrush.Value = FillBrush.Value;
        clone.EdgeThickness.Value = EdgeThickness.Value;
        clone.RotationAngle.Value = RotationAngle.Value;
        clone.PathGeometryNoRotate.Value = CreateGeometry();
        clone.PieCenterPoint.Value = PieCenterPoint.Value;
        clone.DonutWidth.Value = DonutWidth.Value;
        clone.Distance.Value = Distance.Value;
        clone.StartDegree.Value = StartDegree.Value;
        clone.EndDegree.Value = EndDegree.Value;
        clone.SweepDirection.Value = SweepDirection.Value;
        clone.StrokeLineJoin.Value = StrokeLineJoin.Value;
        clone.StrokeDashArray.Value = StrokeDashArray.Value;
        clone.StrokeMiterLimit.Value = StrokeMiterLimit.Value;
        return clone;
    }

    #endregion //IClonable
}