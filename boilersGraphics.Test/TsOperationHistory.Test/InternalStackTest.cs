using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using TsOperationHistory;
using TsOperationHistory.Extensions;
using TsOperationHistory.Internal;

namespace TsOperationHistory.Test
{
    [TestFixture]
    public class InternalStackTest
    {
        // ---- CapacityStack ----

        [Test]
        public void CapacityStack_Push_Peek_Pop()
        {
            var stack = new CapacityStack<int>(10);
            stack.Push(1);
            stack.Push(2);
            stack.Push(3);
            Assert.That(stack.Peek(), Is.EqualTo(3));
            Assert.That(stack.Pop(), Is.EqualTo(3));
            Assert.That(stack.Peek(), Is.EqualTo(2));
        }

        [Test]
        public void CapacityStack_容量超過で先頭が押し出される()
        {
            var stack = new CapacityStack<int>(3);
            stack.Push(1);
            stack.Push(2);
            stack.Push(3);
            stack.Push(4);
            // 1 が押し出されて [2, 3, 4]
            Assert.That(stack.Count, Is.EqualTo(3));
            Assert.That(stack.ToArray(), Is.EqualTo(new[] { 2, 3, 4 }));
            Assert.That(stack.Peek(), Is.EqualTo(4));
        }

        [Test]
        public void CapacityStack_Capacityプロパティ()
        {
            var stack = new CapacityStack<int>(7);
            Assert.That(stack.Capacity, Is.EqualTo(7));
        }

        [Test]
        public void CapacityStack_コレクションコンストラクタ()
        {
            var stack = new CapacityStack<int>(new[] { 1, 2, 3 });
            // capacity ctor じゃない方は ObservableLinkedList の collection ctor を呼ぶ
            // Capacity=0 (default) になるが、要素は入る
            Assert.That(stack.Count, Is.EqualTo(3));
        }

        // ---- ThrottleMergeJudge ----

        [Test]
        public void ThrottleMergeJudge_同一KeyでConvergeTimeSpan内ならCanMergeはtrue()
        {
            var prev = new ThrottleMergeJudge<string>("k", TimeSpan.FromSeconds(10));
            // 直後に作るので時間差は ConvergeTimeSpan 内
            var curr = new ThrottleMergeJudge<string>("k", TimeSpan.FromSeconds(10));
            Assert.That(curr.CanMerge(prev), Is.True);
        }

        [Test]
        public void ThrottleMergeJudge_異なるKeyではCanMergeはfalse()
        {
            var prev = new ThrottleMergeJudge<string>("a", TimeSpan.FromSeconds(10));
            var curr = new ThrottleMergeJudge<string>("b", TimeSpan.FromSeconds(10));
            Assert.That(curr.CanMerge(prev), Is.False);
        }

        [Test]
        public void ThrottleMergeJudge_異なる型のIMergeJudgeはfalse()
        {
            var judge = new ThrottleMergeJudge<string>("a", TimeSpan.FromSeconds(10));
            var fake = new FakeMergeJudge();
            Assert.That(judge.CanMerge(fake), Is.False);
        }

        private sealed class FakeMergeJudge : IMergeJudge
        {
            public bool CanMerge(IMergeJudge operation) => false;
            public IMergeJudge Update(IMergeJudge prev) => this;
            public object GetMergeKey() => null;
        }

        [Test]
        public void ThrottleMergeJudge_Update同型ならTimeStampを更新して同じインスタンスを返す()
        {
            var prev = new ThrottleMergeJudge<string>("k", TimeSpan.FromSeconds(10));
            var curr = new ThrottleMergeJudge<string>("k", TimeSpan.FromSeconds(10));
            var updated = curr.Update(prev);
            Assert.That(updated, Is.SameAs(prev));
        }

        [Test]
        public void ThrottleMergeJudge_Update異型なら自分自身を返す()
        {
            var judge = new ThrottleMergeJudge<string>("k", TimeSpan.FromSeconds(10));
            var fake = new FakeMergeJudge();
            var updated = judge.Update(fake);
            Assert.That(updated, Is.SameAs(judge));
        }

        [Test]
        public void ThrottleMergeJudge_GetMergeKeyはKeyを返す()
        {
            var judge = new ThrottleMergeJudge<string>("the-key", TimeSpan.FromSeconds(10));
            Assert.That(judge.GetMergeKey(), Is.EqualTo("the-key"));
        }

        [Test]
        public void ThrottleMergeJudge_DefaultMergeSpanコンストラクタ()
        {
            var judge = new ThrottleMergeJudge<int>(42);
            Assert.That(judge.GetMergeKey(), Is.EqualTo(42));
            Assert.That(judge.ConvergeTimeSpan, Is.EqualTo(Operation.DefaultMergeSpan));
        }

        // ---- UndoStack ----

