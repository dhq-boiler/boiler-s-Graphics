using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TsOperationHistory.Internal;

namespace boilersGraphics.Test
{
    [TestFixture]
    public class UndoStackTest
    {
        [Test]
        public void A()
        {
            var undoStack = new UndoStack<int>(1024);
            undoStack.Push(1);
            undoStack.Push(2);
            undoStack.Push(3);
            undoStack.Push(4);
            undoStack.Undo();
            Assert.That(undoStack, Is.EqualTo(new int[] { 1, 2, 3, 4 }));
            Assert.That(undoStack.Undos.Value, Is.EqualTo(new int[] { 1, 2, 3 }));
            Assert.That(undoStack.Redos.Value, Is.EqualTo(new int[] { 4 }));
        }

        [Test]
        public void A2()
        {
            var undoStack = new UndoStack<int>(1024);
            undoStack.Push(1);
            undoStack.Push(2);
            undoStack.Push(3);
            undoStack.Push(4);
            undoStack.Undo();
            undoStack.Undo();
            Assert.That(undoStack, Is.EqualTo(new int[] { 1, 2, 3, 4 }));
            Assert.That(undoStack.Undos.Value, Is.EqualTo(new int[] { 1, 2 }));
            Assert.That(undoStack.Redos.Value, Is.EqualTo(new int[] { 4, 3 }));
        }
    }
}
