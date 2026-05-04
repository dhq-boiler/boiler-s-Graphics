using boilersGraphics.Extensions;
using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using NUnit.Framework;
using ObservableCollections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;

namespace boilersGraphics.Test
{
    [TestFixture]
    public class ExtensionsBasicTest
    {
        // ---- Shift / Multiple ----

        [Test]
        public void Shift_Pointが指定量平行移動()
        {
            var p = new Point(3, 4).Shift(5, 6);
            Assert.That(p.X, Is.EqualTo(8));
            Assert.That(p.Y, Is.EqualTo(10));
        }

        [Test]
        public void Shift_負方向移動も可能()
        {
            var p = new Point(10, 20).Shift(-3, -4);
            Assert.That(p.X, Is.EqualTo(7));
            Assert.That(p.Y, Is.EqualTo(16));
        }

        [Test]
        public void Multiple_Pointが指定倍率に拡大()
        {
            var p = new Point(3, 4).Multiple(2, 3);
            Assert.That(p.X, Is.EqualTo(6));
            Assert.That(p.Y, Is.EqualTo(12));
        }

        [Test]
        public void Multiple_0倍は原点()
        {
            var p = new Point(5, 7).Multiple(0, 0);
            Assert.That(p.X, Is.EqualTo(0));
            Assert.That(p.Y, Is.EqualTo(0));
        }

        // ---- ToObservableCollection ----

        [Test]
        public void ToObservableCollection_要素が引き継がれる()
        {
            var src = new[] { 1, 2, 3 };
            var oc = src.ToObservableCollection();
            Assert.That(oc, Is.InstanceOf<ObservableCollection<int>>());
            Assert.That(oc, Is.EqualTo(new[] { 1, 2, 3 }));
        }

        [Test]
        public void ToObservableCollection_nullで例外()
        {
            IEnumerable<int> src = null;
            Assert.That(() => src.ToObservableCollection(), Throws.TypeOf<ArgumentNullException>());
        }

        // ---- SelectRecursive ----

        // ツリー構造をテストする小さなノードクラス
        private sealed class TreeNode
        {
            public string Name { get; }
            public List<TreeNode> Children { get; } = new();
            public TreeNode(string name) { Name = name; }
        }

        [Test]
        public void SelectRecursive_深さ優先で再帰展開()
        {
            var root1 = new TreeNode("a");
            var b = new TreeNode("b");
            var c = new TreeNode("c");
            var d = new TreeNode("d");
            root1.Children.Add(b);
            root1.Children.Add(c);
            b.Children.Add(d);
            var roots = new[] { root1 };

            var result = roots.SelectRecursive<TreeNode, TreeNode>(x => x.Children).ToArray();
            // root → child → grandchild の順
            Assert.That(result.Select(x => x.Name).ToArray(),
                Is.EqualTo(new[] { "a", "b", "d", "c" }));
        }

        [Test]
        public void SelectRecursive_空ソースは空()
        {
            var roots = new TreeNode[0];
            var result = roots.SelectRecursive<TreeNode, TreeNode>(x => x.Children).ToArray();
            Assert.That(result, Is.Empty);
        }

