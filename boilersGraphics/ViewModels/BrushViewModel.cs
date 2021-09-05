using System;
using System.Windows.Media;
using System.Windows.Shapes;

namespace boilersGraphics.ViewModels
{
    public class BrushViewModel : DesignerItemViewModelBase
    {
        public override object Clone()
        {
            var clone = new BrushViewModel();
            clone.Owner = Owner;
            clone.Left.Value = Left.Value;
            clone.Top.Value = Top.Value;
            clone.Width.Value = Width.Value;
            clone.Height.Value = Height.Value;
            clone.EdgeColor.Value = EdgeColor.Value;
            clone.FillColor.Value = FillColor.Value;
            clone.EdgeThickness.Value = EdgeThickness.Value;
            clone.Matrix.Value = Matrix.Value;
            clone.RotationAngle.Value = RotationAngle.Value;
            clone.PathGeometry.Value = PathGeometry.Value.Clone();
            return clone;
        }

        public override PathGeometry CreateGeometry()
        {
            throw new NotSupportedException("brush is not supported.");
        }

        public override PathGeometry CreateGeometry(double angle)
        {
            throw new NotSupportedException("brush is not supported.");
        }

        public override Type GetViewType()
        {
            return typeof(Path);
        }
    }
}
