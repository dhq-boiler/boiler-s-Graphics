using System.Collections.Generic;
using System.Linq;
using TsOperationHistory.Extensions;

namespace TsOperationHistory
{
    public class OperationRecorder
    {
        private readonly IOperationController _root;
        private readonly Stack<IOperationController> _stack = new Stack<IOperationController>();

        public IOperationController Current
        {
            get
            {
                if (_stack.Any())
                    return _stack.Peek();
                return _root;
            }
        }

        public OperationRecorder() 
            : this(new OperationController(1024))
        {
            
        }
        
        public OperationRecorder(IOperationController controller)
        {
            _root = controller;
        }

        public void BeginRecode()
        {
            _stack.Push(new OperationController(1024));
        }

        public void EndRecode( string message )
        {
            var controller = _stack.Pop();
            var operation = controller.Operations.ToCompositeOperation();
            operation.Message = message;
            Current.Push(operation);
        }
    }
}