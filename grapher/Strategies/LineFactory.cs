using grapher.ViewModels;

namespace grapher.Strategies
{
    public abstract class LineFactory
    {
        public abstract ConnectorBaseViewModel Create(IDiagramViewModel viewModel, ConnectorInfoBase sourceConnectorInfo, ConnectorInfoBase sinkConnectorInfo);
    }
}
