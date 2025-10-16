using boilersGraphics.Helpers;
using R3;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using boilersGraphics.Models;

namespace boilersGraphics.ViewModels;

public abstract class DesignerItemViewModelBase : SelectableDesignerItemViewModelBase, ICloneable, IRect, ISizeRps
{
    public static readonly double DefaultWidth = 65d;
    public static readonly double DefaultHeight = 65d;
    private double _MinHeight;

    private double _MinWidth;
    private bool _showConnectors;

    public DesignerItemViewModelBase(int id, IDiagramViewModel parent, double left, double top) : base(id, parent)
    {
        Left.Value = left;
        Top.Value = top;
        Rect = Left.CombineLatest(Top, Width, Height, (left, top, width, height) => new Rect(left, top, width, height))
            .ToReadOnlyBindableReactiveProperty();
        Size = Width.CombineLatest(Height, (w, h) => new Size(w, h)).ToReadOnlyBindableReactiveProperty();
        SizeIncludeFrame = Width.CombineLatest(Height, (w, h) => new Size(w + 1, h + 1)).ToReadOnlyBindableReactiveProperty();
        Init();
    }

    public DesignerItemViewModelBase()
    {
        Rect = Left.CombineLatest(Top, Width, Height, (left, top, width, height) => new Rect(left, top, width, height))
            .ToReadOnlyBindableReactiveProperty();
        Size = Width.CombineLatest(Height, (w, h) => new Size(w, h)).ToReadOnlyBindableReactiveProperty();
        SizeIncludeFrame = Width.CombineLatest(Height, (w, h) => new Size(w + 1, h + 1)).ToReadOnlyBindableReactiveProperty();
        Init();
    }

    public double MinWidth
    {
        get => _MinWidth;
        set => SetProperty(ref _MinWidth, value);
    }

    public double MinHeight
    {
        get => _MinHeight;
        set => SetProperty(ref _MinHeight, value);
    }

    public BindableReactiveProperty<double> Width { get; } = new();

    public BindableReactiveProperty<double> Height { get; } = new();

    public IReadOnlyBindableReactiveProperty<Size> Size { get; }

    public IReadOnlyBindableReactiveProperty<Size> SizeIncludeFrame { get; }

    public bool ShowConnectors
    {
        get => _showConnectors;
        set
        {
            if (_showConnectors != value)
            {
                _showConnectors = value;
                RaisePropertyChanged();
            }
        }
    }

    public BindableReactiveProperty<bool> RenderingEnabled { get; } = new(true);

    public BindableReactiveProperty<string> Pool { get; } = new();

    public BindableReactiveProperty<double> Left { get; } = new();

    public BindableReactiveProperty<double> Top { get; } = new();

    public IReadOnlyBindableReactiveProperty<double> Right { get; private set; }

    public IReadOnlyBindableReactiveProperty<double> Bottom { get; private set; }

    public IReadOnlyBindableReactiveProperty<Rect> Rect { get; set; }

    public BindableReactiveProperty<double> CenterX { get; } = new();
    public BindableReactiveProperty<double> CenterY { get; } = new();

    public IReadOnlyBindableReactiveProperty<Point> CenterPoint { get; private set; }

    public BindableReactiveProperty<Thickness> Margin { get; } = new();

    public IReadOnlyBindableReactiveProperty<Thickness> MarginLeftTop { get; private set; }
    public IReadOnlyBindableReactiveProperty<Thickness> MarginLeftBottom { get; private set; }
    public IReadOnlyBindableReactiveProperty<Thickness> MarginRightTop { get; private set; }
    public IReadOnlyBindableReactiveProperty<Thickness> MarginRightBottom { get; private set; }
    public IReadOnlyBindableReactiveProperty<Thickness> MarginLeft { get; private set; }
    public IReadOnlyBindableReactiveProperty<Thickness> MarginTop { get; private set; }
    public IReadOnlyBindableReactiveProperty<Thickness> MarginRight { get; private set; }
    public IReadOnlyBindableReactiveProperty<Thickness> MarginBottom { get; private set; }
    public BindableReactiveProperty<TransformNotification> TransformNortification { get; } = new();

    internal SnapPointPosition snapPointPosition { get; set; }

    public CompositeDisposable SnapObjs { get; } = new();

    private void UpdateCenterPoint()
    {
        var leftTop = new Point(Left.Value, Top.Value);
        var center = new Point(leftTop.X + Width.Value * 0.5, leftTop.Y + Height.Value * 0.5);
        CenterX.Value = center.X;
        CenterY.Value = center.Y;
    }

