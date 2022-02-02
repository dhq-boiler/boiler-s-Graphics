using boilersGraphics.Helpers;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media;

namespace boilersGraphics.ViewModels
{
    public abstract class DesignerItemViewModelBase : SelectableDesignerItemViewModelBase, ICloneable
    {
        private bool _showConnectors = false;

        private double _MinWidth;
        private double _MinHeight;
        public static readonly double DefaultWidth = 65d;
        public static readonly double DefaultHeight = 65d;

        public DesignerItemViewModelBase(int id, IDiagramViewModel parent, double left, double top) : base(id, parent)
        {
            Left.Value = left;
            Top.Value = top;
            Init();
        }

        public DesignerItemViewModelBase() : base()
        {
            Init();
        }

        public double MinWidth
        {
            get { return _MinWidth; }
            set { SetProperty(ref _MinWidth, value); }
        }

        public double MinHeight
        {
            get { return _MinHeight; }
            set { SetProperty(ref _MinHeight, value); }
        }

        public ReactivePropertySlim<double> Width { get; } = new ReactivePropertySlim<double>(mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe | ReactivePropertyMode.DistinctUntilChanged);

        public ReactivePropertySlim<double> Height { get; } = new ReactivePropertySlim<double>(mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe | ReactivePropertyMode.DistinctUntilChanged);

        public bool ShowConnectors
        {
            get
            {
                return _showConnectors;
            }
            set
            {
                if (_showConnectors != value)
                {
                    _showConnectors = value;
                    RaisePropertyChanged("ShowConnectors");
                }
            }
        }

        public ReactivePropertySlim<double> Left { get; } = new ReactivePropertySlim<double>(mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe | ReactivePropertyMode.DistinctUntilChanged);

        public ReactivePropertySlim<double> Top { get; } = new ReactivePropertySlim<double>(mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe | ReactivePropertyMode.DistinctUntilChanged);

        public ReadOnlyReactivePropertySlim<double> Right { get; private set; }

        public ReadOnlyReactivePropertySlim<double> Bottom { get; private set; }

        public ReactivePropertySlim<PathGeometry> RotatePathGeometry { get; } = new ReactivePropertySlim<PathGeometry>();

        public ReactivePropertySlim<double> CenterX { get; } = new ReactivePropertySlim<double>();
        public ReactivePropertySlim<double> CenterY { get; } = new ReactivePropertySlim<double>();

        public ReactiveProperty<Point> CenterPoint { get; private set; }

        public ReactivePropertySlim<TransformNotification> TransformNortification { get; } = new ReactivePropertySlim<TransformNotification>(mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe | ReactivePropertyMode.DistinctUntilChanged);

        internal SnapPointPosition snapPointPosition { get; set; }

        public List<IDisposable> SnapObjs { get; set; } = new List<IDisposable>();

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
                .Subscribe(x => UpdateTransform(nameof(Left), x.OldItem, x.NewItem))
                .AddTo(_CompositeDisposable);
            Top
                .Zip(Top.Skip(1), (Old, New) => new { OldItem = Old, NewItem = New })
                .Subscribe(x => UpdateTransform(nameof(Top), x.OldItem, x.NewItem))
                .AddTo(_CompositeDisposable);
            Width
                .Zip(Width.Skip(1), (Old, New) => new { OldItem = Old, NewItem = New })
                .Subscribe(x => UpdateTransform(nameof(Width), x.OldItem, x.NewItem))
                .AddTo(_CompositeDisposable);
            Height
                .Zip(Height.Skip(1), (Old, New) => new { OldItem = Old, NewItem = New })
                .Subscribe(x => UpdateTransform(nameof(Height), x.OldItem, x.NewItem))
                .AddTo(_CompositeDisposable);
            RotationAngle
                .Zip(RotationAngle.Skip(1), (Old, New) => new { OldItem = Old, NewItem = New })
                .Subscribe(x =>
                {
                    UpdateTransform(nameof(RotationAngle), x.OldItem, x.NewItem);
                    UpdateMatrix(x.OldItem, x.NewItem);
                })
                .AddTo(_CompositeDisposable);
            Matrix
                .Zip(Matrix.Skip(1), (Old, New) => new { OldItem = Old, NewItem = New })
                .Subscribe(x => UpdateTransform(nameof(Matrix), x.OldItem, x.NewItem))
                .AddTo(_CompositeDisposable);
            Right = Left.CombineLatest(Width, (a, b) => a + b)
                        .ToReadOnlyReactivePropertySlim();
            Bottom = Top.CombineLatest(Height, (a, b) => a + b)
                        .ToReadOnlyReactivePropertySlim();
            CenterX.Subscribe(x => UpdateLeft(x))
                       .AddTo(_CompositeDisposable);
            CenterY.Subscribe(x => UpdateTop(x))
                       .AddTo(_CompositeDisposable);
            CenterPoint = CenterX.CombineLatest(CenterY, (x, y) => new Point(x, y))
                                 .ToReactiveProperty();

