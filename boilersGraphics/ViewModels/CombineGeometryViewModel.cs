using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;

namespace boilersGraphics.ViewModels
{
    public class CombineGeometryViewModel : DesignerItemViewModelBase
    {
        public override bool SupportsPropertyDialog => false;

        public CombineGeometryViewModel()
            : base()
        {
            Init();
        }

        public CombineGeometryViewModel(double left, double top, double width, double height)
            : base()
        {
            Init();
            Left.Value = left;
            Top.Value = top;
            Width.Value = width;
            Height.Value = height;
        }

        public CombineGeometryViewModel(double left, double top, double width, double height, double angleInDegrees)
            : this(left, top, width, height)
        {
            RotationAngle.Value = angleInDegrees;
            Matrix.Value.RotateAt(angleInDegrees, 0, 0);
        }

        public CombineGeometryViewModel(int id, IDiagramViewModel parent, double left, double top)
            : base(id, parent, left, top)
        {
            Init();
        }

        private void Init()
        {
            EnablePathGeometryUpdate.Value = false;
            this.ShowConnectors = false;
        }

        public override PathGeometry CreateGeometry()
        {
            throw new NotSupportedException("combine figures is not supported.");
        }

        public override PathGeometry CreateGeometry(double angle)
        {
            throw new NotSupportedException("combine figures is not supported.");
        }

        public override Type GetViewType()
        {
            return typeof(Path);
        }

        #region IClonable

        public override object Clone()
        {
            var clone = new CombineGeometryViewModel();
            clone.Owner = Owner;
            clone.Left.Value = Left.Value;
            clone.Top.Value = Top.Value;
            clone.Width.Value = Width.Value;
            clone.Height.Value = Height.Value;
            clone.EdgeColor.Value = EdgeColor.Value;
            clone.FillColor.Value = FillColor.Value;
            clone.EdgeThickness.Value = EdgeThickness.Value;
            clone.RotationAngle.Value = RotationAngle.Value;
            clone.PathGeometry.Value = PathGeometry.Value;
            clone.PenLineJoin.Value = PenLineJoin.Value;
            return clone;
        }

        public override void OpenPropertyDialog()
        {
            throw new NotImplementedException();
        }

        #endregion //IClonable
    }
}