    private void Init()
    {
        MinWidth = 0;
        MinHeight = 0;

        Left
            .Zip(Left.Skip(1), (Old, New) => new { OldItem = Old, NewItem = New })
            .Subscribe(async x =>
            {
                UpdateTransform(nameof(Left), x.OldItem, x.NewItem);
                if (RenderingEnabled.Value)
                {
                    await OnRectChanged(new Rect(Left.Value, Top.Value, Width.Value, Height.Value));
                }
                UpdateChangeFormTriggerObject();
            })
            .AddTo(_CompositeDisposable);
        Top
            .Zip(Top.Skip(1), (Old, New) => new { OldItem = Old, NewItem = New })
            .Subscribe(async x =>
            {
                UpdateTransform(nameof(Top), x.OldItem, x.NewItem);
                if (RenderingEnabled.Value)
                {
                    await OnRectChanged(new Rect(Left.Value, Top.Value, Width.Value, Height.Value));
                }
                UpdateChangeFormTriggerObject();
            })
            .AddTo(_CompositeDisposable);
        Width
            .Zip(Width.Skip(1), (Old, New) => new { OldItem = Old, NewItem = New })
            .Subscribe(async x =>
            {
                UpdateTransform(nameof(Width), x.OldItem, x.NewItem);
                if (RenderingEnabled.Value)
                {
                    await OnRectChanged(new Rect(Left.Value, Top.Value, Width.Value, Height.Value));

                    var renderer = DiagramViewModel.Instance.Renderer;
                    renderer?.MarkItemDirty(this);
                }
                UpdateChangeFormTriggerObject();
            })
            .AddTo(_CompositeDisposable);
        Height
            .Zip(Height.Skip(1), (Old, New) => new { OldItem = Old, NewItem = New })
            .Subscribe(async x =>
            {
                UpdateTransform(nameof(Height), x.OldItem, x.NewItem);
                if (RenderingEnabled.Value)
                {
                    await OnRectChanged(new Rect(Left.Value, Top.Value, Width.Value, Height.Value));

                    var renderer = DiagramViewModel.Instance.Renderer;
                    renderer?.MarkItemDirty(this);
                }
                UpdateChangeFormTriggerObject();
            })
            .AddTo(_CompositeDisposable);
        RotationAngle
            .Zip(RotationAngle.Skip(1), (Old, New) => new { OldItem = Old, NewItem = New })
            .Subscribe(async x =>
            {
                UpdateTransform(nameof(RotationAngle), x.OldItem, x.NewItem);
                UpdateMatrix(x.OldItem, x.NewItem);
                if (RenderingEnabled.Value)
                {
                    await OnRectChanged(new Rect(Left.Value, Top.Value, Width.Value, Height.Value));

                    var renderer = DiagramViewModel.Instance.Renderer;
                    renderer?.MarkItemDirty(this);
                }
                UpdateChangeFormTriggerObject();
            })
            .AddTo(_CompositeDisposable);
        ZIndex
            .Zip(ZIndex.Skip(1), (Old, New) => new { OldItem = Old, NewItem = New })
            .Subscribe(async x =>
            {
                if (RenderingEnabled.Value)
                {
                    await OnRectChanged(new Rect(Left.Value, Top.Value, Width.Value, Height.Value));

                    var renderer = DiagramViewModel.Instance.Renderer;
                    renderer?.MarkItemDirty(this);
                }
            })
            .AddTo(_CompositeDisposable);
        Matrix
            .Zip(Matrix.Skip(1), (Old, New) => new { OldItem = Old, NewItem = New })
            .Subscribe(x =>
            {
                UpdateTransform(nameof(Matrix), x.OldItem, x.NewItem);
                UpdateChangeFormTriggerObject();

                var renderer = DiagramViewModel.Instance.Renderer;
                renderer?.MarkItemDirty(this);
            })
            .AddTo(_CompositeDisposable);
        Right = Left.CombineLatest(Width, (a, b) => a + b)
            .ToReadOnlyBindableReactiveProperty();
        Bottom = Top.CombineLatest(Height, (a, b) => a + b)
            .ToReadOnlyBindableReactiveProperty();
        CenterX.Subscribe(x => UpdateLeft(x))
            .AddTo(_CompositeDisposable);
        CenterY.Subscribe(x => UpdateTop(x))
            .AddTo(_CompositeDisposable);
        CenterPoint = CenterX.CombineLatest(CenterY, (x, y) => new Point(x, y))
            .ToReadOnlyBindableReactiveProperty();
        EdgeThickness
            .Zip(EdgeThickness.Skip(1), (Old, New) => new { OldItem = Old, NewItem = New })
            .Subscribe(x =>
            {
                UpdateTransform(nameof(EdgeThickness), x.OldItem, x.NewItem);
                UpdateChangeFormTriggerObject();

                var renderer = DiagramViewModel.Instance.Renderer;
                renderer?.MarkItemDirty(this);
            })
            .AddTo(_CompositeDisposable);

        PathGeometry = PathGeometryNoRotate.ToReadOnlyBindableReactiveProperty();

        Matrix.Value = new Matrix();

        UpdatingStrategy.Where(x => x == PathGeometryUpdatingStrategy.Initial).Subscribe(x =>
        {
            UpdatePathGeometryIfEnable(nameof(UpdatingStrategy), PathGeometryUpdatingStrategy.Unknown, PathGeometryUpdatingStrategy.Initial);
        }).AddTo(_CompositeDisposable);

        MarginLeftTop = ThumbSize.Select(size => new Thickness(-size, -size, 0, 0)).ToReadOnlyBindableReactiveProperty();
        MarginLeftBottom = ThumbSize.Select(size => new Thickness(-size, 0, 0, -size)).ToReadOnlyBindableReactiveProperty();
        MarginRightTop = ThumbSize.Select(size => new Thickness(0, -size, -size, 0)).ToReadOnlyBindableReactiveProperty();
        MarginRightBottom =
            ThumbSize.Select(size => new Thickness(0, 0, -size, -size)).ToReadOnlyBindableReactiveProperty();
        MarginLeft = Observable.Return(ThumbSize.CurrentValue / 2).CombineLatest(DiagramViewModel.Instance.MagnificationRate,
            (size, rate) => { return new Thickness(-size / (rate / 100d), 0, 0, 0); }).ToReadOnlyBindableReactiveProperty();
        MarginTop = Observable.Return(ThumbSize.CurrentValue / 2).CombineLatest(DiagramViewModel.Instance.MagnificationRate,
            (size, rate) => { return new Thickness(0, -size / (rate / 100d), 0, 0); }).ToReadOnlyBindableReactiveProperty();
        MarginRight = Observable.Return(ThumbSize.CurrentValue / 2).CombineLatest(DiagramViewModel.Instance.MagnificationRate,
            (size, rate) => { return new Thickness(0, 0, -size / (rate / 100d), 0); }).ToReadOnlyBindableReactiveProperty();
        MarginBottom = Observable.Return(ThumbSize.CurrentValue / 2).CombineLatest(DiagramViewModel.Instance.MagnificationRate,
            (size, rate) => { return new Thickness(0, 0, 0, -size / (rate / 100d)); }).ToReadOnlyBindableReactiveProperty();
        RenderingEnabled.Subscribe(async renderingEnabled =>
        {
            if (renderingEnabled)
            {
                await OnRectChanged(new Rect(Left.Value, Top.Value, Width.Value, Height.Value));

                var renderer = DiagramViewModel.Instance.Renderer;
                renderer?.MarkItemDirty(this);
            }
        }).AddTo(_CompositeDisposable);
    }

