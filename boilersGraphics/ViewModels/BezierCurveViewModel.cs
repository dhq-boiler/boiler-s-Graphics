using boilersGraphics.Helpers;
using boilersGraphics.Views;
using NLog;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Prism.Unity;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Specialized;
using System.Reactive.Linq;
using System.Windows;
using Path = System.Windows.Shapes.Path;

namespace boilersGraphics.ViewModels;

public class BezierCurveViewModel : ConnectorBaseViewModel
{
    public BezierCurveViewModel(int id, IDiagramViewModel parent)
        : base(id, parent)
    {
        Init();
    }

    public BezierCurveViewModel()
    {
        Init();
    }

    public BezierCurveViewModel(IDiagramViewModel diagramViewModel, Point p1, Point p2, Point c1, Point c2)
    {
        Init();
        AddPoints(diagramViewModel, p1, p2);
        ControlPoint1.Value = c1;
        ControlPoint2.Value = c2;
        P1X = Observable.Return(p1.X).ToReactiveProperty();
        P1Y = Observable.Return(p1.Y).ToReactiveProperty();
        P2X = Observable.Return(p2.X).ToReactiveProperty();
        P2Y = Observable.Return(p2.Y).ToReactiveProperty();
        C1X = Observable.Return(c1.X).ToReactiveProperty();
        C1Y = Observable.Return(c1.Y).ToReactiveProperty();
        C2X = Observable.Return(c2.X).ToReactiveProperty();
        C2Y = Observable.Return(c2.Y).ToReactiveProperty();
    }

    public ReactivePropertySlim<Point> ControlPoint1 { get; set; } = new();
    public ReactivePropertySlim<Point> ControlPoint2 { get; set; } = new();

    public ReactivePropertySlim<Point> ControlLine1LeftTop { get; set; } = new();
    public ReactivePropertySlim<Point> ControlLine2LeftTop { get; set; } = new();

    public ReactiveProperty<double> P1X { get; set; }
    public ReactiveProperty<double> P1Y { get; set; }
    public ReactiveProperty<double> P2X { get; set; }
    public ReactiveProperty<double> P2Y { get; set; }
    public ReactiveProperty<double> C1X { get; set; }
    public ReactiveProperty<double> C1Y { get; set; }
    public ReactiveProperty<double> C2X { get; set; }
    public ReactiveProperty<double> C2Y { get; set; }

    public override bool SupportsPropertyDialog => true;

    private void Init()
    {
        Points.CollectionChanged += Points_CollectionChanged;
        ControlPoint1.Subscribe(x =>
            {
                if (Points.Count > 0)
                {
                    SetLeftTopOfControlLine1();
                    SetLeftTop();
                }
            })
            .AddTo(_CompositeDisposable);
        ControlPoint2.Subscribe(x =>
            {
                if (Points.Count > 1)
                {
                    SetLeftTopOfControlLine2();
                    SetLeftTop();
                }
            })
            .AddTo(_CompositeDisposable);
        UpdatingStrategy.Value = PathGeometryUpdatingStrategy.Fixed;
    }

    private void SetLeftTopOfControlLine1()
    {
        var point = new Point();
        point.X = Math.Min(Points[0].X, ControlPoint1.Value.X);
        point.Y = Math.Min(Points[0].Y, ControlPoint1.Value.Y);
        ControlLine1LeftTop.Value = point;
        LogManager.GetCurrentClassLogger().Trace($"ControlLine1LeftTop={ControlLine1LeftTop.Value}");
    }

    private void SetLeftTopOfControlLine2()
    {
        var point = new Point();
        point.X = Math.Min(Points[1].X, ControlPoint2.Value.X);
        point.Y = Math.Min(Points[1].Y, ControlPoint2.Value.Y);
        ControlLine2LeftTop.Value = point;
        LogManager.GetCurrentClassLogger().Trace($"ControlLine2LeftTop={ControlLine2LeftTop.Value}");
    }

    private void Points_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (Points.Count >= 2)
        {
            LogManager.GetCurrentClassLogger().Trace($"P1={Points[0]} P2={Points[1]}");
            SetLeftTopOfControlLine1();
            SetLeftTopOfControlLine2();
            SetLeftTop();
        }
        else
        {
            LogManager.GetCurrentClassLogger().Trace("Points.Count < 2");
        }
    }

    private void SetLeftTop()
    {
        if (Points.Count != 2)
            throw new Exception("Points.Count == 2");
        var minX = Math.Min(Points[0].X, Points[1].X);
        var minY = Math.Min(Points[0].Y, Points[1].Y);
        var points = new[] { Points[0], ControlPoint1.Value, ControlPoint2.Value, Points[1] };
        var diffT = 1.0 / 64.0;
        for (var t = diffT; t < 1.0; t += diffT)
        {
            var result = BezierCurve.Evaluate(t, points);
            minX = Math.Min(minX, result.X);
            minY = Math.Min(minY, result.Y);
        }

        LeftTop.Value = new Point(minX, minY);
    }

    public override object Clone()
    {
        var clone = new BezierCurveViewModel(Owner, Points[0], Points[1], ControlPoint1.Value, ControlPoint2.Value);
        clone.Owner = Owner;
        clone.EdgeBrush.Value = EdgeBrush.Value;
        clone.FillBrush.Value = FillBrush.Value;
        clone.EdgeThickness.Value = EdgeThickness.Value;
        clone.PathGeometryNoRotate.Value = GeometryCreator.CreateBezierCurve(clone);
        clone.StrokeStartLineCap.Value = StrokeStartLineCap.Value;
        clone.StrokeEndLineCap.Value = StrokeEndLineCap.Value;
        clone.StrokeLineJoin.Value = StrokeLineJoin.Value;
        clone.StrokeDashArray.Value = StrokeDashArray.Value;
        return clone;
    }

    public override Type GetViewType()
    {
        return typeof(Path);
    }

    public override void OpenPropertyDialog()
    {
        var dialogService =
            new DialogService((Application.Current as PrismApplication).Container as IContainerExtension);
        IDialogResult result = null;
        dialogService.Show(nameof(DetailBezier), new DialogParameters { { "ViewModel", this } }, ret => result = ret);
    }
}