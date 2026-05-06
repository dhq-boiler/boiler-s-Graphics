using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using TsOperationHistory;
using TsOperationHistory.Extensions;
using TsOperationHistory.Internal;

namespace TsOperationHistory.Test
{
    [TestFixture]
    public class OperationExtensionsTest
    {
        public class PropOwner
        {
            public int Value { get; set; }
            public string Name { get; set; }
        }

        public static class StaticProp
        {
            public static int Counter { get; set; }
        }

        // ---- Operation.Empty / EmptyOperation ----

        [Test]
        public void OperationEmpty_RollForwardもRollbackも例外なし()
        {
            var empty = Operation.Empty;
            Assert.That(empty.Message.Value, Is.EqualTo("Empty Operation"));
            Assert.That(() => empty.RollForward(), Throws.Nothing);
            Assert.That(() => empty.Rollback(), Throws.Nothing);
        }

        // ---- ExecuteTo / PushTo ----

        [Test]
        public void ExecuteTo_通常Operationはそのままcontroller_Executeに流す()
        {
            var c = new OperationController();
            int forward = 0;
            var op = new DelegateOperation(() => forward++, () => { });
            op.ExecuteTo(c);
            Assert.That(forward, Is.EqualTo(1));
            Assert.That(c.CanUndo, Is.True);
        }

        [Test]
        public void ExecuteTo_MergeableOperationはMergeしてからExecute()
        {
            var c = new OperationController();
            int forward = 0;
            var op1 = new MergeableOperation(() => forward++, () => { },
                new ThrottleMergeJudge<string>("k", TimeSpan.FromSeconds(60)));
            op1.ExecuteTo(c);
            Assert.That(c.UndoStack.Count, Is.EqualTo(1));

            var op2 = new MergeableOperation(() => forward++, () => { },
                new ThrottleMergeJudge<string>("k", TimeSpan.FromSeconds(60)));
            op2.ExecuteTo(c);
            // Merge で op1 は pop されて、op2 だけ残る
            Assert.That(c.UndoStack.Count, Is.EqualTo(1));
            Assert.That(forward, Is.EqualTo(2));
        }

        [Test]
        public void PushTo_通常OperationはRollForwardせずPushのみ()
        {
            var c = new OperationController();
            int forward = 0;
            var op = new DelegateOperation(() => forward++, () => { });
            op.PushTo(c);
            Assert.That(forward, Is.EqualTo(0));
            Assert.That(c.CanUndo, Is.True);
        }

        [Test]
        public void PushTo_MergeableOperationもMerge後にPushのみ()
        {
            var c = new OperationController();
            var op1 = new MergeableOperation(() => { }, () => { },
                new ThrottleMergeJudge<string>("k", TimeSpan.FromSeconds(60)));
            op1.PushTo(c);
            var op2 = new MergeableOperation(() => { }, () => { },
                new ThrottleMergeJudge<string>("k", TimeSpan.FromSeconds(60)));
            op2.PushTo(c);
            Assert.That(c.UndoStack.Count, Is.EqualTo(1));
        }

        // ---- ExecuteAndCombineTop ----

        [Test]
        public void ExecuteAndCombineTop_スタックが空なら通常のExecute()
        {
            var c = new OperationController();
            int forward = 0;
            var op = new DelegateOperation(() => forward++, () => { });
            op.ExecuteAndCombineTop(c);
            Assert.That(forward, Is.EqualTo(1));
            Assert.That(c.UndoStack.Count, Is.EqualTo(1));
        }

        [Test]
        public void ExecuteAndCombineTop_既存先頭と結合してCompositeOperationに()
        {
            var c = new OperationController();
            int countA = 0, countB = 0;
            var op1 = new DelegateOperation(() => countA++, () => countA--);
            c.Execute(op1);

            var op2 = new DelegateOperation(() => countB++, () => countB--);
            op2.Message.Value = "second";
            op2.ExecuteAndCombineTop(c);

            Assert.That(c.UndoStack.Count, Is.EqualTo(1));
            Assert.That(c.Peek(), Is.InstanceOf<ICompositeOperation>());
            Assert.That(countA, Is.EqualTo(1));
            Assert.That(countB, Is.EqualTo(1));

            // Undo で両方戻る
            c.Undo();
            Assert.That(countA, Is.EqualTo(0));
            Assert.That(countB, Is.EqualTo(0));
        }

