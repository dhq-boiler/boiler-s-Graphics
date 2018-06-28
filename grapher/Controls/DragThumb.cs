using grapher.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;

namespace grapher.Controls
{
    public class DragThumb : Thumb
    {
        public DragThumb()
        {
            base.DragDelta += new DragDeltaEventHandler(DragThumb_DragDelta);
        }

        private void DragThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            DesignerItemViewModelBase designerItem = this.DataContext as DesignerItemViewModelBase;

            if (designerItem != null && designerItem.IsSelected)
            {
                double minLeft = double.MaxValue;
                double minTop = double.MaxValue;

                // we only move DesignerItems
                var designerItems = designerItem.SelectedItems;

                foreach (DesignerItemViewModelBase item in designerItems.OfType<DesignerItemViewModelBase>())
                {
                    double left = item.Left.Value;
                    double top = item.Top.Value;
                    minLeft = double.IsNaN(left) ? 0 : Math.Min(left, minLeft);
                    minTop = double.IsNaN(top) ? 0 : Math.Min(top, minTop);

                    double deltaHorizontal = Math.Max(-minLeft, e.HorizontalChange);
                    double deltaVertical = Math.Max(-minTop, e.VerticalChange);
                    item.Left.Value += deltaHorizontal;
                    item.Top.Value += deltaVertical;
                }
                e.Handled = true;
            }
        }
    }
}
