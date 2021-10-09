using boilersGraphics.Helpers;
using System;
using System.Windows.Shapes;

namespace boilersGraphics.ViewModels
{
    public class BackgroundViewModel : NRectangleViewModel
    {
        public override object Clone()
        {
            var clone = new BackgroundViewModel();
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
            clone.PathGeometry.Value = GeometryCreator.CreateRectangle(clone);
            return clone;
        }

        public override Type GetViewType()
        {
            return typeof(Path);
        }
    }
}