        private static IOperation Op(string name)
        {
            var op = new DelegateOperation(() => { }, () => { });
            op.Message.Value = name;
            return op;
        }

        [Test]
        public void UndoStack_PushでCanUndoがtrueになる()
        {
            using var stack = new UndoStack<IOperation>(10);
            Assert.That(stack.CanUndo, Is.False);
            stack.Push(Op("a"));
            Assert.That(stack.CanUndo, Is.True);
        }

        [Test]
        public void UndoStack_Push後にUndoでRedoが可能に()
        {
            using var stack = new UndoStack<IOperation>(10);
            var op = Op("a");
            stack.Push(op);
            stack.Undo();
            Assert.That(stack.CanRedo, Is.True);
            Assert.That(stack.CanUndo, Is.False);
        }

        [Test]
        public void UndoStack_RedoでUndoが再度可能に()
        {
            using var stack = new UndoStack<IOperation>(10);
            stack.Push(Op("a"));
            stack.Undo();
            stack.Redo();
            Assert.That(stack.CanUndo, Is.True);
            Assert.That(stack.CanRedo, Is.False);
        }

        [Test]
        public void UndoStack_PeekはUndo可能ならtopを返す()
        {
            using var stack = new UndoStack<IOperation>(10);
            var op1 = Op("a");
            var op2 = Op("b");
            stack.Push(op1);
            stack.Push(op2);
            Assert.That(stack.Peek(), Is.SameAs(op2));
        }

        [Test]
        public void UndoStack_PeekはCanUndoFalseならdefault()
        {
            using var stack = new UndoStack<IOperation>(10);
            Assert.That(stack.Peek(), Is.Null);
        }

        [Test]
        public void UndoStack_PopはCanUndoFalseならdefault()
        {
            using var stack = new UndoStack<IOperation>(10);
            Assert.That(stack.Pop(), Is.Null);
        }

        [Test]
        public void UndoStack_PopはRedoStackをクリア()
        {
            using var stack = new UndoStack<IOperation>(10);
            stack.Push(Op("a"));
            stack.Push(Op("b"));
            stack.Undo(); // b を Redo に
            Assert.That(stack.CanRedo, Is.True);
            stack.Pop();  // a を pop、Redo もクリア
            Assert.That(stack.CanRedo, Is.False);
        }

        [Test]
        public void UndoStack_PushはRedoStackをクリア()
        {
            using var stack = new UndoStack<IOperation>(10);
            stack.Push(Op("a"));
            stack.Undo();
            Assert.That(stack.CanRedo, Is.True);
            stack.Push(Op("new")); // Redo は捨てられる
            Assert.That(stack.CanRedo, Is.False);
        }

        [Test]
        public void UndoStack_PushはMessageに通し番号をプリペンド()
        {
            using var stack = new UndoStack<IOperation>(10);
            var op = Op("rotate");
            stack.Push(op);
            Assert.That(op.Message.Value, Does.StartWith("#1"));
            Assert.That(op.Message.Value, Does.Contain("rotate"));
        }

        [Test]
        public void UndoStack_Clearで全クリア()
        {
            using var stack = new UndoStack<IOperation>(10);
            stack.Push(Op("a"));
            stack.Push(Op("b"));
            stack.Undo();
            stack.Clear();
            Assert.That(stack.CanUndo, Is.False);
            Assert.That(stack.CanRedo, Is.False);
            Assert.That(stack.Count, Is.EqualTo(0));
        }

        [Test]
        public void UndoStack_Indexerは結合スタック順にアクセス()
        {
            using var stack = new UndoStack<IOperation>(10);
            var op1 = Op("a");
            var op2 = Op("b");
            stack.Push(op1);
            stack.Push(op2);
            // Undos = [a, b], Redos = []
            Assert.That(stack[0], Is.SameAs(op1));
            Assert.That(stack[1], Is.SameAs(op2));
        }

        [Test]
        public void UndoStack_GetEnumerator_全Operationを順に返す()
        {
            using var stack = new UndoStack<IOperation>(10);
            stack.Push(Op("a"));
            stack.Push(Op("b"));
            int count = 0;
            foreach (var _ in stack) count++;
            Assert.That(count, Is.EqualTo(2));
        }

        [Test]
        public void UndoStack_RedoStackプロパティ()
        {
            using var stack = new UndoStack<IOperation>(10);
            stack.Push(Op("a"));
            stack.Push(Op("b"));
            stack.Undo();
            Assert.That(stack.RedoStack.Count(), Is.EqualTo(1));
        }

        [Test]
        public void UndoStack_Dispose2回呼んでも例外なし()
        {
            var stack = new UndoStack<IOperation>(10);
            Assert.That(() => { stack.Dispose(); stack.Dispose(); }, Throws.Nothing);
        }
    }
}
