using grapher.Controls;
using grapher.Helpers;
using System.Windows;

namespace grapher.ViewModels
{
    public class PolygonalConnectorViewModel : ConnectorBaseViewModel
    {
        public PolygonalConnectorViewModel(int id, IDiagramViewModel parent,
            FullyCreatedConnectorInfo sourceConnectorInfo, FullyCreatedConnectorInfo sinkConnectorInfo)
            : base(id, parent, sourceConnectorInfo, sinkConnectorInfo)
        { }

        public PolygonalConnectorViewModel(FullyCreatedConnectorInfo sourceConnectorInfo, ConnectorInfoBase sinkConnectorInfo)
            : base(sourceConnectorInfo, sinkConnectorInfo)
        { }

        public static IPathFinder PathFinder { get; set; }

        protected override void SetConnectionPoints(ConnectorInfo sourceInfo, ConnectorInfo sinkInfo, bool showLastLine)
        {
            ConnectionPoints = PathFinder.GetConnectionLine(sourceInfo, sinkInfo, showLastLine);
        }

        protected override void SetConnectionPoints(ConnectorInfo source, Point sinkPoint, ConnectorOrientation preferredOrientation)
        {
            ConnectionPoints = PathFinder.GetConnectionLine(source, sinkPoint, preferredOrientation);
        }

        protected override void InitPathFinder()
        {
            PathFinder = new OrthogonalPathFinder();
        }
    }
}