        // ---- CombineOperations ----

        [Test]
        public void CombineOperations_先頭プラスサブを順にyield()
        {
            var op1 = new DelegateOperation(() => { }, () => { });
            var op2 = new DelegateOperation(() => { }, () => { });
            var op3 = new DelegateOperation(() => { }, () => { });
            var combined = op1.CombineOperations(op2, op3).ToArray();
            Assert.That(combined, Is.EqualTo(new[] { op1, op2, op3 }));
        }

        // ---- GenerateSetPropertyOperation ----

        [Test]
        public void GenerateSetPropertyOperation_インスタンスプロパティをsetする()
        {
            var owner = new PropOwner { Value = 1 };
            var op = owner.GenerateSetPropertyOperation(nameof(PropOwner.Value), 99, TimeSpan.FromSeconds(60));
            op.RollForward();
            Assert.That(owner.Value, Is.EqualTo(99));
            op.Rollback();
            Assert.That(owner.Value, Is.EqualTo(1));
        }

        [Test]
        public void GenerateSetPropertyOperation_TimeSpan省略版はDefaultMergeSpan()
        {
            var owner = new PropOwner { Value = 1 };
            var op = owner.GenerateSetPropertyOperation(nameof(PropOwner.Value), 5);
            op.RollForward();
            Assert.That(owner.Value, Is.EqualTo(5));
        }

        [Test]
        public void GenerateSetPropertyOperationT_Expression版でプロパティsetする()
        {
            var owner = new PropOwner { Value = 0 };
            var op = owner.GenerateSetPropertyOperation(x => x.Value, 7);
            op.RollForward();
            Assert.That(owner.Value, Is.EqualTo(7));
        }

        [Test]
        public void ToOperation_Expression指定で現在値を初期値とするOperation()
        {
            var owner = new PropOwner { Value = 42 };
            var op = owner.ToOperation(x => x.Value);
            op.RollForward();
            Assert.That(owner.Value, Is.EqualTo(42));
        }

        [Test]
        public void GenerateSetStaticPropertyOperation_静的プロパティをsetする()
        {
            StaticProp.Counter = 5;
            var op = typeof(StaticProp).GenerateSetStaticPropertyOperation(nameof(StaticProp.Counter), 100);
            op.RollForward();
            Assert.That(StaticProp.Counter, Is.EqualTo(100));
            op.Rollback();
            Assert.That(StaticProp.Counter, Is.EqualTo(5));
        }

        // ---- AddPostEvent / AddPreEvent ----

        [Test]
        public void AddPostEvent_DelegateOperationでは新しいDelegateOperationでラップ()
        {
            int forward = 0, post = 0;
            IOperation op = new DelegateOperation(() => forward++, () => { });
            op = op.AddPostEvent(() => post++);
            op.RollForward();
            Assert.That(forward, Is.EqualTo(1));
            Assert.That(post, Is.EqualTo(1));
        }

        [Test]
        public void AddPostEvent_IOperationWithEventならOnExecutedにフック()
        {
            int post = 0;
            IOperation op = new MergeableOperation(() => { }, () => { });
            var wrapped = op.AddPostEvent(() => post++);
            Assert.That(wrapped, Is.SameAs(op));
            wrapped.RollForward();
            Assert.That(post, Is.EqualTo(1));
        }

        [Test]
        public void AddPreEvent_DelegateOperationでは新しいDelegateOperationでラップ()
        {
            int forward = 0, pre = 0;
            IOperation op = new DelegateOperation(() => forward++, () => { });
            op = op.AddPreEvent(() => pre++);
            op.RollForward();
            Assert.That(pre, Is.EqualTo(1));
            Assert.That(forward, Is.EqualTo(1));
        }

        [Test]
        public void AddPreEvent_IOperationWithEventならOnPreviewExecutedにフック()
        {
            int pre = 0;
            IOperation op = new MergeableOperation(() => { }, () => { });
            var wrapped = op.AddPreEvent(() => pre++);
            Assert.That(wrapped, Is.SameAs(op));
            wrapped.RollForward();
            Assert.That(pre, Is.EqualTo(1));
        }

        // ---- ExecuteDispose ----

        [Test]
        public void ExecuteDispose_DisposeOperationを返す()
        {
            int disposed = 0;
            int regenerated = 0;
            var disposable = new DisposableSpy(() => disposed++);
            var op = disposable.ExecuteDispose(() => regenerated++);
            op.RollForward();
            Assert.That(disposed, Is.EqualTo(1));
            op.Rollback();
            Assert.That(regenerated, Is.EqualTo(1));
        }

