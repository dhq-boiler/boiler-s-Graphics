using NUnit.Framework;
using System;
using System.Collections.Generic;
using TsOperationHistory.Internal;

namespace TsOperationHistory.Test
{
    [TestFixture]
    public class PropertyAccessorTest
    {
        public class Sample
        {
            public int Value { get; set; }
            public string Name { get; set; }
            public int ReadOnly => 42;
        }

        public struct ValueSample
        {
            public int X { get; set; }
            public string Tag { get; set; }
        }

        public static class StaticHolder
        {
            public static int Counter { get; set; }
            public static string Tag { get; set; } = "default";
        }

        public class IndexerHolder
        {
            private readonly int[] _items = new int[10];
            public int this[int index]
            {
                get => _items[index];
                set => _items[index] = value;
            }
        }

        // ---- PropertyAccessor<TTarget, TProperty> ----

        [Test]
        public void PropertyAccessor_GetValue_target_でgetter呼び出し()
        {
            var s = new Sample { Value = 7 };
            var getter = (Func<Sample, int>)(t => t.Value);
            Action<Sample, int> setter = (t, v) => t.Value = v;
            var pa = new PropertyAccessor<Sample, int>(getter, setter);
            Assert.That(pa.GetValue(s), Is.EqualTo(7));
        }

        [Test]
        public void PropertyAccessor_SetValue_target_value_でsetter呼び出し()
        {
            var s = new Sample();
            var getter = (Func<Sample, int>)(t => t.Value);
            Action<Sample, int> setter = (t, v) => t.Value = v;
            var pa = new PropertyAccessor<Sample, int>(getter, setter);
            pa.SetValue(s, 99);
            Assert.That(s.Value, Is.EqualTo(99));
        }

        [Test]
        public void PropertyAccessor_GetValue_getterがnullならdefault()
        {
            var s = new Sample();
            var pa = new PropertyAccessor<Sample, int>(null, (t, v) => t.Value = v);
            // object 戻り値で default(TProperty) は boxing されないので null
            Assert.That(pa.GetValue(s), Is.Null);
        }

        [Test]
        public void PropertyAccessor_SetValue_setterがnullなら何もしない_例外なし()
        {
            var s = new Sample { Value = 5 };
            var pa = new PropertyAccessor<Sample, int>(t => t.Value, null);
            Assert.That(() => pa.SetValue(s, 99), Throws.Nothing);
            Assert.That(s.Value, Is.EqualTo(5));
        }

        [Test]
        public void PropertyAccessor_GetValue_target_index_は未対応()
        {
            var pa = new PropertyAccessor<Sample, int>(t => t.Value, null);
            Assert.That(() => pa.GetValue(new Sample(), 0), Throws.TypeOf<NotSupportedException>());
        }

        [Test]
        public void PropertyAccessor_GetValue_引数なし_は未対応()
        {
            var pa = new PropertyAccessor<Sample, int>(t => t.Value, null);
            Assert.That(() => pa.GetValue(), Throws.TypeOf<NotSupportedException>());
        }

        [Test]
        public void PropertyAccessor_SetValue_target_index_value_は未対応()
        {
            var pa = new PropertyAccessor<Sample, int>(t => t.Value, null);
            Assert.That(() => pa.SetValue(new Sample(), 0, 1), Throws.TypeOf<NotSupportedException>());
        }

        [Test]
        public void PropertyAccessor_SetValue_value_のみ_は未対応()
        {
            var pa = new PropertyAccessor<Sample, int>(t => t.Value, null);
            Assert.That(() => pa.SetValue(99), Throws.TypeOf<NotSupportedException>());
        }

