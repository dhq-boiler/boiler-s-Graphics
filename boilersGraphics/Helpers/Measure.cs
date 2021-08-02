using boilersGraphics.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace boilersGraphics.Helpers
{
    public static class Measure
    {
        public static int GetWidth(IEnumerable<SelectableDesignerItemViewModelBase> items, out double minX, out double maxX)
        {
            if (items.Count() == 0)
            {
                throw new ArgumentException("items.Count() > 0");
            }
            minX = double.MaxValue;
            maxX = 0d;
            foreach (var item in items)
            {
                var desingerItem = item as DesignerItemViewModelBase;
                var connectorItem = item as ConnectorBaseViewModel;
                if (desingerItem != null)
                {
                    minX = Math.Min(Math.Min(minX, desingerItem.Left.Value), desingerItem.Left.Value + desingerItem.Width.Value);
                    maxX = Math.Max(Math.Max(maxX, desingerItem.Left.Value), desingerItem.Left.Value + desingerItem.Width.Value);
                }
                if (connectorItem != null)
                {
                    minX = Math.Min(Math.Min(minX, connectorItem.Points[0].X), connectorItem.Points[1].X);
                    maxX = Math.Max(Math.Max(maxX, connectorItem.Points[0].X), connectorItem.Points[1].X);
                }
            }
            return (int)(maxX - minX);
        }

        public static int GetHeight(IEnumerable<SelectableDesignerItemViewModelBase> items, out double minY, out double maxY)
        {
            if (items.Count() == 0)
            {
                throw new ArgumentException("items.Count() > 0");
            }
            minY = double.MaxValue;
            maxY = 0d;
            foreach (var item in items)
            {
                var desingerItem = item as DesignerItemViewModelBase;
                var connectorItem = item as ConnectorBaseViewModel;
                if (desingerItem != null)
                {
                    minY = Math.Min(Math.Min(minY, desingerItem.Top.Value), desingerItem.Top.Value + desingerItem.Height.Value);
                    maxY = Math.Max(Math.Max(maxY, desingerItem.Top.Value), desingerItem.Top.Value + desingerItem.Height.Value);
                }
                if (connectorItem != null)
                {
                    minY = Math.Min(Math.Min(minY, connectorItem.Points[0].X), connectorItem.Points[1].X);
                    maxY = Math.Max(Math.Max(maxY, connectorItem.Points[0].X), connectorItem.Points[1].X);
                }
            }
            return (int)(maxY - minY);
        }
    }
}
