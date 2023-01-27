using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using boilersGraphics.Helpers;
using NLog;
using TsOperationHistory.Extensions;

namespace TsOperationHistory;

public class OperationRecorder
{
    private readonly IOperationController _root;
    private readonly Stack<IOperationController> _stack = new();

    public OperationRecorder()
        : this(new OperationController(1024))
    {
    }

    public OperationRecorder(IOperationController controller)
    {
        _root = controller;
    }

    public IOperationController Current
    {
        get
        {
            if (_stack.Any())
                return _stack.Peek();
            return _root;
        }
    }

    public void BeginRecode([CallerMemberName] string callerMemberName = "")
    {
        var className = Reflections.NameOfCallingClass();
        LogManager.GetCurrentClassLogger().Trace($"{className}.{callerMemberName}() => BeginRecode()");
        _stack.Push(new OperationController(1024));
    }

    public void EndRecode([CallerMemberName] string callerMemberName = "")
    {
        if (_stack.Count() == 0)
        {
            LogManager.GetCurrentClassLogger().Warn("_stackが空ですがEndRecord()が呼び出されました。");
            return;
        }

        var className = Reflections.NameOfCallingClass();
        var message = $"{className}.{callerMemberName}() => EndRecode()";
        LogManager.GetCurrentClassLogger().Trace(message);
        var controller = _stack.Pop();
        var operation = controller.UndoStack.ToCompositeOperation();
        operation.Message.Value = message;
        Current.Push(operation);
    }
}