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
        private double left;
        private double top;
        private bool showConnectors = false;
        private List<FullyCreatedConnectorInfo> connectors = new List<FullyCreatedConnectorInfo>();

        private static double itemWidth = 65;
        private static double itemHeight = 65;
        private double _Left;
        private double _Top;

        public DesignerItemViewModelBase(int id, IDiagramViewModel parent, double left, double top) : base(id, parent)
        {
            this.left = left;
            this.top = top;
            Init();
        }

        public DesignerItemViewModelBase() : base()
        {
            Init();
        }


        public FullyCreatedConnectorInfo TopConnector
        {
            get { return connectors[0]; }
        }


        public FullyCreatedConnectorInfo BottomConnector
        {
            get { return connectors[1]; }
        }


        public FullyCreatedConnectorInfo LeftConnector
        {
            get { return connectors[2]; }
        }


        public FullyCreatedConnectorInfo RightConnector
        {
            get { return connectors[3]; }
        }



        public static double ItemWidth
        {
            get { return itemWidth; }
        }

        public static double ItemHeight
        {
            get { return itemHeight; }
        }

        public bool ShowConnectors
        {
            get
            {
                return showConnectors;
            }
            set
            {
                if (showConnectors != value)
                {
                    showConnectors = value;
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
            connectors.Add(new FullyCreatedConnectorInfo(this, ConnectorOrientation.Top));
            connectors.Add(new FullyCreatedConnectorInfo(this, ConnectorOrientation.Bottom));
            connectors.Add(new FullyCreatedConnectorInfo(this, ConnectorOrientation.Left));
            connectors.Add(new FullyCreatedConnectorInfo(this, ConnectorOrientation.Right));
        }
    }
}
