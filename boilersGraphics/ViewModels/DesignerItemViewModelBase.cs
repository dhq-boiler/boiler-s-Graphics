using boilersGraphics.Controls;
using boilersGraphics.Helpers;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media;

namespace boilersGraphics.ViewModels
{
    public abstract class DesignerItemViewModelBase : SelectableDesignerItemViewModelBase, IObservable<TransformNotification>, ICloneable
    {
        private bool _showConnectors = false;

        private double _MinWidth;
        private double _MinHeight;
        private Color _EdgeColor;
        private Color _FillColor;
        private double _EdgeThickness;
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

        public Color EdgeColor
        {
            get { return _EdgeColor; }
            set { SetProperty(ref _EdgeColor, value); }
        }

        public double EdgeThickness
        {
            get { return _EdgeThickness; }
            set { SetProperty(ref _EdgeThickness, value); }
        }

        public Color FillColor
        {
            get { return _FillColor; }
            set { SetProperty(ref _FillColor, value); }
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

        public ReactiveProperty<double> Width { get; } = new ReactiveProperty<double>(mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe);

        public ReactiveProperty<double> Height { get; } = new ReactiveProperty<double>(mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe);

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

        public ReactiveProperty<double> Left { get; } = new ReactiveProperty<double>(mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe);

        public ReactiveProperty<double> Top { get; } = new ReactiveProperty<double>(mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe);

        public ReadOnlyReactiveProperty<double> Right { get; private set; }

        public ReadOnlyReactiveProperty<double> Bottom { get; private set; }

        public ReactiveProperty<Point> CenterPoint { get; } = new ReactiveProperty<Point>();

        public ReactiveProperty<Point> RotatedCenterPoint { get; } = new ReactiveProperty<Point>();

        private void UpdateCenterPoint()
        {
            var leftTop = new Point(Left.Value, Top.Value);
            var center = new Point(leftTop.X + Width.Value * 0.5, leftTop.Y + Height.Value * 0.5);
            CenterPoint.Value = center;
        }

        private void Init()
        {
            MinWidth = 0;
            MinHeight = 0;

            Left
                .Zip(Left.Skip(1), (Old, New) => new { OldItem = Old, NewItem = New})
                .Subscribe(x => UpdateTransform(nameof(Left), x.OldItem, x.NewItem))
                .AddTo(_CompositeDisposable);
            Top
                .Zip(Left.Skip(1), (Old, New) => new { OldItem = Old, NewItem = New })
                .Subscribe(x => UpdateTransform(nameof(Top), x.OldItem, x.NewItem))
                .AddTo(_CompositeDisposable);
            Width
                .Zip(Left.Skip(1), (Old, New) => new { OldItem = Old, NewItem = New })
                .Subscribe(x => UpdateTransform(nameof(Width), x.OldItem, x.NewItem))
                .AddTo(_CompositeDisposable);
            Height
                .Zip(Left.Skip(1), (Old, New) => new { OldItem = Old, NewItem = New })
                .Subscribe(x => UpdateTransform(nameof(Height), x.OldItem, x.NewItem))
                .AddTo(_CompositeDisposable);
            RotationAngle
                .Zip(Left.Skip(1), (Old, New) => new { OldItem = Old, NewItem = New })
                .Subscribe(x => UpdateTransform(nameof(RotationAngle), x.OldItem, x.NewItem))
                .AddTo(_CompositeDisposable);
            Matrix
                .Zip(Left.Skip(1), (Old, New) => new { OldItem = Old, NewItem = New })
                .Subscribe(x => UpdateTransform(nameof(Matrix), x.OldItem, x.NewItem))
                .AddTo(_CompositeDisposable);
            Right = Left.Select(x => x + Width.Value)
                        .ToReadOnlyReactiveProperty();
            Bottom = Top.Select(x => x + Height.Value)
                        .ToReadOnlyReactiveProperty();

            Matrix.Value = new Matrix();
        }

        public IDisposable Connect(ConnectorBaseViewModel connector)
        {
            var disposable = Subscribe(connector);
            _CompositeDisposable.Add(disposable);
            return disposable;
        }

        public void UpdateTransform(string propertyName, object oldValue, object newValue)
        {
            UpdateCenterPoint();
            TransformObserversOnNext(propertyName, oldValue, newValue);
        }

        public void TransformObserversOnNext(string propertyName, object oldValue, object newValue)
        {
            _observers.ForEach(x => x.OnNext(new TransformNotification() 
            {
                Sender = this,
                PropertyName = propertyName,
                OldValue = oldValue,
                NewValue = newValue
            }));
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

        #region IObservable<TransformNotification>

        private List<IObserver<TransformNotification>> _observers = new List<IObserver<TransformNotification>>();

        public IDisposable Subscribe(IObserver<TransformNotification> observer)
        {
            _observers.Add(observer);
            return new DesignerItemViewModelBaseDisposable(this, observer);
        }

        public class DesignerItemViewModelBaseDisposable : IDisposable
        {
            private DesignerItemViewModelBase _obj;
            private IObserver<TransformNotification> _observer;
            public DesignerItemViewModelBaseDisposable(DesignerItemViewModelBase obj, IObserver<TransformNotification> observer)
            {
                _obj = obj;
                _observer = observer;
            }

            public void Dispose()
            {
                _obj._observers.Remove(_observer);
            }
        }

        #endregion //IObservable<TransformNotification>
    }
}
