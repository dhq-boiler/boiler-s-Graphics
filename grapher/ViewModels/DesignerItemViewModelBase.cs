using grapher.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace grapher.ViewModels
{
    public abstract class DesignerItemViewModelBase : SelectableDesignerItemViewModelBase
    {
        private double _left;
        private double _top;
        private bool _showConnectors = false;
        private List<FullyCreatedConnectorInfo> _connectors = new List<FullyCreatedConnectorInfo>();

        private double _Left;
        private double _Top;
        private double _width;
        private double _height;
        private double _MinWidth;
        private double _MinHeight;
        public static readonly double DefaultWidth = 65d;
        public static readonly double DefaultHeight = 65d;

        public DesignerItemViewModelBase(int id, IDiagramViewModel parent, double left, double top) : base(id, parent)
        {
            _left = left;
            _top = top;
            Init();
        }

        public DesignerItemViewModelBase() : base()
        {
            Init();
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

        public double Width
        {
            get { return _width; }
            set { SetProperty(ref _width, value); }
        }

        public double Height
        {
            get { return _height; }
            set { SetProperty(ref _height, value); }
        }

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


        public double Left
        {
            get { return _Left; }
            set { SetProperty(ref _Left, value); }
        }

        public double Top
        {
            get { return _Top; }
            set { SetProperty(ref _Top, value); }
        }

        private void Init()
        {
            _connectors.Add(new FullyCreatedConnectorInfo(this, ConnectorOrientation.Top));
            _connectors.Add(new FullyCreatedConnectorInfo(this, ConnectorOrientation.Bottom));
            _connectors.Add(new FullyCreatedConnectorInfo(this, ConnectorOrientation.Left));
            _connectors.Add(new FullyCreatedConnectorInfo(this, ConnectorOrientation.Right));

            MinWidth = 0;
            MinHeight = 0;
        }
    }
}
