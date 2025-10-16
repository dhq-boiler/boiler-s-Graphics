using boilersGraphics.Exceptions;
using boilersGraphics.Helpers;
using NLog;
using R3;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using ObservableCollections;
using ZLinq;

namespace boilersGraphics.ViewModels;

public abstract class ConnectorBaseViewModel : SelectableDesignerItemViewModelBase, IObserver<TransformNotification>, IRect, ICloneable, ISizeReadOnlyRps
{
    private ObservableCollection<Point> _Points;

    public ConnectorBaseViewModel(int id, IDiagramViewModel parent) : base(id, parent)
    {
        Init();
    }

    public ConnectorBaseViewModel()
    {
        Init();
    }

    public R3.BindableReactiveProperty<Point> LeftTop { get; set; }

    public R3.IReadOnlyBindableReactiveProperty<double> Width { get; set; }

    public R3.IReadOnlyBindableReactiveProperty<double> Height { get; set; }
    public R3.IReadOnlyBindableReactiveProperty<Rect> Rect { get; set; }

    public ObservableCollection<Point> Points
    {
        get => _Points;
        set => SetProperty(ref _Points, value);
    }

    public IReadOnlyBindableReactiveProperty<SnapPointViewModel> SnapPoint0VM { get; protected set; }
    public IReadOnlyBindableReactiveProperty<SnapPointViewModel> SnapPoint1VM { get; protected set; }

    public NotifyCollectionChangedSynchronizedViewList<PenLineCap> PenLineCaps { get; private set; }

    public BindableReactiveProperty<PenLineCap> StrokeStartLineCap { get; } = new();

    public BindableReactiveProperty<PenLineCap> StrokeEndLineCap { get; } = new();

    public IReadOnlyBindableReactiveProperty<Thickness> ResizeHandleMargin { get; set; }

    public virtual void PostProcess_AddPointP1(Point p1)
    {
    }

    public virtual void PostProcess_AddPointP2(Point p2)
    {
    }

    public void AddPointP1(IDiagramViewModel diagramViewModel, Point p1)
    {
        if (Points.AsValueEnumerable().Count() != 0)
            throw new UnexpectedException("Points.Count() != 0");
        Points.Add(p1);
        SnapPoint0VM = Observable.Return(Points[0])
            .Select(x => new SnapPointViewModel(this, 0, diagramViewModel, x.X, x.Y, 3, 3))
            .ToReadOnlyBindableReactiveProperty();
        LogManager.GetCurrentClassLogger().Debug($"{ID} AddPointP1 {p1}");
        PostProcess_AddPointP1(p1);
    }

    public void AddPointP2(IDiagramViewModel diagramViewModel, Point p2)
    {
        if (Points.AsValueEnumerable().Count() != 1)
            throw new UnexpectedException("Points.Count() != 1");
        Points.Add(p2);
        SnapPoint1VM = Observable.Return(Points[1])
            .Select(x => new SnapPointViewModel(this, 1, diagramViewModel, x.X, x.Y, 3, 3))
            .ToReadOnlyBindableReactiveProperty();
        LogManager.GetCurrentClassLogger().Debug($"{ID} AddPointP2 {p2}");
        PostProcess_AddPointP2(p2);
    }

    public void AddPoints(IDiagramViewModel diagramViewModel, Point p1, Point p2)
    {
        Points.Add(p1);
        Points.Add(p2);
        SnapPoint0VM = Observable.Return(Points[0])
            .Select(x => new SnapPointViewModel(this, 0, diagramViewModel, x.X, x.Y, 3, 3))
            .ToReadOnlyBindableReactiveProperty();
        SnapPoint1VM = Observable.Return(Points[1])
            .Select(x => new SnapPointViewModel(this, 1, diagramViewModel, x.X, x.Y, 3, 3))
            .ToReadOnlyBindableReactiveProperty();
    }

    public void InitIsSelectedOnSnapPoints()
    {
        IsSelected.Subscribe(x =>
            {
                if (x)
                {
                    SnapPoint0VM.Value.IsSelected.Value = true;
                    SnapPoint1VM.Value.IsSelected.Value = true;
                }

                if (!x)
                {
                    SnapPoint0VM.Value.IsSelected.Value = false;
                    SnapPoint1VM.Value.IsSelected.Value = false;
                }
            })
            .AddTo(_CompositeDisposable);
    }

