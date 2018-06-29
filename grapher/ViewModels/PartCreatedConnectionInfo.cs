using grapher.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace grapher.ViewModels
{
    public class PartCreatedConnectionInfo : ConnectorInfoBase
    {
        public Point CurrentLocation { get; set; }

        public PartCreatedConnectionInfo(Point currentLocation) : base(ConnectorOrientation.None)
        {
            this.CurrentLocation = currentLocation;
        }
    }
}
