using boilersGraphics.ViewModels;

namespace boilersGraphics.Strategies
{
    internal class PolygonalLineFactory : LineFactory
    {
        public override ConnectorBaseViewModel Create(IDiagramViewModel viewModel)
        {
            var ret = new PolygonalConnectorViewModel()
            {
                Owner = viewModel
            };
            ret.ZIndex.Value = ret.Owner.Items.Count;
            return ret;
        }
    }
}
