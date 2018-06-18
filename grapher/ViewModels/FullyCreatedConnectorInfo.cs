using grapher.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace grapher.ViewModels
{
    public class FullyCreatedConnectorInfo : ConnectorInfoBase
    {
        private bool _ShowConnectors = false;

        public FullyCreatedConnectorInfo(DesignerItemViewModelBase dataItem, ConnectorOrientation orientation)
            : base(orientation)
        {
            this.DataItem = dataItem;
        }


        public DesignerItemViewModelBase DataItem { get; private set; }

        public bool ShowConnectors
        {
            get { return _ShowConnectors; }
            set { SetProperty(ref _ShowConnectors, value); }
        }
    }
}
