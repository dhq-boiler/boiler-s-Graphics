using grapher.Controls;
using grapher.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace grapher.Helpers
{
    public class PointHelper
    {
        public static Point GetPointForConnector(FullyCreatedConnectorInfo connector)
        {
            Point point = new Point();

            switch (connector.Orientation)
            {
                case ConnectorOrientation.Top:
                    point = new Point(connector.DataItem.Left + (connector.DataItem.Width / 2), connector.DataItem.Top - (ConnectorInfoBase.ConnectorHeight));
                    break;
                case ConnectorOrientation.Bottom:
                    point = new Point(connector.DataItem.Left + (connector.DataItem.Width / 2), (connector.DataItem.Top + connector.DataItem.Height) + (ConnectorInfoBase.ConnectorHeight / 2));
                    break;
                case ConnectorOrientation.Right:
                    point = new Point(connector.DataItem.Left + connector.DataItem.Width + (ConnectorInfoBase.ConnectorWidth), connector.DataItem.Top + (connector.DataItem.Height / 2));
                    break;
                case ConnectorOrientation.Left:
                    point = new Point(connector.DataItem.Left - ConnectorInfoBase.ConnectorWidth, connector.DataItem.Top + (connector.DataItem.Height / 2));
                    break;
            }

            return point;
        }
    }
}
