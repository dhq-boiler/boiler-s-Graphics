using boilersGraphics.Helpers;
using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using Moq;
using NUnit.Framework;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace boilersGraphics.Test
{
    [TestFixture]
    public class ZIndexTest
    {
        [Test]
        public void 同じレイヤーにアイテム３つ追加()
        {
            boilersGraphics.App.IsTest = true;
            var dlgService = new Mock<IDialogService>();
            MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(dlgService.Object);
            var diagramVM = new DiagramViewModel(mainWindowViewModel, 1000, 1000);
            diagramVM.Layers.Clear();
            var layer1 = new Layer();
            layer1.Name.Value = "レイヤー1";
            diagramVM.Layers.Add(layer1);
            layer1.IsSelected.Value = true;

            var item1 = new NRectangleViewModel();
            diagramVM.AddItemCommand.Execute(item1);

            var item2 = new NRectangleViewModel();
            diagramVM.AddItemCommand.Execute(item2);

            var item3 = new NRectangleViewModel();
            diagramVM.AddItemCommand.Execute(item3);

            Assert.That(diagramVM.SelectedLayers.Value.ToList(), Has.Count.EqualTo(1));
            var layer = diagramVM.SelectedLayers.Value[0] as Layer;
            Assert.That((layer.Children[0] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(0));
            Assert.That((layer.Children[1] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(1));
            Assert.That((layer.Children[2] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(2));
        }

        [Test]
        public void 異なるレイヤーにアイテム３つずつ追加()
        {
            boilersGraphics.App.IsTest = true;
            var dlgService = new Mock<IDialogService>();
            MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(dlgService.Object);
            var diagramVM = new DiagramViewModel(mainWindowViewModel, 1000, 1000);
            diagramVM.Layers.Clear();
            var layer1 = new Layer();
            layer1.Name.Value = "レイヤー1";
            diagramVM.Layers.Add(layer1);
            layer1.IsSelected.Value = true; //レイヤー1を選択状態にする
            var layer2 = new Layer();
            layer2.Name.Value = "レイヤー2";
            diagramVM.Layers.Add(layer2);

            var item1 = new NRectangleViewModel();
            diagramVM.AddItemCommand.Execute(item1);

            var item2 = new NRectangleViewModel();
            diagramVM.AddItemCommand.Execute(item2);

            var item3 = new NRectangleViewModel();
            diagramVM.AddItemCommand.Execute(item3);

            Assert.That((diagramVM.Layers[0].Children[0] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(0));
            Assert.That((diagramVM.Layers[0].Children[1] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(1));
            Assert.That((diagramVM.Layers[0].Children[2] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(2));

            //レイヤー2のみを選択状態にする
            layer1.IsSelected.Value = false;
            layer2.IsSelected.Value = true;

            var item4 = new NRectangleViewModel();
            diagramVM.AddItemCommand.Execute(item4);

            var item5 = new NRectangleViewModel();
            diagramVM.AddItemCommand.Execute(item5);

            var item6 = new NRectangleViewModel();
            diagramVM.AddItemCommand.Execute(item6);

            Assert.That((diagramVM.Layers[0].Children[0] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(0));
            Assert.That((diagramVM.Layers[0].Children[1] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(1));
            Assert.That((diagramVM.Layers[0].Children[2] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(2));
            Assert.That((diagramVM.Layers[1].Children[0] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(3));
            Assert.That((diagramVM.Layers[1].Children[1] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(4));
            Assert.That((diagramVM.Layers[1].Children[2] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(5));
        }

        [Test]
        public void レイヤーの手前にもう1つのレイヤーを追加()
        {
            boilersGraphics.App.IsTest = true;
            var dlgService = new Mock<IDialogService>();
            MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(dlgService.Object);
            var diagramVM = new DiagramViewModel(mainWindowViewModel, 1000, 1000);
            diagramVM.Layers.Clear();
            var layer1 = new Layer();
            layer1.Name.Value = "レイヤー1";
            diagramVM.Layers.Add(layer1);
            layer1.IsSelected.Value = true; //レイヤー1を選択状態にする

            var item1 = new NRectangleViewModel();
            diagramVM.AddItemCommand.Execute(item1);

            var item2 = new NRectangleViewModel();
            diagramVM.AddItemCommand.Execute(item2);

            var item3 = new NRectangleViewModel();
            diagramVM.AddItemCommand.Execute(item3);

            Assert.That((diagramVM.Layers[0].Children[0] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(0));
            Assert.That((diagramVM.Layers[0].Children[1] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(1));
            Assert.That((diagramVM.Layers[0].Children[2] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(2));

            var layer2 = new Layer();
            layer2.Name.Value = "レイヤー2";
            diagramVM.Layers.Add(layer2);

            layer1.IsSelected.Value = false;
            layer2.IsSelected.Value = true;

            var item4 = new NRectangleViewModel();
            diagramVM.AddItemCommand.Execute(item4);

            var item5 = new NRectangleViewModel();
            diagramVM.AddItemCommand.Execute(item5);

            var item6 = new NRectangleViewModel();
            diagramVM.AddItemCommand.Execute(item6);

            diagramVM.Layers.Remove(layer2);
            LayerTreeViewItemCollection.InsertBeforeChildren(mainWindowViewModel.Recorder, diagramVM.Layers, diagramVM.Layers, layer2, layer1);

            Assert.That(diagramVM.Layers[0].Name.Value, Is.EqualTo("レイヤー2"));
            Assert.That(diagramVM.Layers[1].Name.Value, Is.EqualTo("レイヤー1"));

            Assert.That((diagramVM.Layers[0].Children[0] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(0));
            Assert.That((diagramVM.Layers[0].Children[1] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(1));
            Assert.That((diagramVM.Layers[0].Children[2] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(2));
            Assert.That((diagramVM.Layers[1].Children[0] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(3));
            Assert.That((diagramVM.Layers[1].Children[1] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(4));
            Assert.That((diagramVM.Layers[1].Children[2] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(5));
        }

        [Test]
        public void レイヤーの後にもう1つのレイヤーを追加()
        {
            boilersGraphics.App.IsTest = true;
            var dlgService = new Mock<IDialogService>();
            MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(dlgService.Object);
            var diagramVM = new DiagramViewModel(mainWindowViewModel, 1000, 1000);
            diagramVM.Layers.Clear();
            var layer1 = new Layer();
            layer1.Name.Value = "レイヤー1";
            diagramVM.Layers.Add(layer1);
            layer1.IsSelected.Value = true; //レイヤー1を選択状態にする

            var item1 = new NRectangleViewModel();
            diagramVM.AddItemCommand.Execute(item1);

            var item2 = new NRectangleViewModel();
            diagramVM.AddItemCommand.Execute(item2);

            var item3 = new NRectangleViewModel();
            diagramVM.AddItemCommand.Execute(item3);

            Assert.That((diagramVM.Layers[0].Children[0] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(0));
            Assert.That((diagramVM.Layers[0].Children[1] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(1));
            Assert.That((diagramVM.Layers[0].Children[2] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(2));

            var layer2 = new Layer();
            layer2.Name.Value = "レイヤー2";
            diagramVM.Layers.Add(layer2);

            layer1.IsSelected.Value = false;
            layer2.IsSelected.Value = true;

            var item4 = new NRectangleViewModel();
            diagramVM.AddItemCommand.Execute(item4);

            var item5 = new NRectangleViewModel();
            diagramVM.AddItemCommand.Execute(item5);

            var item6 = new NRectangleViewModel();
            diagramVM.AddItemCommand.Execute(item6);

            diagramVM.Layers.Remove(layer2);
            LayerTreeViewItemCollection.InsertAfterChildren(mainWindowViewModel.Recorder, diagramVM.Layers, diagramVM.Layers, layer2, layer1);

            Assert.That(diagramVM.Layers[0].Name.Value, Is.EqualTo("レイヤー1"));
            Assert.That(diagramVM.Layers[1].Name.Value, Is.EqualTo("レイヤー2"));

            Assert.That((diagramVM.Layers[0].Children[0] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(0));
            Assert.That((diagramVM.Layers[0].Children[1] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(1));
            Assert.That((diagramVM.Layers[0].Children[2] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(2));
            Assert.That((diagramVM.Layers[1].Children[0] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(3));
            Assert.That((diagramVM.Layers[1].Children[1] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(4));
            Assert.That((diagramVM.Layers[1].Children[2] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(5));
        }

        [Test]
        public void 前面へ移動()
        {
            boilersGraphics.App.IsTest = true;
            var dlgService = new Mock<IDialogService>();
            MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(dlgService.Object);
            var viewModel = new DiagramViewModel(mainWindowViewModel, 1000, 1000);
            viewModel.Layers.Clear();

            Layer layer1 = new Layer();
            layer1.Name.Value = "レイヤー1";
            viewModel.Layers.Add(layer1);

            layer1.IsSelected.Value = true;

            var r0 = new NRectangleViewModel();
            var r1 = new NRectangleViewModel();
            var r2 = new NRectangleViewModel();
            var r3 = new NRectangleViewModel();
            var r4 = new NRectangleViewModel();

            viewModel.AddItemCommand.Execute(r0);
            viewModel.AddItemCommand.Execute(r1);
            viewModel.AddItemCommand.Execute(r2);
            viewModel.AddItemCommand.Execute(r3);
            viewModel.AddItemCommand.Execute(r4);

            Assert.That(r0.ZIndex.Value, Is.EqualTo(0));
            Assert.That(r1.ZIndex.Value, Is.EqualTo(1));
            Assert.That(r2.ZIndex.Value, Is.EqualTo(2));
            Assert.That(r3.ZIndex.Value, Is.EqualTo(3));
            Assert.That(r4.ZIndex.Value, Is.EqualTo(4));

            viewModel.Layers[0].Children[2].IsSelected.Value = true;
            Assert.That(viewModel.SelectedItems.Value.ToList(), Has.Count.EqualTo(1));

            viewModel.BringForwardCommand.Execute();

            Assert.That(r0.ZIndex.Value, Is.EqualTo(0));
            Assert.That(r1.ZIndex.Value, Is.EqualTo(1));
            Assert.That(r2.ZIndex.Value, Is.EqualTo(3));
            Assert.That(r3.ZIndex.Value, Is.EqualTo(2));
            Assert.That(r4.ZIndex.Value, Is.EqualTo(4));

            Assert.That((viewModel.Layers[0].Children[0] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(0));
            Assert.That((viewModel.Layers[0].Children[1] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(1));
            Assert.That((viewModel.Layers[0].Children[2] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(2));
            Assert.That((viewModel.Layers[0].Children[3] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(3));
            Assert.That((viewModel.Layers[0].Children[4] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(4));
        }

        [Test]
        public void 前面へ移動_レイヤー外()
        {
            boilersGraphics.App.IsTest = true;
            var dlgService = new Mock<IDialogService>();
            MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(dlgService.Object);
            var viewModel = new DiagramViewModel(mainWindowViewModel, 1000, 1000);
            viewModel.Layers.Clear();

            Layer layer1 = new Layer();
            layer1.Name.Value = "レイヤー1";
            viewModel.Layers.Add(layer1);

            Layer layer2 = new Layer();
            layer2.Name.Value = "レイヤー2";
            viewModel.Layers.Add(layer2);

            layer1.IsSelected.Value = true;

            var r0 = new NRectangleViewModel();
            var r1 = new NRectangleViewModel();
            var r2 = new NRectangleViewModel();
            var r3 = new NRectangleViewModel();
            var r4 = new NRectangleViewModel();

            viewModel.AddItemCommand.Execute(r0);
            viewModel.AddItemCommand.Execute(r1);
            viewModel.AddItemCommand.Execute(r2);
            viewModel.AddItemCommand.Execute(r3);
            viewModel.AddItemCommand.Execute(r4);

            Assert.That(r0.ZIndex.Value, Is.EqualTo(0));
            Assert.That(r1.ZIndex.Value, Is.EqualTo(1));
            Assert.That(r2.ZIndex.Value, Is.EqualTo(2));
            Assert.That(r3.ZIndex.Value, Is.EqualTo(3));
            Assert.That(r4.ZIndex.Value, Is.EqualTo(4));

            layer1.IsSelected.Value = false;
            layer2.IsSelected.Value = true;

            var r5 = new NRectangleViewModel();
            var r6 = new NRectangleViewModel();
            var r7 = new NRectangleViewModel();
            var r8 = new NRectangleViewModel();
            var r9 = new NRectangleViewModel();

            viewModel.AddItemCommand.Execute(r5);
            viewModel.AddItemCommand.Execute(r6);
            viewModel.AddItemCommand.Execute(r7);
            viewModel.AddItemCommand.Execute(r8);
            viewModel.AddItemCommand.Execute(r9);

            Assert.That(r5.ZIndex.Value, Is.EqualTo(5));
            Assert.That(r6.ZIndex.Value, Is.EqualTo(6));
            Assert.That(r7.ZIndex.Value, Is.EqualTo(7));
            Assert.That(r8.ZIndex.Value, Is.EqualTo(8));
            Assert.That(r9.ZIndex.Value, Is.EqualTo(9));

            layer1.IsSelected.Value = true;
            layer2.IsSelected.Value = false;

            viewModel.Layers[0].Children[4].IsSelected.Value = true;
            Assert.That(viewModel.SelectedItems.Value.ToList(), Has.Count.EqualTo(1));

            viewModel.BringForwardCommand.Execute();

            Assert.That(r0.ZIndex.Value, Is.EqualTo(0));
            Assert.That(r1.ZIndex.Value, Is.EqualTo(1));
            Assert.That(r2.ZIndex.Value, Is.EqualTo(2));
            Assert.That(r3.ZIndex.Value, Is.EqualTo(3));
            Assert.That(r4.ZIndex.Value, Is.EqualTo(4));
            Assert.That(r5.ZIndex.Value, Is.EqualTo(5));
            Assert.That(r6.ZIndex.Value, Is.EqualTo(6));
            Assert.That(r7.ZIndex.Value, Is.EqualTo(7));
            Assert.That(r8.ZIndex.Value, Is.EqualTo(8));
            Assert.That(r9.ZIndex.Value, Is.EqualTo(9));

            Assert.That((viewModel.Layers[0].Children[0] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(0));
            Assert.That((viewModel.Layers[0].Children[1] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(1));
            Assert.That((viewModel.Layers[0].Children[2] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(2));
            Assert.That((viewModel.Layers[0].Children[3] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(3));
            Assert.That((viewModel.Layers[0].Children[4] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(4));

            Assert.That((viewModel.Layers[1].Children[0] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(5));
            Assert.That((viewModel.Layers[1].Children[1] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(6));
            Assert.That((viewModel.Layers[1].Children[2] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(7));
            Assert.That((viewModel.Layers[1].Children[3] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(8));
            Assert.That((viewModel.Layers[1].Children[4] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(9));
        }

        [Test]
        public void 最前面へ移動()
        {
            boilersGraphics.App.IsTest = true;
            var dlgService = new Mock<IDialogService>();
            MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(dlgService.Object);
            var viewModel = new DiagramViewModel(mainWindowViewModel, 1000, 1000);
            viewModel.Layers.Clear();

            Layer layer1 = new Layer();
            layer1.Name.Value = "レイヤー1";
            viewModel.Layers.Add(layer1);

            Layer layer2 = new Layer();
            layer2.Name.Value = "レイヤー2";
            viewModel.Layers.Add(layer2);

            layer1.IsSelected.Value = true;

            var r0 = new NRectangleViewModel();
            var r1 = new NRectangleViewModel();
            var r2 = new NRectangleViewModel();
            var r3 = new NRectangleViewModel();
            var r4 = new NRectangleViewModel();

            viewModel.AddItemCommand.Execute(r0);
            viewModel.AddItemCommand.Execute(r1);
            viewModel.AddItemCommand.Execute(r2);
            viewModel.AddItemCommand.Execute(r3);
            viewModel.AddItemCommand.Execute(r4);

            Assert.That(r0.ZIndex.Value, Is.EqualTo(0));
            Assert.That(r1.ZIndex.Value, Is.EqualTo(1));
            Assert.That(r2.ZIndex.Value, Is.EqualTo(2));
            Assert.That(r3.ZIndex.Value, Is.EqualTo(3));
            Assert.That(r4.ZIndex.Value, Is.EqualTo(4));

            layer1.IsSelected.Value = false;
            layer2.IsSelected.Value = true;

            var r5 = new NRectangleViewModel();
            var r6 = new NRectangleViewModel();
            var r7 = new NRectangleViewModel();
            var r8 = new NRectangleViewModel();
            var r9 = new NRectangleViewModel();

            viewModel.AddItemCommand.Execute(r5);
            viewModel.AddItemCommand.Execute(r6);
            viewModel.AddItemCommand.Execute(r7);
            viewModel.AddItemCommand.Execute(r8);
            viewModel.AddItemCommand.Execute(r9);

            Assert.That(r5.ZIndex.Value, Is.EqualTo(5));
            Assert.That(r6.ZIndex.Value, Is.EqualTo(6));
            Assert.That(r7.ZIndex.Value, Is.EqualTo(7));
            Assert.That(r8.ZIndex.Value, Is.EqualTo(8));
            Assert.That(r9.ZIndex.Value, Is.EqualTo(9));

            layer1.IsSelected.Value = true;
            layer2.IsSelected.Value = false;

            viewModel.Layers[0].Children[2].IsSelected.Value = true;
            Assert.That(viewModel.SelectedItems.Value.ToList(), Has.Count.EqualTo(1));

            viewModel.BringForegroundCommand.Execute();

            Assert.That(r0.ZIndex.Value, Is.EqualTo(0));
            Assert.That(r1.ZIndex.Value, Is.EqualTo(1));
            Assert.That(r2.ZIndex.Value, Is.EqualTo(4));
            Assert.That(r3.ZIndex.Value, Is.EqualTo(2));
            Assert.That(r4.ZIndex.Value, Is.EqualTo(3));

            Assert.That(r5.ZIndex.Value, Is.EqualTo(5));
            Assert.That(r6.ZIndex.Value, Is.EqualTo(6));
            Assert.That(r7.ZIndex.Value, Is.EqualTo(7));
            Assert.That(r8.ZIndex.Value, Is.EqualTo(8));
            Assert.That(r9.ZIndex.Value, Is.EqualTo(9));

            Assert.That((viewModel.Layers[0].Children[0] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(0));
            Assert.That((viewModel.Layers[0].Children[1] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(1));
            Assert.That((viewModel.Layers[0].Children[2] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(2));
            Assert.That((viewModel.Layers[0].Children[3] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(3));
            Assert.That((viewModel.Layers[0].Children[4] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(4));

            Assert.That((viewModel.Layers[1].Children[0] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(5));
            Assert.That((viewModel.Layers[1].Children[1] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(6));
            Assert.That((viewModel.Layers[1].Children[2] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(7));
            Assert.That((viewModel.Layers[1].Children[3] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(8));
            Assert.That((viewModel.Layers[1].Children[4] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(9));
        }

        [Test]
        public void 背面へ移動()
        {
            boilersGraphics.App.IsTest = true;
            var dlgService = new Mock<IDialogService>();
            MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(dlgService.Object);
            var viewModel = new DiagramViewModel(mainWindowViewModel, 1000, 1000);
            viewModel.Layers.Clear();

            Layer layer1 = new Layer();
            layer1.Name.Value = "レイヤー1";
            viewModel.Layers.Add(layer1);

            Layer layer2 = new Layer();
            layer2.Name.Value = "レイヤー2";
            viewModel.Layers.Add(layer2);

            layer1.IsSelected.Value = true;

            var r0 = new NRectangleViewModel();
            var r1 = new NRectangleViewModel();
            var r2 = new NRectangleViewModel();
            var r3 = new NRectangleViewModel();
            var r4 = new NRectangleViewModel();

            viewModel.AddItemCommand.Execute(r0);
            viewModel.AddItemCommand.Execute(r1);
            viewModel.AddItemCommand.Execute(r2);
            viewModel.AddItemCommand.Execute(r3);
            viewModel.AddItemCommand.Execute(r4);

            Assert.That(r0.ZIndex.Value, Is.EqualTo(0));
            Assert.That(r1.ZIndex.Value, Is.EqualTo(1));
            Assert.That(r2.ZIndex.Value, Is.EqualTo(2));
            Assert.That(r3.ZIndex.Value, Is.EqualTo(3));
            Assert.That(r4.ZIndex.Value, Is.EqualTo(4));

            layer1.IsSelected.Value = false;
            layer2.IsSelected.Value = true;

            var r5 = new NRectangleViewModel();
            var r6 = new NRectangleViewModel();
            var r7 = new NRectangleViewModel();
            var r8 = new NRectangleViewModel();
            var r9 = new NRectangleViewModel();

            viewModel.AddItemCommand.Execute(r5);
            viewModel.AddItemCommand.Execute(r6);
            viewModel.AddItemCommand.Execute(r7);
            viewModel.AddItemCommand.Execute(r8);
            viewModel.AddItemCommand.Execute(r9);

            Assert.That(r5.ZIndex.Value, Is.EqualTo(5));
            Assert.That(r6.ZIndex.Value, Is.EqualTo(6));
            Assert.That(r7.ZIndex.Value, Is.EqualTo(7));
            Assert.That(r8.ZIndex.Value, Is.EqualTo(8));
            Assert.That(r9.ZIndex.Value, Is.EqualTo(9));

            layer1.IsSelected.Value = true;
            layer2.IsSelected.Value = false;

            viewModel.Layers[0].Children[2].IsSelected.Value = true;
            Assert.That(viewModel.SelectedItems.Value.ToList(), Has.Count.EqualTo(1));

            viewModel.SendBackwardCommand.Execute();

            Assert.That(r0.ZIndex.Value, Is.EqualTo(0));
            Assert.That(r1.ZIndex.Value, Is.EqualTo(2));
            Assert.That(r2.ZIndex.Value, Is.EqualTo(1));
            Assert.That(r3.ZIndex.Value, Is.EqualTo(3));
            Assert.That(r4.ZIndex.Value, Is.EqualTo(4));

            Assert.That(r5.ZIndex.Value, Is.EqualTo(5));
            Assert.That(r6.ZIndex.Value, Is.EqualTo(6));
            Assert.That(r7.ZIndex.Value, Is.EqualTo(7));
            Assert.That(r8.ZIndex.Value, Is.EqualTo(8));
            Assert.That(r9.ZIndex.Value, Is.EqualTo(9));

            Assert.That((viewModel.Layers[0].Children[0] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(0));
            Assert.That((viewModel.Layers[0].Children[1] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(1));
            Assert.That((viewModel.Layers[0].Children[2] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(2));
            Assert.That((viewModel.Layers[0].Children[3] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(3));
            Assert.That((viewModel.Layers[0].Children[4] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(4));

            Assert.That((viewModel.Layers[1].Children[0] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(5));
            Assert.That((viewModel.Layers[1].Children[1] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(6));
            Assert.That((viewModel.Layers[1].Children[2] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(7));
            Assert.That((viewModel.Layers[1].Children[3] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(8));
            Assert.That((viewModel.Layers[1].Children[4] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(9));
        }

        [Test]
        public void 背面へ移動_レイヤー外()
        {
            boilersGraphics.App.IsTest = true;
            var dlgService = new Mock<IDialogService>();
            MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(dlgService.Object);
            var viewModel = new DiagramViewModel(mainWindowViewModel, 1000, 1000);
            viewModel.Layers.Clear();

            Layer layer1 = new Layer();
            layer1.Name.Value = "レイヤー1";
            viewModel.Layers.Add(layer1);

            Layer layer2 = new Layer();
            layer2.Name.Value = "レイヤー2";
            viewModel.Layers.Add(layer2);

            layer1.IsSelected.Value = true;

            var r0 = new NRectangleViewModel();
            var r1 = new NRectangleViewModel();
            var r2 = new NRectangleViewModel();
            var r3 = new NRectangleViewModel();
            var r4 = new NRectangleViewModel();

            viewModel.AddItemCommand.Execute(r0);
            viewModel.AddItemCommand.Execute(r1);
            viewModel.AddItemCommand.Execute(r2);
            viewModel.AddItemCommand.Execute(r3);
            viewModel.AddItemCommand.Execute(r4);

            Assert.That(r0.ZIndex.Value, Is.EqualTo(0));
            Assert.That(r1.ZIndex.Value, Is.EqualTo(1));
            Assert.That(r2.ZIndex.Value, Is.EqualTo(2));
            Assert.That(r3.ZIndex.Value, Is.EqualTo(3));
            Assert.That(r4.ZIndex.Value, Is.EqualTo(4));

            layer1.IsSelected.Value = false;
            layer2.IsSelected.Value = true;

            var r5 = new NRectangleViewModel();
            var r6 = new NRectangleViewModel();
            var r7 = new NRectangleViewModel();
            var r8 = new NRectangleViewModel();
            var r9 = new NRectangleViewModel();

            viewModel.AddItemCommand.Execute(r5);
            viewModel.AddItemCommand.Execute(r6);
            viewModel.AddItemCommand.Execute(r7);
            viewModel.AddItemCommand.Execute(r8);
            viewModel.AddItemCommand.Execute(r9);

            Assert.That(r5.ZIndex.Value, Is.EqualTo(5));
            Assert.That(r6.ZIndex.Value, Is.EqualTo(6));
            Assert.That(r7.ZIndex.Value, Is.EqualTo(7));
            Assert.That(r8.ZIndex.Value, Is.EqualTo(8));
            Assert.That(r9.ZIndex.Value, Is.EqualTo(9));

            layer1.IsSelected.Value = false;
            layer2.IsSelected.Value = true;

            viewModel.Layers[1].Children[0].IsSelected.Value = true;
            Assert.That(viewModel.SelectedItems.Value.ToList(), Has.Count.EqualTo(1));

            viewModel.SendBackwardCommand.Execute();

            Assert.That(r0.ZIndex.Value, Is.EqualTo(0));
            Assert.That(r1.ZIndex.Value, Is.EqualTo(1));
            Assert.That(r2.ZIndex.Value, Is.EqualTo(2));
            Assert.That(r3.ZIndex.Value, Is.EqualTo(3));
            Assert.That(r4.ZIndex.Value, Is.EqualTo(4));

            Assert.That(r5.ZIndex.Value, Is.EqualTo(5));
            Assert.That(r6.ZIndex.Value, Is.EqualTo(6));
            Assert.That(r7.ZIndex.Value, Is.EqualTo(7));
            Assert.That(r8.ZIndex.Value, Is.EqualTo(8));
            Assert.That(r9.ZIndex.Value, Is.EqualTo(9));

            Assert.That((viewModel.Layers[0].Children[0] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(0));
            Assert.That((viewModel.Layers[0].Children[1] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(1));
            Assert.That((viewModel.Layers[0].Children[2] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(2));
            Assert.That((viewModel.Layers[0].Children[3] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(3));
            Assert.That((viewModel.Layers[0].Children[4] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(4));

            Assert.That((viewModel.Layers[1].Children[0] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(5));
            Assert.That((viewModel.Layers[1].Children[1] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(6));
            Assert.That((viewModel.Layers[1].Children[2] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(7));
            Assert.That((viewModel.Layers[1].Children[3] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(8));
            Assert.That((viewModel.Layers[1].Children[4] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(9));
        }

        [Test]
        public void 最背面へ移動()
        {
            boilersGraphics.App.IsTest = true;
            var dlgService = new Mock<IDialogService>();
            MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(dlgService.Object);
            var viewModel = new DiagramViewModel(mainWindowViewModel, 1000, 1000);
            viewModel.Layers.Clear();

            Layer layer1 = new Layer();
            layer1.Name.Value = "レイヤー1";
            viewModel.Layers.Add(layer1);

            Layer layer2 = new Layer();
            layer2.Name.Value = "レイヤー2";
            viewModel.Layers.Add(layer2);

            layer1.IsSelected.Value = true;

            var r0 = new NRectangleViewModel();
            var r1 = new NRectangleViewModel();
            var r2 = new NRectangleViewModel();
            var r3 = new NRectangleViewModel();
            var r4 = new NRectangleViewModel();

            viewModel.AddItemCommand.Execute(r0);
            viewModel.AddItemCommand.Execute(r1);
            viewModel.AddItemCommand.Execute(r2);
            viewModel.AddItemCommand.Execute(r3);
            viewModel.AddItemCommand.Execute(r4);

            Assert.That(r0.ZIndex.Value, Is.EqualTo(0));
            Assert.That(r1.ZIndex.Value, Is.EqualTo(1));
            Assert.That(r2.ZIndex.Value, Is.EqualTo(2));
            Assert.That(r3.ZIndex.Value, Is.EqualTo(3));
            Assert.That(r4.ZIndex.Value, Is.EqualTo(4));

            layer1.IsSelected.Value = false;
            layer2.IsSelected.Value = true;

            var r5 = new NRectangleViewModel();
            var r6 = new NRectangleViewModel();
            var r7 = new NRectangleViewModel();
            var r8 = new NRectangleViewModel();
            var r9 = new NRectangleViewModel();

            viewModel.AddItemCommand.Execute(r5);
            viewModel.AddItemCommand.Execute(r6);
            viewModel.AddItemCommand.Execute(r7);
            viewModel.AddItemCommand.Execute(r8);
            viewModel.AddItemCommand.Execute(r9);

            Assert.That(r5.ZIndex.Value, Is.EqualTo(5));
            Assert.That(r6.ZIndex.Value, Is.EqualTo(6));
            Assert.That(r7.ZIndex.Value, Is.EqualTo(7));
            Assert.That(r8.ZIndex.Value, Is.EqualTo(8));
            Assert.That(r9.ZIndex.Value, Is.EqualTo(9));

            layer1.IsSelected.Value = false;
            layer2.IsSelected.Value = true;

            viewModel.Layers[1].Children[4].IsSelected.Value = true;
            Assert.That(viewModel.SelectedItems.Value.ToList(), Has.Count.EqualTo(1));

            viewModel.SendBackgroundCommand.Execute();

            Assert.That(r0.ZIndex.Value, Is.EqualTo(0));
            Assert.That(r1.ZIndex.Value, Is.EqualTo(1));
            Assert.That(r2.ZIndex.Value, Is.EqualTo(2));
            Assert.That(r3.ZIndex.Value, Is.EqualTo(3));
            Assert.That(r4.ZIndex.Value, Is.EqualTo(4));

            Assert.That(r5.ZIndex.Value, Is.EqualTo(6));
            Assert.That(r6.ZIndex.Value, Is.EqualTo(7));
            Assert.That(r7.ZIndex.Value, Is.EqualTo(8));
            Assert.That(r8.ZIndex.Value, Is.EqualTo(9));
            Assert.That(r9.ZIndex.Value, Is.EqualTo(5));

            Assert.That((viewModel.Layers[0].Children[0] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(0));
            Assert.That((viewModel.Layers[0].Children[1] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(1));
            Assert.That((viewModel.Layers[0].Children[2] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(2));
            Assert.That((viewModel.Layers[0].Children[3] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(3));
            Assert.That((viewModel.Layers[0].Children[4] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(4));

            Assert.That((viewModel.Layers[1].Children[0] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(5));
            Assert.That((viewModel.Layers[1].Children[1] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(6));
            Assert.That((viewModel.Layers[1].Children[2] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(7));
            Assert.That((viewModel.Layers[1].Children[3] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(8));
            Assert.That((viewModel.Layers[1].Children[4] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(9));
        }
    }
}
