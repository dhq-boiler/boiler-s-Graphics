using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TsOperationHistory.Extensions;

namespace boilersGraphics.Helpers
{
    public static class LayerTreeViewItemCollection
    {
        public static void InsertBeforeChildren(MainWindowViewModel mainWindowViewModel, ReactiveCollection<LayerTreeViewItemBase> layers, ReactiveCollection<LayerTreeViewItemBase> children, LayerTreeViewItemBase from, LayerTreeViewItemBase to)
        {
            var index = children.IndexOf(to);
            if (index < 0)
                return;

            mainWindowViewModel.Recorder.Current.ExecuteInsert(children, from, index);
            Rearrangement(mainWindowViewModel, layers);
        }

        public static void InsertAfterChildren(MainWindowViewModel mainWindowViewModel, ReactiveCollection<LayerTreeViewItemBase> layers, ReactiveCollection<LayerTreeViewItemBase> children, LayerTreeViewItemBase from, LayerTreeViewItemBase to)
        {
            var index = children.IndexOf(to);
            if (index < 0)
                return;

            mainWindowViewModel.Recorder.Current.ExecuteInsert(children, from, index + 1);
            Rearrangement(mainWindowViewModel, layers);
        }

        public static void AddChildren(MainWindowViewModel mainWindowViewModel, ReactiveCollection<LayerTreeViewItemBase> layers, ReactiveCollection<LayerTreeViewItemBase> children, LayerTreeViewItemBase to)
        {
            mainWindowViewModel.Recorder.Current.ExecuteAdd(children, to);
            Rearrangement(mainWindowViewModel, layers);
        }

        private static void Rearrangement(MainWindowViewModel mainWindowViewModel, ReactiveCollection<LayerTreeViewItemBase> layers)
        {
            var queue = new Queue<LayerTreeViewItemBase>(layers);
            LayerTreeViewItemBase item = null;
            int zindex = 0;
            while (queue.Count() > 0)
            {
                item = queue.Dequeue();
                zindex = item.SetZIndex(mainWindowViewModel, zindex);
            }
        }
    }
}
