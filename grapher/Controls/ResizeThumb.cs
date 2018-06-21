using grapher.Extensions;
using grapher.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace grapher.Controls
{
    public class ResizeThumb : Thumb
    {
        public ResizeThumb()
        {
            base.DragDelta += new DragDeltaEventHandler(ResizeThumb_DragDelta);
        }

        private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var designerItem = this.DataContext as DesignerItemViewModelBase;

            if (designerItem != null && designerItem.IsSelected)
            {
                double minLeft, minTop, minDeltaHorizontal, minDeltaVertical;
                double dragDeltaVertical, dragDeltaHorizontal;

                // only resize DesignerItems
                var selectedDesignerItems = from item in designerItem.Parent.SelectedItems
                                            where item is DesignerItemViewModelBase
                                            select item;

                CalculateDragLimits(selectedDesignerItems, out minLeft, out minTop,
                                    out minDeltaHorizontal, out minDeltaVertical);

                foreach (var item in selectedDesignerItems)
                {
                    if (item is DesignerItemViewModelBase)
                    {
                        var viewModel = item as DesignerItemViewModelBase;
                        switch (base.VerticalAlignment)
                        {
                            case VerticalAlignment.Bottom:
                                dragDeltaVertical = Math.Min(-e.VerticalChange, minDeltaVertical);
                                viewModel.Height = viewModel.Height - dragDeltaVertical;
                                break;
                            case VerticalAlignment.Top:
                                double top = viewModel.Top;
                                dragDeltaVertical = Math.Min(Math.Max(-minTop, e.VerticalChange), minDeltaVertical);
                                viewModel.Top = top + dragDeltaVertical;
                                viewModel.Height = viewModel.Height - dragDeltaVertical;
                                break;
                            default:
                                break;
                        }

                        switch (base.HorizontalAlignment)
                        {
                            case HorizontalAlignment.Left:
                                double left = viewModel.Left;
                                dragDeltaHorizontal = Math.Min(Math.Max(-minLeft, e.HorizontalChange), minDeltaHorizontal);
                                viewModel.Left = left + dragDeltaHorizontal;
                                viewModel.Width = viewModel.Width - dragDeltaHorizontal;
                                break;
                            case HorizontalAlignment.Right:
                                dragDeltaHorizontal = Math.Min(-e.HorizontalChange, minDeltaHorizontal);
                                viewModel.Width = viewModel.Width - dragDeltaHorizontal;
                                break;
                            default:
                                break;
                        }
                    }
                }
                e.Handled = true;
            }
        }

        private static void CalculateDragLimits(IEnumerable<SelectableDesignerItemViewModelBase> selectedDesignerItems, out double minLeft, out double minTop, out double minDeltaHorizontal, out double minDeltaVertical)
        {
            minLeft = double.MaxValue;
            minTop = double.MaxValue;
            minDeltaHorizontal = double.MaxValue;
            minDeltaVertical = double.MaxValue;

            // drag limits are set by these parameters: canvas top, canvas left, minHeight, minWidth
            // calculate min value for each parameter for each item
            foreach (var item in selectedDesignerItems)
            {
                if (item is DesignerItemViewModelBase)
                {
                    var viewModel = item as DesignerItemViewModelBase;
                    double left = viewModel.Left;
                    double top = viewModel.Top;

                    minLeft = double.IsNaN(left) ? 0 : Math.Min(left, minLeft);
                    minTop = double.IsNaN(top) ? 0 : Math.Min(top, minTop);

                    minDeltaVertical = Math.Min(minDeltaVertical, viewModel.Height - viewModel.MinHeight);
                    minDeltaHorizontal = Math.Min(minDeltaHorizontal, viewModel.Width - viewModel.MinWidth);
                }
                else if (item is ConnectorBaseViewModel)
                {
                    var viewModel = item as ConnectorBaseViewModel;
                    double left = Math.Min(viewModel.SourceA.X, viewModel.SourceB.X);
                    double top = Math.Min(viewModel.SourceA.Y, viewModel.SourceB.Y);

                    double width = Math.Max(viewModel.SourceA.X, viewModel.SourceB.X) - Math.Min(viewModel.SourceA.X, viewModel.SourceB.X);
                    double height = Math.Max(viewModel.SourceA.Y, viewModel.SourceB.Y) - Math.Min(viewModel.SourceA.Y, viewModel.SourceB.Y);

                    minDeltaVertical = Math.Min(minDeltaVertical, height);
                    minDeltaHorizontal = Math.Min(minDeltaHorizontal, width);
                }
            }
        }
    }
}
