using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using TsOperationHistory;
using TsOperationHistory.Extensions;

namespace TsOperationHistory.Test
{
    [TestFixture]
    public class OperationBuilderTest
    {
        // ---- DelegateOperation ----

        [Test]
        public void DelegateOperation_RollForwardでexecuteRollbackでrollbackが呼ばれる()
        {
            int forward = 0, back = 0;
            var op = new DelegateOperation(() => forward++, () => back++);
            op.RollForward();
            op.Rollback();
            op.RollForward();
            Assert.That(forward, Is.EqualTo(2));
            Assert.That(back, Is.EqualTo(1));
        }

        [Test]
        public void DelegateOperation_デフォルトのMessageは空文字でArrowVisibilityはHidden()
        {
            var op = new DelegateOperation(() => { }, () => { });
            Assert.That(op.Message.Value, Is.Null.Or.Empty);
            Assert.That(op.ArrowVisibility.Value, Is.EqualTo(Visibility.Hidden));
        }

        [Test]
        public void DelegateOperationT_RollForwardはnewValueでRollbackはoldValueで関数呼び出し()
        {
            int observed = 0;
            var op = new DelegateOperation<int>(v => observed = v, newValue: 99, prevValue: 1);
            op.RollForward();
            Assert.That(observed, Is.EqualTo(99));
            op.Rollback();
            Assert.That(observed, Is.EqualTo(1));
        }

        // ---- MakeFromAction(Action, Action) ----

        [Test]
        public void MakeFromAction_BuildしたOperationでDoUndoが回る()
        {
            int forward = 0, back = 0;
            var builder = new OperationBuilder().MakeFromAction(() => forward++, () => back++);
            var op = builder.Build();

            op.RollForward();
            op.Rollback();
            Assert.That(forward, Is.EqualTo(1));
            Assert.That(back, Is.EqualTo(1));
        }

        [Test]
        public void MakeFromAction_MessageでOperationにメッセージが設定される()
        {
            var builder = new OperationBuilder().MakeFromAction(() => { }, () => { });
            var op = builder.Message("rotate 90").Build();
            Assert.That(op.Message.Value, Is.EqualTo("rotate 90"));
        }

        [Test]
        public void MakeFromAction_PostEventはRollForward後とRollback後の両方で呼ばれる()
        {
            int post = 0;
            var op = new OperationBuilder()
                .MakeFromAction(() => { }, () => { })
                .PostEvent(() => post++)
                .Build();

            op.RollForward();
            Assert.That(post, Is.EqualTo(1));
            op.Rollback();
            Assert.That(post, Is.EqualTo(2));
        }

        [Test]
        public void MakeFromAction_PrevEventはRollForward前とRollback前の両方で呼ばれる()
        {
            int pre = 0;
            int forward = 0;
            int observedAtPre = -1;
            var op = new OperationBuilder()
                .MakeFromAction(() => forward++, () => { })
                .PrevEvent(() =>
                {
                    pre++;
                    observedAtPre = forward;
                })
                .Build();

            op.RollForward();
            Assert.That(pre, Is.EqualTo(1));
            Assert.That(observedAtPre, Is.EqualTo(0)); // PrevEvent は実行前
            op.Rollback();
            Assert.That(pre, Is.EqualTo(2));
        }

        // ---- MakeFromAction<T>(Action<T>, T, T) ----

        [Test]
        public void MakeFromActionT_BuildしたOperationでnewOldを使い分ける()
        {
            int observed = 0;
            var op = new OperationBuilder()
                .MakeFromAction<int>(v => observed = v, newValue: 100, oldValue: 5)
                .Build();
            op.RollForward();
            Assert.That(observed, Is.EqualTo(100));
            op.Rollback();
            Assert.That(observed, Is.EqualTo(5));
        }

        // ---- MakeFromAction<T>(Action<T>) (BuilderFromValues) ----

        [Test]
        public void MakeFromActionT_Values指定_DoUndo()
        {
            int observed = 0;
            var op = new OperationBuilder()
                .MakeFromAction<int>(v => observed = v)
                .Values(newValue: 7, prevValue: 3)
                .Build();
            op.RollForward();
            Assert.That(observed, Is.EqualTo(7));
            op.Rollback();
            Assert.That(observed, Is.EqualTo(3));
        }

