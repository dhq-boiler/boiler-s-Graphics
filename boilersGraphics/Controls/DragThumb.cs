using boilersGraphics.AttachedProperties;
using boilersGraphics.Extensions;
using boilersGraphics.UserControls;
using boilersGraphics.ViewModels;
using NLog;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using TsOperationHistory;
using TsOperationHistory.Extensions;
using ZLinq;

namespace boilersGraphics.Controls;

public class DragThumb : Thumb
{
    private const double AUTO_SCROLL_EDGE_THRESHOLD = 30d;
    private const int AUTO_SCROLL_INTERVAL_MS = 30;

    private Dictionary<DesignerItemViewModelBase, (double Left, double Top)> _dragStartPositions;

    private DispatcherTimer _autoScrollTimer;
    private ScrollViewer _scrollViewer;

    public DragThumb()
    {
        DragDelta += DragThumb_DragDelta;
        DragCompleted += DragThumb_DragCompleted;
    }

    public OperationRecorder Recorder { get; } =
        new((Application.Current.MainWindow.DataContext as MainWindowViewModel).Controller);

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);

        // Record start positions for undo/redo
        var designerItem = DataContext as DesignerItemViewModelBase;
        if (designerItem != null)
        {
            _dragStartPositions = new Dictionary<DesignerItemViewModelBase, (double, double)>();
            var items = designerItem.SelectedItems.AsValueEnumerable().OfType<DesignerItemViewModelBase>().ToList();
            foreach (var item in items)
            {
                _dragStartPositions[item] = (item.Left.Value, item.Top.Value);
            }
        }

        StartAutoScroll();
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        base.OnMouseUp(e);
        StopAutoScroll();
        (Application.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = "";
        (Application.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = "";

        // Record undo/redo operations for the entire drag
        if (_dragStartPositions != null)
        {
            Recorder.BeginRecode();
            foreach (var kvp in _dragStartPositions)
            {
                var item = kvp.Key;
                var startLeft = kvp.Value.Left;
                var startTop = kvp.Value.Top;
                var endLeft = item.Left.Value;
                var endTop = item.Top.Value;

                if (startLeft != endLeft || startTop != endTop)
                {
                    Recorder.Current.ExecuteSetProperty(item, "Left.Value", endLeft);
                    Recorder.Current.ExecuteSetProperty(item, "Top.Value", endTop);
                }
            }
            Recorder.EndRecode();
            _dragStartPositions = null;
        }

        var item2 = DataContext as DesignerItemViewModelBase;
        LogManager.GetCurrentClassLogger().Info($"Move item {item2.ShowPropertiesAndFields()}");
    }

    private void DragThumb_DragCompleted(object sender, DragCompletedEventArgs e)
    {
        // Belt-and-braces: OnMouseUp normally stops the timer, but if the
        // drag is cancelled (e.g., capture lost), DragCompleted is still fired.
        StopAutoScroll();
    }

    private void DragThumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
        var designerItem = DataContext as DesignerItemViewModelBase;

        if (!designerItem.CanDrag.Value)
            return;

        if (designerItem != null)
        {
            (Application.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value =
                Properties.Resources.String_Move;

            SelectableDesignerItemViewModelBase.Disconnect(designerItem);

            var minLeft = double.MaxValue;
            var minTop = double.MaxValue;

            // we only move DesignerItems
            var designerItems = designerItem.SelectedItems.AsValueEnumerable().OfType<DesignerItemViewModelBase>().ToList();

            if (designerItem.Owner.BackgroundItem.Value.EdgeBrush.Value == Brushes.Magenta
                && designerItem.Owner.BackgroundItem.Value.EdgeThickness.Value == 10)
                designerItems = designerItems.AsValueEnumerable().Union(new DesignerItemViewModelBase[]
                    { designerItem.Owner.BackgroundItem.Value }).ToList();

            foreach (var item in designerItems)
            {
                var left = item.Left.Value;
                var top = item.Top.Value;

                minLeft = double.IsNaN(left) ? 0 : Math.Min(left, minLeft);
                minTop = double.IsNaN(top) ? 0 : Math.Min(top, minTop);
            }

            var deltaHorizontal = Math.Max(-minLeft, e.HorizontalChange);
            var deltaVertical = Math.Max(-minTop, e.VerticalChange);

            foreach (var item in designerItems)
            {
                var matrixTransform = (Parent as Grid).RenderTransform as MatrixTransform;
                var left = item.Left.Value;
                var top = item.Top.Value;

                if (double.IsNaN(left)) left = 0;
                if (double.IsNaN(top)) top = 0;

                double newLeft, newTop;
                if (matrixTransform != null)
                {
                    var dragDelta = new Point(e.HorizontalChange, e.VerticalChange);
                    dragDelta = matrixTransform.Transform(dragDelta);
                    newLeft = left + dragDelta.X;
                    newTop = top + dragDelta.Y;
                }
                else
                {
                    newLeft = left + deltaHorizontal;
                    newTop = top + deltaVertical;
                }

                // Directly set values without OperationRecorder overhead
                item.Left.Value = newLeft;
                item.Top.Value = newTop;

                // Directly update Canvas position for immediate visual feedback
                CanvasPositionBehavior.UpdateCanvasPosition(item, newLeft, newTop);

                (Application.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value =
                    $"(x, y) = ({newLeft}, {newTop})";
            }

            e.Handled = true;
        }
    }

    private static DiagramViewModel ResolveDiagramViewModel(DesignerItemViewModelBase designerItem)
    {
        // designerItem.Owner is typed as the IDiagramViewModel interface, which
        // doesn't carry the auto-scroll settings. Cast to the concrete VM,
        // falling back to MainWindowViewModel.DiagramViewModel in case Owner
        // is null (mirrors how ResizeThumb reaches the same settings).
        if (designerItem?.Owner is DiagramViewModel ownerVm) return ownerVm;
        return (Application.Current?.MainWindow?.DataContext as MainWindowViewModel)?.DiagramViewModel;
    }

    private void StartAutoScroll()
    {
        var designerItem = DataContext as DesignerItemViewModelBase;
        if (designerItem == null) return;
        var diagramVM = ResolveDiagramViewModel(designerItem);
        if (diagramVM?.EnableAutoScrollOnDrag.Value != true) return;

        if (_scrollViewer == null)
        {
            var diagramControl = Application.Current.MainWindow.GetChildOfType<DiagramControl>();
            _scrollViewer = diagramControl?.GetChildOfType<ScrollViewer>();
        }
        if (_scrollViewer == null) return;

        if (_autoScrollTimer == null)
        {
            _autoScrollTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(AUTO_SCROLL_INTERVAL_MS),
            };
            _autoScrollTimer.Tick += AutoScrollTick;
        }
        _autoScrollTimer.Start();
    }

    private void StopAutoScroll()
    {
        _autoScrollTimer?.Stop();
    }

    private void AutoScrollTick(object sender, EventArgs e)
    {
        if (_scrollViewer == null) return;
        var designerItem = DataContext as DesignerItemViewModelBase;
        if (designerItem == null || !designerItem.CanDrag.Value) return;
        var diagramVM = ResolveDiagramViewModel(designerItem);
        if (diagramVM == null || !diagramVM.EnableAutoScrollOnDrag.Value)
        {
            StopAutoScroll();
            return;
        }

        var speed = diagramVM.AutoScrollOnDragSpeed.Value;
        if (speed <= 0d) return;

        var mousePos = Mouse.GetPosition(_scrollViewer);
        var vw = _scrollViewer.ViewportWidth;
        var vh = _scrollViewer.ViewportHeight;

        double dx = 0d, dy = 0d;
        if (mousePos.X < AUTO_SCROLL_EDGE_THRESHOLD) dx = -speed;
        else if (mousePos.X > vw - AUTO_SCROLL_EDGE_THRESHOLD) dx = speed;
        if (mousePos.Y < AUTO_SCROLL_EDGE_THRESHOLD) dy = -speed;
        else if (mousePos.Y > vh - AUTO_SCROLL_EDGE_THRESHOLD) dy = speed;

        if (dx == 0d && dy == 0d) return;

        // Apply scroll, but only count the actual scrolled distance (clamped
        // by the ScrollViewer's scrollable range). Without this, holding the
        // mouse at an edge with no scroll headroom would still translate the
        // dragged items every tick and they'd march off the canvas extent.
        double actualDx = 0d, actualDy = 0d;
        if (dx != 0d)
        {
            var clampedX = Math.Max(0d, Math.Min(_scrollViewer.HorizontalOffset + dx, _scrollViewer.ScrollableWidth));
            actualDx = clampedX - _scrollViewer.HorizontalOffset;
            if (actualDx != 0d) _scrollViewer.ScrollToHorizontalOffset(clampedX);
        }
        if (dy != 0d)
        {
            var clampedY = Math.Max(0d, Math.Min(_scrollViewer.VerticalOffset + dy, _scrollViewer.ScrollableHeight));
            actualDy = clampedY - _scrollViewer.VerticalOffset;
            if (actualDy != 0d) _scrollViewer.ScrollToVerticalOffset(clampedY);
        }

        if (actualDx == 0d && actualDy == 0d) return;

        // Translate the dragged items by the same delta so they stay under
        // the cursor as the viewport scrolls.
        var items = designerItem.SelectedItems.AsValueEnumerable()
            .OfType<DesignerItemViewModelBase>().ToList();
        foreach (var item in items)
        {
            var newLeft = item.Left.Value + actualDx;
            var newTop = item.Top.Value + actualDy;
            item.Left.Value = newLeft;
            item.Top.Value = newTop;
            CanvasPositionBehavior.UpdateCanvasPosition(item, newLeft, newTop);
        }
    }
}
