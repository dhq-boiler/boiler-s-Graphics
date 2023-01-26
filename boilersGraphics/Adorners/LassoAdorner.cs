using boilersGraphics.Controls;
using boilersGraphics.Dao;
using boilersGraphics.Extensions;
using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace boilersGraphics.Adorners
{
    /// <summary>
    /// なげなわツールのAdorner
    /// </summary>
    public class LassoAdorner : Adorner
    {
        private Point? _startPoint;
        private Point? _endPoint;
        private Pen _lassoPen;
        private HashSet<SelectableDesignerItemViewModelBase> sets = new HashSet<SelectableDesignerItemViewModelBase>();

        private DesignerCanvas _designerCanvas;
        private Statistics statistics;

        public LassoAdorner(DesignerCanvas designerCanvas, Point? dragStartPoint)
            : base(designerCanvas)
        {
            _designerCanvas = designerCanvas;
            _startPoint = dragStartPoint;
            _lassoPen = new Pen(Brushes.LightSlateGray, 1);
            _lassoPen.DashStyle = new DashStyle(new double[] { 2 }, 1);
            statistics = (App.Current.MainWindow.DataContext as MainWindowViewModel).Statistics.Value;
        }

        protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (!this.IsMouseCaptured)
                    this.CaptureMouse();

                _endPoint = e.GetPosition(this);

                (App.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = $"({_startPoint.Value.X}, {_startPoint.Value.Y}) - ({_endPoint.Value.X}, {_endPoint.Value.Y})";

                UpdateSelection();
                this.InvalidateVisual();
            }
            else
            {
                if (this.IsMouseCaptured) this.ReleaseMouseCapture();
            }

            e.Handled = true;
        }

        protected override void OnMouseUp(System.Windows.Input.MouseButtonEventArgs e)
        {
            // release mouse capture
            if (this.IsMouseCaptured) this.ReleaseMouseCapture();

            // remove this adorner from adorner layer
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(_designerCanvas);
            if (adornerLayer != null)
                adornerLayer.Remove(this);
            UpdateStatisticsCount();

            (App.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = "";
            (App.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = "";

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
            DependencyObject parent = VisualTreeHelper.GetParent(dependencyObject);
            if (parent.GetType() == parentType)
                return (T)parent;

            return GetParent<T>(parentType, parent);
        }



        private void UpdateSelection()
        {
            IDiagramViewModel vm = (_designerCanvas.DataContext as IDiagramViewModel);
            Rect lassoRect = new Rect(_startPoint.Value, _endPoint.Value);
            ItemsControl itemsControl = GetParent<ItemsControl>(typeof(ItemsControl), _designerCanvas);

            foreach (SelectableDesignerItemViewModelBase item in vm.Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
                                                                          .Where(x => x is LayerItem)
                                                                          .Select(x => (x as LayerItem).Item.Value))
            {
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
                        DependencyObject container = itemsControl.ItemContainerGenerator.ContainerFromItem(item);

                        Rect itemRect = VisualTreeHelper.GetDescendantBounds((Visual)container);
                        Rect itemBounds = ((Visual)container).TransformToAncestor(_designerCanvas).TransformBounds(itemRect);

                        if (lassoRect.Contains(itemBounds))
                        {
                            item.IsSelected.Value = true;
                            sets.Add(item);
                        }
                        else
                        {
                            if (!(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
                            {
                                item.IsSelected.Value = false;
                            }
                        }
                    }
                }
            }
        }

        private async Task UpdateSelectionSnapPoint(Rect lassoRect, SnapPointViewModel vm)
        {
            LineResizeHandle container = App.Current.MainWindow.GetChildOfType<DesignerCanvas>().GetCorrespondingViews<LineResizeHandle>(vm).First();

            Rect itemRect = VisualTreeHelper.GetDescendantBounds((Visual)container);
            Rect itemBounds = ((Visual)container).TransformToAncestor(_designerCanvas).TransformBounds(itemRect);

            if (lassoRect.Contains(itemBounds))
            {
                vm.IsSelected.Value = true;
                sets.Add(vm);
            }
            else
            {
                if (!(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
                {
                    vm.IsSelected.Value = false;
                }
            }
        }
    }
}