        private sealed class DisposableSpy : IDisposable
        {
            private readonly Action _onDispose;
            public DisposableSpy(Action onDispose) => _onDispose = onDispose;
            public void Dispose() => _onDispose();
        }

        // ---- IsEmpty / IsNullOrEmpty / IsNotEmpty ----

        [Test]
        public void IsEmpty_OperationEmptyはtrue()
        {
            Assert.That(Operation.Empty.IsEmpty(), Is.True);
        }

        [Test]
        public void IsEmpty_中身のないCompositeOperationはtrue()
        {
            var c = new CompositeOperation();
            Assert.That(c.IsEmpty(), Is.True);
        }

        [Test]
        public void IsEmpty_すべてEmptyのCompositeOperationもtrue()
        {
            var c = new CompositeOperation(null, Operation.Empty, Operation.Empty);
            Assert.That(c.IsEmpty(), Is.True);
        }

        [Test]
        public void IsEmpty_実Operationを含むCompositeOperationはfalse()
        {
            var c = new CompositeOperation(null,
                Operation.Empty,
                new DelegateOperation(() => { }, () => { }));
            Assert.That(c.IsEmpty(), Is.False);
        }

        [Test]
        public void IsEmpty_通常DelegateOperationはfalse()
        {
            var op = new DelegateOperation(() => { }, () => { });
            Assert.That(op.IsEmpty(), Is.False);
        }

        [Test]
        public void IsNullOrEmpty_nullはtrue()
        {
            IOperation op = null;
            Assert.That(op.IsNullOrEmpty(), Is.True);
        }

        [Test]
        public void IsNullOrEmpty_OperationEmptyもtrue()
        {
            Assert.That(Operation.Empty.IsNullOrEmpty(), Is.True);
        }

        [Test]
        public void IsNotEmpty_実Operationはtrue()
        {
            var op = new DelegateOperation(() => { }, () => { });
            Assert.That(op.IsNotEmpty(), Is.True);
        }

        // ---- ListOperationExtensions ----

        [Test]
        public void ToAddOperation_リストに追加_Undoで削除()
        {
            var list = new List<int> { 1 };
            var op = list.ToAddOperation(2);
            op.RollForward();
            Assert.That(list, Is.EqualTo(new[] { 1, 2 }));
            op.Rollback();
            Assert.That(list, Is.EqualTo(new[] { 1 }));
        }

        [Test]
        public void ToRemoveOperation_リストから削除_UndoでInsert復元()
        {
            var list = new List<int> { 1, 2, 3 };
            var op = list.ToRemoveOperation(2);
            op.RollForward();
            Assert.That(list, Is.EqualTo(new[] { 1, 3 }));
            op.Rollback();
            Assert.That(list, Is.EqualTo(new[] { 1, 2, 3 }));
        }

        [Test]
        public void ToRemoveOperation_存在しない値はRollForwardでもリスト変更なし()
        {
            var list = new List<int> { 1, 2, 3 };
            var op = list.ToRemoveOperation(99);
            op.RollForward();
            Assert.That(list, Is.EqualTo(new[] { 1, 2, 3 }));
            // Rollback も Index<0 なので無動作
            op.Rollback();
            Assert.That(list, Is.EqualTo(new[] { 1, 2, 3 }));
        }

        [Test]
        public void ToRemoveAtOperation_インデックス指定で削除_Undoで復元()
        {
            System.Collections.IList list = new List<int> { 10, 20, 30 };
            var op = list.ToRemoveAtOperation(1);
            op.RollForward();
            Assert.That(list, Is.EqualTo(new[] { 10, 30 }));
            op.Rollback();
            Assert.That(list, Is.EqualTo(new[] { 10, 20, 30 }));
        }

        [Test]
        public void ToAddRangeOperation_paramsとIEnumerable両方()
        {
            var list = new List<int> { 0 };
            var paramsOp = list.ToAddRangeOperation(1, 2);
            paramsOp.RollForward();
            Assert.That(list, Is.EqualTo(new[] { 0, 1, 2 }));

            var enumerableOp = list.ToAddRangeOperation((IEnumerable<int>)new[] { 3, 4 });
            enumerableOp.RollForward();
            Assert.That(list, Is.EqualTo(new[] { 0, 1, 2, 3, 4 }));
        }

