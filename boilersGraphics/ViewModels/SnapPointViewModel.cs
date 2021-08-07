using boilersGraphics.Helpers;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace boilersGraphics.ViewModels
{
    class SnapPointViewModel : SelectableDesignerItemViewModelBase
    {
        public ReactivePropertySlim<double> Left { get; } = new ReactivePropertySlim<double>();

        public ReactivePropertySlim<double> Top { get; } = new ReactivePropertySlim<double>();
        public ReactivePropertySlim<double> Width { get; } = new ReactivePropertySlim<double>();

        public ReactivePropertySlim<double> Height { get; } = new ReactivePropertySlim<double>();

        public override object Clone()
        {
            var clone = new SnapPointViewModel();
            clone.EdgeColor.Value = EdgeColor.Value;
            clone.EdgeThickness.Value = EdgeThickness.Value;
            clone.EnableForSelection.Value = EnableForSelection.Value;
            clone.EnablePathGeometryUpdate.Value = EnablePathGeometryUpdate.Value;
            clone.FillColor.Value = FillColor.Value;
            clone.ID = ID;
            clone.IsHitTestVisible.Value = IsHitTestVisible.Value;
            clone.IsSelected.Value = IsSelected.Value;
            clone.IsVisible.Value = IsVisible.Value;
            clone.Matrix.Value = Matrix.Value;
            clone.Name = Name;
            clone.Owner = Owner;
            clone.ParentID = ParentID;
            clone.PathGeometry = PathGeometry;
            clone.RotatePathGeometry.Value = RotatePathGeometry.Value;
            clone.RotationAngle.Value = RotationAngle.Value;
            clone.ZIndex.Value = ZIndex.Value;
            clone.Left.Value = Left.Value;
            clone.Top.Value = Top.Value;
            clone.Width.Value = Width.Value;
            clone.Height.Value = Height.Value;
            return clone;
        }

        public override Type GetViewType()
        {
            return typeof(SnapPointViewModel);
        }

        public override void OnNext(GroupTransformNotification value)
        {
            //do nothing
        }
    }
}
