using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using boilersGraphics.Adorners;
using boilersGraphics.Controls;
using boilersGraphics.Helpers;
using boilersGraphics.Properties;
using boilersGraphics.ViewModels;
using Microsoft.Xaml.Behaviors;
using Prism.Services.Dialogs;

namespace boilersGraphics.Views.Behaviors;

public class SliceBehavior : Behavior<DesignerCanvas>
{
    private readonly IDialogService dialogService;
    private Point? _rectangleStartPoint;
    private readonly SnapAction snapAction;

    public SliceBehavior(IDialogService dialogService)
    {
        snapAction = new SnapAction();
        this.dialogService = dialogService;
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
            e.Handled = true;
        }
    }

    private void AssociatedObject_TouchDown(object sender, TouchEventArgs e)
    {
        if (e.Source == AssociatedObject)
        {
            var touchPoint = e.GetTouchPoint(AssociatedObject);
            _rectangleStartPoint = touchPoint.Position;
        }
    }

    private void AssociatedObject_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.StylusDevice != null)
            return;

        if (e.LeftButton == MouseButtonState.Pressed)
            if (e.Source == AssociatedObject)
                _rectangleStartPoint = e.GetPosition(AssociatedObject);
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
            _rectangleStartPoint = current;
            (Application.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value =
                Resources.String_Slice;

            var adornerLayer = AdornerLayer.GetAdornerLayer(canvas);
            if (adornerLayer != null)
            {
                var adorner = new SliceAdorner(canvas, _rectangleStartPoint, dialogService);
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
            _rectangleStartPoint = current;
            (Application.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value =
                Resources.String_Slice;

            var adornerLayer = AdornerLayer.GetAdornerLayer(canvas);
            if (adornerLayer != null)
            {
                var adorner = new SliceAdorner(canvas, _rectangleStartPoint, dialogService);
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