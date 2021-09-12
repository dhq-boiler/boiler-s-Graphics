using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace boilersGraphics.Test
{
    [TestFixture]
    public class SelectedItemsTest
    {
        [Test]
        public void アイテムは選択されていない()
        {
            boilersGraphics.App.IsTest = true;
            var mainWindowViewModel = new MainWindowViewModel(null);
            var viewModel = new DiagramViewModel(mainWindowViewModel, 1000, 1000);
            viewModel.Layers.Clear();
            var layer1 = new Layer();
            layer1.Name.Value = "レイヤー1";
            viewModel.Layers.Add(layer1);
            layer1.IsSelected.Value = true;

            var item1 = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(item1);
            var item2 = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(item2);
            var item3 = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(item3);

            Assert.That(viewModel.SelectedItems.Value.ToList(), Has.Count.EqualTo(0));
        }

        [Test]
        public void アイテム1つが選択されている()
        {
            boilersGraphics.App.IsTest = true;
            var mainWindowViewModel = new MainWindowViewModel(null);
            var viewModel = new DiagramViewModel(mainWindowViewModel, 1000, 1000);
            viewModel.Layers.Clear();
            var layer1 = new Layer();
            layer1.Name.Value = "レイヤー1";
            viewModel.Layers.Add(layer1);
            layer1.IsSelected.Value = true;

            var item1 = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(item1);
            var item2 = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(item2);
            var item3 = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(item3);

            layer1.Children[0].IsSelected.Value = true;

            Assert.That(viewModel.SelectedItems.Value.ToList(), Has.Count.EqualTo(1));
        }

        [Test]
        public void アイテム2つが選択されている()
        {
            boilersGraphics.App.IsTest = true;
            var mainWindowViewModel = new MainWindowViewModel(null);
            var viewModel = new DiagramViewModel(mainWindowViewModel, 1000, 1000);
            viewModel.Layers.Clear();
            var layer1 = new Layer();
            layer1.Name.Value = "レイヤー1";
            viewModel.Layers.Add(layer1);
            layer1.IsSelected.Value = true;

            var item1 = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(item1); //アイテム1
            var item2 = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(item2); //アイテム2
            var item3 = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(item3); //アイテム3

            layer1.Children[0].IsSelected.Value = true;
            layer1.Children[1].IsSelected.Value = true;

            Assert.That(viewModel.SelectedItems.Value.ToList(), Has.Count.EqualTo(2));
            var selectedItems = viewModel.SelectedItems.Value.ToList();
            Assert.That(viewModel.GetLayerTreeViewItemBase(selectedItems[0]).Name.Value, Is.EqualTo("アイテム1"));
            Assert.That(viewModel.GetLayerTreeViewItemBase(selectedItems[1]).Name.Value, Is.EqualTo("アイテム2"));
        }

        [Test]
        public void アイテム2つが選択されている_順序逆()
        {
            boilersGraphics.App.IsTest = true;
            var mainWindowViewModel = new MainWindowViewModel(null);
            var viewModel = new DiagramViewModel(mainWindowViewModel, 1000, 1000);
            viewModel.Layers.Clear();
            var layer1 = new Layer();
            layer1.Name.Value = "レイヤー1";
            viewModel.Layers.Add(layer1);
            layer1.IsSelected.Value = true;

            var item1 = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(item1); //アイテム1
            var item2 = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(item2); //アイテム2
            var item3 = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(item3); //アイテム3

            layer1.Children[1].IsSelected.Value = true;
            layer1.Children[0].IsSelected.Value = true;

            Assert.That(viewModel.SelectedItems.Value.ToList(), Has.Count.EqualTo(2));
            var selectedItems = viewModel.SelectedItems.Value.ToList();
            Assert.That(viewModel.GetLayerTreeViewItemBase(selectedItems[0]).Name.Value, Is.EqualTo("アイテム2"));
            Assert.That(viewModel.GetLayerTreeViewItemBase(selectedItems[1]).Name.Value, Is.EqualTo("アイテム1"));
        }

        [Test]
        public void アイテム3ついや2つが選択されている()
        {
            boilersGraphics.App.IsTest = true;
            var mainWindowViewModel = new MainWindowViewModel(null);
            var viewModel = new DiagramViewModel(mainWindowViewModel, 1000, 1000);
            viewModel.Layers.Clear();
            var layer1 = new Layer();
            layer1.Name.Value = "レイヤー1";
            viewModel.Layers.Add(layer1);
            layer1.IsSelected.Value = true;

            var item1 = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(item1); //アイテム1
            var item2 = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(item2); //アイテム2
            var item3 = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(item3); //アイテム3

            layer1.Children[0].IsSelected.Value = true;
            layer1.Children[1].IsSelected.Value = true;
            layer1.Children[0].IsSelected.Value = false;
            layer1.Children[2].IsSelected.Value = true;

            Assert.That(viewModel.SelectedItems.Value.ToList(), Has.Count.EqualTo(2));
            var selectedItems = viewModel.SelectedItems.Value.ToList();
            Assert.That(viewModel.GetLayerTreeViewItemBase(selectedItems[0]).Name.Value, Is.EqualTo("アイテム2"));
            Assert.That(viewModel.GetLayerTreeViewItemBase(selectedItems[1]).Name.Value, Is.EqualTo("アイテム3"));
        }

        [Test]
        public void 直線は選択されていない()
        {
            boilersGraphics.App.IsTest = true;
            var mainWindowViewModel = new MainWindowViewModel(null);
            var viewModel = new DiagramViewModel(mainWindowViewModel, 1000, 1000);
            viewModel.Layers.Clear();
            var layer1 = new Layer();
            layer1.Name.Value = "レイヤー1";
            viewModel.Layers.Add(layer1);
            layer1.IsSelected.Value = true;

            var item1 = new StraightConnectorViewModel(viewModel, new Point(10, 10), new Point(20, 20));
            viewModel.AddItemCommand.Execute(item1);

            layer1.Children[0].IsSelected.Value = false;

            Assert.That(viewModel.SelectedItems.Value.ToList(), Has.Count.EqualTo(0));
        }

        [Test]
        public void 直線1つが選択されている()
        {
            boilersGraphics.App.IsTest = true;
            var mainWindowViewModel = new MainWindowViewModel(null);
            var viewModel = new DiagramViewModel(mainWindowViewModel, 1000, 1000);
            viewModel.Layers.Clear();
            var layer1 = new Layer();
            layer1.Name.Value = "レイヤー1";
            viewModel.Layers.Add(layer1);
            layer1.IsSelected.Value = true;

            var item1 = new StraightConnectorViewModel(viewModel, new Point(10, 10), new Point(20, 20));
            viewModel.AddItemCommand.Execute(item1);

            layer1.Children[0].IsSelected.Value = true;

            Assert.That(viewModel.SelectedItems.Value.ToList(), Has.Count.EqualTo(2));
            Assert.That(viewModel.SelectedItems.Value.ElementAt(0).IsSelected.Value, Is.True);
            Assert.That(viewModel.SelectedItems.Value.ElementAt(1).IsSelected.Value, Is.True);
        }

        [Test]
        public void 直線の頂点2つが選択されている()
        {
            boilersGraphics.App.IsTest = true;
            var mainWindowViewModel = new MainWindowViewModel(null);
            var viewModel = new DiagramViewModel(mainWindowViewModel, 1000, 1000);
            viewModel.Layers.Clear();
            var layer1 = new Layer();
            layer1.Name.Value = "レイヤー1";
            viewModel.Layers.Add(layer1);
            layer1.IsSelected.Value = true;

            var item1 = new StraightConnectorViewModel(viewModel, new Point(10, 10), new Point(20, 20));
            viewModel.AddItemCommand.Execute(item1);

            layer1.Children[0].IsSelected.Value = true;
            item1.SnapPoint0VM.Value.IsSelected.Value = true;
            item1.SnapPoint1VM.Value.IsSelected.Value = true;

            Assert.That(viewModel.SelectedItems.Value.ToList(), Has.Count.EqualTo(2));
            Assert.That(viewModel.SelectedItems.Value.ElementAt(0).IsSelected.Value, Is.True);
            Assert.That(viewModel.SelectedItems.Value.ElementAt(1).IsSelected.Value, Is.True);
        }

        [Test]
        public void 直線の頂点1つが選択されている()
        {
            boilersGraphics.App.IsTest = true;
            var mainWindowViewModel = new MainWindowViewModel(null);
            var viewModel = new DiagramViewModel(mainWindowViewModel, 1000, 1000);
            viewModel.Layers.Clear();
            var layer1 = new Layer();
            layer1.Name.Value = "レイヤー1";
            viewModel.Layers.Add(layer1);
            layer1.IsSelected.Value = true;

            var item1 = new StraightConnectorViewModel(viewModel, new Point(10, 10), new Point(20, 20));
            viewModel.AddItemCommand.Execute(item1);

            layer1.Children[0].IsSelected.Value = true;
            item1.SnapPoint0VM.Value.IsSelected.Value = true;
            item1.SnapPoint1VM.Value.IsSelected.Value = false;

            Assert.That(viewModel.SelectedItems.Value.ToList(), Has.Count.EqualTo(1));
            Assert.That(viewModel.SelectedItems.Value.ElementAt(0).IsSelected.Value, Is.True);
        }
    }
}
