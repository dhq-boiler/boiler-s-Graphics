using grapher.Controls;
using grapher.Helpers;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace grapher.ViewModels
{
    public abstract class DesignerItemViewModelBase : SelectableDesignerItemViewModelBase, IObservable<TransformNotification>
    {
        private bool _showConnectors = false;
        private List<FullyCreatedConnectorInfo> _connectors = new List<FullyCreatedConnectorInfo>();

        private double _MinWidth;
        private double _MinHeight;
        private Color _EdgeColor;
        private Color _FillColor;
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

        public Color FillColor
        {
            get { return _FillColor; }
            set { SetProperty(ref _FillColor, value); }
        }

        public FullyCreatedConnectorInfo TopConnector
        {
            get { return _connectors[0]; }
        }


        public FullyCreatedConnectorInfo BottomConnector
        {
            get { return _connectors[1]; }
        }


        public FullyCreatedConnectorInfo LeftConnector
        {
            get { return _connectors[2]; }
        }


        public FullyCreatedConnectorInfo RightConnector
        {
            get { return _connectors[3]; }
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

        public ReactiveProperty<double> Width { get; } = new ReactiveProperty<double>();

        public ReactiveProperty<double> Height { get; } = new ReactiveProperty<double>();

        public ReactiveProperty<double> RotateAngle { get; } = new ReactiveProperty<double>();

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
                    TopConnector.ShowConnectors = value;
                    BottomConnector.ShowConnectors = value;
                    RightConnector.ShowConnectors = value;
                    LeftConnector.ShowConnectors = value;
                    RaisePropertyChanged("ShowConnectors");
                }
            }
        }

        public ReactiveProperty<double> Left { get; } = new ReactiveProperty<double>();

        public ReactiveProperty<double> Top { get; } = new ReactiveProperty<double>();

        public ReactiveProperty<Point> CenterPoint { get; } = new ReactiveProperty<Point>();

        private void UpdateCenterPoint()
        {
            var leftTop = new Point(Left.Value, Top.Value);
            var center = new Point(leftTop.X + Width.Value * 0.5, leftTop.Y + Height.Value * 0.5);
            CenterPoint.Value = center;
        }

        private void Init()
        {
            _connectors.Add(new FullyCreatedConnectorInfo(this, ConnectorOrientation.Top, 270));
            _connectors.Add(new FullyCreatedConnectorInfo(this, ConnectorOrientation.Bottom, 90));
            _connectors.Add(new FullyCreatedConnectorInfo(this, ConnectorOrientation.Left, 180));
            _connectors.Add(new FullyCreatedConnectorInfo(this, ConnectorOrientation.Right, 0));

            MinWidth = 0;
            MinHeight = 0;

            Left.Subscribe(_ => UpdateTransform());
            Top.Subscribe(_ => UpdateTransform());
            Width.Subscribe(_ => UpdateTransform());
            Height.Subscribe(_ => UpdateTransform());
            RotateAngle.Subscribe(_ => UpdateTransform());
        }

        public void UpdateTransform()
        {
            UpdateCenterPoint();
            TransformObserversOnNext();
        }

        public void TransformObserversOnNext()
        {
            _observers.ForEach(x => x.OnNext(new TransformNotification()));
        }

        public override void OnNext(GroupTransformNotification value)
        {
            Left.Value += value.LeftChange;
            Top.Value += value.TopChange;
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
