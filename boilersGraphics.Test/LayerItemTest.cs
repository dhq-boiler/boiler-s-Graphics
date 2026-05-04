using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using NUnit.Framework;
using System;
using System.Threading;

namespace boilersGraphics.Test
{
    [TestFixture]
    public class LayerItemTest
    {
        // ---- CompareTo ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void CompareTo_object_nullなら1()
        {
            App.IsTest = true;
            var item = new NRectangleViewModel();
            var li = new LayerItem(item);
            Assert.That(li.CompareTo((object)null), Is.EqualTo(1));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void CompareTo_object_LayerItem以外なら1()
        {
            App.IsTest = true;
            var item = new NRectangleViewModel();
            var li = new LayerItem(item);
            Assert.That(li.CompareTo((object)"not layer item"), Is.EqualTo(1));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void CompareTo_object_ZIndexで比較()
        {
            App.IsTest = true;
            var a = new NRectangleViewModel(); a.ZIndex.Value = 1;
            var b = new NRectangleViewModel(); b.ZIndex.Value = 5;
            var liA = new LayerItem(a);
            var liB = new LayerItem(b);
            Assert.That(liA.CompareTo((object)liB), Is.LessThan(0));
            Assert.That(liB.CompareTo((object)liA), Is.GreaterThan(0));
            Assert.That(liA.CompareTo((object)liA), Is.EqualTo(0));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void CompareTo_LayerTreeViewItemBase_nullなら1()
        {
            App.IsTest = true;
            var li = new LayerItem(new NRectangleViewModel());
            Assert.That(li.CompareTo((LayerTreeViewItemBase)null), Is.EqualTo(1));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void CompareTo_LayerTreeViewItemBase_LayerItem以外なら1()
        {
            App.IsTest = true;
            var li = new LayerItem(new NRectangleViewModel());
            var layer = new Layer();
            Assert.That(li.CompareTo((LayerTreeViewItemBase)layer), Is.EqualTo(1));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void CompareTo_LayerTreeViewItemBase_ZIndexで比較()
        {
            App.IsTest = true;
            var a = new NRectangleViewModel(); a.ZIndex.Value = 0;
            var b = new NRectangleViewModel(); b.ZIndex.Value = 100;
            var liA = new LayerItem(a);
            var liB = new LayerItem(b);
            Assert.That(liA.CompareTo((LayerTreeViewItemBase)liB), Is.LessThan(0));
        }

        // ---- ShouldRender ----

        [Test]
        public void ShouldRender_未登録キーは初回true_直後は再trueにならない()
        {
            var key = new object();
            Assert.That(LayerItem.ShouldRender(key), Is.True);
            Assert.That(LayerItem.ShouldRender(key), Is.False);
        }

        [Test]
        public void ShouldRender_別キーは独立判定()
        {
            var k1 = new object();
            var k2 = new object();
            LayerItem.ShouldRender(k1);
            // k2 は別キーなので true
            Assert.That(LayerItem.ShouldRender(k2), Is.True);
        }

        [Test]
        public void ShouldRender_十分時間経過後は再びtrue()
        {
            var key = new object();
            LayerItem.ShouldRender(key); // 登録
            Thread.Sleep(50); // 16ms+ 待つ
            Assert.That(LayerItem.ShouldRender(key), Is.True);
        }

        // ---- ShowPropertiesAndFields ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void ShowPropertiesAndFields_型名とプロパティ列を含む()
        {
            App.IsTest = true;
            var item = new NRectangleViewModel();
            var li = new LayerItem(item);
            var s = li.ShowPropertiesAndFields();
            Assert.That(s, Does.StartWith("<LayerItem>{"));
            Assert.That(s, Does.EndWith("}"));
            // 主要プロパティが含まれる
            Assert.That(s, Does.Contain("Item="));
            Assert.That(s, Does.Contain("Appearance="));
        }

        // ---- ToString ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void ToString_主要フィールドを含む()
        {
            App.IsTest = true;
            var item = new NRectangleViewModel();
            item.ZIndex.Value = 7;
            var li = new LayerItem(item, new Layer(), "MyLayerItem");
            var s = li.ToString();
            Assert.That(s, Does.Contain("Name=MyLayerItem"));
            Assert.That(s, Does.Contain("ZIndex=7"));
            Assert.That(s, Does.Contain("IsSelected="));
            Assert.That(s, Does.Contain("IsVisible="));
            Assert.That(s, Does.Contain("ID="));
        }

        // ---- ctor + Init 副作用 ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void ctor_新規LayerItemはIsVisibleがtrue()
        {
            App.IsTest = true;
            var item = new NRectangleViewModel();
            var li = new LayerItem(item);
            Assert.That(li.IsVisible.Value, Is.True);
            // Init で Item.Value.IsVisible に伝播
            Assert.That(item.IsVisible.Value, Is.True);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void ctor_3引数版はNameとParentが設定される()
        {
            App.IsTest = true;
            var owner = new Layer();
            var item = new NRectangleViewModel();
            var li = new LayerItem(item, owner, "named");
            Assert.That(li.Name.Value, Is.EqualTo("named"));
            Assert.That(li.Parent.Value, Is.SameAs(owner));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void SwitchVisibilityCommand_実行でIsVisibleがトグル()
        {
            App.IsTest = true;
            var item = new NRectangleViewModel();
            var li = new LayerItem(item);
            Assert.That(li.IsVisible.Value, Is.True);
            li.SwitchVisibilityCommand.Execute(R3.Unit.Default);
            Assert.That(li.IsVisible.Value, Is.False);
            Assert.That(item.IsVisible.Value, Is.False);
            li.SwitchVisibilityCommand.Execute(R3.Unit.Default);
            Assert.That(li.IsVisible.Value, Is.True);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void IsSelected_setでItemにも伝播()
        {
            App.IsTest = true;
            var item = new NRectangleViewModel();
            var li = new LayerItem(item);
            li.IsSelected.Value = true;
            Assert.That(item.IsSelected.Value, Is.True);
            li.IsSelected.Value = false;
            Assert.That(item.IsSelected.Value, Is.False);
        }

        // ---- Dispose ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void Dispose_2回呼んでも例外なし()
        {
            App.IsTest = true;
            var li = new LayerItem(new NRectangleViewModel());
            Assert.That(() => { li.Dispose(); li.Dispose(); }, Throws.Nothing);
        }
    }
}
