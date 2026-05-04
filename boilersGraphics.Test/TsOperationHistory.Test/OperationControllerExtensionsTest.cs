using NUnit.Framework;
using System;
using System.Linq;
using TsOperationHistory;
using TsOperationHistory.Extensions;
using TsOperationHistory.Internal;

namespace TsOperationHistory.Test
{
    [TestFixture]
    public class OperationControllerExtensionsTest
    {
        private static IOperation NamedOp(string name)
        {
            var op = new DelegateOperation(() => { }, () => { });
            op.Message.Value = name;
            return op;
        }

        private static MergeableOperation MergeOp(string key)
        {
            return new MergeableOperation(() => { }, () => { },
                new ThrottleMergeJudge<string>(key, TimeSpan.FromSeconds(60)));
        }

        private static MergeableOperation<int> MergeOpT(string key, int newValue, int oldValue, Action<int> setter = null)
        {
            return new MergeableOperation<int>(setter ?? (_ => { }), newValue, oldValue,
                new ThrottleMergeJudge<string>(key, TimeSpan.FromSeconds(60)));
        }

        // ---- MoveTo ----

        [Test]
        public void MoveTo_targetがUndoStackにあれば該当までUndo()
        {
            var c = new OperationController();
            var op1 = NamedOp("a");
            var op2 = NamedOp("b");
            var op3 = NamedOp("c");
            c.Execute(op1);
            c.Execute(op2);
            c.Execute(op3);
            // 上から op3, op2, op1 の順 (Peek は op3)

            c.MoveTo(op1);

            Assert.That(c.Peek(), Is.SameAs(op1));
            Assert.That(c.RollForwardTargets.Count(), Is.EqualTo(2));
        }

        // NOTE: MoveTo の Redo 経路 (target が RollForwardTargets のみに存在するケース)
        // は OperationControllerExtensions.MoveTo の実装が UndoStack.Contains で判定する
        // 都合上、UndoStack の列挙が Undos と Redos.Reverse の両方を返すため isRollBack
        // も同時に true となり、Undo 側の while ループが Peek=null で無限ループする。
        // 実装側の修正が必要なため、現時点では Redo 経路のテストは追加しない。

        [Test]
        public void MoveTo_targetが両方に存在しないなら何もしない()
        {
            var c = new OperationController();
            c.Execute(NamedOp("a"));
            var stranger = NamedOp("stranger");

            int events = 0;
            c.StackChanged += (_, _) => events++;
            c.MoveTo(stranger);
            Assert.That(events, Is.EqualTo(0));
        }

        // ---- Distinct (non-generic) ----

        [Test]
        public void Distinct_同keyのMergeableOperationを1つに統合()
        {
            var c = new OperationController();
            var key = "shared";
            c.Execute(MergeOp(key));
            c.Execute(MergeOp(key));
            c.Execute(MergeOp(key));
            Assert.That(c.UndoStack.Count, Is.EqualTo(3));

            c.Distinct(key);

            Assert.That(c.UndoStack.Count, Is.EqualTo(1));
        }

        [Test]
        public void Distinct_同key操作が0個なら何もしない()
        {
            var c = new OperationController();
            c.Execute(MergeOp("other"));
            int countBefore = c.UndoStack.Count;
            c.Distinct("missing");
            Assert.That(c.UndoStack.Count, Is.EqualTo(countBefore));
        }

        [Test]
        public void Distinct_異キーのOperationは順序を保ったまま残す()
        {
            var c = new OperationController();
            var a1 = MergeOp("a");
            var b1 = MergeOp("b");
            var a2 = MergeOp("a");
            var c1 = MergeOp("c");
            var a3 = MergeOp("a");
            c.Execute(a1);
            c.Execute(b1);
            c.Execute(a2);
            c.Execute(c1);
            c.Execute(a3);

            c.Distinct("a");

            // a 系 (3つ) が 1つにマージされて、合計 5 - 3 + 1 = 3
            Assert.That(c.UndoStack.Count, Is.EqualTo(3));
            // b1, c1 が残っている
            var rest = c.UndoStack.ToList();
            Assert.That(rest, Does.Contain(b1));
            Assert.That(rest, Does.Contain(c1));
        }

        // ---- Distinct<T> (generic) ----

        [Test]
        public void DistinctT_同keyのMergeableOperationTを1つに統合()
        {
            var c = new OperationController();
            var key = "prop";
            c.Execute(MergeOpT(key, 10, 0));
            c.Execute(MergeOpT(key, 20, 10));
            c.Execute(MergeOpT(key, 30, 20));
            Assert.That(c.UndoStack.Count, Is.EqualTo(3));

            c.Distinct<int>(key);

            Assert.That(c.UndoStack.Count, Is.EqualTo(1));
        }

        [Test]
        public void DistinctT_同key無しなら何もしない()
        {
            var c = new OperationController();
            c.Execute(MergeOpT("x", 1, 0));
            int before = c.UndoStack.Count;
            c.Distinct<int>("missing");
            Assert.That(c.UndoStack.Count, Is.EqualTo(before));
        }

        [Test]
        public void DistinctT_統合後のRollback_最初のoldValueに戻る()
        {
            var c = new OperationController();
            int observed = 0;
            var key = "p";
            c.Execute(MergeOpT(key, 10, 0, v => observed = v));
            c.Execute(MergeOpT(key, 20, 10, v => observed = v));
            c.Execute(MergeOpT(key, 30, 20, v => observed = v));
            // 直近実行で observed=30
            Assert.That(observed, Is.EqualTo(30));

            c.Distinct<int>(key);
            // 統合された Operation が 1 つ残るはず
            Assert.That(c.UndoStack.Count, Is.EqualTo(1));

            c.Undo();
            // first.PrevProperty = 0 まで戻る
            Assert.That(observed, Is.EqualTo(0));
        }
    }
}
