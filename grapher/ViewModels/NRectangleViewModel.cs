
namespace grapher.ViewModels
{
    public class NRectangleViewModel : DesignerItemViewModelBase
    {
        public NRectangleViewModel()
            : base()
        {
            Init();
        }

        public NRectangleViewModel(double left, double top, double width, double height)
            : base()
        {
            Init();
            Left.Value = left;
            Top.Value = top;
            Width.Value = width;
            Height.Value = height;
        }

        public NRectangleViewModel(double left, double top, double width, double height, double angleInDegrees)
            : this(left, top, width, height)
        {
            RotationAngle.Value = angleInDegrees;
            Matrix.Value.RotateAt(angleInDegrees, 0, 0);
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
