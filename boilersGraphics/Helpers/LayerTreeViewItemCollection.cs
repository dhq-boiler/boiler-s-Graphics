using boilersGraphics.Models;
using ObservableCollections;
using System.Collections.Generic;
using TsOperationHistory;
using TsOperationHistory.Extensions;
using ZLinq;

namespace boilersGraphics.Helpers;

public static class LayerTreeViewItemCollection
{
    public static void InsertBeforeChildren(OperationRecorder recorder,
        NotifyCollectionChangedSynchronizedViewList<LayerTreeViewItemBase> layers, NotifyCollectionChangedSynchronizedViewList<LayerTreeViewItemBase> children,
        LayerTreeViewItemBase from, LayerTreeViewItemBase to)
    {
        var index = children.IndexOf(to);
        if (index < 0)
            return;

        recorder.Current.ExecuteInsert(children, from, index);
        Rearrangement(recorder, layers);
    }

    public static void InsertAfterChildren(OperationRecorder recorder, NotifyCollectionChangedSynchronizedViewList<LayerTreeViewItemBase> layers,
        NotifyCollectionChangedSynchronizedViewList<LayerTreeViewItemBase> children, LayerTreeViewItemBase from, LayerTreeViewItemBase to)
    {
        var index = children.IndexOf(to);
        if (index < 0)
            return;

        recorder.Current.ExecuteInsert(children, from, index + 1);
        Rearrangement(recorder, layers);
    }

    public static void AddChildren(OperationRecorder recorder, NotifyCollectionChangedSynchronizedViewList<LayerTreeViewItemBase> layers,
        NotifyCollectionChangedSynchronizedViewList<LayerTreeViewItemBase> children, LayerTreeViewItemBase to)
    {
        recorder.Current.ExecuteAdd(children, to);
        Rearrangement(recorder, layers);
    }

    private static void Rearrangement(OperationRecorder recorder, NotifyCollectionChangedSynchronizedViewList<LayerTreeViewItemBase> layers)
    {
        var snapshot = layers.AsValueEnumerable().ToList();
        var zindex = 0;
        foreach (var item in snapshot)
        {
            zindex = item.SetZIndex(recorder, zindex);
        }
    }
}