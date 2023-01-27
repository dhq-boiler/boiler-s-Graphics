using System.Collections.Generic;
using System.Linq;
using boilersGraphics.Models;
using Reactive.Bindings;
using TsOperationHistory;
using TsOperationHistory.Extensions;

namespace boilersGraphics.Helpers;

public static class LayerTreeViewItemCollection
{
    public static void InsertBeforeChildren(OperationRecorder recorder,
        ReactiveCollection<LayerTreeViewItemBase> layers, ReactiveCollection<LayerTreeViewItemBase> children,
        LayerTreeViewItemBase from, LayerTreeViewItemBase to)
    {
        var index = children.IndexOf(to);
        if (index < 0)
            return;

        recorder.Current.ExecuteInsert(children, from, index);
        Rearrangement(recorder, layers);
    }

    public static void InsertAfterChildren(OperationRecorder recorder, ReactiveCollection<LayerTreeViewItemBase> layers,
        ReactiveCollection<LayerTreeViewItemBase> children, LayerTreeViewItemBase from, LayerTreeViewItemBase to)
    {
        var index = children.IndexOf(to);
        if (index < 0)
            return;

        recorder.Current.ExecuteInsert(children, from, index + 1);
        Rearrangement(recorder, layers);
    }

    public static void AddChildren(OperationRecorder recorder, ReactiveCollection<LayerTreeViewItemBase> layers,
        ReactiveCollection<LayerTreeViewItemBase> children, LayerTreeViewItemBase to)
    {
        recorder.Current.ExecuteAdd(children, to);
        Rearrangement(recorder, layers);
    }

    private static void Rearrangement(OperationRecorder recorder, ReactiveCollection<LayerTreeViewItemBase> layers)
    {
        var queue = new Queue<LayerTreeViewItemBase>(layers);
        LayerTreeViewItemBase item = null;
        var zindex = 0;
        while (queue.Count() > 0)
        {
            item = queue.Dequeue();
            zindex = item.SetZIndex(recorder, zindex);
        }
    }
}