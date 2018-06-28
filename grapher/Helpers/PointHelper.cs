using grapher.Controls;
using grapher.ViewModels;
using System;
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
                    point = new Point(connector.DataItem.Left.Value + (connector.DataItem.Width.Value / 2), connector.DataItem.Top.Value - (ConnectorInfoBase.ConnectorHeight));
                    break;
                case ConnectorOrientation.Bottom:
                    point = new Point(connector.DataItem.Left.Value + (connector.DataItem.Width.Value / 2), (connector.DataItem.Top.Value + connector.DataItem.Height.Value) + (ConnectorInfoBase.ConnectorHeight / 2));
                    break;
                case ConnectorOrientation.Right:
                    point = new Point(connector.DataItem.Left.Value + connector.DataItem.Width.Value + (ConnectorInfoBase.ConnectorWidth), connector.DataItem.Top.Value + (connector.DataItem.Height.Value / 2));
                    break;
                case ConnectorOrientation.Left:
                    point = new Point(connector.DataItem.Left.Value - ConnectorInfoBase.ConnectorWidth, connector.DataItem.Top.Value + (connector.DataItem.Height.Value / 2));
                    break;
            }

            var centerPoint = connector.DataItem.CenterPoint.Value;
            var rotateAngle = connector.DataItem.RotateAngle.Value;
            var initialDegree = connector.Degree;
            var rad = (rotateAngle + initialDegree) * Math.PI / 180d;
            var z1 = point.X - centerPoint.X;
            var z2 = point.Y - centerPoint.Y;

            point.X = centerPoint.X + Math.Sqrt(Math.Pow(z1, 2) + Math.Pow(z2, 2)) * Math.Cos(rad);
            point.Y = centerPoint.Y + Math.Sqrt(Math.Pow(z1, 2) + Math.Pow(z2, 2)) * Math.Sin(rad);

            return point;
        }
    }
}
