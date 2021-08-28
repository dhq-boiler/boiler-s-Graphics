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
        public static void InsertBeforeChildren(ReactiveCollection<LayerTreeViewItemBase> layers, ReactiveCollection<LayerTreeViewItemBase> children, LayerTreeViewItemBase from, LayerTreeViewItemBase to)
        {
            var mainWindowViewModel = (App.Current.MainWindow.DataContext as MainWindowViewModel);
            var index = children.IndexOf(to);
            if (index < 0)
                return;

            mainWindowViewModel.Recorder.Current.ExecuteInsert(children, from, index);
            Rearrangement(layers);
        }

        public static void InsertAfterChildren(ReactiveCollection<LayerTreeViewItemBase> layers, ReactiveCollection<LayerTreeViewItemBase> children, LayerTreeViewItemBase from, LayerTreeViewItemBase to)
        {
            var mainWindowViewModel = (App.Current.MainWindow.DataContext as MainWindowViewModel);
            var index = children.IndexOf(to);
            if (index < 0)
                return;

            mainWindowViewModel.Recorder.Current.ExecuteInsert(children, from, index + 1);
            Rearrangement(layers);
        }

        public static void AddChildren(ReactiveCollection<LayerTreeViewItemBase> layers, ReactiveCollection<LayerTreeViewItemBase> children, LayerTreeViewItemBase to)
        {
            var mainWindowViewModel = (App.Current.MainWindow.DataContext as MainWindowViewModel);
            mainWindowViewModel.Recorder.Current.ExecuteAdd(children, to);
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
