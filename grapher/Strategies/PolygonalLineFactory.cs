using grapher.ViewModels;

namespace grapher.Strategies
{
    internal class PolygonalLineFactory : LineFactory
    {
        public override ConnectorBaseViewModel Create(ConnectorInfoBase sourceConnectorInfo, ConnectorInfoBase sinkConnectorInfo)
        {
            return new PolygonalConnectorViewModel(sourceConnectorInfo, sinkConnectorInfo);
        }
    }
}