        [Test]
        public void MakeFromActionT_ThrottleとMessageとPostPrevEventのチェイン()
        {
            int post = 0;
            int pre = 0;
            var op = new OperationBuilder()
                .MakeFromAction<int>(_ => { })
                .Values(1, 0)
                .Throttle("k", TimeSpan.FromMilliseconds(100))
                .Message("set value")
                .PostEvent(() => post++)
                .PrevEvent(() => pre++)
                .Build();

            Assert.That(op.Message.Value, Is.EqualTo("set value"));
            op.RollForward();
            Assert.That(pre, Is.EqualTo(1));
            Assert.That(post, Is.EqualTo(1));
        }

        // ---- MakeThrottle / MakeMergeable ----

        [Test]
        public void MakeThrottle_BuildでMergeableなOperationが返る()
        {
            int forward = 0;
            var builder = new OperationBuilder()
                .MakeThrottle(() => forward++, () => { }, key: "k", convergeTimeSpan: TimeSpan.FromSeconds(1));
            var op = builder.Build();
            op.RollForward();
            Assert.That(forward, Is.EqualTo(1));
            Assert.That(op, Is.InstanceOf<IMergeableOperation>());
        }

        [Test]
        public void MakeThrottle_SetActionNameで実行ロールバックの名前を設定()
        {
            var builder = new OperationBuilder()
                .MakeThrottle(() => { }, () => { }, "k", TimeSpan.FromSeconds(1));
            var b2 = builder.SetActionName("exec", "roll");
            Assert.That(b2, Is.SameAs(builder));
        }

        [Test]
        public void MakeThrottleT_値指定でMergeableなOperationが返る()
        {
            int observed = 0;
            var builder = new OperationBuilder()
                .MakeThrottle<int>(v => observed = v, newValue: 5, oldValue: 1, key: "k", TimeSpan.FromSeconds(1));
            var op = builder.SetActionName("set5", "set1").Build();
            op.RollForward();
            Assert.That(observed, Is.EqualTo(5));
            op.Rollback();
            Assert.That(observed, Is.EqualTo(1));
        }

        [Test]
        public void MakeMergeable_DefaultMergeSpanのThrottleを生成()
        {
            int forward = 0;
            var op = new OperationBuilder()
                .MakeMergeable(() => forward++, () => { }, key: "k")
                .Build();
            op.RollForward();
            Assert.That(forward, Is.EqualTo(1));
        }

        // ---- MakeFromProperty ----

        [Test]
        public void MakeFromProperty_NewValue指定でプロパティ設定Operation()
        {
            var target = new Bindable { Value = 0 };
            var op = new OperationBuilder()
                .MakeFromProperty<int>(target, nameof(Bindable.Value))
                .NewValue(42)
                .Message("set 42")
                .Build();

            Assert.That(op.Message.Value, Is.EqualTo("set 42"));
            op.RollForward();
            Assert.That(target.Value, Is.EqualTo(42));
            op.Rollback();
            Assert.That(target.Value, Is.EqualTo(0));
        }

        [Test]
        public void MakeFromProperty_ThrottleとPostPrevEventがチェインできる()
        {
            var target = new Bindable { Value = 0 };
            int post = 0;
            int pre = 0;
            var op = new OperationBuilder()
                .MakeFromProperty<int>(target, nameof(Bindable.Value))
                .NewValue(7)
                .Throttle(TimeSpan.FromMilliseconds(50))
                .Message("set7")
                .PostEvent(() => post++)
                .PrevEvent(() => pre++)
                .Build();

            op.RollForward();
            Assert.That(target.Value, Is.EqualTo(7));
            Assert.That(post, Is.EqualTo(1));
            Assert.That(pre, Is.EqualTo(1));
        }

        // ---- MakeFromCollection (CollectionOperationBuilder) ----

        [Test]
        public void MakeFromCollection_BuildAddOperationでリスト追加()
        {
            var list = new List<int> { 1, 2 };
            var op = new OperationBuilder().MakeFromCollection(list).BuildAddOperation(3);
            op.RollForward();
            Assert.That(list, Is.EqualTo(new[] { 1, 2, 3 }));
            op.Rollback();
            Assert.That(list, Is.EqualTo(new[] { 1, 2 }));
        }

        [Test]
        public void MakeFromCollection_BuildRemoveOperation()
        {
            var list = new List<int> { 1, 2, 3 };
            var op = new OperationBuilder().MakeFromCollection(list).BuildRemoveOperation(2);
            op.RollForward();
            Assert.That(list, Is.EqualTo(new[] { 1, 3 }));
            op.Rollback();
            Assert.That(list, Does.Contain(2));
        }

