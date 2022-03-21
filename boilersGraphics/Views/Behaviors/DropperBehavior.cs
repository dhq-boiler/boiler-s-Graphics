using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using Microsoft.Xaml.Behaviors;
using NLog;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace boilersGraphics.Views.Behaviors
{
    public class DropperBehavior : Behavior<DesignerCanvas>
    {
        protected override void OnAttached()
        {
            this.AssociatedObject.StylusDown += AssociatedObject_StylusDown;
            this.AssociatedObject.StylusMove += AssociatedObject_StylusMove;
            this.AssociatedObject.TouchDown += AssociatedObject_TouchDown;
            this.AssociatedObject.MouseDown += AssociatedObject_MouseDown;
            this.AssociatedObject.MouseMove += AssociatedObject_MouseMove;
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            this.AssociatedObject.StylusDown -= AssociatedObject_StylusDown;
            this.AssociatedObject.StylusMove -= AssociatedObject_StylusMove;
            this.AssociatedObject.TouchDown -= AssociatedObject_TouchDown;
            this.AssociatedObject.MouseDown -= AssociatedObject_MouseDown;
            this.AssociatedObject.MouseMove -= AssociatedObject_MouseMove;
            base.OnDetaching();
        }

        private void AssociatedObject_TouchDown(object sender, System.Windows.Input.TouchEventArgs e)
        {
            LogManager.GetCurrentClassLogger().Debug("TouchDown");
            SetEdgeColor(e);
        }

        private void AssociatedObject_StylusMove(object sender, System.Windows.Input.StylusEventArgs e)
        {
            LogManager.GetCurrentClassLogger().Debug("StylusMove");
            if (e.StylusDevice.InAir)
            {
                e.Handled = true;
                return;
            }

            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                SetFillColor(e);
            }
            else
            {
                SetEdgeColor(e);
            }
            e.Handled = true;
        }

        private void AssociatedObject_StylusDown(object sender, System.Windows.Input.StylusDownEventArgs e)
        {
            LogManager.GetCurrentClassLogger().Debug("StylusDown");
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                SetFillColor(e);
            }
            else
            {
                SetEdgeColor(e);
            }
            e.Handled = true;
        }

        private void AssociatedObject_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            LogManager.GetCurrentClassLogger().Debug("MouseMove");
            if (e.StylusDevice != null && e.StylusDevice.IsValid)
                return;

            if (e.MouseDevice.LeftButton == MouseButtonState.Pressed &&
                (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)))
            {
                SetFillColor(e);
            }
            else if (e.MouseDevice.LeftButton == MouseButtonState.Pressed)
            {
                SetEdgeColor(e);
            }
        }

        private void AssociatedObject_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            LogManager.GetCurrentClassLogger().Debug("MouseDown");
            if (e.StylusDevice != null && e.StylusDevice.IsValid)
                return;

            if (e.MouseDevice.LeftButton == MouseButtonState.Pressed && 
                (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)))
            {
                SetFillColor(e);
            }
            else if (e.MouseDevice.LeftButton == MouseButtonState.Pressed)
            {
                SetEdgeColor(e);
            } 
        }

        private static void SetFillColor(TouchEventArgs e)
        {
            var designerCanvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();
            var rtb = GetCurrentBitmap();
            var writeableBitmap = new WriteableBitmap(rtb);
            var position = e.GetTouchPoint(designerCanvas);
            var color = writeableBitmap.GetPixel((int)position.Position.X, (int)position.Position.Y);
            (App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel.FillBrush.Value = new SolidColorBrush(color);
        }

        private static void SetEdgeColor(TouchEventArgs e)
        {
            var designerCanvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();
            var rtb = GetCurrentBitmap();
            var writeableBitmap = new WriteableBitmap(rtb);
            var position = e.GetTouchPoint(designerCanvas);
            var color = writeableBitmap.GetPixel((int)position.Position.X, (int)position.Position.Y);
            (App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel.EdgeBrush.Value = new SolidColorBrush(color);
        }

        private static void SetFillColor(StylusEventArgs e)
        {
            var designerCanvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();
            var rtb = GetCurrentBitmap();
            var writeableBitmap = new WriteableBitmap(rtb);
            var position = e.GetPosition(designerCanvas);
            var color = writeableBitmap.GetPixel((int)position.X, (int)position.Y);
            (App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel.FillBrush.Value = new SolidColorBrush(color);
        }

        private static void SetEdgeColor(StylusEventArgs e)
        {
            var designerCanvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();
            var rtb = GetCurrentBitmap();
            var writeableBitmap = new WriteableBitmap(rtb);
            var position = e.GetPosition(designerCanvas);
            var color = writeableBitmap.GetPixel((int)position.X, (int)position.Y);
            (App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel.EdgeBrush.Value = new SolidColorBrush(color);
        }

        private static void SetFillColor(MouseEventArgs e)
        {
            var designerCanvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();
            var rtb = GetCurrentBitmap();
            var writeableBitmap = new WriteableBitmap(rtb);
            var position = e.GetPosition(designerCanvas);
            var color = writeableBitmap.GetPixel((int)position.X, (int)position.Y);
            (App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel.FillBrush.Value = new SolidColorBrush(color);
        }

        private static void SetEdgeColor(MouseEventArgs e)
        {
            var designerCanvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();
            var rtb = GetCurrentBitmap();
            var writeableBitmap = new WriteableBitmap(rtb);
            var position = e.GetPosition(designerCanvas);
            var color = writeableBitmap.GetPixel((int)position.X, (int)position.Y);
            (App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel.EdgeBrush.Value = new SolidColorBrush(color);
        }

        private static RenderTargetBitmap GetCurrentBitmap()
        {
            var diagramViewModel = (App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel;
            var designerCanvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)diagramViewModel.BackgroundItem.Value.Width.Value, (int)diagramViewModel.BackgroundItem.Value.Height.Value, 96, 96, PixelFormats.Pbgra32);
            DrawingVisual visual = new DrawingVisual();
            var background = diagramViewModel.BackgroundItem.Value;

            using (DrawingContext context = visual.RenderOpen())
            {
                var views = designerCanvas.GetCorrespondingViews<FrameworkElement>(background);
                var view = views.First(x => x.GetType() == background.GetViewType());
                var bounds = VisualTreeHelper.GetDescendantBounds(view);
                Rect rect = bounds;
                VisualBrush brush = new VisualBrush(view);
                brush.Stretch = Stretch.None;
                view.UpdateLayout();
                context.DrawRectangle(brush, null, rect);
            }
            renderTargetBitmap.Render(visual);

            using (DrawingContext context = visual.RenderOpen())
            {
                foreach (var item in diagramViewModel.AllItems.Value.Except(new SelectableDesignerItemViewModelBase[] { background }))
                {
                    var layerItems = diagramViewModel.Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children);
                    if (!layerItems.OfType<LayerItem>().First(x => x.Item.Value == item).IsVisible.Value)
                        continue;
                    var views = designerCanvas.GetCorrespondingViews<FrameworkElement>(item);
                    var view = views.First(x => x.GetType() == item.GetViewType());
                    view.SnapsToDevicePixels = true;
                    VisualBrush brush = new VisualBrush(view);
                    brush.Stretch = Stretch.None;
                    brush.TileMode = TileMode.None;
                    Rect rect = new Rect();
                    if (item is DesignerItemViewModelBase designerItem)
                    {
                        var bounds = VisualTreeHelper.GetDescendantBounds(view);
                        rect = new Rect(designerItem.Left.Value, designerItem.Top.Value, designerItem.Width.Value, designerItem.Height.Value);
                        context.PushTransform(new RotateTransform(item.RotationAngle.Value, (item as DesignerItemViewModelBase).CenterX.Value, (item as DesignerItemViewModelBase).CenterY.Value));
                        context.DrawRectangle(brush, null, rect);
                        context.Pop();
                    }
                    else if (item is ConnectorBaseViewModel connector)
                    {
                        var bounds = VisualTreeHelper.GetDescendantBounds(view);
                        rect = new Rect(connector.LeftTop.Value, bounds.Size);
                        context.DrawRectangle(brush, null, rect);
                    }
                }
            }
            renderTargetBitmap.Render(visual);
            return renderTargetBitmap;
        }
    }
}
