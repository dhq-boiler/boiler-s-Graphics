using boilersGraphics.Helpers;
using boilersGraphics.Views;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Prism.Unity;
using Reactive.Bindings;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using Path = System.Windows.Shapes.Path;

namespace boilersGraphics.ViewModels;

public class PolyBezierViewModel : ConnectorBaseViewModel
{
    public PolyBezierViewModel()
    {
    }

    public PolyBezierViewModel(int id, IDiagramViewModel parent)
        : base(id, parent)
    {
        Init(parent);
    }

    public PolyBezierViewModel(IDiagramViewModel parent, Point beginPoint)
        : base(0, parent)
    {
        Init(parent);
        Points.Add(beginPoint);
    }

    public override bool SupportsPropertyDialog => true;

    public void InitializeSnapPoints(Point begin, Point end)
    {
        SnapPoint0VM = Observable.Return(begin)
            .Select(x => new SnapPointViewModel(this, 0, Owner, x.X, x.Y, 3, 3))
            .ToReadOnlyReactivePropertySlim();
        SnapPoint1VM = Observable.Return(end)
            .Select(x => new SnapPointViewModel(this, 1, Owner, x.X, x.Y, 3, 3))
            .ToReadOnlyReactivePropertySlim();
    }

    private void Init(IDiagramViewModel diagramViewModel)
    {
        Points.CollectionChanged += Points_CollectionChanged;
    }

    private void Points_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (SnapPoint0VM != null)
            SnapPoint0VM.Dispose();
        SnapPoint0VM = Observable.Return(Points.First())
            .Select(x => new SnapPointViewModel(this, 0, Owner, x.X, x.Y, 3, 3))
            .ToReadOnlyReactivePropertySlim();
        if (SnapPoint1VM != null)
            SnapPoint1VM.Dispose();
        SnapPoint1VM = Observable.Return(Points.Last())
            .Select(x => new SnapPointViewModel(this, 1, Owner, x.X, x.Y, 3, 3))
            .ToReadOnlyReactivePropertySlim();
    }

    public override object Clone()
    {
        var clone = new PolyBezierViewModel(0, Owner);
        clone.EdgeBrush.Value = EdgeBrush.Value;
        clone.FillBrush.Value = FillBrush.Value;
        clone.EdgeThickness.Value = EdgeThickness.Value;
        clone.Points = Points;
        clone.PathGeometryNoRotate.Value = GeometryCreator.CreatePolyBezier(clone);
        clone.StrokeStartLineCap.Value = StrokeStartLineCap.Value;
        clone.StrokeEndLineCap.Value = StrokeEndLineCap.Value;
        clone.StrokeLineJoin.Value = StrokeLineJoin.Value;
        clone.StrokeDashArray.Value = StrokeDashArray.Value;
        clone.StrokeMiterLimit.Value = StrokeMiterLimit.Value;
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
        dialogService.Show(nameof(DetailPolyBezier), new DialogParameters { { "ViewModel", this } },
            ret => result = ret);
    }
}