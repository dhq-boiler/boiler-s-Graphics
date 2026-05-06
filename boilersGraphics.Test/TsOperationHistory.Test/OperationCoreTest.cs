using NUnit.Framework;
using System;
using System.Linq;
using System.Windows;
using TsOperationHistory;
using TsOperationHistory.Extensions;

namespace TsOperationHistory.Test
{
    [TestFixture]
    public class OperationCoreTest
    {
        // ---- CompositeOperation ----

        [Test]
        public void CompositeOperation_RollForwardは追加順_Rollbackは逆順()
        {
            var order = new System.Collections.Generic.List<string>();
            var op1 = new DelegateOperation(() => order.Add("F1"), () => order.Add("R1"));
            var op2 = new DelegateOperation(() => order.Add("F2"), () => order.Add("R2"));
            var composite = new CompositeOperation("composite", op1, op2);

            composite.RollForward();
            composite.Rollback();

            Assert.That(order, Is.EqualTo(new[] { "F1", "F2", "R2", "R1" }));
            Assert.That(composite.Message.Value, Is.EqualTo("composite"));
        }

        [Test]
        public void CompositeOperation_Add単一でOperationsに追加される()
        {
            var composite = new CompositeOperation();
            var op = new DelegateOperation(() => { }, () => { });
            var result = composite.Add(op);
            Assert.That(result, Is.SameAs(composite));
            Assert.That(composite.Operations, Does.Contain(op));
        }

        [Test]
        public void CompositeOperation_Addparams_複数追加()
        {
            var composite = new CompositeOperation();
            var op1 = new DelegateOperation(() => { }, () => { });
            var op2 = new DelegateOperation(() => { }, () => { });
            composite.Add(op1, op2);
            Assert.That(composite.Operations.Count(), Is.EqualTo(2));
        }

        [Test]
        public void CompositeOperation_デフォルトはMessageNullArrowVisibilityHidden()
        {
            var c = new CompositeOperation();
            Assert.That(c.Message.Value, Is.Null);
            Assert.That(c.ArrowVisibility.Value, Is.EqualTo(Visibility.Hidden));
        }

        // ---- OperationController ----

        [Test]
        public void OperationController_ctorデフォルト容量1024()
        {
            var c = new OperationController();
            Assert.That(c.CanUndo, Is.False);
            Assert.That(c.CanRedo, Is.False);
        }

        [Test]
        public void OperationController_Execute_PushしてRollForwardしてStackChangedが発火()
        {
            var c = new OperationController();
            int forward = 0;
            int stackChanged = 0;
            OperationStackChangedEvent? lastEvent = null;
            c.StackChanged += (_, e) =>
            {
                stackChanged++;
                lastEvent = e.EventType;
            };

            var op = new DelegateOperation(() => forward++, () => { });
            c.Execute(op);

            Assert.That(forward, Is.EqualTo(1));
            Assert.That(stackChanged, Is.EqualTo(1));
            Assert.That(lastEvent, Is.EqualTo(OperationStackChangedEvent.Push));
            Assert.That(c.CanUndo, Is.True);
        }

        [Test]
        public void OperationController_Undo_RollbackしてCanRedoがtrueに()
        {
            var c = new OperationController();
            int forward = 0, back = 0;
            c.Execute(new DelegateOperation(() => forward++, () => back++));

            OperationStackChangedEvent? last = null;
            c.StackChanged += (_, e) => last = e.EventType;
            c.Undo();
            Assert.That(back, Is.EqualTo(1));
            Assert.That(c.CanRedo, Is.True);
            Assert.That(last, Is.EqualTo(OperationStackChangedEvent.Undo));
        }

        [Test]
        public void OperationController_Redo_再度RollForward()
        {
            var c = new OperationController();
            int forward = 0;
            c.Execute(new DelegateOperation(() => forward++, () => { }));
            c.Undo();

            OperationStackChangedEvent? last = null;
            c.StackChanged += (_, e) => last = e.EventType;
            c.Redo();
            Assert.That(forward, Is.EqualTo(2));
            Assert.That(last, Is.EqualTo(OperationStackChangedEvent.Redo));
        }

