using boilersGraphics.Exceptions;
using boilersGraphics.Helpers;
using NLog;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media;

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

    public ReactiveProperty<Point> LeftTop { get; set; }

    public ReadOnlyReactivePropertySlim<double> Width { get; set; }

    public ReadOnlyReactivePropertySlim<double> Height { get; set; }
    public ReadOnlyReactivePropertySlim<Rect> Rect { get; set; }

    public ObservableCollection<Point> Points
    {
        get => _Points;
        set => SetProperty(ref _Points, value);
    }

    public ReadOnlyReactivePropertySlim<SnapPointViewModel> SnapPoint0VM { get; protected set; }
    public ReadOnlyReactivePropertySlim<SnapPointViewModel> SnapPoint1VM { get; protected set; }

    public ReactiveCollection<PenLineCap> PenLineCaps { get; private set; }

    public ReactivePropertySlim<PenLineCap> StrokeStartLineCap { get; } = new();

    public ReactivePropertySlim<PenLineCap> StrokeEndLineCap { get; } = new();

    public ReadOnlyReactivePropertySlim<Thickness> ResizeHandleMargin { get; set; }

    public virtual void PostProcess_AddPointP1(Point p1)
    {
    }

    public virtual void PostProcess_AddPointP2(Point p2)
    {
    }

    public void AddPointP1(IDiagramViewModel diagramViewModel, Point p1)
    {
        if (Points.Count() != 0)
            throw new UnexpectedException("Points.Count() != 0");
        Points.Add(p1);
        SnapPoint0VM = Observable.Return(Points[0])
            .Select(x => new SnapPointViewModel(this, 0, diagramViewModel, x.X, x.Y, 3, 3))
            .ToReadOnlyReactivePropertySlim();
        LogManager.GetCurrentClassLogger().Debug($"{ID} AddPointP1 {p1}");
        PostProcess_AddPointP1(p1);
    }

    public void AddPointP2(IDiagramViewModel diagramViewModel, Point p2)
    {
        if (Points.Count() != 1)
            throw new UnexpectedException("Points.Count() != 1");
        Points.Add(p2);
        SnapPoint1VM = Observable.Return(Points[1])
            .Select(x => new SnapPointViewModel(this, 1, diagramViewModel, x.X, x.Y, 3, 3))
            .ToReadOnlyReactivePropertySlim();
        LogManager.GetCurrentClassLogger().Debug($"{ID} AddPointP2 {p2}");
        PostProcess_AddPointP2(p2);
    }

    public void AddPoints(IDiagramViewModel diagramViewModel, Point p1, Point p2)
    {
        Points.Add(p1);
        Points.Add(p2);
        SnapPoint0VM = Observable.Return(Points[0])
            .Select(x => new SnapPointViewModel(this, 0, diagramViewModel, x.X, x.Y, 3, 3))
            .ToReadOnlyReactivePropertySlim();
        SnapPoint1VM = Observable.Return(Points[1])
            .Select(x => new SnapPointViewModel(this, 1, diagramViewModel, x.X, x.Y, 3, 3))
            .ToReadOnlyReactivePropertySlim();
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
        LeftTop = Points.ObserveProperty(x => x.Count)
            .Where(x => x > 0)
            .Select(_ => new Point(Points.Min(x => x.X), Points.Min(x => x.Y)))
            .ToReactiveProperty();
        Width = Points.ObserveProperty(x => x.Count)
            .Where(x => x > 0)
            .Select(_ => Points.Max(x => x.X) - Points.Min(x => x.X))
            .ToReadOnlyReactivePropertySlim();
        Height = Points.ObserveProperty(x => x.Count)
            .Where(x => x > 0)
            .Select(_ => Points.Max(x => x.Y) - Points.Min(x => x.Y))
            .ToReadOnlyReactivePropertySlim();
        Rect = LeftTop.CombineLatest(Width, Height, (lt, width, height) => new Rect(lt, new Size(width, height)))
            .ToReadOnlyReactivePropertySlim();
        PenLineCaps = new ReactiveCollection<PenLineCap>();
        PenLineCaps.Add(PenLineCap.Flat);
        PenLineCaps.Add(PenLineCap.Round);
        PenLineCaps.Add(PenLineCap.Square);
        PenLineCaps.Add(PenLineCap.Triangle);
        ResizeHandleMargin = Observable.Return(3).Select(size => new Thickness(-size, -size, -size, -size))
            .ToReadOnlyReactivePropertySlim();
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