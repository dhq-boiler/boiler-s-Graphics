using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.ViewModels;
using Microsoft.Xaml.Behaviors;
using NLog;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace boilersGraphics.Views.Behaviors;

public class DropperBehavior : Behavior<DesignerCanvas>
{
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

    private void AssociatedObject_TouchDown(object sender, TouchEventArgs e)
    {
        LogManager.GetCurrentClassLogger().Debug("TouchDown");
        SetEdgeColor(e);
    }

    private void AssociatedObject_StylusMove(object sender, StylusEventArgs e)
    {
        LogManager.GetCurrentClassLogger().Debug("StylusMove");
        if (e.StylusDevice.InAir)
        {
            e.Handled = true;
            return;
        }

        if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            SetFillColor(e);
        else
            SetEdgeColor(e);
        e.Handled = true;
    }

    private void AssociatedObject_StylusDown(object sender, StylusDownEventArgs e)
    {
        LogManager.GetCurrentClassLogger().Debug("StylusDown");
        if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            SetFillColor(e);
        else
            SetEdgeColor(e);
        e.Handled = true;
    }

    private void AssociatedObject_MouseMove(object sender, MouseEventArgs e)
    {
        LogManager.GetCurrentClassLogger().Debug("MouseMove");
        if (e.StylusDevice != null && e.StylusDevice.IsValid)
            return;

        if (e.MouseDevice.LeftButton == MouseButtonState.Pressed &&
            (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)))
            SetFillColor(e);
        else if (e.MouseDevice.LeftButton == MouseButtonState.Pressed &&
            (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)))
            AddNewColorSpot(e);
        else if (e.MouseDevice.LeftButton == MouseButtonState.Pressed) SetEdgeColor(e);
    }

    private void AssociatedObject_MouseDown(object sender, MouseButtonEventArgs e)
    {
        LogManager.GetCurrentClassLogger().Debug("MouseDown");
        if (e.StylusDevice != null && e.StylusDevice.IsValid)
            return;

        if (e.MouseDevice.LeftButton == MouseButtonState.Pressed &&
            (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)))
            SetFillColor(e);
        else if (e.MouseDevice.LeftButton == MouseButtonState.Pressed &&
            (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)))
            AddNewColorSpot(e);
        else if (e.MouseDevice.LeftButton == MouseButtonState.Pressed) SetEdgeColor(e);
    }

    private static void SetFillColor(TouchEventArgs e)
    {
        var designerCanvas = Application.Current.MainWindow.GetChildOfType<DesignerCanvas>();
        var rtb = new EffectRenderer(new WpfVisualTreeHelper()).Render(null,
            DesignerCanvas.GetInstance(), DiagramViewModel.Instance, DiagramViewModel.Instance.BackgroundItem.Value,
            null);
        var writeableBitmap = new WriteableBitmap(rtb);
        var position = e.GetTouchPoint(designerCanvas);
        var color = writeableBitmap.GetPixel((int)position.Position.X, (int)position.Position.Y);
        DiagramViewModel.Instance.FillBrush.Value = new SolidColorBrush(color);
    }

    private static void SetEdgeColor(TouchEventArgs e)
    {
        var designerCanvas = Application.Current.MainWindow.GetChildOfType<DesignerCanvas>();
        var rtb = new EffectRenderer(new WpfVisualTreeHelper()).Render(null,
            DesignerCanvas.GetInstance(), DiagramViewModel.Instance, DiagramViewModel.Instance.BackgroundItem.Value,
            null);
        var writeableBitmap = new WriteableBitmap(rtb);
        var position = e.GetTouchPoint(designerCanvas);
        var color = writeableBitmap.GetPixel((int)position.Position.X, (int)position.Position.Y);
        DiagramViewModel.Instance.EdgeBrush.Value = new SolidColorBrush(color);
    }

    private static void SetFillColor(StylusEventArgs e)
    {
        var designerCanvas = Application.Current.MainWindow.GetChildOfType<DesignerCanvas>();
        var rtb = new EffectRenderer(new WpfVisualTreeHelper()).Render(null,
            DesignerCanvas.GetInstance(), DiagramViewModel.Instance, DiagramViewModel.Instance.BackgroundItem.Value,
            null);
        var writeableBitmap = new WriteableBitmap(rtb);
        var position = e.GetPosition(designerCanvas);
        var color = writeableBitmap.GetPixel((int)position.X, (int)position.Y);
        DiagramViewModel.Instance.FillBrush.Value = new SolidColorBrush(color);
    }

    private static void SetEdgeColor(StylusEventArgs e)
    {
        var designerCanvas = Application.Current.MainWindow.GetChildOfType<DesignerCanvas>();
        var rtb = new EffectRenderer(new WpfVisualTreeHelper()).Render(null,
            DesignerCanvas.GetInstance(), DiagramViewModel.Instance, DiagramViewModel.Instance.BackgroundItem.Value,
            null);
        var writeableBitmap = new WriteableBitmap(rtb);
        var position = e.GetPosition(designerCanvas);
        var color = writeableBitmap.GetPixel((int)position.X, (int)position.Y);
        DiagramViewModel.Instance.EdgeBrush.Value = new SolidColorBrush(color);
    }

    private static void SetFillColor(MouseEventArgs e)
    {
        var designerCanvas = Application.Current.MainWindow.GetChildOfType<DesignerCanvas>();
        var rtb = new EffectRenderer(new WpfVisualTreeHelper()).Render(null,
            DesignerCanvas.GetInstance(), DiagramViewModel.Instance, DiagramViewModel.Instance.BackgroundItem.Value,
            null);
        var writeableBitmap = new WriteableBitmap(rtb);
        var position = e.GetPosition(designerCanvas);
        var color = writeableBitmap.GetPixel((int)position.X, (int)position.Y);
        DiagramViewModel.Instance.FillBrush.Value = new SolidColorBrush(color);
    }

    private static void SetEdgeColor(MouseEventArgs e)
    {
        var designerCanvas = Application.Current.MainWindow.GetChildOfType<DesignerCanvas>();
        var rtb = new EffectRenderer(new WpfVisualTreeHelper()).Render(null,
            DesignerCanvas.GetInstance(), DiagramViewModel.Instance, DiagramViewModel.Instance.BackgroundItem.Value,
            null);
        var writeableBitmap = new WriteableBitmap(rtb);
        var position = e.GetPosition(designerCanvas);
        var color = writeableBitmap.GetPixel((int)position.X, (int)position.Y);
        DiagramViewModel.Instance.EdgeBrush.Value = new SolidColorBrush(color);
    }

    private static void AddNewColorSpot(MouseEventArgs e)
    {
        var designerCanvas = Application.Current.MainWindow.GetChildOfType<DesignerCanvas>();
        var rtb = new EffectRenderer(new WpfVisualTreeHelper()).Render(null,
            DesignerCanvas.GetInstance(), DiagramViewModel.Instance, DiagramViewModel.Instance.BackgroundItem.Value,
            null);
        var writeableBitmap = new WriteableBitmap(rtb);
        var position = e.GetPosition(designerCanvas);
        var color = writeableBitmap.GetPixel((int)position.X, (int)position.Y);
        DiagramViewModel.Instance.OverwriteColorSpot(new SolidColorBrush(color));
    }
}