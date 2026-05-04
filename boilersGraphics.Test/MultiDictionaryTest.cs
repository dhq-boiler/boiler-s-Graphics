using boilersGraphics.Helpers;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace boilersGraphics.Test
{
    [TestFixture]
    public class MultiDictionaryTest
    {
        [Test]
        public void Add_新規キーで値1つ()
        {
            var d = new MultiDictionary<string, int>();
            d.Add("a", 1);
            Assert.That(d.Count, Is.EqualTo(1));
            Assert.That(d["a"], Is.EqualTo(new List<int> { 1 }));
        }

        [Test]
        public void Add_同一キーで複数値()
        {
            var d = new MultiDictionary<string, int>();
            d.Add("a", 1);
            d.Add("a", 2);
            d.Add("a", 3);
            Assert.That(d.Count, Is.EqualTo(1));
            Assert.That(d["a"], Is.EqualTo(new List<int> { 1, 2, 3 }));
        }

        [Test]
        public void Add_paramsオーバーロード()
        {
            var d = new MultiDictionary<string, int>();
            d.Add("a", 1, 2, 3);
            Assert.That(d["a"], Is.EqualTo(new List<int> { 1, 2, 3 }));
        }

        [Test]
        public void Add_IEnumerableオーバーロード()
        {
            var d = new MultiDictionary<string, int>();
            d.Add("a", new List<int> { 4, 5, 6 });
            Assert.That(d["a"], Is.EqualTo(new List<int> { 4, 5, 6 }));
        }

        [Test]
        public void Indexer_setで値リストを置き換え()
        {
            var d = new MultiDictionary<string, int>();
            d.Add("a", 1);
            d["a"] = new List<int> { 99, 100 };
            Assert.That(d["a"], Is.EqualTo(new List<int> { 99, 100 }));
        }

        [Test]
        public void Remove_keyValue_対象値を削除しtrueを返す()
        {
            var d = new MultiDictionary<string, int>();
            d.Add("a", 1, 2, 3);
            Assert.That(d.Remove("a", 2), Is.True);
            Assert.That(d["a"], Is.EqualTo(new List<int> { 1, 3 }));
        }

        [Test]
        public void Remove_存在しない値はfalse()
        {
            var d = new MultiDictionary<string, int>();
            d.Add("a", 1);
            Assert.That(d.Remove("a", 99), Is.False);
        }

        [Test]
        public void Remove_keyのみ_キー全体を削除()
        {
            var d = new MultiDictionary<string, int>();
            d.Add("a", 1, 2);
            d.Add("b", 3);
            Assert.That(d.Remove("a"), Is.True);
            Assert.That(d.Count, Is.EqualTo(1));
            Assert.That(d.ContainsKey("a"), Is.False);
            Assert.That(d.ContainsKey("b"), Is.True);
        }

        [Test]
        public void Clear_空にする()
        {
            var d = new MultiDictionary<string, int>();
            d.Add("a", 1);
            d.Add("b", 2);
            d.Clear();
            Assert.That(d.Count, Is.EqualTo(0));
        }

        [Test]
        public void Contains_キーと値の組合せ()
        {
            var d = new MultiDictionary<string, int>();
            d.Add("a", 1, 2);
            Assert.That(d.Contains("a", 1), Is.True);
            Assert.That(d.Contains("a", 3), Is.False);
        }

        [Test]
        public void ContainsKey_キーの有無()
        {
            var d = new MultiDictionary<string, int>();
            d.Add("a", 1);
            Assert.That(d.ContainsKey("a"), Is.True);
            Assert.That(d.ContainsKey("b"), Is.False);
        }

        [Test]
        public void KeysとValues()
        {
            var d = new MultiDictionary<string, int>();
            d.Add("a", 1);
            d.Add("b", 2, 3);
            Assert.That(d.Keys.OrderBy(x => x).ToList(), Is.EqualTo(new[] { "a", "b" }));
            Assert.That(d.Values.Count, Is.EqualTo(2));
        }

        [Test]
        public void GetEnumerator_yield版で全件列挙()
        {
            var d = new MultiDictionary<string, int>();
            d.Add("a", 1);
            d.Add("b", 2);
            var keys = new List<string>();
            foreach (var kvp in d)
                keys.Add(kvp.Key);
            Assert.That(keys.OrderBy(x => x).ToList(), Is.EqualTo(new[] { "a", "b" }));
        }

        [Test]
        public void IEnumerableT_GetEnumeratorでEnumeratorを返す()
        {
            var d = new MultiDictionary<string, int>();
            d.Add("a", 1);
            d.Add("b", 2);
            var en = ((IEnumerable<KeyValuePair<string, List<int>>>)d).GetEnumerator();
            int count = 0;
            while (en.MoveNext()) count++;
            Assert.That(count, Is.EqualTo(2));
        }

        [Test]
        public void IEnumerable_NonGeneric_GetEnumeratorでEnumeratorを返す()
        {
            var d = new MultiDictionary<string, int>();
            d.Add("a", 1);
            var en = ((IEnumerable)d).GetEnumerator();
            int count = 0;
            while (en.MoveNext()) count++;
            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public void Enumerator_MoveNextとCurrent()
        {
            var list = new List<KeyValuePair<string, List<int>>>
            {
                new("a", new List<int> { 1 }),
                new("b", new List<int> { 2 }),
            };
            var en = new MultiDictionary<string, int>.MultiDictionaryEnumerator(list);
            Assert.That(en.MoveNext(), Is.True);
            Assert.That(en.Current.Key, Is.EqualTo("a"));
            Assert.That(en.MoveNext(), Is.True);
            Assert.That(en.Current.Key, Is.EqualTo("b"));
            Assert.That(en.MoveNext(), Is.False);
        }

        [Test]
        public void Enumerator_Resetで先頭に戻る()
        {
            var list = new List<KeyValuePair<string, List<int>>>
            {
                new("a", new List<int> { 1 }),
            };
            var en = new MultiDictionary<string, int>.MultiDictionaryEnumerator(list);
            en.MoveNext();
            en.Reset();
            Assert.That(en.MoveNext(), Is.True);
            Assert.That(en.Current.Key, Is.EqualTo("a"));
        }

        [Test]
        public void Enumerator_NonGeneric_Currentも同じ値を返す()
        {
            var list = new List<KeyValuePair<string, List<int>>>
            {
                new("a", new List<int> { 1 }),
            };
            var en = new MultiDictionary<string, int>.MultiDictionaryEnumerator(list);
            en.MoveNext();
            object cur = ((IEnumerator)en).Current;
            Assert.That(cur, Is.EqualTo(en.Current));
        }

        [Test]
        public void Enumerator_Dispose_2回呼んでも例外なし()
        {
            var en = new MultiDictionary<string, int>.MultiDictionaryEnumerator(
                new List<KeyValuePair<string, List<int>>>());
            Assert.That(() => { en.Dispose(); en.Dispose(); }, Throws.Nothing);
        }
    }
}
