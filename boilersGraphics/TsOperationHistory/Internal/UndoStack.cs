using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.ViewModels;
using boilersGraphics.Views;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace TsOperationHistory.Internal;

public class UndoStack<T> : BindableBase, IStack<T>, IDisposable where T : IOperation
{
    private CompositeDisposable disposables = new();
    private bool disposedValue;

    public UndoStack(int capacity)
    {
        Undos.Value = new CapacityStack<T>(capacity);
        Redos.Value = new CapacityStack<T>(capacity);
        Redos.Subscribe(x => { Debug.WriteLine(string.Join(", ", Redos.Select(y => y?.ToString() ?? "null"))); });
        History = Undos.Value.CollectionChangedAsObservable()
            .Merge(Redos.Value.CollectionChangedAsObservable())
            .Select(_ => (IStack<T>)new CapacityStack<T>(Undos.Value.Union(Redos.Value.Reverse())))
            .Do(x => ScrollToCurrentPosition())
            .ToReadOnlyReactivePropertySlim();
    }

    public bool CanUndo => Undos.Value.Any();
    public bool CanRedo => Redos.Value.Any();

    public int Count => Undos.Value.Union(Redos.Value).Count();

    public ReactivePropertySlim<IStack<T>> Undos { get; } = new();
    public ReactivePropertySlim<IStack<T>> Redos { get; } = new();
    public ReadOnlyReactivePropertySlim<IStack<T>> History { get; }

    public T this[int index] => Undos.Value.Union(Redos.Value.Reverse()).ElementAt(index);

    public IEnumerable<T> RedoStack => Redos.Value;

    public void Dispose()
    {
        // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public event NotifyCollectionChangedEventHandler CollectionChanged;

    public T Peek()
    {
        if (CanUndo is false)
            return default;

        return Undos.Value.Peek();
    }

    public T Push(T item)
    {
        Redos.Value.Clear();
        item.Message.Value = $"#{Undos.Value.Count() + 1} {item.Message.Value}";
        return Undos.Value.Push(item);
    }

    public T Pop()
    {
        Redos.Value.Clear();

        if (CanUndo is false)
            return default;

        return Undos.Value.Pop();
    }

    public void Clear()
    {
        Undos.Value.Clear();
        Redos.Value.Clear();
    }

    public IEnumerator<T> GetEnumerator()
    {
        return Undos.Value.Union(Redos.Value.Reverse()).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private void ScrollToCurrentPosition()
    {
        var undoHistoryVM = UndoHistoryViewModel.Instance;
        if (undoHistoryVM == null)
            return;
        var treeview = Application.Current.MainWindow.GetVisualChild<BindableSelectedItemTreeView>(undoHistoryVM);
        if (treeview == null)
            return;
        var scrollViewer = treeview.GetChildOfType<ScrollViewer>();
        if (scrollViewer == null)
            return;
        if (History.Value == null)
            return;
        var h = scrollViewer.ExtentHeight;
        if (History.Value.Count() == 0)
            return;
        var hunit = h / History.Value.Count();
        if (double.IsNaN(hunit))
            return;
        var targetH = hunit * undoHistoryVM.CurrentPosition.Value;
        scrollViewer.ScrollToVerticalOffset(targetH);
    }

    public T Undo()
    {
        return Redos.Value.Push(Undos.Value.Pop());
    }

    public T Redo()
    {
        return Undos.Value.Push(Redos.Value.Pop());
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                Undos.Dispose();
                Redos.Dispose();
                History.Dispose();
                disposables.Dispose();
            }

            disposables = null;
            disposedValue = true;
        }
    }
}