
namespace grapher.ViewModels
{
    public class PersistDesignerItemViewModel : DesignerItemViewModelBase
    {
        public PersistDesignerItemViewModel(int id, DiagramViewModel parent, double left, double top)
            : base(id, parent, left, top)
        {
            Init();
        }

        public PersistDesignerItemViewModel() : base()
        {
            Init();
        }

        private void Init()
        {
            this.ShowConnectors = false;
        }

        #region IClonable

        public override object Clone()
        {
            var clone = new PersistDesignerItemViewModel();
            clone.Owner = Owner;
            clone.Left.Value = Left.Value;
            clone.Top.Value = Top.Value;
            clone.Width.Value = Width.Value;
            clone.Height.Value = Height.Value;
            clone.EdgeColor = EdgeColor;
            clone.FillColor = FillColor;
            clone.Matrix.Value = Matrix.Value;
            clone.RotationAngle.Value = RotationAngle.Value;
            return clone;
        }

        #endregion //IClonable
    }
}
