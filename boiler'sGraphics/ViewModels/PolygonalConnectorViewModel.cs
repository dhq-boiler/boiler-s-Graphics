using boiler_sGraphics.Controls;
using boiler_sGraphics.Helpers;
using System.Windows;

namespace boiler_sGraphics.ViewModels
{
    public class PolygonalConnectorViewModel : ConnectorBaseViewModel
    {
        public PolygonalConnectorViewModel(int id, IDiagramViewModel parent,
            FullyCreatedConnectorInfo sourceConnectorInfo, FullyCreatedConnectorInfo sinkConnectorInfo)
            : base(id, parent, sourceConnectorInfo, sinkConnectorInfo)
        { }

        public PolygonalConnectorViewModel(ConnectorInfoBase sourceConnectorInfo, ConnectorInfoBase sinkConnectorInfo)
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

        #region IClonable

        public override object Clone()
        {
            var clone = new PolygonalConnectorViewModel(SourceConnectorInfo, SinkConnectorInfo);
            clone.Owner = Owner;
            clone.EdgeColor = EdgeColor;
            clone.SourceA = SourceA;
            clone.SourceB = SourceB;

            return clone;
        }

        #endregion //IClonable
    }
}
