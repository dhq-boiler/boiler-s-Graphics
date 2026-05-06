using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using NUnit.Framework;
using R3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace boilersGraphics.Test
{
    [TestFixture]
    public class LayerTreeViewItemBaseTest
    {
        private static Layer NewLayer(string name = "L", bool preview = true)
        {
            App.IsTest = true;
            var layer = new Layer(isPreview: preview);
            layer.Name.Value = name;
            return layer;
        }

        private static LayerItem NewLayerItem(string name = "I", int zIndex = 0)
        {
            App.IsTest = true;
            var item = new NRectangleViewModel();
            item.ZIndex.Value = zIndex;
            return new LayerItem(item, NewLayer("parent"), name);
        }

        // ---- ToString ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void ToString_Layer_Name_IsSelected_Parentを含む()
        {
            var layer = NewLayer("layer-A");
            var s = layer.ToString();
            Assert.That(s, Does.Contain("Name=layer-A"));
            Assert.That(s, Does.Contain("IsSelected=False"));
            Assert.That(s, Does.Contain("Parent="));
        }

        // ---- Subscribe (IObservable<LayerTreeViewItemBaseObservable>) ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void Subscribe_observerを登録_即OnNext()
        {
            var layer = NewLayer();
            int onNext = 0;
            var observer = new ManualObserver<LayerTreeViewItemBaseObservable>(_ => onNext++);
            using var d = layer.Subscribe(observer);
            Assert.That(onNext, Is.EqualTo(1));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void Subscribe_Dispose_observerが解除()
        {
            var layer = NewLayer();
            var observer = new ManualObserver<LayerTreeViewItemBaseObservable>(_ => { });
            var d = layer.Subscribe(observer);
            // 内部的に observer がリスト追加された後、Dispose で削除
            Assert.That(() => d.Dispose(), Throws.Nothing);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void Subscribe_Layer専用ObservableのSubscribe()
        {
            var layer = NewLayer();
            int onNext = 0;
            var observer = new ManualObserver<LayerObservable>(_ => onNext++);
            // ここの Subscribe は Layer のオーバーロード
            using var d = ((IObservable<LayerObservable>)layer).Subscribe(observer);
            Assert.That(onNext, Is.EqualTo(1));
        }

        // Helper observer
        private sealed class ManualObserver<T> : IObserver<T>
        {
            private readonly Action<T> _onNext;
            public ManualObserver(Action<T> onNext) { _onNext = onNext; }
            public void OnNext(T value) => _onNext(value);
            public void OnError(Exception error) { }
            public void OnCompleted() { }
        }

        // ---- CompareTo (Layer) ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void Layer_CompareTo_object_nullなら1()
        {
            var layer = NewLayer();
            Assert.That(layer.CompareTo((object)null), Is.EqualTo(1));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void Layer_CompareTo_object_LayerTreeViewItemBase以外なら1()
        {
            var layer = NewLayer();
            Assert.That(layer.CompareTo((object)"not"), Is.EqualTo(1));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void Layer_CompareTo_LayerTreeViewItemBase_nullなら1()
        {
            var layer = NewLayer();
            Assert.That(layer.CompareTo((LayerTreeViewItemBase)null), Is.EqualTo(1));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void Layer_CompareTo_ZIndexMaxで比較()
        {
            var layerA = NewLayer("A");
            var item1 = new NRectangleViewModel(); item1.ZIndex.Value = 1;
            var li1 = new LayerItem(item1, layerA, "x");
            layerA.Children.Add(li1);

            var layerB = NewLayer("B");
            var item2 = new NRectangleViewModel(); item2.ZIndex.Value = 5;
            var li2 = new LayerItem(item2, layerB, "y");
            layerB.Children.Add(li2);

            Assert.That(layerA.CompareTo(layerB), Is.LessThan(0));
        }

        // ---- Layer.SwitchVisibilityCommand ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void Layer_SwitchVisibilityCommandでIsVisibleがトグル()
        {
            var layer = NewLayer();
            Assert.That(layer.IsVisible.Value, Is.True);
            layer.SwitchVisibilityCommand.Execute(R3.Unit.Default);
            Assert.That(layer.IsVisible.Value, Is.False);
            layer.SwitchVisibilityCommand.Execute(R3.Unit.Default);
            Assert.That(layer.IsVisible.Value, Is.True);
        }

        // ---- Layer.ShouldRender (static) ----

        [Test]
        public void Layer_ShouldRender_未登録キーは初回true_直後はfalse()
        {
            var key = new object();
            Assert.That(Layer.ShouldRender(key), Is.True);
            Assert.That(Layer.ShouldRender(key), Is.False);
        }

        [Test]
        public void Layer_ShouldRender_十分時間経過後は再びtrue()
        {
            var key = new object();
            Layer.ShouldRender(key);
            Thread.Sleep(50);
            Assert.That(Layer.ShouldRender(key), Is.True);
        }

        // ---- ChildrenSwitchVisibility / ChildrenSwitchIsHitTestVisible ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void ChildrenSwitchVisibility_全ての子に再帰()
        {
            var root = NewLayer("root");
            var item = new NRectangleViewModel();
            var li = new LayerItem(item, root, "child");
            root.Children.Add(li);

            root.ChildrenSwitchVisibility(false);
            Assert.That(li.IsVisible.Value, Is.False);
            root.ChildrenSwitchVisibility(true);
            Assert.That(li.IsVisible.Value, Is.True);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void ChildrenSwitchIsHitTestVisible_LayerItemのIsHitTestVisibleを設定()
        {
            var root = NewLayer("root");
            var item = new NRectangleViewModel();
            var li = new LayerItem(item, root, "child");
            root.Children.Add(li);

            root.ChildrenSwitchIsHitTestVisible(false);
            Assert.That(item.IsHitTestVisible.Value, Is.False);
            root.ChildrenSwitchIsHitTestVisible(true);
            Assert.That(item.IsHitTestVisible.Value, Is.True);
        }

        // ---- SetParentToChildren ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void SetParentToChildren_全ての子のParentを設定()
        {
            var root = NewLayer("root");
            var child = NewLayer("child");
            root.Children.Add(child);

            root.SetParentToChildren();
            Assert.That(root.Parent.Value, Is.Null);
            Assert.That(child.Parent.Value, Is.SameAs(root));
        }

        // ---- InsertBeforeChildren / InsertAfterChildren ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void InsertBeforeChildren_指定要素の前に挿入()
        {
            var root = NewLayer("root");
            var a = NewLayer("a");
            var b = NewLayer("b");
            var c = NewLayer("c");
            root.Children.Add(a);
            root.Children.Add(b);

            root.InsertBeforeChildren(c, b);

            Assert.That(root.Children.IndexOf(c), Is.EqualTo(1));
            Assert.That(root.Children.IndexOf(b), Is.EqualTo(2));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void InsertBeforeChildren_to未存在は何もしない()
        {
            var root = NewLayer("root");
            var a = NewLayer("a");
            var b = NewLayer("b");
            root.Children.Add(a);

            root.InsertBeforeChildren(b, NewLayer("not-in-tree"));
            Assert.That(root.Children.Count, Is.EqualTo(1));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void InsertAfterChildren_指定要素の後に挿入()
        {
            var root = NewLayer("root");
            var a = NewLayer("a");
            var b = NewLayer("b");
            var c = NewLayer("c");
            root.Children.Add(a);
            root.Children.Add(b);

            root.InsertAfterChildren(c, a);

            Assert.That(root.Children.IndexOf(a), Is.EqualTo(0));
            Assert.That(root.Children.IndexOf(c), Is.EqualTo(1));
            Assert.That(root.Children.IndexOf(b), Is.EqualTo(2));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void InsertAfterChildren_to未存在は何もしない()
        {
            var root = NewLayer("root");
            var a = NewLayer("a");
            root.Children.Add(a);

            root.InsertAfterChildren(NewLayer("x"), NewLayer("not-in-tree"));
            Assert.That(root.Children.Count, Is.EqualTo(1));
        }

        // ---- ContainsParent ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void ContainsParent_直接の親はtrue()
        {
            var root = NewLayer("root");
            var child = NewLayer("child");
            child.Parent.Value = root;
            Assert.That(child.ContainsParent(root), Is.True);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void ContainsParent_祖先もtrue()
        {
            var grandparent = NewLayer("g");
            var parent = NewLayer("p");
            var child = NewLayer("c");
            parent.Parent.Value = grandparent;
            child.Parent.Value = parent;
            Assert.That(child.ContainsParent(grandparent), Is.True);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void ContainsParent_Parent未設定はfalse()
        {
            var orphan = NewLayer("orphan");
            var someone = NewLayer("someone");
            Assert.That(orphan.ContainsParent(someone), Is.False);
        }

        // ---- GetNewZIndex ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void GetNewZIndex_空なら0()
        {
            var layer = NewLayer();
            Assert.That(layer.GetNewZIndex(new List<LayerTreeViewItemBase>()), Is.EqualTo(0));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void GetNewZIndex_最大プラス1を返す()
        {
            var layer = NewLayer();
            var item1 = new NRectangleViewModel(); item1.ZIndex.Value = 5;
            var item2 = new NRectangleViewModel(); item2.ZIndex.Value = 10;
            var li1 = new LayerItem(item1, layer, "a");
            var li2 = new LayerItem(item2, layer, "b");
            layer.Children.Add(li1);
            layer.Children.Add(li2);
            Assert.That(layer.GetNewZIndex(new[] { (LayerTreeViewItemBase)layer }), Is.EqualTo(11));
        }

        // ---- Dispose ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void Dispose_2回呼んでも例外なし()
        {
            var layer = NewLayer();
            Assert.That(() => { layer.Dispose(); layer.Dispose(); }, Throws.Nothing);
        }

        // ---- LayerItemsChangedAsObservable / LayerChangedAsObservable / SelectedLayerItemsChangedAsObservable ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void LayerItemsChangedAsObservable_Subscribeで例外なし()
        {
            var layer = NewLayer();
            using var sub = layer.LayerItemsChangedAsObservable().Subscribe((Action<Unit>)(_ => { }));
            Assert.That(sub, Is.Not.Null);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void LayerChangedAsObservable_Subscribeで例外なし()
        {
            var layer = NewLayer();
            using var sub = layer.LayerChangedAsObservable().Subscribe((Action<Unit>)(_ => { }));
            Assert.That(sub, Is.Not.Null);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void SelectedLayerItemsChangedAsObservable_Subscribeで例外なし()
        {
            var layer = NewLayer();
            using var sub = layer.SelectedLayerItemsChangedAsObservable().Subscribe((Action<Unit>)(_ => { }));
            Assert.That(sub, Is.Not.Null);
        }
    }
}