            Matrix.Value = new Matrix();
        }

        private void UpdateMatrix(double oldAngle, double newAngle)
        {
            var targetMatrix = Matrix.Value;
            targetMatrix.RotateAt(newAngle - oldAngle, 0, 0);
            Matrix.Value = targetMatrix;
        }

        private void UpdateLeft(double value)
        {
            Left.Value = value - Width.Value / 2;
        }

        private void UpdateTop(double value)
        {
            Top.Value = value - Height.Value / 2;
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
            TransformObserversOnNext(propertyName, oldValue, newValue);
            UpdatePathGeometryInCase(propertyName);
        }

        private void UpdatePathGeometryInCase(string propertyName)
        {
            switch (propertyName)
            {
                case "Left":
                case "Top":
                case "Width":
                case "Height":
                case "RotationAngle":
                case "Matrix":
                    UpdatePathGeometryIfEnable();
                    break;
                default:
                    break;
            }
        }

        public void UpdatePathGeometryIfEnable()
        {
            if (EnablePathGeometryUpdate.Value)
            {
                if (RotationAngle.Value == 0)
                {
                    PathGeometry.Value = CreateGeometry();
                }
                else
                {
                    RotatePathGeometry.Value = CreateGeometry(RotationAngle.Value);
                }
            }
        }

        public abstract PathGeometry CreateGeometry();

        public abstract PathGeometry CreateGeometry(double angle);

        public void TransformObserversOnNext(string propertyName, object oldValue, object newValue)
        {
            var tn = new TransformNotification()
            {
                Sender = this,
                PropertyName = propertyName,
                OldValue = oldValue,
                NewValue = newValue,
                SnapPointPosition = snapPointPosition,
            };
            this.TransformNortification.Value = tn;
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
                    Left.Value = (Left.Value - value.GroupLeftTop.X) * ((oldWidth + value.WidthChange) / (oldWidth)) + value.GroupLeftTop.X;
                    Top.Value = (Top.Value - value.GroupLeftTop.Y) * ((oldHeight + value.HeightChange) / (oldHeight)) + value.GroupLeftTop.Y;
                    Width.Value = (oldWidth + value.WidthChange) / oldWidth * Width.Value;
                    Height.Value = (oldHeight + value.HeightChange) / oldHeight * Height.Value;
                    break;
                case TransformType.Rotate:
                    var diffAngle = value.RotateAngleChange;
                    RotationAngle.Value += diffAngle; //for only calculate rotation angle sum
                    var matrix = Matrix.Value;
                    matrix.RotateAt(diffAngle, value.GroupCenter.X - Left.Value - Width.Value / 2, value.GroupCenter.Y - Top.Value - Height.Value / 2);
                    Left.Value += matrix.OffsetX;
                    Top.Value += matrix.OffsetY;
                    matrix.OffsetX = 0;
                    matrix.OffsetY = 0;
                    Matrix.Value = matrix;
                    break;
            }
        }
    }
}