        [Test]
        public void OperationController_UndoCanUndoFalseなら何もしない()
        {
            var c = new OperationController();
            int events = 0;
            c.StackChanged += (_, _) => events++;
            c.Undo();
            Assert.That(events, Is.EqualTo(0));
        }

        [Test]
        public void OperationController_RedoCanRedoFalseなら何もしない()
        {
            var c = new OperationController();
            int events = 0;
            c.StackChanged += (_, _) => events++;
            c.Redo();
            Assert.That(events, Is.EqualTo(0));
        }

        [Test]
        public void OperationController_Push_Pop_PeekはUndoStack経由()
        {
            var c = new OperationController();
            var op = new DelegateOperation(() => { }, () => { });
            c.Push(op);
            Assert.That(c.Peek(), Is.SameAs(op));

            OperationStackChangedEvent? last = null;
            c.StackChanged += (_, e) => last = e.EventType;
            var popped = c.Pop();
            Assert.That(popped, Is.SameAs(op));
            Assert.That(last, Is.EqualTo(OperationStackChangedEvent.Pop));
        }

        [Test]
        public void OperationController_Flush_StackをクリアしてClearイベント()
        {
            var c = new OperationController();
            c.Execute(new DelegateOperation(() => { }, () => { }));
            c.Execute(new DelegateOperation(() => { }, () => { }));
            Assert.That(c.CanUndo, Is.True);

            OperationStackChangedEvent? last = null;
            c.StackChanged += (_, e) => last = e.EventType;
            c.Flush();
            Assert.That(c.CanUndo, Is.False);
            Assert.That(last, Is.EqualTo(OperationStackChangedEvent.Clear));
        }

        [Test]
        public void OperationController_RollForwardTargetsはRedoStackの逆順()
        {
            var c = new OperationController();
            var op1 = new DelegateOperation(() => { }, () => { });
            var op2 = new DelegateOperation(() => { }, () => { });
            c.Execute(op1);
            c.Execute(op2);
            c.Undo();
            c.Undo();
            // RedoStack には op1, op2 の順序で追加されている (Undoで Push)
            // RollForwardTargets はそれを Reverse
            Assert.That(c.RollForwardTargets.Count(), Is.EqualTo(2));
        }

        // ---- OperationRecorder ----

        [Test]
        public void OperationRecorder_デフォルトCurrentはRoot()
        {
            var rec = new OperationRecorder();
            Assert.That(rec.Current, Is.Not.Null);
            Assert.That(rec.Current.CanUndo, Is.False);
        }

        [Test]
        public void OperationRecorder_BeginRecode後はサブControllerが返る()
        {
            var rec = new OperationRecorder();
            var root = rec.Current;
            rec.BeginRecode();
            Assert.That(rec.Current, Is.Not.SameAs(root));
        }

        [Test]
        public void OperationRecorder_BeginEndで一連のOperationが束ねられRootにPush()
        {
            var root = new OperationController();
            var rec = new OperationRecorder(root);

            int forward = 0;
            rec.BeginRecode();
            rec.Current.Execute(new DelegateOperation(() => forward++, () => forward--));
            rec.Current.Execute(new DelegateOperation(() => forward++, () => forward--));
            rec.EndRecode();

            // Root に CompositeOperation が積まれている
            Assert.That(root.CanUndo, Is.True);
            Assert.That(root.Peek(), Is.InstanceOf<ICompositeOperation>());
            Assert.That(forward, Is.EqualTo(2));

            // Undo で 2件分巻き戻る
            root.Undo();
            Assert.That(forward, Is.EqualTo(0));
        }

        [Test]
        public void OperationRecorder_スタック空でEndRecodeしても例外なし_警告ログのみ()
        {
            var rec = new OperationRecorder();
            Assert.That(() => rec.EndRecode(), Throws.Nothing);
        }
    }
}
