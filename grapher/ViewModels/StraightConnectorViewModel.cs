namespace grapher.ViewModels
{
    public class StraightConnectorViewModel : ConnectorBaseViewModel
    {
        public StraightConnectorViewModel(int id, IDiagramViewModel parent,
            FullyCreatedConnectorInfo sourceConnectorInfo, FullyCreatedConnectorInfo sinkConnectorInfo)
            : base(id, parent, sourceConnectorInfo, sinkConnectorInfo)
        { }

        public StraightConnectorViewModel(FullyCreatedConnectorInfo sourceConnectorInfo, ConnectorInfoBase sinkConnectorInfo)
            : base(sourceConnectorInfo, sinkConnectorInfo)
        { }
    }
}
