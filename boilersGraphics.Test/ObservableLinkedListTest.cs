using boilersGraphics.Helpers;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace boilersGraphics.Test
{
    [TestFixture]
    public class ObservableLinkedListTest
    {
        [Test]
        public void デフォルトコンストラクタは空リスト()
        {
            var list = new ObservableLinkedList<int>();
            Assert.That(list.Count, Is.EqualTo(0));
            Assert.That(list.First, Is.Null);
            Assert.That(list.Last, Is.Null);
        }

        [Test]
        public void コレクション初期化コンストラクタ()
        {
            var list = new ObservableLinkedList<int>(new[] { 1, 2, 3 });
            Assert.That(list.Count, Is.EqualTo(3));
            Assert.That(list.First.Value, Is.EqualTo(1));
            Assert.That(list.Last.Value, Is.EqualTo(3));
        }

        [Test]
        public void AddFirst_先頭に追加してCollectionChangedが発火()
        {
            var list = new ObservableLinkedList<int>(new[] { 2, 3 });
            int fired = 0;
            list.CollectionChanged += (s, e) =>
            {
                fired++;
                Assert.That(e.Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));
            };
            var node = list.AddFirst(1);
            Assert.That(list.First.Value, Is.EqualTo(1));
            Assert.That(node.Value, Is.EqualTo(1));
            Assert.That(fired, Is.EqualTo(1));
        }

        [Test]
        public void AddFirst_LinkedListNode版()
        {
            var list = new ObservableLinkedList<int>(new[] { 2 });
            list.AddFirst(new LinkedListNode<int>(1));
            Assert.That(list.First.Value, Is.EqualTo(1));
        }

        [Test]
        public void AddLast_末尾に追加()
        {
            var list = new ObservableLinkedList<int>(new[] { 1 });
            var node = list.AddLast(2);
            Assert.That(list.Last.Value, Is.EqualTo(2));
            Assert.That(node.Value, Is.EqualTo(2));
        }

        [Test]
        public void AddLast_LinkedListNode版()
        {
            var list = new ObservableLinkedList<int>(new[] { 1 });
            list.AddLast(new LinkedListNode<int>(2));
            Assert.That(list.Last.Value, Is.EqualTo(2));
        }

        [Test]
        public void AddBefore_指定ノードの前に挿入()
        {
            var list = new ObservableLinkedList<int>(new[] { 1, 3 });
            var node = list.AddBefore(list.Last, 2);
            Assert.That(list.Count, Is.EqualTo(3));
            Assert.That(list[1], Is.EqualTo(2));
            Assert.That(node.Value, Is.EqualTo(2));
        }

        [Test]
        public void AddBefore_LinkedListNode版()
        {
            var list = new ObservableLinkedList<int>(new[] { 1, 3 });
            list.AddBefore(list.Last, new LinkedListNode<int>(2));
            Assert.That(list[1], Is.EqualTo(2));
        }

        [Test]
        public void AddAfter_指定ノードの後に挿入()
        {
            var list = new ObservableLinkedList<int>(new[] { 1, 3 });
            var node = list.AddAfter(list.First, 2);
            Assert.That(list[1], Is.EqualTo(2));
            Assert.That(node.Value, Is.EqualTo(2));
        }

        [Test]
        public void AddAfter_LinkedListNode版()
        {
            var list = new ObservableLinkedList<int>(new[] { 1, 3 });
            list.AddAfter(list.First, new LinkedListNode<int>(2));
            Assert.That(list[1], Is.EqualTo(2));
        }

        [Test]
        public void Clear_空にしてCollectionChanged発火()
        {
            var list = new ObservableLinkedList<int>(new[] { 1, 2, 3 });
            int fired = 0;
            list.CollectionChanged += (_, _) => fired++;
            list.Clear();
            Assert.That(list.Count, Is.EqualTo(0));
            Assert.That(fired, Is.EqualTo(1));
        }

        [Test]
        public void Contains_含まれる値()
        {
            var list = new ObservableLinkedList<int>(new[] { 1, 2, 3 });
            Assert.That(list.Contains(2), Is.True);
            Assert.That(list.Contains(99), Is.False);
        }

        [Test]
        public void CopyTo_配列にコピー()
        {
            var list = new ObservableLinkedList<int>(new[] { 1, 2, 3 });
            var arr = new int[5];
            list.CopyTo(arr, 1);
            Assert.That(arr, Is.EqualTo(new[] { 0, 1, 2, 3, 0 }));
        }

        [Test]
        public void Find_最初に一致するノードを返す()
        {
            var list = new ObservableLinkedList<int>(new[] { 1, 2, 2, 3 });
            var node = list.Find(2);
            Assert.That(node, Is.Not.Null);
            Assert.That(node.Value, Is.EqualTo(2));
            Assert.That(node.Previous.Value, Is.EqualTo(1));
        }

        [Test]
        public void FindLast_最後に一致するノードを返す()
        {
            var list = new ObservableLinkedList<int>(new[] { 1, 2, 2, 3 });
            var node = list.FindLast(2);
            Assert.That(node, Is.Not.Null);
            Assert.That(node.Next.Value, Is.EqualTo(3));
        }

        [Test]
        public void GetLinkedListType_内部リスト型を返す()
        {
            var list = new ObservableLinkedList<int>();
            Assert.That(list.GetLinkedListType(), Is.EqualTo(typeof(LinkedList<int>)));
        }

        [Test]
        public void Remove_値版_存在すれば削除しtrueを返す()
        {
            var list = new ObservableLinkedList<int>(new[] { 1, 2, 3 });
            int fired = 0;
            list.CollectionChanged += (_, _) => fired++;
            Assert.That(list.Remove(2), Is.True);
            Assert.That(list.Count, Is.EqualTo(2));
            Assert.That(fired, Is.EqualTo(1));
        }

        [Test]
        public void Remove_値版_存在しなければfalse()
        {
            var list = new ObservableLinkedList<int>(new[] { 1 });
            Assert.That(list.Remove(99), Is.False);
        }

        [Test]
        public void Remove_ノード版()
        {
            var list = new ObservableLinkedList<int>(new[] { 1, 2, 3 });
            var middle = list.Find(2);
            list.Remove(middle);
            Assert.That(list.Count, Is.EqualTo(2));
            Assert.That(list[0], Is.EqualTo(1));
            Assert.That(list[1], Is.EqualTo(3));
        }

        [Test]
        public void RemoveFirst()
        {
            var list = new ObservableLinkedList<int>(new[] { 1, 2, 3 });
            list.RemoveFirst();
            Assert.That(list.First.Value, Is.EqualTo(2));
        }

        [Test]
        public void RemoveLast()
        {
            var list = new ObservableLinkedList<int>(new[] { 1, 2, 3 });
            list.RemoveLast();
            Assert.That(list.Last.Value, Is.EqualTo(2));
        }

        [Test]
        public void Indexer_インデックスアクセス()
        {
            var list = new ObservableLinkedList<int>(new[] { 10, 20, 30 });
            Assert.That(list[0], Is.EqualTo(10));
            Assert.That(list[2], Is.EqualTo(30));
        }

        [Test]
        public void GetEnumerator_全件を順に列挙()
        {
            var list = new ObservableLinkedList<int>(new[] { 1, 2, 3 });
            var collected = new List<int>();
            foreach (var v in list) collected.Add(v);
            Assert.That(collected, Is.EqualTo(new[] { 1, 2, 3 }));
        }

        [Test]
        public void IEnumerable_NonGeneric_GetEnumerator()
        {
            var list = new ObservableLinkedList<int>(new[] { 1, 2 });
            var en = ((IEnumerable)list).GetEnumerator();
            int count = 0;
            while (en.MoveNext()) count++;
            Assert.That(count, Is.EqualTo(2));
        }

        [Test]
        public void LinkedListEquals_同インスタンスのみtrue()
        {
            var list = new ObservableLinkedList<int>();
            Assert.That(list.LinkedListEquals(new object()), Is.False);
        }
    }
}
