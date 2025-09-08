using R3;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using ZLinq;

namespace TsOperationHistory;

/// <summary>
///     追加オペレーション
/// </summary>
public class InsertOperation<T> : IOperation
{
    private readonly Func<IList<T>> _generator;
    private readonly int _insertIndex;
    private readonly IList<T> _list;
    private readonly T _property;


    public InsertOperation(Func<IList<T>> listGenerator, T insertValue, int insertIndex = -1)
    {
        Debug.Assert(listGenerator != null);
        _generator = listGenerator;
        _property = insertValue;
        _insertIndex = insertIndex;
    }

    public InsertOperation(IList<T> list, T insertValue, int insertIndex = -1, string message = null)
    {
        Debug.Assert(list != null);
        _list = list;
        _property = insertValue;
        _insertIndex = insertIndex;
        Message.Value = message;
    }

    public BindableReactiveProperty<string> Message { get; } = new();
    public BindableReactiveProperty<Visibility> ArrowVisibility { get; } = new(Visibility.Hidden);

    public void RollForward()
    {
        if (_insertIndex < 0)
            get_list().Add(_property);
        else
            get_list().Insert(_insertIndex, _property);
    }

    public void Rollback()
    {
        var list = get_list();
        list.RemoveAt(_insertIndex < 0 ? list.Count - 1 : _insertIndex);
    }

    private IList<T> get_list()
    {
        return _list ?? _generator?.Invoke();
    }
}

/// <summary>
///     削除オペレーション
///     RollBack時に削除位置も復元する
/// </summary>
public class RemoveOperation<T> : IOperation
{
    private readonly Func<IList<T>> _generator;
    private readonly IList<T> _list;
    private readonly T _property;
    private int _insertIndex = -1;

    public RemoveOperation(Func<IList<T>> listGenerator, T removeValue)
    {
        Debug.Assert(listGenerator != null);
        _generator = listGenerator;
        _property = removeValue;
    }

    public RemoveOperation(IList<T> list, T removeValue, string message = null)
    {
        Debug.Assert(list != null);
        _list = list;
        _property = removeValue;
        Message.Value = message;
    }

    public BindableReactiveProperty<string> Message { get; } = new();
    public BindableReactiveProperty<Visibility> ArrowVisibility { get; } = new(Visibility.Hidden);

    public void RollForward()
    {
        _insertIndex = get_list().IndexOf(_property);

        if (_insertIndex < 0)
            return;

        get_list().RemoveAt(_insertIndex);
    }

    public void Rollback()
    {
        if (_insertIndex < 0)
            return;

        get_list().Insert(_insertIndex, _property);
    }

    private IList<T> get_list()
    {
        return _list ?? _generator?.Invoke();
    }
}

/// <summary>
///     インデックス指定削除オペレーション
///     RollBack時に削除位置も復元する
/// </summary>
public class RemoveAtOperation : IOperation
{
    private readonly Func<IList> _generator;
    private readonly int _index;
    private readonly IList _list;
    private object _data;

    public RemoveAtOperation(Func<IList> listGenerator, int index)
    {
        Debug.Assert(listGenerator != null);
        Debug.Assert(_index >= 0);
        _generator = listGenerator;
        _index = index;
    }

    public RemoveAtOperation(IList list, int index)
    {
        Debug.Assert(list != null);
        Debug.Assert(_index >= 0);
        _list = list;
        _index = index;
    }

    public BindableReactiveProperty<string> Message { get; } = new();
    public BindableReactiveProperty<Visibility> ArrowVisibility { get; } = new(Visibility.Hidden);

    public void RollForward()
    {
        var list = get_list();
        _data = list[_index];
        list.RemoveAt(_index);
    }

    public void Rollback()
    {
        get_list().Insert(_index, _data);
    }

    private IList get_list()
    {
        return _list ?? _generator?.Invoke();
    }
}

/// <summary>
///     クリアオペレーション
/// </summary>
public class ClearOperation<T> : IOperation
{
    private readonly Func<IList<T>> _generator;
    private readonly IList<T> _list;
    private T[] _prevData;


    public ClearOperation(Func<IList<T>> listGenerator)
    {
        Debug.Assert(listGenerator != null);
        _generator = listGenerator;
    }

    public ClearOperation(IList<T> list)
    {
        Debug.Assert(list != null);
        _list = list;
    }

    public BindableReactiveProperty<string> Message { get; } = new();
    public BindableReactiveProperty<Visibility> ArrowVisibility { get; } = new(Visibility.Hidden);

    public void RollForward()
    {
        _prevData = get_list().AsValueEnumerable().ToArray();
        get_list().Clear();
    }

    public void Rollback()
    {
        foreach (var data in _prevData)
            get_list().Add(data);
    }

    private IList<T> get_list()
    {
        return _list ?? _generator?.Invoke();
    }
}