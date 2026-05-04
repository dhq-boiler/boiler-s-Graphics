using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using TsOperationHistory.Internal;

namespace TsOperationHistory.Test
{
    [TestFixture]
    public class FastReflectionTest
    {
        public class Sample
        {
            public int Value { get; set; }
            public string Name { get; set; }
            public Sample Inner { get; set; }
            public List<int> Items { get; set; } = new();
            internal int InternalValue { get; set; }
            public int ReadOnly => 42;
            public int WriteOnly { set { } }
        }

        public static class StaticHolder
        {
            public static int StaticValue { get; set; }
            public static string StaticName { get; set; } = "default";
        }

        public struct ValueSample
        {
            public int X { get; set; }
        }

        // ---- GetProperty / SetProperty ----

        [Test]
        public void SetPropertyAndGetProperty_インスタンスのpublicプロパティ()
        {
            var s = new Sample();
            FastReflection.SetProperty(s, nameof(Sample.Value), 42);
            Assert.That(FastReflection.GetProperty(s, nameof(Sample.Value)), Is.EqualTo(42));
        }

        [Test]
        public void GetPropertyT_型指定のジェネリック取得()
        {
            var s = new Sample { Name = "hello" };
            var name = FastReflection.GetProperty<string>(s, nameof(Sample.Name));
            Assert.That(name, Is.EqualTo("hello"));
        }

        [Test]
        public void GetProperty_階層プロパティ()
        {
            var s = new Sample { Inner = new Sample { Value = 99 } };
            var v = FastReflection.GetProperty(s, "Inner.Value");
            Assert.That(v, Is.EqualTo(99));
        }

        [Test]
        public void SetProperty_階層プロパティ()
        {
            var s = new Sample { Inner = new Sample() };
            FastReflection.SetProperty(s, "Inner.Value", 7);
            Assert.That(s.Inner.Value, Is.EqualTo(7));
        }

        [Test]
        public void GetProperty_インデクサ()
        {
            var s = new Sample { Items = new List<int> { 10, 20, 30 } };
            var v = FastReflection.GetProperty(s, "Items[1]");
            Assert.That(v, Is.EqualTo(20));
        }

        [Test]
        public void SetProperty_インデクサ()
        {
            var s = new Sample { Items = new List<int> { 10, 20, 30 } };
            FastReflection.SetProperty(s, "Items[2]", 99);
            Assert.That(s.Items[2], Is.EqualTo(99));
        }

        // ---- Static property ----

        [Test]
        public void SetStaticPropertyAndGetStaticProperty()
        {
            FastReflection.SetStaticProperty(typeof(StaticHolder), nameof(StaticHolder.StaticValue), 123);
            Assert.That(FastReflection.GetStaticProperty(typeof(StaticHolder), nameof(StaticHolder.StaticValue)),
                Is.EqualTo(123));
        }

        // ---- Property type / accessor presence ----

        [Test]
        public void GetPropertyType_プロパティ型を返す()
        {
            var s = new Sample();
            Assert.That(FastReflection.GetPropertyType(s, nameof(Sample.Name)), Is.EqualTo(typeof(string)));
            Assert.That(FastReflection.GetPropertyType(s, nameof(Sample.Value)), Is.EqualTo(typeof(int)));
        }

        [Test]
        public void ExistsGetter_読み書き両方ある場合()
        {
            var s = new Sample();
            Assert.That(FastReflection.ExistsGetter(s, nameof(Sample.Value)), Is.True);
            Assert.That(FastReflection.ExistsSetter(s, nameof(Sample.Value)), Is.True);
        }

        [Test]
        public void ExistsGetter_読み取り専用プロパティ()
        {
            var s = new Sample();
            Assert.That(FastReflection.ExistsGetter(s, nameof(Sample.ReadOnly)), Is.True);
            Assert.That(FastReflection.ExistsSetter(s, nameof(Sample.ReadOnly)), Is.False);
        }

        [Test]
        public void ExistsSetter_書き込み専用プロパティ()
        {
            var s = new Sample();
            Assert.That(FastReflection.ExistsGetter(s, nameof(Sample.WriteOnly)), Is.False);
            Assert.That(FastReflection.ExistsSetter(s, nameof(Sample.WriteOnly)), Is.True);
        }

        // ---- Cache behavior ----

        [Test]
        public void GetProperty_繰り返し呼んでも結果が変わらない_キャッシュ動作()
        {
            var s = new Sample { Value = 7 };
            var v1 = FastReflection.GetProperty<int>(s, nameof(Sample.Value));
            var v2 = FastReflection.GetProperty<int>(s, nameof(Sample.Value));
            Assert.That(v1, Is.EqualTo(7));
            Assert.That(v2, Is.EqualTo(7));
        }

        // ---- GetMemberName (Expression) ----

        [Test]
        public void GetMemberName_ExpressionからプロパティName取得()
        {
            Expression<Func<Sample, int>> expr = x => x.Value;
            var name = expr.GetMemberName();
            Assert.That(name, Is.EqualTo("Value"));
        }

        [Test]
        public void GetMemberName_ネストプロパティでも末尾を返す()
        {
            Expression<Func<Sample, int>> expr = x => x.Inner.Value;
            var name = expr.GetMemberName();
            Assert.That(name, Is.EqualTo("Value"));
        }

        // ---- CreateDelegate ----

        [Test]
        public void CreateDelegate_メソッドからデリゲートを作る()
        {
            var instance = "abc";
            var method = typeof(string).GetMethod(nameof(string.GetHashCode), Type.EmptyTypes);
            var del = FastReflection.CreateDelegate<Func<int>>(instance, method);
            Assert.That(del(), Is.EqualTo("abc".GetHashCode()));
        }

        // ---- InvokeGenericMethod ----

        public class GenericMethodHost
        {
            public List<object> Calls { get; } = new();

            public void Run<T>(T arg)
            {
                Calls.Add(typeof(T));
            }
        }

        [Test]
        public void InvokeGenericMethod_型を指定してジェネリックメソッドを呼べる()
        {
            var host = new GenericMethodHost();
            FastReflection.InvokeGenericMethod(host, typeof(int), nameof(GenericMethodHost.Run), 42);
            Assert.That(host.Calls.Count, Is.EqualTo(1));
            Assert.That(host.Calls[0], Is.EqualTo(typeof(int)));
        }

        // ---- PublicOnly toggle ----

        [Test]
        public void PublicOnly_falseでinternalプロパティもアクセス可能()
        {
            var s = new SamplePrivate();
            var prev = FastReflection.PublicOnly;
            try
            {
                FastReflection.PublicOnly = false;
                FastReflection.SetProperty(s, "InternalValue", 7);
                Assert.That(FastReflection.GetProperty(s, "InternalValue"), Is.EqualTo(7));
            }
            finally
            {
                FastReflection.PublicOnly = prev;
            }
        }

        // 別クラスにすることで、Sample 側のキャッシュ汚染を避ける
        public class SamplePrivate
        {
            internal int InternalValue { get; set; }
        }

        // ---- 階層プロパティのインデクサ ----

        public class IndexHost
        {
            public List<int> Numbers { get; set; } = new() { 1, 2, 3 };
        }

        [Test]
        public void GetProperty_階層プロパティ末尾がインデクサ()
        {
            var host = new IndexHost();
            var v = FastReflection.GetProperty(host, "Numbers[1]");
            Assert.That(v, Is.EqualTo(2));
        }
    }
}
