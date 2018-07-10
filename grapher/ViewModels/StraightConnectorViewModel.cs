
using System.Windows;

namespace grapher.ViewModels
{
    public class StraightConnectorViewModel : ConnectorBaseViewModel
    {
        public StraightConnectorViewModel(int id, IDiagramViewModel parent,
            FullyCreatedConnectorInfo sourceConnectorInfo, FullyCreatedConnectorInfo sinkConnectorInfo)
            : base(id, parent, sourceConnectorInfo, sinkConnectorInfo)
        { }

        public StraightConnectorViewModel(ConnectorInfoBase sourceConnectorInfo, ConnectorInfoBase sinkConnectorInfo)
            : base(sourceConnectorInfo, sinkConnectorInfo)
        { }

        public StraightConnectorViewModel(Point p1, Point p2)
            : base(new PartCreatedConnectionInfo(p1), new PartCreatedConnectionInfo(p2))
        { }

        #region IClonable

        public override object Clone()
        {
            var clone = new StraightConnectorViewModel(SourceA, SourceB);
            clone.Owner = Owner;
            clone.EdgeColor = EdgeColor;
            clone.SourceA = SourceA;
            clone.SourceB = SourceB;

            return clone;
        }

        #endregion //IClonable
    }
}
