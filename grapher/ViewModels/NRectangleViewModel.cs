
namespace grapher.ViewModels
{
    public class NRectangleViewModel : DesignerItemViewModelBase
    {
        public NRectangleViewModel()
            : base()
        {
            Init();
        }

        public NRectangleViewModel(int id, IDiagramViewModel parent, double left, double top)
            : base(id, parent, left, top)
        {
            Init();
        }

        private void Init()
        {
            this.ShowConnectors = false;
        }
    }
}
