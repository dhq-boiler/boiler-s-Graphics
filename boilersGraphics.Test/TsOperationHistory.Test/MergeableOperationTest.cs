using NUnit.Framework;
using System;
using System.Collections.Generic;
using TsOperationHistory;
using TsOperationHistory.Extensions;
using TsOperationHistory.Internal;

namespace TsOperationHistory.Test
{
    [TestFixture]
    public class MergeableOperationTest
    {
        private static ThrottleMergeJudge<string> Judge(string key, double seconds = 60)
            => new(key, TimeSpan.FromSeconds(seconds));

        // ---- NamedAction ----

        [Test]
        public void NamedAction_InvokeでActionが実行される()
        {
            int count = 0;
            var na = new NamedAction { Name = "test", Action = () => count++ };
            na.Invoke();
            Assert.That(count, Is.EqualTo(1));
            Assert.That(na.Name, Is.EqualTo("test"));
        }

        [Test]
        public void NamedAction_ActionがnullでもInvoke例外なし()
        {
            var na = new NamedAction { Name = "noop" };
            Assert.That(() => na.Invoke(), Throws.Nothing);
        }

        // ---- MergeableOperation (non-generic) ----

        [Test]
        public void Mergeable_RollForwardはPrevExecuted_action_Executedの順()
        {
            var order = new List<string>();
            var op = new MergeableOperation(
                () => order.Add("forward"),
                () => order.Add("back"),
                Judge("k"));
            op.OnPreviewExecuted += () => order.Add("pre");
            op.OnExecuted += () => order.Add("post");

            op.RollForward();
            op.Rollback();

            Assert.That(order, Is.EqualTo(new[] { "pre", "forward", "post", "pre", "back", "post" }));
        }

        [Test]
        public void Mergeable_SetActionNameでRollForwardInfoとRollBackInfoが更新()
        {
            var op = new MergeableOperation(() => { }, () => { });
            op.SetActionName("exec-name", "roll-name");
            Assert.That(op.RollForwardInfo, Is.EqualTo("exec-name"));
            Assert.That(op.RollBackInfo, Is.EqualTo("roll-name"));
        }

        [Test]
        public void Mergeable_NamedAction_ctor()
        {
            int forward = 0, back = 0;
            var op = new MergeableOperation(
                new NamedAction { Name = "f", Action = () => forward++ },
                new NamedAction { Name = "b", Action = () => back++ },
                Judge("k"));
            op.RollForward();
            op.Rollback();
            Assert.That(forward, Is.EqualTo(1));
            Assert.That(back, Is.EqualTo(1));
            Assert.That(op.RollForwardInfo, Is.EqualTo("f"));
            Assert.That(op.RollBackInfo, Is.EqualTo("b"));
        }

        [Test]
        public void Merge_CanUndoFalseなら自分を返す()
        {
            var controller = new OperationController();
            var op = new MergeableOperation(() => { }, () => { }, Judge("k"));
            var result = op.Merge(controller);
            Assert.That(result, Is.SameAs(op));
        }

        [Test]
        public void Merge_MergeJudgeがnullなら自分を返す()
        {
            var controller = new OperationController();
            controller.Execute(new MergeableOperation(() => { }, () => { }, Judge("k")));
            var op = new MergeableOperation(() => { }, () => { }, mergeJudge: null);
            var result = op.Merge(controller);
            Assert.That(result, Is.SameAs(op));
        }

        [Test]
        public void Merge_同じキーの先頭Operationを統合してUndoStackから除外()
        {
            var controller = new OperationController();
            var prev = new MergeableOperation(() => { }, () => { }, Judge("k"));
            controller.Execute(prev);
            Assert.That(controller.CanUndo, Is.True);

            var curr = new MergeableOperation(() => { }, () => { }, Judge("k"));
            curr.Merge(controller);

            // prev は pop されて統合済み
            Assert.That(controller.CanUndo, Is.False);
        }

        [Test]
        public void Merge_異なるキーは統合されずに先頭が残る()
        {
            var controller = new OperationController();
            var prev = new MergeableOperation(() => { }, () => { }, Judge("a"));
            controller.Execute(prev);

            var curr = new MergeableOperation(() => { }, () => { }, Judge("b"));
            curr.Merge(controller);

            // prev は残る
            Assert.That(controller.CanUndo, Is.True);
            Assert.That(controller.Peek(), Is.SameAs(prev));
        }

        [Test]
        public void MakeMerged_CanMerge成功で結合Operationを返す()
        {
            var prev = new MergeableOperation(() => { }, () => { }, Judge("k"));
            var post = new MergeableOperation(() => { }, () => { }, Judge("k"));
            var merged = MergeableOperation.MakeMerged(prev, post);
            Assert.That(merged, Is.InstanceOf<MergeableOperation>());
            Assert.That(merged, Is.Not.SameAs(prev));
            Assert.That(merged, Is.Not.SameAs(post));
        }

        [Test]
        public void MakeMerged_CanMerge失敗でOperationEmptyを返す()
        {
            var prev = new MergeableOperation(() => { }, () => { }, Judge("a"));
            var post = new MergeableOperation(() => { }, () => { }, Judge("b"));
            var merged = MergeableOperation.MakeMerged(prev, post);
            Assert.That(merged, Is.SameAs(Operation.Empty));
        }

        [Test]
        public void MakeMerged_checkKeyFalseで強制マージ()
        {
            var prev = new MergeableOperation(() => { }, () => { }, Judge("a"));
            var post = new MergeableOperation(() => { }, () => { }, Judge("b"));
            var merged = MergeableOperation.MakeMerged(prev, post, checkKey: false);
            Assert.That(merged, Is.InstanceOf<MergeableOperation>());
            Assert.That(merged, Is.Not.SameAs(Operation.Empty));
        }

