using boilersGraphics.Adorners;
using boilersGraphics.Controls;
using boilersGraphics.Helpers;
using boilersGraphics.Properties;
using boilersGraphics.ViewModels;
using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace boilersGraphics.Views.Behaviors;

public class ColorCorrectBehavior : Behavior<DesignerCanvas>
{
    private Point? _rectangleStartPoint;
    private ColorCorrectViewModel item;
    private readonly SnapAction snapAction;

    public ColorCorrectBehavior()
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
            _rectangleStartPoint = e.GetPosition(AssociatedObject);
            var viewModel = AssociatedObject.DataContext as IDiagramViewModel;
            item = new ColorCorrectViewModel();
            item.Owner = viewModel;
            e.Handled = true;
        }
    }

    private void AssociatedObject_TouchDown(object sender, TouchEventArgs e)
    {
        if (e.Source == AssociatedObject)
        {
            var touchPoint = e.GetTouchPoint(AssociatedObject);
            _rectangleStartPoint = touchPoint.Position;
            var viewModel = AssociatedObject.DataContext as IDiagramViewModel;
            item = new ColorCorrectViewModel();
            item.Owner = viewModel;
        }
    }

    private void AssociatedObject_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.StylusDevice != null)
            return;

        if (e.LeftButton == MouseButtonState.Pressed)
            if (e.Source == AssociatedObject)
            {
                _rectangleStartPoint = e.GetPosition(AssociatedObject);
                var viewModel = AssociatedObject.DataContext as IDiagramViewModel;
                item = new ColorCorrectViewModel();
                item.Owner = viewModel;
            }
    }

    private void AssociatedObject_StylusMove(object sender, StylusEventArgs e)
    {
        var canvas = AssociatedObject;
        var current = e.GetPosition(canvas);
        snapAction.OnMouseMove(ref current);

        if (e.InAir)
            _rectangleStartPoint = null;

        if (_rectangleStartPoint.HasValue)
        {
            if (_rectangleStartPoint.Value.X < current.X && _rectangleStartPoint.Value.Y <= current.Y)
                snapAction.PostProcess(SnapPointPosition.LeftTop, item);
            else if (_rectangleStartPoint.Value.X < current.X && current.Y < _rectangleStartPoint.Value.Y)
                snapAction.PostProcess(SnapPointPosition.LeftBottom, item);
            else if (current.X <= _rectangleStartPoint.Value.X && _rectangleStartPoint.Value.Y <= current.Y)
                snapAction.PostProcess(SnapPointPosition.RightTop, item);
            else if (current.X <= _rectangleStartPoint.Value.X && current.Y < _rectangleStartPoint.Value.Y)
                snapAction.PostProcess(SnapPointPosition.RightBottom, item);
            _rectangleStartPoint = current;
            (Application.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value =
                Resources.String_Draw;

            var adornerLayer = AdornerLayer.GetAdornerLayer(canvas);
            if (adornerLayer != null)
            {
                var adorner = new ColorCorrectAdorner(canvas, _rectangleStartPoint, item);
                if (adorner != null) adornerLayer.Add(adorner);
            }
        }
    }

    private void AssociatedObject_MouseMove(object sender, MouseEventArgs e)
    {
        var canvas = AssociatedObject;
        var current = e.GetPosition(canvas);
        snapAction.OnMouseMove(ref current);

        if (e.LeftButton != MouseButtonState.Pressed)
            _rectangleStartPoint = null;

        if (_rectangleStartPoint.HasValue)
        {
            if (_rectangleStartPoint.Value.X < current.X && _rectangleStartPoint.Value.Y <= current.Y)
                snapAction.PostProcess(SnapPointPosition.LeftTop, item);
            else if (_rectangleStartPoint.Value.X < current.X && current.Y < _rectangleStartPoint.Value.Y)
                snapAction.PostProcess(SnapPointPosition.LeftBottom, item);
            else if (current.X <= _rectangleStartPoint.Value.X && _rectangleStartPoint.Value.Y <= current.Y)
                snapAction.PostProcess(SnapPointPosition.RightTop, item);
            else if (current.X <= _rectangleStartPoint.Value.X && current.Y < _rectangleStartPoint.Value.Y)
                snapAction.PostProcess(SnapPointPosition.RightBottom, item);
            _rectangleStartPoint = current;
            (Application.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value =
                Resources.String_Draw;

            var adornerLayer = AdornerLayer.GetAdornerLayer(canvas);
            if (adornerLayer != null)
            {
                var adorner = new ColorCorrectAdorner(canvas, _rectangleStartPoint, item);
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