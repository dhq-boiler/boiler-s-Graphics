using grapher.Controls;
using grapher.Helpers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace grapher.ViewModels
{
    public class ConnectorBaseViewModel : SelectableDesignerItemViewModelBase
    {
        private ConnectorInfoBase sourceConnectorInfo;
        private ConnectorInfoBase sinkConnectorInfo;
        private Point sourceB;
        private Point sourceA;
        private List<Point> connectionPoints;
        private Point endPoint;
        private Rect area;
        private bool _IsHitTestVisible;

        public ConnectorBaseViewModel(int id, IDiagramViewModel parent,
            FullyCreatedConnectorInfo sourceConnectorInfo, FullyCreatedConnectorInfo sinkConnectorInfo) : base(id, parent)
        {
            Init(sourceConnectorInfo, sinkConnectorInfo);
        }

        public ConnectorBaseViewModel(ConnectorInfoBase sourceConnectorInfo, ConnectorInfoBase sinkConnectorInfo)
        {
            Init(sourceConnectorInfo, sinkConnectorInfo);
        }

        public bool IsHitTestVisible
        {
            get { return _IsHitTestVisible; }
            set { SetProperty(ref _IsHitTestVisible, value); }
        }

        public bool IsFullConnection
        {
            get { return sinkConnectorInfo is FullyCreatedConnectorInfo; }
        }

        public Point SourceA
        {
            get
            {
                return sourceA;
            }
            set
            {
                if (sourceA != value)
                {
                    sourceA = value;
                    UpdateArea();
                    RaisePropertyChanged("SourceA");
                }
            }
        }

        public Point SourceB
        {
            get
            {
                return sourceB;
            }
            set
            {
                if (sourceB != value)
                {
                    sourceB = value;
                    UpdateArea();
                    RaisePropertyChanged("SourceB");
                }
            }
        }

        public List<Point> ConnectionPoints
        {
            get
            {
                return connectionPoints;
            }
            protected set
            {
                if (connectionPoints != value)
                {
                    connectionPoints = value;
                    RaisePropertyChanged("ConnectionPoints");
                }
            }
        }

        public Point EndPoint
        {
            get
            {
                return endPoint;
            }
            private set
            {
                if (endPoint != value)
                {
                    endPoint = value;
                    RaisePropertyChanged("EndPoint");
                }
            }
        }

        public Rect Area
        {
            get
            {
                return area;
            }
            private set
            {
                if (area != value)
                {
                    area = value;
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
                return sourceConnectorInfo;
            }
            set
            {
                if (sourceConnectorInfo != value)
                {

                    sourceConnectorInfo = value;
                    if (sourceConnectorInfo is PartCreatedConnectionInfo)
                    {
                        SourceA = (sourceConnectorInfo as PartCreatedConnectionInfo).CurrentLocation;
                    }
                    if (sourceConnectorInfo is FullyCreatedConnectorInfo)
                    {
                        SourceA = PointHelper.GetPointForConnector(this.SourceConnectorInfo as FullyCreatedConnectorInfo);
                    }
                    RaisePropertyChanged("SourceConnectorInfo");
                    if (sourceConnectorInfo is FullyCreatedConnectorInfo)
                    {
                        ((sourceConnectorInfo as FullyCreatedConnectorInfo).DataItem as INotifyPropertyChanged).PropertyChanged += new WeakINPCEventHandler(ConnectorViewModel_PropertyChanged).Handler;
                    }
                }
            }
        }

        public ConnectorInfoBase SinkConnectorInfo
        {
            get
            {
                return sinkConnectorInfo;
            }
            set
            {
                if (sinkConnectorInfo != value)
                {

                    sinkConnectorInfo = value;
                    if (SinkConnectorInfo is FullyCreatedConnectorInfo)
                    {
                        SourceB = PointHelper.GetPointForConnector((FullyCreatedConnectorInfo)SinkConnectorInfo);
                        (((FullyCreatedConnectorInfo)sinkConnectorInfo).DataItem as INotifyPropertyChanged).PropertyChanged += new WeakINPCEventHandler(ConnectorViewModel_PropertyChanged).Handler;
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

        private void ConnectorViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Left":
                case "Top":
                case "Width":
                case "Height":
                    if (SourceConnectorInfo is FullyCreatedConnectorInfo)
                    {
                        SourceA = PointHelper.GetPointForConnector(this.SourceConnectorInfo as FullyCreatedConnectorInfo);
                    }
                    if (this.SinkConnectorInfo is FullyCreatedConnectorInfo)
                    {
                        SourceB = PointHelper.GetPointForConnector(this.SinkConnectorInfo as FullyCreatedConnectorInfo);
                    }
                    break;
            }
        }

        private void Init(ConnectorInfoBase sourceConnectorInfo, ConnectorInfoBase sinkConnectorInfo)
        {
            IsHitTestVisible = true;
            InitPathFinder();
            if (sourceConnectorInfo is FullyCreatedConnectorInfo)
            {
                this.Parent = (sourceConnectorInfo as FullyCreatedConnectorInfo).DataItem.Parent;
            }
            this.SourceConnectorInfo = sourceConnectorInfo;
            this.SinkConnectorInfo = sinkConnectorInfo;
        }

        protected virtual void InitPathFinder() { }
    }
}