    public virtual async Task OnRectChanged(Rect rect)
    {
    }

    private void UpdateMatrix(double oldAngle, double newAngle)
    {
        var matrix = new Matrix();
        matrix.Rotate(newAngle);
        Matrix.Value = matrix;
    }

    private void UpdateLeft(double value)
    {
        Left.Value = value - Width.Value / 2;
    }

    private void UpdateTop(double value)
    {
        Top.Value = value - Height.Value / 2;
    }

    public virtual void UpdateMargin(string propertyName, object oldValue, object newValue)
    {
    }

    public void UpdateTransform(string propertyName, object oldValue, object newValue)
    {
        switch (propertyName)
        {
            case "Left":
            case "Top":
            case "Width":
            case "Height":
                UpdateCenterPoint();
                break;
        }

        UpdateMargin(propertyName, oldValue, newValue);
        TransformObserversOnNext(propertyName, oldValue, newValue);
        UpdatePathGeometryInCase(propertyName, oldValue, newValue);
    }

    private void UpdatePathGeometryInCase(string propertyName, object oldValue, object newValue)
    {
        switch (propertyName)
        {
            case "Left":
            case "Top":
            case "Width":
            case "Height":
            case "RotationAngle":
            case "Matrix":
            case "EdgeThickness":
            case "RadiusX":
            case "RadiusY":
                UpdatePathGeometryIfEnable(propertyName, oldValue, newValue);
                AfterUpdatePathGeometry(propertyName, oldValue, newValue);
                break;
        }
    }

