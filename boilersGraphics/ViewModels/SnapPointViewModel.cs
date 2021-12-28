using boilersGraphics.Helpers;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Windows;

namespace boilersGraphics.ViewModels
{
    public class SnapPointViewModel : SelectableDesignerItemViewModelBase
    {
        public SnapPointViewModel()
            : base()
        { }

        public SnapPointViewModel(ConnectorBaseViewModel parent, int index, IDiagramViewModel owner, double left, double top, double width, double height)
            : base()
        {
            Owner = owner;
            Parent.Value = parent;
            Left.Value = left;
            Top.Value = top;
            Width.Value = width;
            Height.Value = height;

            IsSelected.Subscribe(x =>
            {
                if (x)
                    parent.IsSelected.Value = true;
            })
            .AddTo(_CompositeDisposable);
            Left.Subscribe(x =>
            {
                parent.Points[index] = new Point(x, parent.Points[index].Y);
            })
            .AddTo(_CompositeDisposable);
            Top.Subscribe(x =>
            {
                parent.Points[index] = new Point(parent.Points[index].X, x);
            })
            .AddTo(_CompositeDisposable);
        }

        public ReactivePropertySlim<SelectableDesignerItemViewModelBase> Parent { get; } = new ReactivePropertySlim<SelectableDesignerItemViewModelBase>();

        public ReactivePropertySlim<double> Left { get; } = new ReactivePropertySlim<double>();

        public ReactivePropertySlim<double> Top { get; } = new ReactivePropertySlim<double>();
        public ReactivePropertySlim<double> Width { get; } = new ReactivePropertySlim<double>();

        public ReactivePropertySlim<double> Height { get; } = new ReactivePropertySlim<double>();

        public ReactivePropertySlim<double> Opacity { get; } = new ReactivePropertySlim<double>();

        public List<IDisposable> SnapObjs { get; set; } = new List<IDisposable>();

        public override bool SupportsPropertyDialog => false;

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
            clone.RotationAngle.Value = RotationAngle.Value;
            clone.ZIndex.Value = ZIndex.Value;
            clone.Left.Value = Left.Value;
            clone.Top.Value = Top.Value;
            clone.Width.Value = Width.Value;
            clone.Height.Value = Height.Value;
            clone.Opacity.Value = Opacity.Value;
            clone.SnapObjs = SnapObjs;
            return clone;
        }

        public override Type GetViewType()
        {
            return typeof(Controls.SnapPoint);
        }

        public override void OnNext(GroupTransformNotification value)
        {
            //do nothing
        }

        public override void OpenPropertyDialog()
        {
            Parent.Value.OpenPropertyDialog();
        }
    }
}
