using boilersGraphics.Controls;
using boilersGraphics.Helpers;
using System.Windows;

namespace boilersGraphics.ViewModels
{
    public class PolygonalConnectorViewModel : ConnectorBaseViewModel
    {
        public PolygonalConnectorViewModel(int id, IDiagramViewModel parent)
            : base(id, parent)
        { }

        public PolygonalConnectorViewModel()
            : base()
        { }

        public static IPathFinder PathFinder { get; set; }

        protected override void InitPathFinder()
        {
            PathFinder = new OrthogonalPathFinder();
        }

        #region IClonable

        public override object Clone()
        {
            var clone = new PolygonalConnectorViewModel();
            clone.Owner = Owner;
            clone.EdgeColor = EdgeColor;
            clone.Points[0] = Points[0];
            clone.Points[1] = Points[1];

            return clone;
        }

        #endregion //IClonable
    }
}
