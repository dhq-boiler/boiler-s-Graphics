using NUnit.Framework;
using System;
using System.Reflection;
using TsOperationHistory.Internal;

namespace TsOperationHistory.Test
{
    [TestFixture]
    public class StructStaticAccessorTest
    {
        public class Sample
        {
            public int Value { get; set; }
            public string Name { get; set; }
            public int ReadOnly => 42;
            internal int InternalValue { get; set; }
        }

        public static class StaticHolder
        {
            public static int Counter { get; set; }
            public static string Tag { get; set; } = "default";
            public static int ReadOnly => 7;
        }

        // ---- StructAccessor ----

        [Test]
        public void StructAccessor_GetValue_publicプロパティを取得()
        {
            var prop = typeof(Sample).GetProperty(nameof(Sample.Value));
            var sa = new StructAccessor(prop, publicOnly: true);
            var s = new Sample { Value = 7 };
            Assert.That(sa.GetValue(s), Is.EqualTo(7));
        }

        [Test]
        public void StructAccessor_SetValue_publicプロパティに設定()
        {
            var prop = typeof(Sample).GetProperty(nameof(Sample.Value));
            var sa = new StructAccessor(prop, publicOnly: true);
            var s = new Sample();
            sa.SetValue(s, 42);
            Assert.That(s.Value, Is.EqualTo(42));
        }

        [Test]
        public void StructAccessor_HasGetter_HasSetter_読み書き可能()
        {
            var prop = typeof(Sample).GetProperty(nameof(Sample.Value));
            var sa = new StructAccessor(prop, publicOnly: true);
            Assert.That(sa.HasGetter, Is.True);
            Assert.That(sa.HasSetter, Is.True);
            Assert.That(sa.PropertyType, Is.EqualTo(typeof(int)));
        }

        [Test]
        public void StructAccessor_HasSetter_読取専用ならfalse()
        {
            var prop = typeof(Sample).GetProperty(nameof(Sample.ReadOnly));
            var sa = new StructAccessor(prop, publicOnly: true);
            Assert.That(sa.HasGetter, Is.True);
            Assert.That(sa.HasSetter, Is.False);
        }

        [Test]
        public void StructAccessor_publicOnlyFalse_internalもgetter_setter両方true()
        {
            var prop = typeof(Sample).GetProperty(
                nameof(Sample.InternalValue),
                BindingFlags.NonPublic | BindingFlags.Instance);
            var sa = new StructAccessor(prop, publicOnly: false);
            Assert.That(sa.HasGetter, Is.True);
            Assert.That(sa.HasSetter, Is.True);
        }

        [Test]
        public void StructAccessor_GetValue_target_index_は未対応()
        {
            var prop = typeof(Sample).GetProperty(nameof(Sample.Value));
            var sa = new StructAccessor(prop, publicOnly: true);
            Assert.That(() => sa.GetValue(new Sample(), 0), Throws.TypeOf<NotSupportedException>());
        }

        [Test]
        public void StructAccessor_GetValue_引数なし_は未対応()
        {
            var prop = typeof(Sample).GetProperty(nameof(Sample.Value));
            var sa = new StructAccessor(prop, publicOnly: true);
            Assert.That(() => sa.GetValue(), Throws.TypeOf<NotSupportedException>());
        }

        [Test]
        public void StructAccessor_SetValue_target_index_value_は未対応()
        {
            var prop = typeof(Sample).GetProperty(nameof(Sample.Value));
            var sa = new StructAccessor(prop, publicOnly: true);
            Assert.That(() => sa.SetValue(new Sample(), 0, 1), Throws.TypeOf<NotSupportedException>());
        }

        [Test]
        public void StructAccessor_SetValue_value_のみ_は未対応()
        {
            var prop = typeof(Sample).GetProperty(nameof(Sample.Value));
            var sa = new StructAccessor(prop, publicOnly: true);
            Assert.That(() => sa.SetValue(99), Throws.TypeOf<NotSupportedException>());
        }

        // ---- StaticPropertyAccessor<TTarget, TProperty> ----

        [Test]
        public void StaticPropertyAccessor_GetValue_引数なし_でgetter呼び出し()
        {
            StaticHolder.Counter = 7;
            var sa = new StaticPropertyAccessor<object, int>(
                () => StaticHolder.Counter,
                v => StaticHolder.Counter = v);
            Assert.That(sa.GetValue(), Is.EqualTo(7));
        }

        [Test]
        public void StaticPropertyAccessor_SetValue_value_でsetter呼び出し()
        {
            StaticHolder.Counter = 0;
            var sa = new StaticPropertyAccessor<object, int>(
                () => StaticHolder.Counter,
                v => StaticHolder.Counter = v);
            sa.SetValue(99);
            Assert.That(StaticHolder.Counter, Is.EqualTo(99));
        }

        [Test]
        public void StaticPropertyAccessor_GetValue_target_は未対応()
        {
            var sa = new StaticPropertyAccessor<object, int>(
                () => StaticHolder.Counter, v => { });
            Assert.That(() => sa.GetValue(new object()), Throws.TypeOf<NotSupportedException>());
        }

        [Test]
        public void StaticPropertyAccessor_GetValue_target_index_は未対応()
        {
            var sa = new StaticPropertyAccessor<object, int>(
                () => StaticHolder.Counter, v => { });
            Assert.That(() => sa.GetValue(new object(), 0), Throws.TypeOf<NotSupportedException>());
        }

        [Test]
        public void StaticPropertyAccessor_SetValue_target_value_は未対応()
        {
            var sa = new StaticPropertyAccessor<object, int>(
                () => StaticHolder.Counter, v => { });
            Assert.That(() => sa.SetValue(new object(), 1), Throws.TypeOf<NotSupportedException>());
        }

        [Test]
        public void StaticPropertyAccessor_SetValue_target_index_value_は未対応()
        {
            var sa = new StaticPropertyAccessor<object, int>(
                () => StaticHolder.Counter, v => { });
            Assert.That(() => sa.SetValue(new object(), 0, 1), Throws.TypeOf<NotSupportedException>());
        }

        [Test]
        public void StaticPropertyAccessor_HasGetter_HasSetter_PropertyType()
        {
            var both = new StaticPropertyAccessor<object, int>(
                () => StaticHolder.Counter,
                v => StaticHolder.Counter = v);
            Assert.That(both.HasGetter, Is.True);
            Assert.That(both.HasSetter, Is.True);
            Assert.That(both.PropertyType, Is.EqualTo(typeof(int)));

            var readOnly = new StaticPropertyAccessor<object, int>(
                () => StaticHolder.ReadOnly, null);
            Assert.That(readOnly.HasGetter, Is.True);
            Assert.That(readOnly.HasSetter, Is.False);
        }
    }
}
