using boilersGraphics.Adorners;
using boilersGraphics.Controls;
using boilersGraphics.Helpers;
using boilersGraphics.Properties;
using boilersGraphics.ViewModels;
using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using ZLinq;

namespace boilersGraphics.Views.Behaviors;

public class NDrawStraightLineBehavior : Behavior<DesignerCanvas>
{
    private Point? _straightLineStartPoint;
    private StraightConnectorViewModel item;
    private readonly SnapAction snapAction;

    public NDrawStraightLineBehavior()
    {
        snapAction = new SnapAction();
    }

    protected override void OnAttached()
    {
        AssociatedObject.StylusDown += AssociatedObject_StylusDown;
        AssociatedObject.StylusMove += AssociatedObject_StylusMove;
        AssociatedObject.TouchDown += AssociatedObject_TouchDown;
        AssociatedObject.MouseDown += AssociatedObject_MouseDown;
        AssociatedObject.MouseMove += AssociatedObject_MouseMove;
        AssociatedObject.MouseUp += AssociatedObject_MouseUp;
        base.OnAttached();
    }

    protected override void OnDetaching()
    {
        AssociatedObject.StylusDown -= AssociatedObject_StylusDown;
        AssociatedObject.StylusMove -= AssociatedObject_StylusMove;
        AssociatedObject.TouchDown -= AssociatedObject_TouchDown;
        AssociatedObject.MouseDown -= AssociatedObject_MouseDown;
        AssociatedObject.MouseMove -= AssociatedObject_MouseMove;
        AssociatedObject.MouseUp -= AssociatedObject_MouseUp;
        base.OnDetaching();
    }

    private void AssociatedObject_StylusDown(object sender, StylusDownEventArgs e)
    {
        if (e.Source == AssociatedObject)
        {
            _straightLineStartPoint = e.GetPosition(AssociatedObject);
            var viewModel = AssociatedObject.DataContext as IDiagramViewModel;
            item = new StraightConnectorViewModel(viewModel, _straightLineStartPoint.Value);
            e.Handled = true;
        }
    }

    private void AssociatedObject_TouchDown(object sender, TouchEventArgs e)
    {
        if (e.Source == AssociatedObject)
        {
            var touchPoint = e.GetTouchPoint(AssociatedObject);
            _straightLineStartPoint = touchPoint.Position;
            var viewModel = AssociatedObject.DataContext as IDiagramViewModel;
            item = new StraightConnectorViewModel(viewModel, _straightLineStartPoint.Value);
        }
    }

    private void AssociatedObject_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
            if (e.Source == AssociatedObject)
            {
                _straightLineStartPoint = e.GetPosition(AssociatedObject);
                var viewModel = AssociatedObject.DataContext as IDiagramViewModel;
                item = new StraightConnectorViewModel(viewModel, _straightLineStartPoint.Value);
                e.Handled = true;
            }
    }

    private void AssociatedObject_MouseMove(object sender, MouseEventArgs e)
    {
        if (e.StylusDevice != null)
            return;

        var canvas = AssociatedObject;
        var current = e.GetPosition(canvas);
        var ellipses = (AssociatedObject.DataContext as DiagramViewModel).AllItems.Value.AsValueEnumerable().OfType<NEllipseViewModel>();
        var pies = (AssociatedObject.DataContext as DiagramViewModel).AllItems.Value.AsValueEnumerable().OfType<NPieViewModel>();
        var appendIntersectionPoints = new List<Tuple<Point, object>>();
        var vec = new Vector();
        if (_straightLineStartPoint.HasValue)
        {
            vec = Point.Subtract(current, _straightLineStartPoint.Value);
            snapAction.SnapIntersectionOfEllipseAndTangent(ellipses, _straightLineStartPoint.Value, current,
                appendIntersectionPoints);
            snapAction.SnapIntersectionOfPieAndTangent(pies, _straightLineStartPoint.Value, current,
                appendIntersectionPoints);
        }

        snapAction.OnMouseMove(ref current, vec, appendIntersectionPoints);

        if (e.LeftButton != MouseButtonState.Pressed)
            _straightLineStartPoint = null;

        if (_straightLineStartPoint.HasValue)
        {
            _straightLineStartPoint = current;
            (Application.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value =
                Resources.String_Draw;

            var adornerLayer = AdornerLayer.GetAdornerLayer(canvas);
            if (adornerLayer != null)
            {
                var adorner = new StraightLineAdorner(canvas, _straightLineStartPoint, item);
                if (adorner != null) adornerLayer.Add(adorner);
            }
        }
    }

    private void AssociatedObject_StylusMove(object sender, StylusEventArgs e)
    {
        var canvas = AssociatedObject;
        var current = e.GetPosition(canvas);
        var ellipses = (AssociatedObject.DataContext as DiagramViewModel).AllItems.Value.AsValueEnumerable().OfType<NEllipseViewModel>();
        var pies = (AssociatedObject.DataContext as DiagramViewModel).AllItems.Value.AsValueEnumerable().OfType<NPieViewModel>();
        var appendIntersectionPoints = new List<Tuple<Point, object>>();
        var vec = new Vector();
        if (_straightLineStartPoint.HasValue)
        {
            vec = Point.Subtract(current, _straightLineStartPoint.Value);
            snapAction.SnapIntersectionOfEllipseAndTangent(ellipses, _straightLineStartPoint.Value, current,
                appendIntersectionPoints);
            snapAction.SnapIntersectionOfPieAndTangent(pies, _straightLineStartPoint.Value, current,
                appendIntersectionPoints);
        }

        snapAction.OnMouseMove(ref current, vec, appendIntersectionPoints);

        if (e.InAir)
            _straightLineStartPoint = null;

        if (_straightLineStartPoint.HasValue)
        {
            _straightLineStartPoint = current;
            (Application.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value =
                Resources.String_Draw;

            var adornerLayer = AdornerLayer.GetAdornerLayer(canvas);
            if (adornerLayer != null)
            {
                var adorner = new StraightLineAdorner(canvas, _straightLineStartPoint, item);
                if (adorner != null) adornerLayer.Add(adorner);
            }
        }
    }

    private void AssociatedObject_MouseUp(object sender, MouseButtonEventArgs e)
    {
        // release mouse capture
        if (AssociatedObject.IsMouseCaptured) AssociatedObject.ReleaseMouseCapture();
    }
}