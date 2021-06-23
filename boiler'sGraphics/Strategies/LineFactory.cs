using boiler_sGraphics.ViewModels;

namespace boiler_sGraphics.Strategies
{
    public abstract class LineFactory
    {
        public abstract ConnectorBaseViewModel Create(IDiagramViewModel viewModel, ConnectorInfoBase sourceConnectorInfo, ConnectorInfoBase sinkConnectorInfo);
    }
}