    protected virtual void AfterUpdatePathGeometry(string propertyName, object oldValue, object newValue)
    {
    }

    public virtual void UpdatePathGeometryIfEnable(string propertyName, object oldValue, object newValue,
        bool flag = false)
    {
        if (UpdatingStrategy.Value != PathGeometryUpdatingStrategy.Unknown)
        {
            if (!flag) PathGeometryNoRotate.Value = CreateGeometry(flag);

            if (RotationAngle.Value != 0d) PathGeometryRotate.Value = GeometryCreator.Rotate(PathGeometryNoRotate.Value, RotationAngle.Value, CenterPoint.Value);
        }

        if (UpdatingStrategy.Value == PathGeometryUpdatingStrategy.Initial)
        {
            UpdatingStrategy.Value = PathGeometryUpdatingStrategy.ResizeWhilePreservingOriginalShape;
        }
    }

    public abstract PathGeometry CreateGeometry(bool flag = false);


    public void TransformObserversOnNext(string propertyName, object oldValue, object newValue)
    {
        var tn = new TransformNotification
        {
            Sender = this,
            PropertyName = propertyName,
            OldValue = oldValue,
            NewValue = newValue,
            SnapPointPosition = snapPointPosition
        };
        TransformNortification.Value = tn;
        _observers.ForEach(x => x.OnNext(tn));
    }

    public override void OnNext(GroupTransformNotification value)
    {
        var oldLeft = Left.Value;
        var oldTop = Top.Value;
        var oldWidth = value.OldWidth;
        var oldHeight = value.OldHeight;

        switch (value.Type)
        {
            case TransformType.Move:
                Left.Value += value.LeftChange;
                Top.Value += value.TopChange;
                break;
            case TransformType.Resize:
                Left.Value = (Left.Value - value.GroupLeftTop.X) * ((oldWidth + value.WidthChange) / oldWidth) +
                             value.GroupLeftTop.X;
                Top.Value = (Top.Value - value.GroupLeftTop.Y) * ((oldHeight + value.HeightChange) / oldHeight) +
                            value.GroupLeftTop.Y;
                Width.Value = (oldWidth + value.WidthChange) / oldWidth * Width.Value;
                Height.Value = (oldHeight + value.HeightChange) / oldHeight * Height.Value;
                break;
            case TransformType.Rotate:
                var diffAngle = value.RotateAngleChange;
                RotationAngle.Value += diffAngle; //for only calculate rotation angle sum
                var matrix = Matrix.Value;
                matrix.RotateAt(diffAngle, value.GroupCenter.X - Left.Value - Width.Value / 2,
                    value.GroupCenter.Y - Top.Value - Height.Value / 2);
                Left.Value += matrix.OffsetX;
                Top.Value += matrix.OffsetY;
                matrix.OffsetX = 0;
                matrix.OffsetY = 0;
                Matrix.Value = matrix;
                break;
        }
    }

    public override IDisposable BeginMonitor(Action action)
    {
        var compositeDisposable = new CompositeDisposable();
        base.BeginMonitor(action).AddTo(compositeDisposable);
        Rect.AsObservable().Subscribe(_ => action()).AddTo(compositeDisposable);
        RotationAngle.Subscribe(_ => action()).AddTo(compositeDisposable);
        return compositeDisposable;
    }

    #region IDisposable

    public override void Dispose()
    {
        Width.Dispose();
        Height.Dispose();
        Size.Dispose();
        SizeIncludeFrame.Dispose();
        Pool.Dispose();
        Left.Dispose();
        Top.Dispose();
        Right.Dispose();
        Bottom.Dispose();
        Rect.Dispose();
        CenterX.Dispose();
        CenterY.Dispose();
        CenterPoint.Dispose();
        MarginLeftTop.Dispose();
        MarginLeftBottom.Dispose();
        MarginRightTop.Dispose();
        MarginRightBottom.Dispose();
        MarginLeft.Dispose();
        MarginTop.Dispose();
        MarginRight.Dispose();
        MarginBottom.Dispose();
        TransformNortification.Dispose();
        SnapObjs.Dispose();
        base.Dispose();
    }

    #endregion
}