        // ---- MergeableOperation<T> ----

        [Test]
        public void MergeableT_RollForwardでnewValue_RollbackでoldValueをsetterに渡す()
        {
            int observed = 0;
            var op = new MergeableOperation<int>(v => observed = v, newValue: 10, oldValue: 1, Judge("k"), "set");
            op.RollForward();
            Assert.That(observed, Is.EqualTo(10));
            op.Rollback();
            Assert.That(observed, Is.EqualTo(1));
            Assert.That(op.Message.Value, Is.EqualTo("set"));
        }

        [Test]
        public void MergeableT_OnPreviewExecutedとOnExecutedが発火する()
        {
            int pre = 0, post = 0;
            var op = new MergeableOperation<int>(_ => { }, 1, 0, Judge("k"));
            op.OnPreviewExecuted += () => pre++;
            op.OnExecuted += () => post++;

            op.RollForward();
            Assert.That(pre, Is.EqualTo(1));
            Assert.That(post, Is.EqualTo(1));
        }

        [Test]
        public void MergeT_CanUndoFalseなら自分を返す()
        {
            var controller = new OperationController();
            var op = new MergeableOperation<int>(_ => { }, 1, 0, Judge("k"));
            var result = op.Merge(controller);
            Assert.That(result, Is.SameAs(op));
        }

        [Test]
        public void MergeT_MergeJudgeがnullなら自分を返す()
        {
            var controller = new OperationController();
            controller.Execute(new MergeableOperation<int>(_ => { }, 1, 0, Judge("k")));
            var op = new MergeableOperation<int>(_ => { }, 2, 1, mergeJudge: null);
            var result = op.Merge(controller);
            Assert.That(result, Is.SameAs(op));
        }

        [Test]
        public void MergeT_同キーで先頭OperationのPrevPropertyを引き継ぐ()
        {
            var controller = new OperationController();
            int observed = 100;
            var prev = new MergeableOperation<int>(v => observed = v, newValue: 50, oldValue: 100, Judge("k"));
            controller.Execute(prev);
            Assert.That(observed, Is.EqualTo(50));

            var curr = new MergeableOperation<int>(v => observed = v, newValue: 7, oldValue: 50, Judge("k"));
            curr.Merge(controller);
            // prev は pop されている
            Assert.That(controller.CanUndo, Is.False);

            // curr で Rollback すると、PrevProperty が prev.PrevProperty (=100) になる
            curr.RollForward();
            Assert.That(observed, Is.EqualTo(7));
            curr.Rollback();
            Assert.That(observed, Is.EqualTo(100));
        }

        [Test]
        public void MakeMergedT_CanMerge成功で結合()
        {
            int observed = 0;
            var prev = new MergeableOperation<int>(v => observed = v, newValue: 10, oldValue: 0, Judge("k"));
            var post = new MergeableOperation<int>(v => observed = v, newValue: 20, oldValue: 10, Judge("k"));
            var merged = MergeableOperation<int>.MakeMerged(prev, post);
            Assert.That(merged, Is.InstanceOf<MergeableOperation<int>>());
            merged.RollForward();
            Assert.That(observed, Is.EqualTo(20));
            merged.Rollback();
            Assert.That(observed, Is.EqualTo(0));
        }

        [Test]
        public void MakeMergedT_CanMerge失敗でEmpty()
        {
            var prev = new MergeableOperation<int>(_ => { }, 10, 0, Judge("a"));
            var post = new MergeableOperation<int>(_ => { }, 20, 10, Judge("b"));
            var merged = MergeableOperation<int>.MakeMerged(prev, post);
            Assert.That(merged, Is.SameAs(Operation.Empty));
        }

        [Test]
        public void MakeMergedT_checkKeyFalseで強制マージ()
        {
            int observed = 0;
            var prev = new MergeableOperation<int>(v => observed = v, 10, 0, Judge("a"));
            var post = new MergeableOperation<int>(v => observed = v, 20, 10, Judge("b"));
            var merged = MergeableOperation<int>.MakeMerged(prev, post, checkKey: false);
            Assert.That(merged, Is.InstanceOf<MergeableOperation<int>>());
        }

        // ---- DisposeOperation<T> ----

        [Test]
        public void DisposeOperation_RollForwardでDispose_Rollbackで再生成()
        {
            int disposed = 0;
            int regenerated = 0;
            var disposable = new DisposableSpy(() => disposed++);
            var op = new DisposeOperation<DisposableSpy>(disposable, () => regenerated++);

            op.RollForward();
            Assert.That(disposed, Is.EqualTo(1));
            op.Rollback();
            Assert.That(regenerated, Is.EqualTo(1));
        }

        [Test]
        public void DisposeOperation_OnPreviewExecutedとOnExecutedが発火()
        {
            int pre = 0, post = 0;
            var op = new DisposeOperation<DisposableSpy>(new DisposableSpy(() => { }), () => { });
            op.OnPreviewExecuted += () => pre++;
            op.OnExecuted += () => post++;
            op.RollForward();
            Assert.That(pre, Is.EqualTo(1));
            Assert.That(post, Is.EqualTo(1));
        }

        private sealed class DisposableSpy : IDisposable
        {
            private readonly Action _onDispose;
            public DisposableSpy(Action onDispose) { _onDispose = onDispose; }
            public void Dispose() => _onDispose();
        }
    }
}
