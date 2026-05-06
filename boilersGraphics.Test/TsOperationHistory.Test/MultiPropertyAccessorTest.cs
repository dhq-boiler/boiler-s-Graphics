using NUnit.Framework;
using System;
using System.Collections.Generic;
using TsOperationHistory.Internal;

namespace TsOperationHistory.Test
{
    [TestFixture]
    public class MultiPropertyAccessorTest
    {
        public class Outer
        {
            public Inner Inner { get; set; } = new();
        }

        public class Inner
        {
            public int Value { get; set; }
            public Leaf Leaf { get; set; } = new();
        }

        public class Leaf
        {
            public int Number { get; set; }
        }

        public class IndexerHolder
        {
            private readonly int[] _items = new int[5];
            public int this[int index]
            {
                get => _items[index];
                set => _items[index] = value;
            }
        }

        public static class StaticHolder
        {
            public static int Counter { get; set; }
        }

        // ---- 単純 (1段) MultiPropertyAccessor ----

        [Test]
        public void GetValue_target_単一AccessorはそのままGetValueを呼ぶ()
        {
            var pa = new PropertyAccessor<Inner, int>(t => t.Value, (t, v) => t.Value = v);
            var multi = new MultiPropertyAccessor(new IAccessor[] { pa });
            var inner = new Inner { Value = 42 };
            Assert.That(multi.GetValue(inner), Is.EqualTo(42));
        }

        // ---- 階層チェイン (2 段) ----

        [Test]
        public void GetValue_target_階層チェインで深い値を取得()
        {
            var outerToInner = new PropertyAccessor<Outer, Inner>(t => t.Inner, (t, v) => t.Inner = v);
            var innerToValue = new PropertyAccessor<Inner, int>(t => t.Value, (t, v) => t.Value = v);
            var multi = new MultiPropertyAccessor(new IAccessor[] { outerToInner, innerToValue });

            var outer = new Outer();
            outer.Inner.Value = 99;
            Assert.That(multi.GetValue(outer), Is.EqualTo(99));
        }

        [Test]
        public void SetValue_target_value_階層チェインで深い値を設定()
        {
            var outerToInner = new PropertyAccessor<Outer, Inner>(t => t.Inner, (t, v) => t.Inner = v);
            var innerToValue = new PropertyAccessor<Inner, int>(t => t.Value, (t, v) => t.Value = v);
            var multi = new MultiPropertyAccessor(new IAccessor[] { outerToInner, innerToValue });

            var outer = new Outer();
            multi.SetValue(outer, 7);
            Assert.That(outer.Inner.Value, Is.EqualTo(7));
        }

        [Test]
        public void GetValue_target_3段の階層チェインで葉まで到達()
        {
            var oToI = new PropertyAccessor<Outer, Inner>(t => t.Inner, (t, v) => t.Inner = v);
            var iToL = new PropertyAccessor<Inner, Leaf>(t => t.Leaf, (t, v) => t.Leaf = v);
            var lToN = new PropertyAccessor<Leaf, int>(t => t.Number, (t, v) => t.Number = v);
            var multi = new MultiPropertyAccessor(new IAccessor[] { oToI, iToL, lToN });

            var outer = new Outer();
            outer.Inner.Leaf.Number = 12345;
            Assert.That(multi.GetValue(outer), Is.EqualTo(12345));
        }

        // ---- AccessorChain プロパティ ----

        [Test]
        public void AccessorChain_渡したAccessorリストを保持()
        {
            var a1 = new PropertyAccessor<Outer, Inner>(t => t.Inner, null);
            var a2 = new PropertyAccessor<Inner, int>(t => t.Value, null);
            var multi = new MultiPropertyAccessor(new IAccessor[] { a1, a2 });
            Assert.That(multi.AccessorChain.Count, Is.EqualTo(2));
            Assert.That(multi.AccessorChain[0], Is.SameAs(a1));
            Assert.That(multi.AccessorChain[1], Is.SameAs(a2));
        }

        // ---- HasGetter / HasSetter / PropertyType ----

