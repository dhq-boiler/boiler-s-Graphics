using boilersGraphics.Models;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace boilersGraphics.Helpers
{
    public static class LayerTreeViewItemCollection
    {
        public static void InsertBeforeChildren(ReactiveCollection<LayerTreeViewItemBase> layers, ReactiveCollection<LayerTreeViewItemBase> children, LayerTreeViewItemBase from, LayerTreeViewItemBase to)
        {
            var index = children.IndexOf(to);
            if (index < 0)
                return;

            children.Insert(index, from);
            Rearrangement(layers);
        }

        public static void InsertAfterChildren(ReactiveCollection<LayerTreeViewItemBase> layers, ReactiveCollection<LayerTreeViewItemBase> children, LayerTreeViewItemBase from, LayerTreeViewItemBase to)
        {
            var index = children.IndexOf(to);
            if (index < 0)
                return;

            children.Insert(index + 1, from);
            Rearrangement(layers);
        }

        public static void AddChildren(ReactiveCollection<LayerTreeViewItemBase> layers, ReactiveCollection<LayerTreeViewItemBase> children, LayerTreeViewItemBase to)
        {
            children.Add(to);
            Rearrangement(layers);
        }

        private static void Rearrangement(ReactiveCollection<LayerTreeViewItemBase> layers)
        {
            var queue = new Queue<LayerTreeViewItemBase>(layers);
            LayerTreeViewItemBase item = null;
            int zindex = 0;
            while (queue.Count() > 0)
            {
                item = queue.Dequeue();
                zindex = item.SetZIndex(zindex);
            }
        }
    }
}
