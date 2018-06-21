using grapher.ViewModels;

namespace grapher.Strategies
{
    internal class StraightLineFactory : LineFactory
    {
        public override ConnectorBaseViewModel Create(ConnectorInfoBase sourceConnectorInfo, ConnectorInfoBase sinkConnectorInfo)
        {
            return new StraightConnectorViewModel(sourceConnectorInfo, sinkConnectorInfo);
        }
    }
}
