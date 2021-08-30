
using System;

namespace TsOperationHistory.Internal
{
    internal class Disposer : IDisposable
    {
        private readonly Action _action;
        public Disposer(Action action)
        {
            _action = action;
        }
        public void Dispose()
        {
            _action();
        }
    }
}