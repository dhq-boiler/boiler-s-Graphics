using grapher.Controls;
using grapher.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace grapher.ViewModels
{
    public class ConnectorBaseViewModel : SelectableDesignerItemViewModelBase, IObserver<TransformNotification>
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
                        _sourceConnectorDisconnecting = (_sourceConnectorInfo as FullyCreatedConnectorInfo).DataItem.Subscribe(this);
                    }
                    RaisePropertyChanged("SourceConnectorInfo");
                }
            }
        }

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
                        _sinkConnectorDisconnecting = (_sinkConnectorInfo as FullyCreatedConnectorInfo).DataItem.Subscribe(this);
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
            if (SourceConnectorInfo is FullyCreatedConnectorInfo)
            {
                SourceA = PointHelper.GetPointForConnector(this.SourceConnectorInfo as FullyCreatedConnectorInfo);
            }
            if (this.SinkConnectorInfo is FullyCreatedConnectorInfo)
            {
                SourceB = PointHelper.GetPointForConnector(this.SinkConnectorInfo as FullyCreatedConnectorInfo);
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
                    RotationAngle.Value += diffAngle; //for only calcurate rotation angle sum
                    var matrix = Matrix.Value;
                    matrix.RotateAt(diffAngle, value.GroupCenter.X - a.X / 2 - b.X / 2, value.GroupCenter.Y - a.Y / 2 - b.Y / 2);
                    a.X += matrix.OffsetX;
                    b.X += matrix.OffsetX;
                    a.Y += matrix.OffsetY;
                    b.Y += matrix.OffsetY;
                    matrix.OffsetX = 0;
                    matrix.OffsetY = 0;
                    SourceA = a;
                    SourceB = b;
                    Matrix.Value = matrix;
                    break;
            }
        }

        #endregion //IObserver<TransformNotification>
    }
}
