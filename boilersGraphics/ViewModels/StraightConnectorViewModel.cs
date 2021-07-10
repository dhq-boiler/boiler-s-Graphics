
using System.Collections.Generic;
using System.Windows;

namespace boilersGraphics.ViewModels
{
    public class StraightConnectorViewModel : ConnectorBaseViewModel
    {
        public StraightConnectorViewModel(int id, IDiagramViewModel parent)
            : base(id, parent)
        { }

        public StraightConnectorViewModel()
            : base()
        { }

        public StraightConnectorViewModel(Point p1, Point p2)
            : base()
        {
            Points.Add(p1);
            Points.Add(p2);
        }

        #region IClonable

        public override object Clone()
        {
            var clone = new StraightConnectorViewModel(Points[0], Points[1]);
            clone.Owner = Owner;
            clone.EdgeColor = EdgeColor;
            clone.Points[0] = Points[0];
            clone.Points[1] = Points[1];

            return clone;
        }

        #endregion //IClonable
    }
}
