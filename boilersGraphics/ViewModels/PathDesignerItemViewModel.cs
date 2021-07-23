using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace boilersGraphics.ViewModels
{
    public class PathDesignerItemViewModel : DesignerItemViewModelBase
    {
        public ReactiveCollection<Point> Points { get; } = new ReactiveCollection<Point>();

        public override PathGeometry CreateGeometry()
        {
            throw new NotSupportedException();
        }
        public override PathGeometry CreateGeometry(double angle)
        {
            throw new NotSupportedException();
        }

        public override object Clone()
        {
            var clone = new PathDesignerItemViewModel();
            clone.Owner = Owner;
            clone.Left.Value = Left.Value;
            clone.Top.Value = Top.Value;
            clone.Width.Value = Width.Value;
            clone.Height.Value = Height.Value;
            clone.EdgeColor.Value = EdgeColor.Value;
            clone.FillColor = FillColor;
            clone.EdgeThickness.Value = EdgeThickness.Value;
            clone.Matrix.Value = Matrix.Value;
            clone.RotationAngle.Value = RotationAngle.Value;
            return clone;
        }

    }
}
