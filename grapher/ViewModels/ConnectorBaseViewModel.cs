using grapher.Controls;
using grapher.Helpers;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace grapher.ViewModels
{
    public abstract class ConnectorBaseViewModel : SelectableDesignerItemViewModelBase, IObserver<TransformNotification>, ICloneable
    {
        private ConnectorInfoBase _sourceConnectorInfo;
        private ConnectorInfoBase _sinkConnectorInfo;
        private Point _sourceB;
        private Point _sourceA;
        private List<Point> _connectionPoints;
        private Point _endPoint;
        private Rect _area;
        private bool _IsHitTestVisible;
        private Color _EdgeColor;
        private IDisposable _sourceConnectorDisconnecting;
        private IDisposable _sinkConnectorDisconnecting;

        public ConnectorBaseViewModel(int id, IDiagramViewModel parent,
            FullyCreatedConnectorInfo sourceConnectorInfo, FullyCreatedConnectorInfo sinkConnectorInfo) : base(id, parent)
        {
            Init(sourceConnectorInfo, sinkConnectorInfo);
        }

        public ConnectorBaseViewModel(ConnectorInfoBase sourceConnectorInfo, ConnectorInfoBase sinkConnectorInfo)
        {
            Init(sourceConnectorInfo, sinkConnectorInfo);
        }

        public Color EdgeColor
        {
            get { return _EdgeColor; }
            set { SetProperty(ref _EdgeColor, value); }
        }

        public bool IsHitTestVisible
        {
            get { return _IsHitTestVisible; }
            set { SetProperty(ref _IsHitTestVisible, value); }
        }

        public bool IsFullConnection
        {
            get { return _sinkConnectorInfo is FullyCreatedConnectorInfo; }
        }

        public Point SourceA
        {
            get
            {
                return _sourceA;
            }
            set
            {
                if (_sourceA != value)
                {
                    _sourceA = value;
                    UpdateArea();
                    RaisePropertyChanged("SourceA");
                }
            }
        }

        public Point SourceB
        {
            get
            {
                return _sourceB;
            }
            set
            {
                if (_sourceB != value)
                {
                    _sourceB = value;
                    UpdateArea();
                    RaisePropertyChanged("SourceB");
                }
            }
        }

        public List<Point> ConnectionPoints
        {
            get
            {
                return _connectionPoints;
            }
            protected set
            {
                if (_connectionPoints != value)
                {
                    _connectionPoints = value;
                    RaisePropertyChanged("ConnectionPoints");
                }
            }
        }

        public Point EndPoint
        {
            get
            {
                return _endPoint;
            }
            private set
            {
                if (_endPoint != value)
                {
                    _endPoint = value;
                    RaisePropertyChanged("EndPoint");
                }
            }
        }

        public Rect Area
        {
            get
            {
                return _area;
            }
            private set
            {
                if (_area != value)
                {
                    _area = value;
                    UpdateConnectionPoints();
                    RaisePropertyChanged("Area");
                }
            }
        }

        public ConnectorInfo ConnectorInfo(ConnectorOrientation orientation, double left, double top, Point position)
        {
            return new ConnectorInfo()
            {
                Orientation = orientation,
                DesignerItemSize = new Size(DesignerItemViewModelBase.DefaultWidth, DesignerItemViewModelBase.DefaultHeight),
                DesignerItemLeft = left,
                DesignerItemTop = top,
                Position = position
            };
        }

        public Guid SourceConnectedDataItemID { get; set; }

        public ConnectorInfoBase SourceConnectorInfo
        {
            get
            {
                return _sourceConnectorInfo;
            }
            set
            {
                if (_sourceConnectorInfo != value)
                {
                    _sourceConnectorInfo = value;
                    if (_sourceConnectorInfo is PartCreatedConnectionInfo)
                    {
                        SourceA = (_sourceConnectorInfo as PartCreatedConnectionInfo).CurrentLocation;
                    }
                    if (_sourceConnectorInfo is FullyCreatedConnectorInfo)
                    {
                        SourceA = PointHelper.GetPointForConnector(this.SourceConnectorInfo as FullyCreatedConnectorInfo);
                        _sourceConnectorDisconnecting?.Dispose();
                        _sourceConnectorDisconnecting = (_sourceConnectorInfo as FullyCreatedConnectorInfo).DataItem.Connect(this);
                        _sourceConnectorDisconnecting.AddTo(_CompositeDisposable);
                        SourceConnectedDataItemID = (_sourceConnectorInfo as FullyCreatedConnectorInfo).DataItem.ID;
                    }
                    RaisePropertyChanged("SourceConnectorInfo");
                }
            }
        }

        public Guid SinkConnectedDataItemID { get; set; }

        public ConnectorInfoBase SinkConnectorInfo
        {
            get
            {
                return _sinkConnectorInfo;
            }
            set
            {
                if (_sinkConnectorInfo != value)
                {
                    _sinkConnectorInfo = value;
                    if (SinkConnectorInfo is FullyCreatedConnectorInfo)
                    {
                        SourceB = PointHelper.GetPointForConnector((FullyCreatedConnectorInfo)SinkConnectorInfo);
                        _sinkConnectorDisconnecting?.Dispose();
                        _sinkConnectorDisconnecting = (_sinkConnectorInfo as FullyCreatedConnectorInfo).DataItem.Connect(this);
                        _sinkConnectorDisconnecting.AddTo(_CompositeDisposable);
                        SinkConnectedDataItemID = (_sinkConnectorInfo as FullyCreatedConnectorInfo).DataItem.ID;
                    }
                    else
                    {
                        SourceB = ((PartCreatedConnectionInfo)SinkConnectorInfo).CurrentLocation;
                    }
                    RaisePropertyChanged("SinkConnectorInfo");
                }
            }
        }

        private void UpdateArea()
        {
            Area = new Rect(SourceA, SourceB);
        }

        private void UpdateConnectionPoints()
        {
            ConnectionPoints = new List<Point>()
                                   {
                                       new Point( SourceA.X  <  SourceB.X ? 0d : Area.Width, SourceA.Y  <  SourceB.Y ? 0d : Area.Height ),
                                       new Point(SourceA.X  >  SourceB.X ? 0d : Area.Width, SourceA.Y  >  SourceB.Y ? 0d : Area.Height)
                                   };

            ConnectorInfo sourceInfo = ConnectorInfo(SourceConnectorInfo.Orientation,
                                            ConnectionPoints[0].X,
                                            ConnectionPoints[0].Y,
                                            ConnectionPoints[0]);

            if (IsFullConnection)
            {
                EndPoint = ConnectionPoints.Last();
                ConnectorInfo sinkInfo = ConnectorInfo(SinkConnectorInfo.Orientation,
                                  ConnectionPoints[1].X,
                                  ConnectionPoints[1].Y,
                                  ConnectionPoints[1]);

                SetConnectionPoints(sourceInfo, sinkInfo, true);
            }
            else
            {
                SetConnectionPoints(sourceInfo, ConnectionPoints[1], ConnectorOrientation.Left);
                EndPoint = ConnectionPoints.Last();
            }
        }

        protected virtual void SetConnectionPoints(ConnectorInfo sourceInfo, ConnectorInfo sinkInfo, bool showLastLine) { }

        protected virtual void SetConnectionPoints(ConnectorInfo source, Point sinkPoint, ConnectorOrientation preferredOrientation) { }

        private void Init(ConnectorInfoBase sourceConnectorInfo, ConnectorInfoBase sinkConnectorInfo)
        {
            IsHitTestVisible = true;
            InitPathFinder();
            if (sourceConnectorInfo is FullyCreatedConnectorInfo)
            {
                this.Owner = (sourceConnectorInfo as FullyCreatedConnectorInfo).DataItem.Owner;
            }
            this.SourceConnectorInfo = sourceConnectorInfo;
            this.SinkConnectorInfo = sinkConnectorInfo;
        }

        protected virtual void InitPathFinder() { }

        #region IObserver<TransformNotification>

        public void OnNext(TransformNotification value)
        {
            if (!value.Sender.IsSameGroup(this))
            {
                if (SourceConnectorInfo is FullyCreatedConnectorInfo sourceInfo && sourceInfo.DataItem == value.Sender)
                {
                    SourceA = PointHelper.GetPointForConnector(this.SourceConnectorInfo as FullyCreatedConnectorInfo);
                }
                if (this.SinkConnectorInfo is FullyCreatedConnectorInfo sinkInfo && sinkInfo.DataItem == value.Sender)
                {
                    SourceB = PointHelper.GetPointForConnector(this.SinkConnectorInfo as FullyCreatedConnectorInfo);
                }
            }
        }

        #endregion //IObserver<TransformNotification>

        #region IObserver<GroupTransformNotification>

        public override void OnNext(GroupTransformNotification value)
        {
            var oldWidth = value.OldWidth;
            var oldHeight = value.OldHeight;

            switch (value.Type)
            {
                case TransformType.Move:
                    var a = SourceA;
                    var b = SourceB;
                    a.X += value.LeftChange;
                    b.X += value.LeftChange;
                    a.Y += value.TopChange;
                    b.Y += value.TopChange;
                    SourceA = a;
                    SourceB = b;
                    break;
                case TransformType.Resize:
                    a = SourceA;
                    b = SourceB;
                    a.X = (a.X - value.GroupLeftTop.X) * ((oldWidth + value.WidthChange) / (oldWidth)) + value.GroupLeftTop.X;
                    b.X = (b.X - value.GroupLeftTop.X) * ((oldWidth + value.WidthChange) / (oldWidth)) + value.GroupLeftTop.X;
                    a.Y = (a.Y - value.GroupLeftTop.Y) * ((oldHeight + value.HeightChange) / (oldHeight)) + value.GroupLeftTop.Y;
                    b.Y = (b.Y - value.GroupLeftTop.Y) * ((oldHeight + value.HeightChange) / (oldHeight)) + value.GroupLeftTop.Y;
                    SourceA = a;
                    SourceB = b;
                    break;
                case TransformType.Rotate:
                    a = SourceA;
                    b = SourceB;
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
                    SourceA = newA;
                    SourceB = newB;
                    break;
            }

            if (SourceConnectorInfo is FullyCreatedConnectorInfo sourceInfo && !sourceInfo.DataItem.IsSameGroup(this))
            {
                SourceA = PointHelper.GetPointForConnector(sourceInfo);
            }
            if (SinkConnectorInfo is FullyCreatedConnectorInfo sinkInfo && !sinkInfo.DataItem.IsSameGroup(this))
            {
                SourceB = PointHelper.GetPointForConnector(sinkInfo);
            }
        }

        #endregion //IObserver<TransformNotification>
    }
}