        [Test]
        public void MakeFromCollection_BuildAddRangeとRemoveRange()
        {
            var list = new List<int> { 1 };
            var addOp = new OperationBuilder().MakeFromCollection(list).BuildAddRangeOperation(new[] { 2, 3 });
            addOp.RollForward();
            Assert.That(list, Is.EqualTo(new[] { 1, 2, 3 }));

            var removeOp = new OperationBuilder().MakeFromCollection(list).BuildRemoveRangeOperation(new[] { 2, 3 });
            removeOp.RollForward();
            Assert.That(list, Is.EqualTo(new[] { 1 }));
        }

        [Test]
        public void MakeFromCollection_BuildClearOperation()
        {
            var list = new List<int> { 1, 2, 3 };
            var op = new OperationBuilder().MakeFromCollection(list).BuildClearOperation();
            op.RollForward();
            Assert.That(list, Is.Empty);
            op.Rollback();
            Assert.That(list, Is.EqualTo(new[] { 1, 2, 3 }));
        }

        // ---- MakeCollectionOperationCustomizer ----

        [Test]
        public void Customizer_RegisterAddRemove経由でAddOperationが正しく動く()
        {
            var list = new List<int>();
            var customizer = new OperationBuilder()
                .MakeCollectionOperationCustomizer<int>()
                .RegisterAdd(v => list.Add(v))
                .RegisterRemove(v => list.Remove(v));

            var op = customizer.BuildAddOperation(10);
            op.RollForward();
            Assert.That(list, Is.EqualTo(new[] { 10 }));
            op.Rollback();
            Assert.That(list, Is.Empty);
        }

        [Test]
        public void Customizer_RegisterAddとRemoveの引数なしフックも呼ばれる()
        {
            var list = new List<int>();
            int added = 0, removed = 0;
            var customizer = new OperationBuilder()
                .MakeCollectionOperationCustomizer<int>()
                .RegisterAdd(v => list.Add(v))
                .RegisterRemove(v => list.Remove(v))
                .RegisterAdd(() => added++)
                .RegisterRemove(() => removed++);

            var op = customizer.BuildAddOperation(1);
            op.RollForward();
            Assert.That(added, Is.EqualTo(1));
            op.Rollback();
            Assert.That(removed, Is.EqualTo(1));
        }

        [Test]
        public void Customizer_RemoveOperation()
        {
            var list = new List<int> { 9 };
            var customizer = new OperationBuilder()
                .MakeCollectionOperationCustomizer<int>()
                .RegisterAdd(v => list.Add(v))
                .RegisterRemove(v => list.Remove(v));

            var op = customizer.BuildRemoveOperation(9);
            op.RollForward();
            Assert.That(list, Is.Empty);
            op.Rollback();
            Assert.That(list, Is.EqualTo(new[] { 9 }));
        }

        [Test]
        public void Customizer_AddRangeRemoveRangeはCompositeOperationを生成()
        {
            var list = new List<int>();
            var customizer = new OperationBuilder()
                .MakeCollectionOperationCustomizer<int>()
                .RegisterAdd(v => list.Add(v))
                .RegisterRemove(v => list.Remove(v));

            var addRange = customizer.BuildAddRangeOperation(new[] { 1, 2, 3 });
            addRange.RollForward();
            Assert.That(list, Is.EqualTo(new[] { 1, 2, 3 }));
            addRange.Rollback();
            Assert.That(list, Is.Empty);

            list.AddRange(new[] { 4, 5 });
            var removeRange = customizer.BuildRemoveRangeOperation(new[] { 4, 5 });
            removeRange.RollForward();
            Assert.That(list, Is.Empty);
        }

        [Test]
        public void Customizer_ClearとRollbackフックでBuildClearOperation()
        {
            var snapshot = new List<int>();
            var customizer = new OperationBuilder()
                .MakeCollectionOperationCustomizer<int>()
                .RegisterClear(() => snapshot.Add(-1))
                .RegisterRollback(() => snapshot.Add(99));

            var op = customizer.BuildClearOperation();
            op.RollForward();
            Assert.That(snapshot, Is.EqualTo(new[] { -1 }));
            op.Rollback();
            Assert.That(snapshot, Is.EqualTo(new[] { -1, 99 }));
        }

        // テスト用の Bindable
        public sealed class Bindable
        {
            public int Value { get; set; }
        }
    }
}
