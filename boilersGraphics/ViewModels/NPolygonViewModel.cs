using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace boilersGraphics.ViewModels
{
    public class NPolygonViewModel : DesignerItemViewModelBase
    {
        public NPolygonViewModel()
            : base()
        {
            Init();
        }

        public NPolygonViewModel(double left, double top, double width, double height)
            : base()
        {
            Init();
            Left.Value = left;
            Top.Value = top;
            Width.Value = width;
            Height.Value = height;
        }

        public NPolygonViewModel(double left, double top, double width, double height, double angleInDegrees)
            : this(left, top, width, height)
        {
            RotationAngle.Value = angleInDegrees;
            Matrix.Value.RotateAt(angleInDegrees, 0, 0);
        }

        public NPolygonViewModel(int id, IDiagramViewModel parent, double left, double top)
            : base(id, parent, left, top)
        {
            Init();
        }

        private void Init()
        {
            this.ShowConnectors = false;
            EnablePathGeometryUpdate.Value = true;
        }

        public ReactiveProperty<string> Data { get; set; } = new ReactiveProperty<string>();

        public override PathGeometry CreateGeometry()
        {
            return System.Windows.Media.PathGeometry.CreateFromGeometry(Geometry.Parse(Data.Value));
        }

        public override PathGeometry CreateGeometry(double angle)
        {
            var geometry = Geometry.Parse(Data.Value);
            geometry.Transform = new RotateTransform(angle);
            return System.Windows.Media.PathGeometry.CreateFromGeometry(geometry);
        }

        #region IClonable

        public override object Clone()
        {
            var clone = new NPolygonViewModel();
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
            clone.Data.Value = Data.Value;
            return clone;
        }

        #endregion //IClonable
    }
}
