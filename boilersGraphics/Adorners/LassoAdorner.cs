using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using boilersGraphics.Controls;
using boilersGraphics.Dao;
using boilersGraphics.Extensions;
using boilersGraphics.Models;
using boilersGraphics.ViewModels;

namespace boilersGraphics.Adorners;

/// <summary>
///     なげなわツールのAdorner
/// </summary>
public class LassoAdorner : Adorner
{
    private readonly DesignerCanvas _designerCanvas;
    private Point? _endPoint;
    private readonly Pen _lassoPen;
    private readonly Point? _startPoint;
    private readonly HashSet<SelectableDesignerItemViewModelBase> sets = new();
    private readonly Statistics statistics;

    public LassoAdorner(DesignerCanvas designerCanvas, Point? dragStartPoint)
        : base(designerCanvas)
    {
        _designerCanvas = designerCanvas;
        _startPoint = dragStartPoint;
        _lassoPen = new Pen(Brushes.LightSlateGray, 1);
        _lassoPen.DashStyle = new DashStyle(new double[] { 2 }, 1);
        statistics = (Application.Current.MainWindow.DataContext as MainWindowViewModel).Statistics.Value;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            if (!IsMouseCaptured)
                CaptureMouse();

            _endPoint = e.GetPosition(this);

            (Application.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value =
                $"({_startPoint.Value.X}, {_startPoint.Value.Y}) - ({_endPoint.Value.X}, {_endPoint.Value.Y})";

            UpdateSelection();
            InvalidateVisual();
        }
        else
        {
            if (IsMouseCaptured) ReleaseMouseCapture();
        }

        e.Handled = true;
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        // release mouse capture
        if (IsMouseCaptured) ReleaseMouseCapture();

        // remove this adorner from adorner layer
        var adornerLayer = AdornerLayer.GetAdornerLayer(_designerCanvas);
        if (adornerLayer != null)
            adornerLayer.Remove(this);
        UpdateStatisticsCount();

        (Application.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = "";
        (Application.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = "";

        e.Handled = true;
    }

    private void UpdateStatisticsCount()
    {
        statistics.CumulativeTotalOfItemsSelectedWithTheLassoTool += sets.Count();
        var dao = new StatisticsDao();
        dao.Update(statistics);
        sets.Clear();
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        // without a background the OnMouseMove event would not be fired !
        // Alternative: implement a Canvas as a child of this adorner, like
        // the ConnectionAdorner does.
        dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));

        if (_startPoint.HasValue && _endPoint.HasValue)
            dc.DrawRectangle(Brushes.Transparent, _lassoPen, new Rect(_startPoint.Value, _endPoint.Value));
    }


    private T GetParent<T>(Type parentType, DependencyObject dependencyObject) where T : DependencyObject
    {
        var parent = VisualTreeHelper.GetParent(dependencyObject);
        if (parent.GetType() == parentType)
            return (T)parent;

        return GetParent<T>(parentType, parent);
    }


    private void UpdateSelection()
    {
        var vm = _designerCanvas.DataContext as IDiagramViewModel;
        var lassoRect = new Rect(_startPoint.Value, _endPoint.Value);
        var itemsControl = GetParent<ItemsControl>(typeof(ItemsControl), _designerCanvas);

        foreach (var item in vm.Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
                     .Where(x => x is LayerItem)
                     .Select(x => (x as LayerItem).Item.Value))
            if (item is SelectableDesignerItemViewModelBase)
            {
                if (item is ConnectorBaseViewModel connector)
                {
                    var snapPointVM = connector.SnapPoint0VM.Value;
                    UpdateSelectionSnapPoint(lassoRect, snapPointVM);
                    snapPointVM = connector.SnapPoint1VM.Value;
                    UpdateSelectionSnapPoint(lassoRect, snapPointVM);
                }
                else
                {
                    var container = itemsControl.ItemContainerGenerator.ContainerFromItem(item);

                    var itemRect = VisualTreeHelper.GetDescendantBounds((Visual)container);
                    var itemBounds = ((Visual)container).TransformToAncestor(_designerCanvas).TransformBounds(itemRect);

                    if (lassoRect.Contains(itemBounds))
                    {
                        item.IsSelected.Value = true;
                        sets.Add(item);
                    }
                    else
                    {
                        if (!(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
                            item.IsSelected.Value = false;
                    }
                }
            }
    }

    private async Task UpdateSelectionSnapPoint(Rect lassoRect, SnapPointViewModel vm)
    {
        var container = Application.Current.MainWindow.GetChildOfType<DesignerCanvas>()
            .GetVisualChild<LineResizeHandle>(vm);

        var itemRect = VisualTreeHelper.GetDescendantBounds(container);
        var itemBounds = container.TransformToAncestor(_designerCanvas).TransformBounds(itemRect);

        if (lassoRect.Contains(itemBounds))
        {
            vm.IsSelected.Value = true;
            sets.Add(vm);
        }
        else
        {
            if (!(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))) vm.IsSelected.Value = false;
        }
    }
}