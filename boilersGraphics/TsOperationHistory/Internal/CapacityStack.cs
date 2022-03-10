using boilersGraphics.Helpers;
using Prism.Mvvm;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace TsOperationHistory.Internal
{
    public interface IStack<T> : IEnumerable<T>, INotifyCollectionChanged
    {
        T Push(T item);
        T Peek();
        T Pop();
        void Clear();
    }

    internal class CapacityStack<T> : ObservableLinkedList<T>, IStack<T>
    {
        public CapacityStack(int capacity) { Capacity = capacity; }

        public CapacityStack(IEnumerable<T> collection)
            : base(collection)
        { }

        public int Capacity { get; }

        public T Push(T item)
        {
            AddLast(item);
            if (Count > Capacity)
                RemoveFirst();
            return item;
        }
        public T Peek() => this.Last.Value;

        public T Pop()
        {
            var item = this.Last.Value;
            RemoveLast();
            return item;
        }
    }
}