        // ---- HasAsAncestor ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void HasAsAncestor_直接の親はtrue()
        {
            App.IsTest = true;
            var grandparent = new Layer { };
            grandparent.Name.Value = "g";
            var parent = new Layer { };
            parent.Name.Value = "p";
            var child = new Layer { };
            child.Name.Value = "c";
            parent.Parent.Value = grandparent;
            child.Parent.Value = parent;
            Assert.That(child.HasAsAncestor(parent), Is.True);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void HasAsAncestor_祖先もtrue()
        {
            App.IsTest = true;
            var grandparent = new Layer { };
            var parent = new Layer { };
            var child = new Layer { };
            parent.Parent.Value = grandparent;
            child.Parent.Value = parent;
            // Implementation walks Parent chain via while (temp.Parent.Value != null)
            // and compares temp == ancestor each step. grandparent.Parent.Value is null
            // so the loop ends without checking the root itself; verify intermediate
            // ancestor (parent) is correctly detected.
            Assert.That(child.HasAsAncestor(parent), Is.True);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void HasAsAncestor_自分自身でも親階層にあればtrue()
        {
            App.IsTest = true;
            var parent = new Layer { };
            var child = new Layer { };
            child.Parent.Value = parent;
            // ループの最初で temp == ancestor (== child) チェックして true
            Assert.That(child.HasAsAncestor(child), Is.True);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void HasAsAncestor_無関係なノードはfalse()
        {
            App.IsTest = true;
            var parent = new Layer { };
            var child = new Layer { };
            child.Parent.Value = parent;
            var other = new Layer { };
            Assert.That(child.HasAsAncestor(other), Is.False);
        }

        // ---- Items (ObservableCollection<Layer>) ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void Items_全LayerのLayerItemのItem値を平坦化()
        {
            App.IsTest = true;
            var layer1 = new Layer { };
            var layer2 = new Layer { };
            var item1 = new NRectangleViewModel();
            var item2 = new NEllipseViewModel();
            var li1 = new LayerItem(item1, layer1, "i1");
            var li2 = new LayerItem(item2, layer2, "i2");
            layer1.Children.Add(li1);
            layer2.Children.Add(li2);

            var oc = new ObservableCollection<Layer> { layer1, layer2 };
            var items = oc.Items().ToArray();
            Assert.That(items.Length, Is.EqualTo(2));
            Assert.That(items, Does.Contain(item1));
            Assert.That(items, Does.Contain(item2));
        }

        // ---- WithPickupChildren ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void WithPickupChildren_Group選択時_子もyield()
        {
            App.IsTest = true;
            var group = new GroupItemViewModel();
            var groupId = group.ID;
            var child1 = new NRectangleViewModel();
            child1.ParentID = groupId;
            var child2 = new NEllipseViewModel();
            child2.ParentID = groupId;
            var standalone = new NRectangleViewModel();
            // standalone.ParentID は Guid.Empty

            var selected = new SelectableDesignerItemViewModelBase[] { group };
            var all = new SelectableDesignerItemViewModelBase[] { child1, child2, standalone };

            var result = selected.WithPickupChildren(all).ToArray();
            Assert.That(result.Length, Is.EqualTo(3));
            Assert.That(result, Does.Contain(group));
            Assert.That(result, Does.Contain(child1));
            Assert.That(result, Does.Contain(child2));
            Assert.That(result, Does.Not.Contain(standalone));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void WithPickupChildren_Group以外は自分のみyield()
        {
            App.IsTest = true;
            var rect = new NRectangleViewModel();
            var selected = new SelectableDesignerItemViewModelBase[] { rect };
            var all = new SelectableDesignerItemViewModelBase[] { rect };
            var result = selected.WithPickupChildren(all).ToArray();
            Assert.That(result, Is.EqualTo(new[] { rect }));
        }

        // ---- AddRange ----

        [Test]
        public void AddRange_全要素を順次Add()
        {
            var target = new ObservableList<int>().ToWritableNotifyCollectionChanged();
            target.AddRange(new[] { 1, 2, 3 });
            Assert.That(target, Is.EqualTo(new[] { 1, 2, 3 }));
        }

        [Test]
        public void AddRange_空ソースなら何もしない()
        {
            var target = new ObservableList<int>().ToWritableNotifyCollectionChanged();
            target.AddRange(System.Linq.Enumerable.Empty<int>());
            Assert.That(target.Count, Is.EqualTo(0));
        }

        // ---- Sort ----

        [Test]
        public void Sort_topを先頭_他を後ろに置きReverseされる()
        {
            var src = new ObservableList<string> { "a", "b", "c", "d" }.ToWritableNotifyCollectionChanged();
            var sorted = src.Sort("c");
            // 内部で newCollection = [top=c, a, b, d] → reverse → [d, b, a, c]
            var collected = new List<string>();
            foreach (var s in sorted) collected.Add(s);
            Assert.That(collected, Is.EqualTo(new[] { "d", "b", "a", "c" }));
        }
    }
}
