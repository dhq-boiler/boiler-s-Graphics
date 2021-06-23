using boiler_sGraphics.ViewModels;

namespace boiler_sGraphics.Strategies
{
    internal class StraightLineFactory : LineFactory
    {
        public override ConnectorBaseViewModel Create(IDiagramViewModel viewModel, ConnectorInfoBase sourceConnectorInfo, ConnectorInfoBase sinkConnectorInfo)
        {
            var ret = new StraightConnectorViewModel(sourceConnectorInfo, sinkConnectorInfo)
            {
                Owner = viewModel
            };
            ret.ZIndex.Value = ret.Owner.Items.Count;
            return ret;
        }
    }
}