        [Test]
        public void ToRemoveRangeOperation_paramsとIEnumerable両方()
        {
            var list = new List<int> { 1, 2, 3, 4 };
            var paramsOp = list.ToRemoveRangeOperation(2, 3);
            paramsOp.RollForward();
            Assert.That(list, Is.EqualTo(new[] { 1, 4 }));

            var enumerableOp = list.ToRemoveRangeOperation((IEnumerable<int>)new[] { 1, 4 });
            enumerableOp.RollForward();
            Assert.That(list, Is.Empty);
        }

        [Test]
        public void ToClearOperation_リストを空にしてUndoで全部復元()
        {
            var list = new List<int> { 5, 6, 7 };
            var op = list.ToClearOperation();
            op.RollForward();
            Assert.That(list, Is.Empty);
            op.Rollback();
            Assert.That(list, Is.EqualTo(new[] { 5, 6, 7 }));
        }

        // ---- CompositeOperationExtensions ----

        [Test]
        public void ToCompositeOperation_IEnumerableを束ねる()
        {
            var ops = new[]
            {
                new DelegateOperation(() => { }, () => { }),
                new DelegateOperation(() => { }, () => { }),
            };
            var composite = ((IEnumerable<IOperation>)ops).ToCompositeOperation();
            Assert.That(composite.Operations.Count(), Is.EqualTo(2));
        }

        [Test]
        public void Union_paramsで複数のIOperationを連結()
        {
            var a = new DelegateOperation(() => { }, () => { });
            var b = new DelegateOperation(() => { }, () => { });
            var c = new DelegateOperation(() => { }, () => { });
            var unioned = a.Union(b, c);
            Assert.That(unioned.Operations.Count(), Is.EqualTo(3));
        }

        [Test]
        public void Union_IEnumerable版()
        {
            var a = new DelegateOperation(() => { }, () => { });
            var others = new[] { new DelegateOperation(() => { }, () => { }) };
            var unioned = a.Union((IEnumerable<IOperation>)others);
            Assert.That(unioned.Operations.Count(), Is.EqualTo(2));
        }

        [Test]
        public void GetAllOperation_ネストしたCompositeも展開する()
        {
            var leaf1 = new DelegateOperation(() => { }, () => { });
            var leaf2 = new DelegateOperation(() => { }, () => { });
            var leaf3 = new DelegateOperation(() => { }, () => { });
            var inner = new CompositeOperation(null, leaf2, leaf3);
            var outer = new CompositeOperation(null, leaf1, inner);

            var all = outer.GetAllOperation().ToArray();
            Assert.That(all, Is.EquivalentTo(new IOperation[] { leaf1, leaf2, leaf3 }));
        }

        // ---- InsertOperation 直接 (insertIndex 指定 + generator ctor) ----

        [Test]
        public void InsertOperation_indexを指定して挿入()
        {
            var list = new List<int> { 10, 30 };
            var op = new InsertOperation<int>(list, 20, 1);
            op.RollForward();
            Assert.That(list, Is.EqualTo(new[] { 10, 20, 30 }));
            op.Rollback();
            Assert.That(list, Is.EqualTo(new[] { 10, 30 }));
        }

        [Test]
        public void InsertOperation_generatorコンストラクタで遅延リスト解決()
        {
            var backing = new List<int>();
            var op = new InsertOperation<int>(() => backing, 99);
            op.RollForward();
            Assert.That(backing, Is.EqualTo(new[] { 99 }));
        }

        [Test]
        public void RemoveOperation_generatorコンストラクタで遅延リスト解決()
        {
            var backing = new List<int> { 1, 2, 3 };
            var op = new RemoveOperation<int>(() => backing, 2);
            op.RollForward();
            Assert.That(backing, Is.EqualTo(new[] { 1, 3 }));
            op.Rollback();
            Assert.That(backing, Is.EqualTo(new[] { 1, 2, 3 }));
        }

        [Test]
        public void ClearOperation_generatorコンストラクタで遅延リスト解決()
        {
            var backing = new List<int> { 7, 8, 9 };
            var op = new ClearOperation<int>(() => backing);
            op.RollForward();
            Assert.That(backing, Is.Empty);
            op.Rollback();
            Assert.That(backing, Is.EqualTo(new[] { 7, 8, 9 }));
        }
    }
}
