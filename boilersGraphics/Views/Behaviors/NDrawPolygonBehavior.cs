using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using boilersGraphics.Adorners;
using boilersGraphics.Controls;
using boilersGraphics.Helpers;
using boilersGraphics.Models;
using boilersGraphics.Properties;
using boilersGraphics.ViewModels;
using Microsoft.Xaml.Behaviors;

namespace boilersGraphics.Views.Behaviors;

public class NDrawPolygonBehavior : Behavior<DesignerCanvas>
{
    private Point? _polygonDrawingStartPoint;
    private readonly SnapAction snapAction;

    public NDrawPolygonBehavior(ObservableCollection<Corner> corners, string data)
    {
        Corners = corners;
        Data = data;
        snapAction = new SnapAction();
    }

    public ObservableCollection<Corner> Corners { get; }
    public string Data { get; }

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
            _polygonDrawingStartPoint = e.GetPosition(AssociatedObject);
            e.Handled = true;
        }
    }

    private void AssociatedObject_TouchDown(object sender, TouchEventArgs e)
    {
        if (e.Source == AssociatedObject)
        {
            var touchPoint = e.GetTouchPoint(AssociatedObject);
            _polygonDrawingStartPoint = touchPoint.Position;
        }
    }

    private void AssociatedObject_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
            if (e.Source == AssociatedObject)
            {
                _polygonDrawingStartPoint = e.GetPosition(AssociatedObject);

                e.Handled = true;
            }
    }

    private void AssociatedObject_MouseMove(object sender, MouseEventArgs e)
    {
        if (e.StylusDevice != null)
            return;

        var canvas = AssociatedObject;
        var current = e.GetPosition(canvas);
        snapAction.OnMouseMove(ref current);

        if (e.LeftButton != MouseButtonState.Pressed)
            _polygonDrawingStartPoint = null;

        if (_polygonDrawingStartPoint.HasValue)
        {
            _polygonDrawingStartPoint = current;
            (Application.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value =
                Resources.String_Draw;

            var adornerLayer = AdornerLayer.GetAdornerLayer(canvas);
            if (adornerLayer != null)
            {
                var adorner = new PolygonAdorner(canvas, _polygonDrawingStartPoint, Corners, Data);
                if (adorner != null) adornerLayer.Add(adorner);
            }
        }
    }

    private void AssociatedObject_StylusMove(object sender, StylusEventArgs e)
    {
        var canvas = AssociatedObject;
        var current = e.GetPosition(canvas);
        snapAction.OnMouseMove(ref current);

        if (e.InAir)
            _polygonDrawingStartPoint = null;

        if (_polygonDrawingStartPoint.HasValue)
        {
            _polygonDrawingStartPoint = current;
            (Application.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value =
                Resources.String_Draw;

            var adornerLayer = AdornerLayer.GetAdornerLayer(canvas);
            if (adornerLayer != null)
            {
                var adorner = new PolygonAdorner(canvas, _polygonDrawingStartPoint, Corners, Data);
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