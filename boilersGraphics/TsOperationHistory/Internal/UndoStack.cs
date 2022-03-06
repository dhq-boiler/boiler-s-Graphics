using Prism.Mvvm;
using Reactive.Bindings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;

namespace TsOperationHistory.Internal
{
    public class UndoStack<T> : BindableBase, IStack<T>
    {
        public bool CanUndo => Undos.Value.Any();
        public bool CanRedo => Redos.Value.Any();

        public int Count => Undos.Value.Count();

        public UndoStack(int capacity)
        {
            Undos.Value = new CapacityStack<T>(capacity);
            Redos.Value = new CapacityStack<T>(capacity);
            Redos.Subscribe(x =>
            {
                Debug.WriteLine(string.Join(", ", Redos.Select(y => y?.ToString() ?? "null")));
            });
            History = Observable.Concat(Undos, Redos).ToReadOnlyReactivePropertySlim();
        }

        public ReactivePropertySlim<IStack<T>> Undos { get; } = new ReactivePropertySlim<IStack<T>>();
        public ReactivePropertySlim<IStack<T>> Redos { get; } = new ReactivePropertySlim<IStack<T>>();
        public ReadOnlyReactivePropertySlim<IStack<T>> History { get; }

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

        public IEnumerator<T> GetEnumerator()
        {
            return Undos.Value.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerable<T> RedoStack => Redos.Value;
    }
}