        [Test]
        public void PropertyAccessor_HasGetter_HasSetter_PropertyType()
        {
            var both = new PropertyAccessor<Sample, int>(t => t.Value, (t, v) => t.Value = v);
            Assert.That(both.HasGetter, Is.True);
            Assert.That(both.HasSetter, Is.True);
            Assert.That(both.PropertyType, Is.EqualTo(typeof(int)));

            var readOnly = new PropertyAccessor<Sample, int>(t => t.ReadOnly, null);
            Assert.That(readOnly.HasGetter, Is.True);
            Assert.That(readOnly.HasSetter, Is.False);

            var writeOnly = new PropertyAccessor<Sample, int>(null, (t, v) => { });
            Assert.That(writeOnly.HasGetter, Is.False);
            Assert.That(writeOnly.HasSetter, Is.True);
        }

        // ---- IndexerAccessor<TTarget, TProperty> ----

        [Test]
        public void IndexerAccessor_GetValue_target_index_でgetter呼び出し()
        {
            var holder = new IndexerHolder();
            holder[3] = 42;
            var getter = (Func<IndexerHolder, int, int>)((t, i) => t[i]);
            Action<IndexerHolder, int, int> setter = (t, i, v) => t[i] = v;
            var ia = new IndexerAccessor<IndexerHolder, int>(getter, setter);
            Assert.That(ia.GetValue(holder, 3), Is.EqualTo(42));
        }

        [Test]
        public void IndexerAccessor_SetValue_target_index_value_でsetter呼び出し()
        {
            var holder = new IndexerHolder();
            var ia = new IndexerAccessor<IndexerHolder, int>(
                (t, i) => t[i], (t, i, v) => t[i] = v);
            ia.SetValue(holder, 5, 99);
            Assert.That(holder[5], Is.EqualTo(99));
        }

        [Test]
        public void IndexerAccessor_GetValue_getterがnullならdefault()
        {
            var ia = new IndexerAccessor<IndexerHolder, int>(null, (t, i, v) => t[i] = v);
            Assert.That(ia.GetValue(new IndexerHolder(), 0), Is.Null);
        }

        [Test]
        public void IndexerAccessor_SetValue_setterがnullなら何もしない()
        {
            var holder = new IndexerHolder();
            var ia = new IndexerAccessor<IndexerHolder, int>((t, i) => t[i], null);
            Assert.That(() => ia.SetValue(holder, 0, 1), Throws.Nothing);
        }

        [Test]
        public void IndexerAccessor_GetValue_target_は未対応()
        {
            var ia = new IndexerAccessor<IndexerHolder, int>((t, i) => t[i], null);
            Assert.That(() => ia.GetValue(new IndexerHolder()), Throws.TypeOf<NotSupportedException>());
        }

        [Test]
        public void IndexerAccessor_GetValue_引数なし_は未対応()
        {
            var ia = new IndexerAccessor<IndexerHolder, int>((t, i) => t[i], null);
            Assert.That(() => ia.GetValue(), Throws.TypeOf<NotSupportedException>());
        }

        [Test]
        public void IndexerAccessor_SetValue_target_value_は未対応()
        {
            var ia = new IndexerAccessor<IndexerHolder, int>((t, i) => t[i], null);
            Assert.That(() => ia.SetValue(new IndexerHolder(), 99), Throws.TypeOf<NotSupportedException>());
        }

        [Test]
        public void IndexerAccessor_SetValue_value_のみ_は未対応()
        {
            var ia = new IndexerAccessor<IndexerHolder, int>((t, i) => t[i], null);
            Assert.That(() => ia.SetValue(99), Throws.TypeOf<NotSupportedException>());
        }

        [Test]
        public void IndexerAccessor_HasGetter_HasSetter_PropertyType()
        {
            var ia = new IndexerAccessor<IndexerHolder, int>((t, i) => t[i], (t, i, v) => t[i] = v);
            Assert.That(ia.HasGetter, Is.True);
            Assert.That(ia.HasSetter, Is.True);
            Assert.That(ia.PropertyType, Is.EqualTo(typeof(int)));

            var readOnly = new IndexerAccessor<IndexerHolder, int>((t, i) => t[i], null);
            Assert.That(readOnly.HasSetter, Is.False);
        }
    }
}
