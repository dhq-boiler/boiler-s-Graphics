using boilersGraphics.Adorners;
using boilersGraphics.Controls;
using boilersGraphics.Properties;
using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.Connectors;
using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace boilersGraphics.Views.Behaviors;

/// <summary>
/// Phase 3-c §5.2: L 字コネクタ用 Behavior。
/// MouseDown で始点確定、Drag 中にプレビュー Adorner を表示。
/// 配置確定は <see cref="OrthogonalConnectorAdorner"/>.OnMouseUp で行う。
/// </summary>
public class NDrawOrthogonalConnectorBehavior : Behavior<DesignerCanvas>
{
    private Point? _dragStartPoint;
    private OrthogonalConnectorViewModel _pendingItem;

    protected override void OnAttached()
    {
        AssociatedObject.MouseDown += AssociatedObject_MouseDown;
        AssociatedObject.MouseMove += AssociatedObject_MouseMove;
        base.OnAttached();
    }

    protected override void OnDetaching()
    {
        AssociatedObject.MouseDown -= AssociatedObject_MouseDown;
        AssociatedObject.MouseMove -= AssociatedObject_MouseMove;
        base.OnDetaching();
    }

    private void AssociatedObject_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed) return;
        if (e.Source != AssociatedObject) return;

        _dragStartPoint = e.GetPosition(AssociatedObject);
        var diagram = AssociatedObject.DataContext as IDiagramViewModel;
        _pendingItem = new OrthogonalConnectorViewModel(diagram, _dragStartPoint.Value);
        e.Handled = true;
    }

    private void AssociatedObject_MouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed)
        {
            _dragStartPoint = null;
            _pendingItem = null;
            return;
        }
        if (!_dragStartPoint.HasValue || _pendingItem is null) return;

        if (Application.Current?.MainWindow?.DataContext is MainWindowViewModel mvm)
            mvm.CurrentOperation.Value = Resources.String_Draw;

        var canvas = AssociatedObject;
        var layer = AdornerLayer.GetAdornerLayer(canvas);
        layer?.Add(new OrthogonalConnectorAdorner(canvas, _dragStartPoint, _pendingItem));
        _dragStartPoint = null; // Adorner が以降を担う
        e.Handled = true;
    }
}
