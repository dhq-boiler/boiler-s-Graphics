using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using boilersGraphics.Adorners;
using boilersGraphics.Controls;
using boilersGraphics.Properties;
using boilersGraphics.ViewModels;
using Microsoft.Xaml.Behaviors;

namespace boilersGraphics.Views.Behaviors;

public class LetterBehavior : Behavior<DesignerCanvas>
{
    private Point? _pictureDrawingStartPoint;

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
            _pictureDrawingStartPoint = e.GetPosition(AssociatedObject);
            e.Handled = true;
        }
    }

    private void AssociatedObject_TouchDown(object sender, TouchEventArgs e)
    {
        if (e.Source == AssociatedObject)
        {
            var touchPoint = e.GetTouchPoint(AssociatedObject);
            _pictureDrawingStartPoint = touchPoint.Position;
        }
    }

    private void AssociatedObject_MouseMove(object sender, MouseEventArgs e)
    {
        var canvas = AssociatedObject;
        if (e.LeftButton != MouseButtonState.Pressed)
            _pictureDrawingStartPoint = null;

        if (_pictureDrawingStartPoint.HasValue)
        {
            (Application.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value =
                Resources.String_Draw;

            var adornerLayer = AdornerLayer.GetAdornerLayer(canvas);
            if (adornerLayer != null)
            {
                var adorner = new LetterAdorner(canvas, _pictureDrawingStartPoint);
                if (adorner != null) adornerLayer.Add(adorner);
            }
        }

        e.Handled = true;
    }

    private void AssociatedObject_StylusMove(object sender, StylusEventArgs e)
    {
        var canvas = AssociatedObject;

        if (e.InAir)
            _pictureDrawingStartPoint = null;

        if (_pictureDrawingStartPoint.HasValue)
        {
            (Application.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value =
                Resources.String_Draw;

            var adornerLayer = AdornerLayer.GetAdornerLayer(canvas);
            if (adornerLayer != null)
            {
                var adorner = new LetterAdorner(canvas, _pictureDrawingStartPoint);
                if (adorner != null) adornerLayer.Add(adorner);
            }
        }
    }

    private void AssociatedObject_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
            if (e.Source == AssociatedObject)
            {
                _pictureDrawingStartPoint = e.GetPosition(AssociatedObject);

                e.Handled = true;
            }
    }
}