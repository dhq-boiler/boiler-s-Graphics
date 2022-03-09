using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;

namespace TsOperationHistory.Internal
{
    public class UndoStack<T> : BindableBase, IStack<T>
    {
        public bool CanUndo => Undos.Value.Any();
        public bool CanRedo => Redos.Value.Any();

        public int Count => Undos.Value.Union(Redos.Value).Count();

        public UndoStack(int capacity)
        {
            Undos.Value = new CapacityStack<T>(capacity);
            Redos.Value = new CapacityStack<T>(capacity);
            Redos.Subscribe(x =>
            {
                Debug.WriteLine(string.Join(", ", Redos.Select(y => y?.ToString() ?? "null")));
            });
            History = Undos.Value.CollectionChangedAsObservable()
                                 .Merge(Redos.Value.CollectionChangedAsObservable())
                                 .Select(_ => (IStack<T>)new CapacityStack<T>(Undos.Value.Union(Redos.Value.Reverse())))
                                 .ToReadOnlyReactivePropertySlim();
        }

        public ReactivePropertySlim<IStack<T>> Undos { get; } = new ReactivePropertySlim<IStack<T>>();
        public ReactivePropertySlim<IStack<T>> Redos { get; } = new ReactivePropertySlim<IStack<T>>();
        public ReadOnlyReactivePropertySlim<IStack<T>> History { get; }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public T Undo()
        {
            return Redos.Value.Push(Undos.Value.Pop());
        }

        public T Redo()
        {
            return  Undos.Value.Push(Redos.Value.Pop());
        }

        public T Peek()
        {
            if (CanUndo is false)
                return default(T);

            return Undos.Value.Peek();
        }

        public T Push(T item)
        {
            Redos.Value.Clear();
            return Undos.Value.Push(item);
        }

        public T Pop()
        {
            Redos.Value.Clear();

            if (CanUndo is false)
                return default(T);

            return Undos.Value.Pop();
        }

        public void Clear()
        {
            Undos.Value.Clear();
            Redos.Value.Clear();
        }

        public T this[int index]
        {
            get
            {
                return Undos.Value.Union(Redos.Value.Reverse()).ElementAt(index);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Undos.Value.Union(Redos.Value.Reverse()).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerable<T> RedoStack => Redos.Value;
    }
}
