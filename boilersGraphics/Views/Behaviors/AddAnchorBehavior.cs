using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.Models;
using boilersGraphics.Models.Anchors;
using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.Anchors;
using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;
using System.Windows.Input;
using ZLinq;

namespace boilersGraphics.Views.Behaviors;

/// <summary>
/// Phase 3-b §5.3 / Q-6 案 A: 「アンカー追加」ツール用 Behavior。
/// 図形をクリックすると、その図形の Bounds に対する相対座標 (RelativeX/Y) を計算し、
/// <see cref="AnchorViewModel"/> を生成して AllItems に追加する。
/// 図形外をクリックしても何も起きない (空キャンバスへの誤配置を防ぐ)。
/// Rotation 非 0 の図形へのヒット判定は軸並行 bounding box 近似 (Phase 3-b 最小実装、Phase 3.5 で改善余地)。
/// </summary>
public class AddAnchorBehavior : Behavior<DesignerCanvas>
{
    protected override void OnAttached()
    {
        AssociatedObject.MouseDown += AssociatedObject_MouseDown;
        AssociatedObject.TouchDown += AssociatedObject_TouchDown;
        AssociatedObject.StylusDown += AssociatedObject_StylusDown;
        base.OnAttached();
    }

    protected override void OnDetaching()
    {
        AssociatedObject.MouseDown -= AssociatedObject_MouseDown;
        AssociatedObject.TouchDown -= AssociatedObject_TouchDown;
        AssociatedObject.StylusDown -= AssociatedObject_StylusDown;
        base.OnDetaching();
    }

    private void AssociatedObject_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.StylusDevice is not null) return;
        if (e.LeftButton != MouseButtonState.Pressed) return;
        TryAddAt(e.GetPosition(AssociatedObject));
        e.Handled = true;
    }

    private void AssociatedObject_TouchDown(object sender, TouchEventArgs e)
    {
        TryAddAt(e.GetTouchPoint(AssociatedObject).Position);
        e.Handled = true;
    }

    private void AssociatedObject_StylusDown(object sender, StylusDownEventArgs e)
    {
        TryAddAt(e.GetPosition(AssociatedObject));
        e.Handled = true;
    }

    private void TryAddAt(Point click)
    {
        if (AssociatedObject?.DataContext is not IDiagramViewModel diagram) return;

        var owner = FindOwnerUnder(diagram, click);
        if (owner is null) return;

        // 図形の Bounds から相対座標を算出。Rotation > 0 のときは AABB 近似で誤差が出るが、
        // Phase 3-b 最小スコープでは許容 (Phase 3.5 で逆回転変換に改善余地)。
        var width = owner.Width.Value;
        var height = owner.Height.Value;
        if (width <= 0 || height <= 0) return;
        var relX = Math.Clamp((click.X - owner.Left.Value) / width, 0.0, 1.0);
        var relY = Math.Clamp((click.Y - owner.Top.Value) / height, 0.0, 1.0);

        var model = new Anchor
        {
            OwnerId = owner.ID,
            RelativeX = relX,
            RelativeY = relY,
        };
        var vm = new AnchorViewModel(model)
        {
            Owner = diagram,
        };
        vm.IsVisible.Value = true;
        vm.ZIndex.Value = diagram.Layers
            .SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
            .AsValueEnumerable().Count();
        diagram.AddItemCommand.Execute(vm);
        vm.RebindOwner();
    }

    private static DesignerItemViewModelBase FindOwnerUnder(IDiagramViewModel diagram, Point click)
    {
        // ZIndex 降順で走査して最初にヒットしたものを採用 (上に乗っている図形が優先)
        var hits = diagram.AllItems.Value
            .AsValueEnumerable()
            .OfType<DesignerItemViewModelBase>()
            .Where(d => Contains(d, click))
            .ToArray();
        if (hits.Length == 0) return null;
        DesignerItemViewModelBase top = null;
        var topZ = int.MinValue;
        foreach (var h in hits)
        {
            if (h.ZIndex.Value >= topZ)
            {
                topZ = h.ZIndex.Value;
                top = h;
            }
        }
        return top;
    }

    private static bool Contains(DesignerItemViewModelBase item, Point p)
    {
        var left = item.Left.Value;
        var top = item.Top.Value;
        var right = left + item.Width.Value;
        var bottom = top + item.Height.Value;
        return p.X >= left && p.X <= right && p.Y >= top && p.Y <= bottom;
    }
}
