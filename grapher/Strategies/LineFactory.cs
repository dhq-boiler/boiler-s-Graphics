using grapher.ViewModels;

namespace grapher.Strategies
{
    public abstract class LineFactory
    {
        public abstract ConnectorBaseViewModel Create(ConnectorInfoBase sourceConnectorInfo, ConnectorInfoBase sinkConnectorInfo);
    }
}
