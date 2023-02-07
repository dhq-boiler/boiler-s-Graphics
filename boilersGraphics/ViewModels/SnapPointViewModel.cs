using System;
using System.Collections.Generic;
using System.Linq;
using boilersGraphics.Controls;
using boilersGraphics.Helpers;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace boilersGraphics.ViewModels;

public class SnapPointViewModel : SelectableDesignerItemViewModelBase
{
    public SnapPointViewModel()
    {
    }

    public SnapPointViewModel(ConnectorBaseViewModel parent, int index, IDiagramViewModel owner, double left,
        double top, double width, double height)
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
                if (parent.Points.Count() < index + 1)
                    return;
                var point = parent.Points[index];
                point.X = x;
            })
            .AddTo(_CompositeDisposable);
        Top.Subscribe(y =>
            {
                if (parent.Points.Count() < index + 1)
                    return;
                var point = parent.Points[index];
                point.Y = y;
            })
            .AddTo(_CompositeDisposable);
    }

    public ReactivePropertySlim<SelectableDesignerItemViewModelBase> Parent { get; } = new();

    public ReactivePropertySlim<double> Left { get; } = new(0, ReactivePropertyMode.RaiseLatestValueOnSubscribe);

    public ReactivePropertySlim<double> Top { get; } = new(0, ReactivePropertyMode.RaiseLatestValueOnSubscribe);
    public ReactivePropertySlim<double> Width { get; } = new();

    public ReactivePropertySlim<double> Height { get; } = new();

    public ReactivePropertySlim<double> Opacity { get; } = new();

    public List<IDisposable> SnapObjs { get; set; } = new();

    public override bool SupportsPropertyDialog => false;

    public override object Clone()
    {
        var clone = new SnapPointViewModel();
        clone.EdgeBrush.Value = EdgeBrush.Value;
        clone.FillBrush.Value = FillBrush.Value;
        clone.EdgeThickness.Value = EdgeThickness.Value;
        clone.EnableForSelection.Value = EnableForSelection.Value;
        clone.UpdatingStrategy.Value = UpdatingStrategy.Value;
        clone.ID = ID;
        clone.IsHitTestVisible.Value = IsHitTestVisible.Value;
        clone.IsSelected.Value = IsSelected.Value;
        clone.IsVisible.Value = IsVisible.Value;
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
        return typeof(SnapPoint);
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