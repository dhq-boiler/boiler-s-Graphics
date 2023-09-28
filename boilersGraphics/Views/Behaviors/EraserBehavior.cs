using boilersGraphics.Controls;
using boilersGraphics.Dao;
using boilersGraphics.Helpers;
using boilersGraphics.ViewModels;
using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace boilersGraphics.Views.Behaviors;

public class EraserBehavior : Behavior<DesignerCanvas>
{
    private BrushViewModel currentBrush;

    private bool downFlag;

    public EraserBehavior()
    {
        currentBrush = BrushViewModel.CreateInstance();
    }

    protected override void OnAttached()
    {
        AssociatedObject.StylusDown += AssociatedObject_StylusDown;
        AssociatedObject.StylusMove += AssociatedObject_StylusMove;
        AssociatedObject.TouchDown += AssociatedObject_TouchDown;
        AssociatedObject.MouseDown += AssociatedObject_MouseDown;
        AssociatedObject.MouseMove += AssociatedObject_MouseMove;
        AssociatedObject.MouseUp += AssociatedObject_MouseUp;
        AssociatedObject.TouchUp += AssociatedObject_TouchUp;
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
        AssociatedObject.TouchUp -= AssociatedObject_TouchUp;
        base.OnDetaching();
    }

    private void AssociatedObject_StylusDown(object sender, StylusDownEventArgs e)
    {
        if (downFlag)
            return;

        if (e.Source == AssociatedObject)
        {
            (AssociatedObject.DataContext as DiagramViewModel).MainWindowVM.Recorder.BeginRecode();
            if (!(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                currentBrush.CloseThicknessDialog();
                currentBrush.Dispose();
                currentBrush = null;
            }

            e.StylusDevice.Capture(AssociatedObject);
            var point = e.GetPosition(AssociatedObject);
            if (currentBrush == null)
            {
                currentBrush = BrushViewModel.CreateInstance();
                currentBrush.OpenThicknessDialog();
            }

            var selectable = currentBrush as SelectableDesignerItemViewModelBase;
            EraserInternal.Down((AssociatedObject.DataContext as DiagramViewModel).MainWindowVM, AssociatedObject,
                ref selectable, e, point);
            downFlag = true;
            e.Handled = true;
        }
    }

    private void AssociatedObject_TouchDown(object sender, TouchEventArgs e)
    {
        if (downFlag)
            return;

        if (e.Source == AssociatedObject)
        {
            (AssociatedObject.DataContext as DiagramViewModel).MainWindowVM.Recorder.BeginRecode();
            if (!(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                currentBrush.CloseThicknessDialog();
                currentBrush.Dispose();
                currentBrush = null;
            }

            e.TouchDevice.Capture(AssociatedObject);
            var touchPoint = e.GetTouchPoint(AssociatedObject);
            var point = touchPoint.Position;
            if (currentBrush == null)
            {
                currentBrush = BrushViewModel.CreateInstance();
                currentBrush.OpenThicknessDialog();
            }

            var selectable = currentBrush as SelectableDesignerItemViewModelBase;
            EraserInternal.Down((AssociatedObject.DataContext as DiagramViewModel).MainWindowVM, AssociatedObject,
                ref selectable, e, point);
            downFlag = true;
        }
    }

    private void AssociatedObject_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (downFlag)
            return;

        if (e.LeftButton == MouseButtonState.Pressed)
            if (e.Source == AssociatedObject)
            {
                (AssociatedObject.DataContext as DiagramViewModel).MainWindowVM.Recorder.BeginRecode();
                if (!(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
                {
                    currentBrush.CloseThicknessDialog();
                    currentBrush.Dispose();
                    currentBrush = null;
                }

                e.MouseDevice.Capture(AssociatedObject);
                var point = e.GetPosition(AssociatedObject);
                if (currentBrush == null)
                {
                    currentBrush = BrushViewModel.CreateInstance();
                    currentBrush.OpenThicknessDialog();
                }

                var selectable = currentBrush as SelectableDesignerItemViewModelBase;
                EraserInternal.Down((AssociatedObject.DataContext as DiagramViewModel).MainWindowVM, AssociatedObject,
                    ref selectable, e, point);
                downFlag = true;
            }
    }

    private void AssociatedObject_MouseMove(object sender, MouseEventArgs e)
    {
        if (!downFlag)
            return;

        if (currentBrush == null)
            return;

        if (e.StylusDevice != null)
            return;

        var point = e.GetPosition(AssociatedObject);
        var selectable =
            (VisualTreeHelper.HitTest(AssociatedObject, point)?.VisualHit as FrameworkElement)?.DataContext as
            SelectableDesignerItemViewModelBase;
        if (selectable is BackgroundViewModel) return;
        if (selectable is null) return;
        var designer = selectable as DesignerItemViewModelBase;
        EraserInternal.Erase((AssociatedObject.DataContext as DiagramViewModel).MainWindowVM, ref selectable, point,
            p => GeometryCreator.CreateEllipse(p.X, p.Y, currentBrush.Thickness.Value));
    }

    private void AssociatedObject_StylusMove(object sender, StylusEventArgs e)
    {
        if (!downFlag)
            return;

        if (currentBrush == null)
            return;

        var point = e.GetPosition(AssociatedObject);
        var selectable = currentBrush as SelectableDesignerItemViewModelBase;
        var designer = selectable as DesignerItemViewModelBase;
        EraserInternal.Erase((AssociatedObject.DataContext as DiagramViewModel).MainWindowVM, ref selectable, point,
            p => GeometryCreator.CreateEllipse(p.X, p.Y, currentBrush.Thickness.Value));
    }

    private void AssociatedObject_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (!downFlag)
            return;

        (AssociatedObject.DataContext as DiagramViewModel).MainWindowVM.Recorder.EndRecode();

        UpdateStatisticsCount();

        // release mouse capture
        if (AssociatedObject.IsMouseCaptured) AssociatedObject.ReleaseMouseCapture();
        // release stylus capture
        if (AssociatedObject.IsStylusCaptured) AssociatedObject.ReleaseStylusCapture();

        downFlag = false;
    }

    private static void UpdateStatisticsCount()
    {
        var statistics = (Application.Current.MainWindow.DataContext as MainWindowViewModel).Statistics.Value;
        statistics.NumberOfTimesTheEraserToolHasBeenUsed++;
        var dao = new StatisticsDao();
        dao.Update(statistics);
    }

    private void AssociatedObject_TouchUp(object sender, TouchEventArgs e)
    {
        if (!downFlag)
            return;

        (AssociatedObject.DataContext as DiagramViewModel).MainWindowVM.Recorder.EndRecode();

        // release touch capture
        if (e.TouchDevice.Captured != null) AssociatedObject.ReleaseTouchCapture(e.TouchDevice);

        downFlag = false;
    }
}