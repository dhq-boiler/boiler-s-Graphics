using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using ZLinq;

namespace boilersGraphics.Helpers;

/// <summary>
///     This class is a LinkedList that can be used in a WPF MVVM scenario. Composition was used instead of inheritance,
///     because inheriting from LinkedList does not allow overriding its methods.
/// </summary>
/// <typeparam name="T"></typeparam>
public class ObservableLinkedList<T> : INotifyCollectionChanged, IEnumerable<T>
{
    private readonly LinkedList<T> m_UnderLyingLinkedList;

    #region Variables accessors

    public int Count => m_UnderLyingLinkedList.Count;

    public LinkedListNode<T> First => m_UnderLyingLinkedList.First;

    public LinkedListNode<T> Last => m_UnderLyingLinkedList.Last;

    #endregion

    #region Constructors

    public ObservableLinkedList()
    {
        m_UnderLyingLinkedList = new LinkedList<T>();
    }

    public ObservableLinkedList(IEnumerable<T> collection)
    {
        m_UnderLyingLinkedList = new LinkedList<T>(collection);
    }

    #endregion

    #region LinkedList<T> Composition

    public LinkedListNode<T> AddAfter(LinkedListNode<T> prevNode, T value)
    {
        var ret = m_UnderLyingLinkedList.AddAfter(prevNode, value);
        OnNotifyCollectionChanged();
        return ret;
    }

    public void AddAfter(LinkedListNode<T> node, LinkedListNode<T> newNode)
    {
        m_UnderLyingLinkedList.AddAfter(node, newNode);
        OnNotifyCollectionChanged();
    }

    public LinkedListNode<T> AddBefore(LinkedListNode<T> node, T value)
    {
        var ret = m_UnderLyingLinkedList.AddBefore(node, value);
        OnNotifyCollectionChanged();
        return ret;
    }

    public void AddBefore(LinkedListNode<T> node, LinkedListNode<T> newNode)
    {
        m_UnderLyingLinkedList.AddBefore(node, newNode);
        OnNotifyCollectionChanged();
    }

    public LinkedListNode<T> AddFirst(T value)
    {
        var ret = m_UnderLyingLinkedList.AddFirst(value);
        OnNotifyCollectionChanged();
        return ret;
    }

    public void AddFirst(LinkedListNode<T> node)
    {
        m_UnderLyingLinkedList.AddFirst(node);
        OnNotifyCollectionChanged();
    }

    public LinkedListNode<T> AddLast(T value)
    {
        var ret = m_UnderLyingLinkedList.AddLast(value);
        OnNotifyCollectionChanged();
        return ret;
    }

    public void AddLast(LinkedListNode<T> node)
    {
        m_UnderLyingLinkedList.AddLast(node);
        OnNotifyCollectionChanged();
    }

    public void Clear()
    {
        m_UnderLyingLinkedList.Clear();
        OnNotifyCollectionChanged();
    }

    public bool Contains(T value)
    {
        return m_UnderLyingLinkedList.Contains(value);
    }

    public void CopyTo(T[] array, int index)
    {
        m_UnderLyingLinkedList.CopyTo(array, index);
    }

    public bool LinkedListEquals(object obj)
    {
        return m_UnderLyingLinkedList.Equals(obj);
    }

    public LinkedListNode<T> Find(T value)
    {
        return m_UnderLyingLinkedList.Find(value);
    }

    public LinkedListNode<T> FindLast(T value)
    {
        return m_UnderLyingLinkedList.FindLast(value);
    }

    public Type GetLinkedListType()
    {
        return m_UnderLyingLinkedList.GetType();
    }

    public bool Remove(T value)
    {
        var ret = m_UnderLyingLinkedList.Remove(value);
        OnNotifyCollectionChanged();
        return ret;
    }

    public void Remove(LinkedListNode<T> node)
    {
        m_UnderLyingLinkedList.Remove(node);
        OnNotifyCollectionChanged();
    }

    public void RemoveFirst()
    {
        m_UnderLyingLinkedList.RemoveFirst();
        OnNotifyCollectionChanged();
    }

    public void RemoveLast()
    {
        m_UnderLyingLinkedList.RemoveLast();
        OnNotifyCollectionChanged();
    }

    public T this[int index] => m_UnderLyingLinkedList.AsValueEnumerable().ElementAt(index);

    #endregion

    #region INotifyCollectionChanged Members

    public event NotifyCollectionChangedEventHandler CollectionChanged;

    public void OnNotifyCollectionChanged()
    {
        if (CollectionChanged != null)
            CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    #endregion

    #region IEnumerable Members

    IEnumerator IEnumerable.GetEnumerator()
    {
        return (m_UnderLyingLinkedList as IEnumerable<T>).GetEnumerator();
    }

    public IEnumerator<T> GetEnumerator()
    {
        return (m_UnderLyingLinkedList as IEnumerable<T>).GetEnumerator();
    }

    #endregion
}