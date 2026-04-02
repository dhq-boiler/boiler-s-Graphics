using boilersGraphics.AttachedProperties;
using boilersGraphics.ViewModels;
using NLog;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using TsOperationHistory;
using TsOperationHistory.Extensions;
using ZLinq;

namespace boilersGraphics.Controls;

public class DragThumb : Thumb
{
    private Dictionary<DesignerItemViewModelBase, (double Left, double Top)> _dragStartPositions;

    public DragThumb()
    {
        DragDelta += DragThumb_DragDelta;
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
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        base.OnMouseUp(e);
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
}
