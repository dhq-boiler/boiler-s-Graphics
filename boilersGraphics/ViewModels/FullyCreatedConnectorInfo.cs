using boilersGraphics.Controls;
using System;

namespace boilersGraphics.ViewModels
{
    [Obsolete]
    public class FullyCreatedConnectorInfo : ConnectorInfoBase
    {
        private bool _ShowConnectors = false;

        public FullyCreatedConnectorInfo(DesignerItemViewModelBase dataItem, ConnectorOrientation orientation, double degree)
            : base(orientation)
        {
            this.DataItem = dataItem;
            Degree = degree;
        }


        public DesignerItemViewModelBase DataItem { get; private set; }

        public double Degree { get; private set; }

        public bool ShowConnectors
        {
            get { return _ShowConnectors; }
            set { SetProperty(ref _ShowConnectors, value); }
        }
    }
}
