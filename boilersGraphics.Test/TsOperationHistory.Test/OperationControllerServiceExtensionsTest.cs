using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TsOperationHistory;
using TsOperationHistory.Extensions;

namespace TsOperationHistory.Test
{
    [TestFixture]
    public class OperationControllerServiceExtensionsTest
    {
        public class Owner : INotifyPropertyChanged
        {
            private int _value;
            private Inner _inner = new();

            public int Value
            {
                get => _value;
                set { _value = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value))); }
            }

            public Inner Inner
            {
                get => _inner;
                set { _inner = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Inner))); }
            }

            public event PropertyChangedEventHandler PropertyChanged;
        }

        public class Inner : INotifyPropertyChanged
        {
            private int _depth;
            public int Depth
            {
                get => _depth;
                set { _depth = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Depth))); }
            }

            public event PropertyChangedEventHandler PropertyChanged;
        }

        public static class StaticOwner
        {
            public static int Counter { get; set; }
        }

        // ---- ExecuteAdd / ExecuteInsert / ExecuteAddRange ----

        [Test]
        public void ExecuteAdd_リストに値を足してUndoで戻る()
        {
            var c = new OperationController();
            var list = new List<int> { 1 };
            c.ExecuteAdd(list, 2);
            Assert.That(list, Is.EqualTo(new[] { 1, 2 }));
            c.Undo();
            Assert.That(list, Is.EqualTo(new[] { 1 }));
        }

        [Test]
        public void ExecuteInsert_インデックス指定で挿入してUndoで戻る()
        {
            var c = new OperationController();
            var list = new List<int> { 10, 30 };
            c.ExecuteInsert(list, 20, 1);
            Assert.That(list, Is.EqualTo(new[] { 10, 20, 30 }));
            c.Undo();
            Assert.That(list, Is.EqualTo(new[] { 10, 30 }));
        }

        [Test]
        public void ExecuteAddRange_複数追加()
        {
            var c = new OperationController();
            var list = new List<int> { 1 };
            c.ExecuteAddRange(list, new[] { 2, 3 });
            Assert.That(list, Is.EqualTo(new[] { 1, 2, 3 }));
            c.Undo();
            Assert.That(list, Is.EqualTo(new[] { 1 }));
        }

        // ---- ExecuteRemove / ExecuteRemoveAt / ExecuteRemoveItems ----

        [Test]
        public void ExecuteRemove_値指定で削除()
        {
            var c = new OperationController();
            var list = new List<int> { 1, 2, 3 };
            c.ExecuteRemove(list, 2);
            Assert.That(list, Is.EqualTo(new[] { 1, 3 }));
        }

        [Test]
        public void ExecuteRemoveAt_IListなら直接RemoveAtOperation()
        {
            var c = new OperationController();
            var list = new List<int> { 10, 20, 30 };
            c.ExecuteRemoveAt(list, 1);
            Assert.That(list, Is.EqualTo(new[] { 10, 30 }));
            c.Undo();
            Assert.That(list, Is.EqualTo(new[] { 10, 20, 30 }));
        }

        [Test]
        public void ExecuteRemoveItems_複数削除()
        {
            var c = new OperationController();
            var list = new List<int> { 1, 2, 3, 4 };
            c.ExecuteRemoveItems(list, new[] { 2, 4 });
            Assert.That(list, Is.EqualTo(new[] { 1, 3 }));
        }

        // ---- ExecuteSetProperty / WithEnforcePropertyType / Static ----

        [Test]
        public void ExecuteSetProperty_プロパティ設定をスタックに積む()
        {
            var c = new OperationController();
            var owner = new Owner { Value = 1 };
            c.ExecuteSetProperty(owner, nameof(Owner.Value), 42);
            Assert.That(owner.Value, Is.EqualTo(42));
            c.Undo();
            Assert.That(owner.Value, Is.EqualTo(1));
        }

        [Test]
        public void ExecuteSetPropertyWithEnforcePropertyType_objectで受けて型キャスト()
        {
            var c = new OperationController();
            var owner = new Owner { Value = 0 };
            c.ExecuteSetPropertyWithEnforcePropertyType<Owner, int>(owner, nameof(Owner.Value), (object)99);
            Assert.That(owner.Value, Is.EqualTo(99));
        }

        [Test]
        public void ExecuteSetStaticProperty_静的プロパティを設定()
        {
            StaticOwner.Counter = 1;
            var c = new OperationController();
            c.ExecuteSetStaticProperty(typeof(StaticOwner), nameof(StaticOwner.Counter), 77);
            Assert.That(StaticOwner.Counter, Is.EqualTo(77));
            c.Undo();
            Assert.That(StaticOwner.Counter, Is.EqualTo(1));
        }

        // ---- ExecuteDispose ----

        [Test]
        public void ExecuteDispose_RollForwardでDispose_Undoで再生成()
        {
            int disposed = 0;
            int regen = 0;
            var disposable = new DisposableSpy(() => disposed++);
            var c = new OperationController();
            c.ExecuteDispose(disposable, () => regen++);
            Assert.That(disposed, Is.EqualTo(1));
            c.Undo();
            Assert.That(regen, Is.EqualTo(1));
        }

        private sealed class DisposableSpy : IDisposable
        {
            private readonly Action _onDispose;
            public DisposableSpy(Action onDispose) => _onDispose = onDispose;
            public void Dispose() => _onDispose();
        }

        // ---- BindPropertyChanged ----

        [Test]
        public void BindPropertyChanged_対象プロパティ変更でPushされる()
        {
            var c = new OperationController();
            var owner = new Owner { Value = 1 };
            using (c.BindPropertyChanged<int>(owner, nameof(Owner.Value), autoMerge: false))
            {
                owner.Value = 5;
                Assert.That(c.UndoStack.Count, Is.EqualTo(1));
            }
            // dispose 後はバインド解除
            owner.Value = 99;
            Assert.That(c.UndoStack.Count, Is.EqualTo(1));
        }

        [Test]
        public void BindPropertyChanged_別プロパティが変更されても無視()
        {
            var c = new OperationController();
            var owner = new Owner();
            using (c.BindPropertyChanged<int>(owner, nameof(Owner.Value), autoMerge: false))
            {
                // 別プロパティ
                owner.Inner = new Inner();
            }
            Assert.That(c.UndoStack.Count, Is.EqualTo(0));
        }

        [Test]
        public void BindPropertyChanged_autoMergeで連続変更は1つにまとまる()
        {
            var c = new OperationController();
            var owner = new Owner { Value = 0 };
            using (c.BindPropertyChanged<int>(owner, nameof(Owner.Value), autoMerge: true))
            {
                owner.Value = 1;
                owner.Value = 2;
                owner.Value = 3;
            }
            // Throttle は DefaultMergeSpan (Max)、key は HashCode 同一なので 3回 push されても全部マージ→1つだけ
            Assert.That(c.UndoStack.Count, Is.EqualTo(1));
        }

        [Test]
        public void BindPropertyChanged_ネストしたプロパティ名_ドット記法()
        {
            var c = new OperationController();
            var owner = new Owner();
            using (c.BindPropertyChanged<int>(owner, "Inner.Depth", autoMerge: false))
            {
                owner.Inner.Depth = 5;
                Assert.That(c.UndoStack.Count, Is.EqualTo(1));
            }
        }
    }
}