        [Test]
        public void HasGetter_全AccessorがGetterを持つならtrue()
        {
            var a1 = new PropertyAccessor<Outer, Inner>(t => t.Inner, null);
            var a2 = new PropertyAccessor<Inner, int>(t => t.Value, null);
            var multi = new MultiPropertyAccessor(new IAccessor[] { a1, a2 });
            Assert.That(multi.HasGetter, Is.True);
        }

        [Test]
        public void HasGetter_どれかのGetterがnullならfalse()
        {
            var a1 = new PropertyAccessor<Outer, Inner>(t => t.Inner, null);
            var a2 = new PropertyAccessor<Inner, int>(null, (t, v) => t.Value = v);
            var multi = new MultiPropertyAccessor(new IAccessor[] { a1, a2 });
            Assert.That(multi.HasGetter, Is.False);
        }

        [Test]
        public void HasSetter_全AccessorがSetterを持つならtrue()
        {
            var a1 = new PropertyAccessor<Outer, Inner>(t => t.Inner, (t, v) => t.Inner = v);
            var a2 = new PropertyAccessor<Inner, int>(t => t.Value, (t, v) => t.Value = v);
            var multi = new MultiPropertyAccessor(new IAccessor[] { a1, a2 });
            Assert.That(multi.HasSetter, Is.True);
        }

        [Test]
        public void HasSetter_どれかのSetterがnullならfalse()
        {
            var a1 = new PropertyAccessor<Outer, Inner>(t => t.Inner, (t, v) => t.Inner = v);
            var a2 = new PropertyAccessor<Inner, int>(t => t.Value, null);
            var multi = new MultiPropertyAccessor(new IAccessor[] { a1, a2 });
            Assert.That(multi.HasSetter, Is.False);
        }

        [Test]
        public void PropertyType_最後のAccessorのPropertyTypeを返す()
        {
            var a1 = new PropertyAccessor<Outer, Inner>(t => t.Inner, null);
            var a2 = new PropertyAccessor<Inner, int>(t => t.Value, null);
            var multi = new MultiPropertyAccessor(new IAccessor[] { a1, a2 });
            Assert.That(multi.PropertyType, Is.EqualTo(typeof(int)));
        }

        // ---- Indexer 経由の GetValue/SetValue (target, index) ----

        [Test]
        public void GetValue_target_index_最後のIndexerでアクセス()
        {
            var holder = new IndexerHolder();
            holder[2] = 555;
            // 1 段だけだが、最後の Accessor が IndexerAccessor の場合
            var ia = new IndexerAccessor<IndexerHolder, int>((t, i) => t[i], (t, i, v) => t[i] = v);
            var multi = new MultiPropertyAccessor(new IAccessor[] { ia });
            Assert.That(multi.GetValue(holder, 2), Is.EqualTo(555));
        }

        [Test]
        public void SetValue_target_index_value_最後のIndexerに設定()
        {
            var holder = new IndexerHolder();
            var ia = new IndexerAccessor<IndexerHolder, int>((t, i) => t[i], (t, i, v) => t[i] = v);
            var multi = new MultiPropertyAccessor(new IAccessor[] { ia });
            multi.SetValue(holder, 3, 42);
            Assert.That(holder[3], Is.EqualTo(42));
        }

        // ---- Static 経由の GetValue/SetValue (引数なし) ----

        [Test]
        public void GetValue_引数なし_最初のAccessorのGetValueを返す()
        {
            StaticHolder.Counter = 99;
            var sa = new StaticPropertyAccessor<object, int>(
                () => StaticHolder.Counter, v => StaticHolder.Counter = v);
            var multi = new MultiPropertyAccessor(new IAccessor[] { sa });
            Assert.That(multi.GetValue(), Is.EqualTo(99));
        }

        [Test]
        public void SetValue_value_最初のAccessorのSetValueを呼ぶ()
        {
            StaticHolder.Counter = 0;
            var sa = new StaticPropertyAccessor<object, int>(
                () => StaticHolder.Counter, v => StaticHolder.Counter = v);
            var multi = new MultiPropertyAccessor(new IAccessor[] { sa });
            multi.SetValue(7);
            Assert.That(StaticHolder.Counter, Is.EqualTo(7));
        }
    }
}