    private void Init()
    {
        _Points = new ObservableCollection<Point>();
        InitPathFinder();
        LeftTop = Points.ObservePropertyChanged(x => x.Count)
            .Where(x => x > 0)
            .Select(_ => new Point(Points.AsValueEnumerable().Min(x => x.X), Points.AsValueEnumerable().Min(x => x.Y)))
            .ToBindableReactiveProperty();
        Width = Points.ObservePropertyChanged(x => x.Count)
            .Where(x => x > 0)
            .Select(_ => Points.AsValueEnumerable().Max(x => x.X) - Points.AsValueEnumerable().Min(x => x.X))
            .ToReadOnlyBindableReactiveProperty();
        Height = Points.ObservePropertyChanged(x => x.Count)
            .Where(x => x > 0)
            .Select(_ => Points.AsValueEnumerable().Max(x => x.Y) - Points.AsValueEnumerable().Min(x => x.Y))
            .ToReadOnlyBindableReactiveProperty();
        Rect = LeftTop.CombineLatest(Width.AsObservable(), Height.AsObservable(), (lt, width, height) => new Rect(lt, new Size(width, height)))
            .ToReadOnlyBindableReactiveProperty();
        PenLineCaps = new ObservableList<PenLineCap>().ToWritableNotifyCollectionChanged();
        PenLineCaps.Add(PenLineCap.Flat);
        PenLineCaps.Add(PenLineCap.Round);
        PenLineCaps.Add(PenLineCap.Square);
        PenLineCaps.Add(PenLineCap.Triangle);
        ResizeHandleMargin = Observable.Return(3).Select(size => new Thickness(-size, -size, -size, -size))
            .ToReadOnlyBindableReactiveProperty();
    }

    protected virtual void InitPathFinder()
    {
    }

    #region IObserver<GroupTransformNotification>

    public override void OnNext(GroupTransformNotification value)
    {
        var oldWidth = value.OldWidth;
        var oldHeight = value.OldHeight;

        switch (value.Type)
        {
            case TransformType.Move:
                var a = Points[0];
                var b = Points[1];
                a.X += value.LeftChange;
                b.X += value.LeftChange;
                a.Y += value.TopChange;
                b.Y += value.TopChange;
                Points[0] = a;
                Points[1] = b;
                break;
            case TransformType.Resize:
                a = Points[0];
                b = Points[1];
                a.X = (a.X - value.GroupLeftTop.X) * ((oldWidth + value.WidthChange) / oldWidth) + value.GroupLeftTop.X;
                b.X = (b.X - value.GroupLeftTop.X) * ((oldWidth + value.WidthChange) / oldWidth) + value.GroupLeftTop.X;
                a.Y = (a.Y - value.GroupLeftTop.Y) * ((oldHeight + value.HeightChange) / oldHeight) +
                      value.GroupLeftTop.Y;
                b.Y = (b.Y - value.GroupLeftTop.Y) * ((oldHeight + value.HeightChange) / oldHeight) +
                      value.GroupLeftTop.Y;
                Points[0] = a;
                Points[1] = b;
                break;
            case TransformType.Rotate:
                a = Points[0];
                b = Points[1];
                var diffAngle = value.RotateAngleChange;
                var center = value.GroupCenter;
                var matrix = new Matrix();
                //derive rotated 0 degree point
                matrix.RotateAt(-RotationAngle.Value, center.X, center.Y);
                var origA = matrix.Transform(a);
                var origB = matrix.Transform(b);
                //derive rotated N degrees point from rotated 0 degree point in transform result
                matrix = new Matrix();
                RotationAngle.Value += diffAngle;
                matrix.RotateAt(RotationAngle.Value, center.X, center.Y);
                var newA = matrix.Transform(origA);
                var newB = matrix.Transform(origB);
                Points[0] = newA;
                Points[1] = newB;
                break;
        }
    }

    #endregion //IObserver<TransformNotification>
}