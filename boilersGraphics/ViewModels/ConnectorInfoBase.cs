using boilersGraphics.Controls;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace boilersGraphics.ViewModels
{
    public abstract class ConnectorInfoBase : BindableBase
    {
        private static double s_connectorWidth = 8;
        private static double s_connectorHeight = 8;

        public ConnectorInfoBase(ConnectorOrientation orientation)
        {
            this.Orientation = orientation;
        }

        public ConnectorOrientation Orientation { get; private set; }

        public static double ConnectorWidth
        {
            get { return s_connectorWidth; }
        }

        public static double ConnectorHeight
        {
            get { return s_connectorHeight; }
        }
    }
}
