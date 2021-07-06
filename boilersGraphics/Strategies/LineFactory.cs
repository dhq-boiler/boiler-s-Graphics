using boilersGraphics.ViewModels;

namespace boilersGraphics.Strategies
{
    public abstract class LineFactory
    {
        public abstract ConnectorBaseViewModel Create(IDiagramViewModel viewModel);
    }
}
