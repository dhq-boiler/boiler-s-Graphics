using boilersGraphics.ViewModels;

namespace boilersGraphics.Strategies
{
    internal class StraightLineFactory : LineFactory
    {
        public override ConnectorBaseViewModel Create(IDiagramViewModel viewModel)
        {
            var ret = new StraightConnectorViewModel()
            {
                Owner = viewModel
            };
            ret.ZIndex.Value = ret.Owner.Items.Count;
            return ret;
        }
    }
}
