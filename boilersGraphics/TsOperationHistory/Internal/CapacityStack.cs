using System.Collections.Generic;
using System.Collections.Specialized;
using boilersGraphics.Helpers;

namespace TsOperationHistory.Internal;

public interface IStack<T> : IEnumerable<T>, INotifyCollectionChanged
{
    T Push(T item);
    T Peek();
    T Pop();
    void Clear();
}

internal class CapacityStack<T> : ObservableLinkedList<T>, IStack<T>
{
    public CapacityStack(int capacity)
    {
        Capacity = capacity;
    }

    public CapacityStack(IEnumerable<T> collection)
        : base(collection)
    {
    }

    public int Capacity { get; }

    public T Push(T item)
    {
        AddLast(item);
        if (Count > Capacity)
            RemoveFirst();
        return item;
    }

    public T Peek()
    {
        return Last.Value;
    }

    public T Pop()
    {
        var item = Last.Value;
        RemoveLast();
        return item;
    }
}