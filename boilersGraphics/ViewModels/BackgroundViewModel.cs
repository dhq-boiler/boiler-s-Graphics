using boilersGraphics.Helpers;
using Reactive.Bindings.Extensions;
using System;
using System.Windows.Shapes;

namespace boilersGraphics.ViewModels
{
    public class BackgroundViewModel : NRectangleViewModel
    {
        private DiagramViewModel diagramViewModel;
        public BackgroundViewModel(DiagramViewModel diagramViewModel)
        {
            this.diagramViewModel = diagramViewModel;
            Width.Value = 1000;
            Height.Value = 1000;
        }

        public override object Clone()
        {
            var clone = new BackgroundViewModel(this.diagramViewModel);
            clone.Owner = Owner;
            clone.Left.Value = Left.Value;
            clone.Top.Value = Top.Value;
            clone.Width.Value = Width.Value;
            clone.Height.Value = Height.Value;
            clone.EdgeBrush.Value = EdgeBrush.Value;
            clone.FillBrush.Value = FillBrush.Value;
            clone.EdgeThickness.Value = EdgeThickness.Value;
            clone.Matrix.Value = Matrix.Value;
            clone.RotationAngle.Value = RotationAngle.Value;
            clone.PathGeometryNoRotate.Value = GeometryCreator.CreateRectangle(clone);
            return clone;
        }

        public override Type GetViewType()
        {
            return typeof(System.Windows.Shapes.Path);
        }
    }
}
