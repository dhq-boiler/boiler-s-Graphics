using boilersGraphics.Adorners;
using boilersGraphics.Controls;
using boilersGraphics.Properties;
using boilersGraphics.ViewModels;
using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace boilersGraphics.Views.Behaviors;

/// <summary>
/// Phase 2.5-a: テキストマトリクスツール用 Behavior。NumberSequenceBlockBehavior と同じ枠組み。
/// </summary>
public class TextMatrixBlockBehavior : Behavior<DesignerCanvas>
{
    private Point? _dragStartPoint;

    protected override void OnAttached()
    {
        AssociatedObject.StylusDown += AssociatedObject_StylusDown;
        AssociatedObject.StylusMove += AssociatedObject_StylusMove;
        AssociatedObject.TouchDown += AssociatedObject_TouchDown;
        AssociatedObject.MouseDown += AssociatedObject_MouseDown;
        AssociatedObject.MouseMove += AssociatedObject_MouseMove;
        base.OnAttached();
    }

    protected override void OnDetaching()
    {
        AssociatedObject.StylusDown -= AssociatedObject_StylusDown;
        AssociatedObject.StylusMove -= AssociatedObject_StylusMove;
        AssociatedObject.TouchDown -= AssociatedObject_TouchDown;
        AssociatedObject.MouseDown -= AssociatedObject_MouseDown;
        AssociatedObject.MouseMove -= AssociatedObject_MouseMove;
        base.OnDetaching();
    }

    private void AssociatedObject_StylusDown(object sender, StylusDownEventArgs e)
    {
        if (e.Source == AssociatedObject)
        {
            _dragStartPoint = e.GetPosition(AssociatedObject);
            e.Handled = true;
        }
    }

    private void AssociatedObject_TouchDown(object sender, TouchEventArgs e)
    {
        if (e.Source == AssociatedObject)
        {
            _dragStartPoint = e.GetTouchPoint(AssociatedObject).Position;
        }
    }

    private void AssociatedObject_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed) return;
        if (e.Source != AssociatedObject) return;

        _dragStartPoint = e.GetPosition(AssociatedObject);
        e.Handled = true;
    }

    private void AssociatedObject_MouseMove(object sender, MouseEventArgs e)
    {
        var canvas = AssociatedObject;
        if (e.LeftButton != MouseButtonState.Pressed)
            _dragStartPoint = null;

        if (!_dragStartPoint.HasValue) { e.Handled = true; return; }

        if (Application.Current?.MainWindow?.DataContext is MainWindowViewModel mvm)
            mvm.CurrentOperation.Value = Resources.String_Draw;

        var adornerLayer = AdornerLayer.GetAdornerLayer(canvas);
        adornerLayer?.Add(new TextMatrixBlockAdorner(canvas, _dragStartPoint));

        e.Handled = true;
    }

    private void AssociatedObject_StylusMove(object sender, StylusEventArgs e)
    {
        var canvas = AssociatedObject;
        if (e.InAir) _dragStartPoint = null;
        if (!_dragStartPoint.HasValue) return;

        if (Application.Current?.MainWindow?.DataContext is MainWindowViewModel mvm)
            mvm.CurrentOperation.Value = Resources.String_Draw;

        var adornerLayer = AdornerLayer.GetAdornerLayer(canvas);
        adornerLayer?.Add(new TextMatrixBlockAdorner(canvas, _dragStartPoint));
    }
}
