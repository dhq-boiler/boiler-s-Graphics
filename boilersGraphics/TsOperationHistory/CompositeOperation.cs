using Reactive.Bindings;
using System.Collections.Generic;
using System.Windows;
using ZLinq;

namespace TsOperationHistory;

public interface ICompositeOperation : IOperation
{
    IEnumerable<IOperation> Operations { get; }
    ICompositeOperation Add(IOperation operation);
}

/// <summary>
///     サブオペレーションを持つオペレーション
///     実行/ロールバック 時にサブオペレーションをまとめて実行
/// </summary>
public class CompositeOperation : ICompositeOperation
{
    private readonly ICollection<IOperation> _operations = new HashSet<IOperation>();

    public CompositeOperation(string message = null, params IOperation[] operations)
    {
        Message.Value = message;
        Add(operations);
    }

    public IEnumerable<IOperation> Operations => _operations;
    public ReactivePropertySlim<string> Message { get; } = new();
    public ReactivePropertySlim<Visibility> ArrowVisibility { get; } = new(Visibility.Hidden);

    public void RollForward()
    {
        foreach (var operation in _operations)
            operation.RollForward();
    }

    public void Rollback()
    {
        foreach (var operation in _operations.AsValueEnumerable().Reverse())
            operation.Rollback();
    }

    /// <summary>
    ///     サブオペレーションを追加
    /// </summary>
    public ICompositeOperation Add(IOperation operation)
    {
        _operations.Add(operation);
        return this;
    }

    public ICompositeOperation Add(params IOperation[] operations)
    {
        foreach (var operation in operations)
            _operations.Add(operation);
        return this;
    }
}