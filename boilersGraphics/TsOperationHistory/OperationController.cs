using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TsOperationHistory.Internal;

namespace TsOperationHistory;

/// <summary>
///     標準的なオペレーションコントローラ
/// </summary>
public class OperationController : IOperationController
{
    public OperationController()
        : this(1024)
    {
    }

    public OperationController(int capacity)
    {
        Debug.Assert(capacity > 0, ErrorMessages.InvalidOperation);
        UndoStack = new UndoStack<IOperation>(capacity);
    }

    public bool CanUndo => UndoStack.CanUndo;
    public bool CanRedo => UndoStack.CanRedo;

    public void Undo()
    {
        if (!CanUndo)
            return;

        PreStackChanged();
        UndoStack.Undo().Rollback();
        OnStackChanged(OperationStackChangedEvent.Undo);
    }

    public void Redo()
    {
        if (!CanRedo)
            return;

        PreStackChanged();
        UndoStack.Redo().RollForward();
        OnStackChanged(OperationStackChangedEvent.Redo);
    }

    public IOperation Peek()
    {
        return UndoStack.Peek();
    }

    public IOperation Pop()
    {
        PreStackChanged();
        var result = UndoStack.Pop();
        OnStackChanged(OperationStackChangedEvent.Pop);
        return result;
    }

    public IOperation Push(IOperation operation)
    {
        PreStackChanged();
        UndoStack.Push(operation);
        OnStackChanged(OperationStackChangedEvent.Push);
        return operation;
    }

    public IOperation Execute(IOperation operation)
    {
        Debug.Assert(operation != null, ErrorMessages.NotNull);
        Push(operation).RollForward();
        return operation;
    }

    public void Flush()
    {
        PreStackChanged();
        UndoStack.Clear();
        OnStackChanged(OperationStackChangedEvent.Clear);
    }

    public UndoStack<IOperation> UndoStack { get; }

    public bool IsOperating => _preStackChangedCall != 0;

    #region PropertyChanged

    private int _preStackChangedCall;

    public IEnumerable<IOperation> RollForwardTargets => UndoStack.RedoStack.Reverse();
    public event EventHandler<OperationStackChangedEventArgs> StackChanged;

    private void PreStackChanged()
    {
        //! Operationの再帰呼び出しを検知するとassert
        Debug.Assert(_preStackChangedCall == 0, ErrorMessages.InvalidOperation);
        _preStackChangedCall++;
    }

    private void OnStackChanged(OperationStackChangedEvent eventType)
    {
        Debug.Assert(_preStackChangedCall == 1, ErrorMessages.InvalidOperation);
        _preStackChangedCall--;
        StackChanged?.Invoke(this, new OperationStackChangedEventArgs { EventType = eventType });
    }

    #endregion
}