using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace boilersGraphics.ViewModels
{
    public class PathDesignerItemViewModel : DesignerItemViewModelBase
    {
        public ReactiveCollection<Point> Points { get; } = new ReactiveCollection<Point>();

        public override object Clone()
        {
            var clone = new PathDesignerItemViewModel();
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
    }
}
