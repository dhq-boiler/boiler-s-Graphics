using boilersGraphics.Models;
using boilersGraphics.Properties;
using boilersGraphics.ViewModels;
using Moq;
using NUnit.Framework;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;

namespace boilersGraphics.Test
{
    [TestFixture]
    public class GroupingTest
    {
        [Test]
        public void Group()
        {
            boilersGraphics.App.IsTest = true;
            var dlgService = new Mock<IDialogService>();
            MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(dlgService.Object);
            var diagramVM = new DiagramViewModel(mainWindowViewModel, 1000, 1000);
            diagramVM.Layers.Clear();
            var layer1 = new Layer();
            layer1.Name.Value = $"{Resources.Name_Layer}1";
            diagramVM.Layers.Add(layer1);
            layer1.IsSelected.Value = true; //レイヤー1を選択状態にする

            var item1 = new NRectangleViewModel();
            item1.Left.Value = 10;
            item1.Top.Value = 10;
            item1.Width.Value = 10;
            item1.Height.Value = 10;
            diagramVM.AddItemCommand.Execute(item1);

            var item2 = new NRectangleViewModel();
            item2.Left.Value = 20;
            item2.Top.Value = 20;
            item2.Width.Value = 10;
            item2.Height.Value = 10;
            diagramVM.AddItemCommand.Execute(item2);

            diagramVM.Layers[0].Children.Value[0].IsSelected.Value = true;
            diagramVM.Layers[0].Children.Value[1].IsSelected.Value = true;

            Assert.That(item1.ZIndex.Value, Is.EqualTo(0));
            Assert.That(item2.ZIndex.Value, Is.EqualTo(1));

            diagramVM.GroupCommand.Execute();

            Assert.That(diagramVM.Layers[0].Children, Has.Count.EqualTo(1));
            Assert.That(diagramVM.Layers[0].Children.Value[0].Name.Value, Is.EqualTo($"{Resources.Name_Item}3"));
            Assert.That(diagramVM.Layers[0].Children.Value[0].Children.Value[0].Name.Value, Is.EqualTo($"{Resources.Name_Item}1"));
            Assert.That(diagramVM.Layers[0].Children.Value[0].Children.Value[1].Name.Value, Is.EqualTo($"{Resources.Name_Item}2"));
        }

        [Test]
        public void Group_Move()
        {
            boilersGraphics.App.IsTest = true;
            var dlgService = new Mock<IDialogService>();
            MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(dlgService.Object);
            var diagramVM = new DiagramViewModel(mainWindowViewModel, 1000, 1000);
            diagramVM.Layers.Clear();
            var layer1 = new Layer();
            layer1.Name.Value = $"{Resources.Name_Layer}1";
            diagramVM.Layers.Add(layer1);
            layer1.IsSelected.Value = true; //レイヤー1を選択状態にする

            var item1 = new NRectangleViewModel();
            item1.Left.Value = 10;
            item1.Top.Value = 10;
            item1.Width.Value = 10;
            item1.Height.Value = 10;
            diagramVM.AddItemCommand.Execute(item1);

            var item2 = new NRectangleViewModel();
            item2.Left.Value = 20;
            item2.Top.Value = 20;
            item2.Width.Value = 10;
            item2.Height.Value = 10;
            diagramVM.AddItemCommand.Execute(item2);

            var item3 = new NRectangleViewModel();
            item3.Left.Value = 30;
            item3.Top.Value = 30;
            item3.Width.Value = 10;
            item3.Height.Value = 10;
            diagramVM.AddItemCommand.Execute(item3);

            diagramVM.Layers[0].Children.Value[0].IsSelected.Value = true;
            diagramVM.Layers[0].Children.Value[1].IsSelected.Value = true;
            diagramVM.Layers[0].Children.Value[2].IsSelected.Value = true;

            Assert.That(item1.ZIndex.Value, Is.EqualTo(0));
            Assert.That(item2.ZIndex.Value, Is.EqualTo(1));
            Assert.That(item3.ZIndex.Value, Is.EqualTo(2));

            diagramVM.GroupCommand.Execute();

            var group = diagramVM.AllItems.Value.First(x => x is GroupItemViewModel) as GroupItemViewModel;
            Assert.That(group, Is.TypeOf<GroupItemViewModel>());

            group.IsSelected.Value = true;
            group.Left.Value += 10;
            group.Top.Value += 10;

            Assert.That(item1.ZIndex.Value, Is.EqualTo(0));
            Assert.That(item2.ZIndex.Value, Is.EqualTo(1));
            Assert.That(item3.ZIndex.Value, Is.EqualTo(2));
            Assert.That(group.ZIndex.Value, Is.EqualTo(3));

            diagramVM.Layers[0].Children.Value[0].IsSelected.Value = true;

            diagramVM.UngroupCommand.Execute();

            Assert.That(item1.ZIndex.Value, Is.EqualTo(0));
            Assert.That(item2.ZIndex.Value, Is.EqualTo(1));
            Assert.That(item3.ZIndex.Value, Is.EqualTo(2));

            Assert.That(item1.Left.Value, Is.EqualTo(20));
            Assert.That(item1.Top.Value, Is.EqualTo(20));

            Assert.That(item2.Left.Value, Is.EqualTo(30));
            Assert.That(item2.Top.Value, Is.EqualTo(30));

            Assert.That(item3.Left.Value, Is.EqualTo(40));
            Assert.That(item3.Top.Value, Is.EqualTo(40));
        }


        [Test]
        public void Group_Duplicate()
        {
            boilersGraphics.App.IsTest = true;
            var dlgService = new Mock<IDialogService>();
            MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(dlgService.Object);
            var diagramVM = new DiagramViewModel(mainWindowViewModel, 1000, 1000);
            diagramVM.Layers.Clear();
            var layer1 = new Layer();
            layer1.Name.Value = $"{Resources.Name_Layer}1";
            diagramVM.Layers.Add(layer1);
            layer1.IsSelected.Value = true; //レイヤー1を選択状態にする

            var item1 = new NRectangleViewModel();
            item1.Left.Value = 10;
            item1.Top.Value = 10;
            item1.Width.Value = 10;
            item1.Height.Value = 10;
            diagramVM.AddItemCommand.Execute(item1);

            var item2 = new NRectangleViewModel();
            item2.Left.Value = 20;
            item2.Top.Value = 20;
            item2.Width.Value = 10;
            item2.Height.Value = 10;
            diagramVM.AddItemCommand.Execute(item2);

            diagramVM.Layers[0].Children.Value[0].IsSelected.Value = true;
            diagramVM.Layers[0].Children.Value[1].IsSelected.Value = true;

            Assert.That(item1.ZIndex.Value, Is.EqualTo(0));
            Assert.That(item2.ZIndex.Value, Is.EqualTo(1));

            diagramVM.GroupCommand.Execute();

            Assert.That(diagramVM.Layers[0].Children, Has.Count.EqualTo(1));
            Assert.That(diagramVM.Layers[0].Children.Value[0].Name.Value, Is.EqualTo($"{Resources.Name_Item}3"));
            Assert.That(diagramVM.Layers[0].Children.Value[0].Children.Value[0].Name.Value, Is.EqualTo($"{Resources.Name_Item}1"));
            Assert.That(diagramVM.Layers[0].Children.Value[0].Children.Value[1].Name.Value, Is.EqualTo($"{Resources.Name_Item}2"));

            var group = diagramVM.AllItems.Value.First(x => x is GroupItemViewModel) as GroupItemViewModel;
            Assert.That(group, Is.TypeOf<GroupItemViewModel>());

            group.IsSelected.Value = true;

            Assert.That(item1.ZIndex.Value, Is.EqualTo(0));
            Assert.That(item2.ZIndex.Value, Is.EqualTo(1));
            Assert.That(group.ZIndex.Value, Is.EqualTo(2));

            diagramVM.DuplicateCommand.Execute();

            Assert.That(diagramVM.Layers[0].Children, Has.Count.EqualTo(2));
            Assert.That(diagramVM.Layers[0].Children.Value[0].Name.Value, Is.EqualTo($"{Resources.Name_Item}3"));
            Assert.That(diagramVM.Layers[0].Children.Value[0].Children.Value[0].Name.Value, Is.EqualTo($"{Resources.Name_Item}1"));
            Assert.That(diagramVM.Layers[0].Children.Value[0].Children.Value[1].Name.Value, Is.EqualTo($"{Resources.Name_Item}2"));
            Assert.That(diagramVM.Layers[0].Children.Value[1].Name.Value, Is.EqualTo($"{Resources.Name_Item}4"));
            Assert.That(diagramVM.Layers[0].Children.Value[1].Children.Value[0].Name.Value, Is.EqualTo($"{Resources.Name_Item}1"));
            Assert.That(diagramVM.Layers[0].Children.Value[1].Children.Value[1].Name.Value, Is.EqualTo($"{Resources.Name_Item}2"));
        }


        [Test]
        public void Group_Duplicate_Move()
        {
            boilersGraphics.App.IsTest = true;
            var dlgService = new Mock<IDialogService>();
            MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(dlgService.Object);
            var diagramVM = new DiagramViewModel(mainWindowViewModel, 1000, 1000);
            diagramVM.Layers.Clear();
            var layer1 = new Layer();
            layer1.Name.Value = $"{Resources.Name_Layer}1";
            diagramVM.Layers.Add(layer1);
            layer1.IsSelected.Value = true; //レイヤー1を選択状態にする

            var item1 = new NRectangleViewModel();
            item1.Left.Value = 10;
            item1.Top.Value = 10;
            item1.Width.Value = 10;
            item1.Height.Value = 10;
            diagramVM.AddItemCommand.Execute(item1);

            var item2 = new NRectangleViewModel();
            item2.Left.Value = 20;
            item2.Top.Value = 20;
            item2.Width.Value = 10;
            item2.Height.Value = 10;
            diagramVM.AddItemCommand.Execute(item2);

            diagramVM.Layers[0].Children.Value[0].IsSelected.Value = true;
            diagramVM.Layers[0].Children.Value[1].IsSelected.Value = true;

            Assert.That(item1.ZIndex.Value, Is.EqualTo(0));
            Assert.That(item2.ZIndex.Value, Is.EqualTo(1));

            diagramVM.GroupCommand.Execute();

            Assert.That(diagramVM.Layers[0].Children, Has.Count.EqualTo(1));
            Assert.That(diagramVM.Layers[0].Children.Value[0].Name.Value, Is.EqualTo($"{Resources.Name_Item}3"));
            Assert.That(diagramVM.Layers[0].Children.Value[0].Children.Value[0].Name.Value, Is.EqualTo($"{Resources.Name_Item}1"));
            Assert.That(diagramVM.Layers[0].Children.Value[0].Children.Value[1].Name.Value, Is.EqualTo($"{Resources.Name_Item}2"));

            var group = diagramVM.AllItems.Value.First(x => x is GroupItemViewModel) as GroupItemViewModel;
            Assert.That(group, Is.TypeOf<GroupItemViewModel>());

            group.IsSelected.Value = true;

            Assert.That(item1.ZIndex.Value, Is.EqualTo(0));
            Assert.That(item2.ZIndex.Value, Is.EqualTo(1));
            Assert.That(group.ZIndex.Value, Is.EqualTo(2));

            diagramVM.DuplicateCommand.Execute();

            Assert.That(diagramVM.Layers[0].Children, Has.Count.EqualTo(2));
            Assert.That(diagramVM.Layers[0].Children.Value[0].Name.Value, Is.EqualTo($"{Resources.Name_Item}3"));
            Assert.That(diagramVM.Layers[0].Children.Value[0].Children.Value[0].Name.Value, Is.EqualTo($"{Resources.Name_Item}1"));
            Assert.That(diagramVM.Layers[0].Children.Value[0].Children.Value[1].Name.Value, Is.EqualTo($"{Resources.Name_Item}2"));
            Assert.That(diagramVM.Layers[0].Children.Value[1].Name.Value, Is.EqualTo($"{Resources.Name_Item}4"));
            Assert.That(diagramVM.Layers[0].Children.Value[1].Children.Value[0].Name.Value, Is.EqualTo($"{Resources.Name_Item}1"));
            Assert.That(diagramVM.Layers[0].Children.Value[1].Children.Value[1].Name.Value, Is.EqualTo($"{Resources.Name_Item}2"));

            var secondGroup = diagramVM.AllItems.Value.Skip(1).First(x => x is GroupItemViewModel) as GroupItemViewModel;
            Assert.That(secondGroup, Is.TypeOf<GroupItemViewModel>());

            secondGroup.Left.Value += 100;
            secondGroup.Top.Value += 100;

            Assert.That(diagramVM.Layers[0].Children, Has.Count.EqualTo(2));
            Assert.That(((diagramVM.Layers[0].Children.Value[0] as LayerItem).Item.Value as DesignerItemViewModelBase).Left.Value, Is.EqualTo(10));
            Assert.That(((diagramVM.Layers[0].Children.Value[0].Children.Value[0] as LayerItem).Item.Value as DesignerItemViewModelBase).Left.Value, Is.EqualTo(10));
            Assert.That(((diagramVM.Layers[0].Children.Value[0].Children.Value[1] as LayerItem).Item.Value as DesignerItemViewModelBase).Left.Value, Is.EqualTo(20));
            Assert.That(((diagramVM.Layers[0].Children.Value[1] as LayerItem).Item.Value as DesignerItemViewModelBase).Left.Value, Is.EqualTo(110));
            Assert.That(((diagramVM.Layers[0].Children.Value[1].Children.Value[0] as LayerItem).Item.Value as DesignerItemViewModelBase).Left.Value, Is.EqualTo(110));
            Assert.That(((diagramVM.Layers[0].Children.Value[1].Children.Value[1] as LayerItem).Item.Value as DesignerItemViewModelBase).Left.Value, Is.EqualTo(120));
        }


        [Test]
        public void Group_Duplicate_Include_Connector()
        {
            boilersGraphics.App.IsTest = true;
            var dlgService = new Mock<IDialogService>();
            MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(dlgService.Object);
            var diagramVM = new DiagramViewModel(mainWindowViewModel, 1000, 1000);
            diagramVM.Layers.Clear();
            var layer1 = new Layer();
            layer1.Name.Value = $"{Resources.Name_Layer}1";
            diagramVM.Layers.Add(layer1);
            layer1.IsSelected.Value = true; //レイヤー1を選択状態にする

            var item1 = new NRectangleViewModel();
            item1.Left.Value = 10;
            item1.Top.Value = 10;
            item1.Width.Value = 10;
            item1.Height.Value = 10;
            diagramVM.AddItemCommand.Execute(item1);

            var item2 = new StraightConnectorViewModel();
            item2.AddPointP1(diagramVM, new System.Windows.Point(10, 10));
            item2.AddPointP2(diagramVM, new System.Windows.Point(20, 20));
            diagramVM.AddItemCommand.Execute(item2);

            diagramVM.Layers[0].Children.Value[0].IsSelected.Value = true;
            diagramVM.Layers[0].Children.Value[1].IsSelected.Value = true;
            ((diagramVM.Layers[0].Children.Value[1] as LayerItem).Item.Value as ConnectorBaseViewModel).SnapPoint0VM.Value.IsSelected.Value = true;
            ((diagramVM.Layers[0].Children.Value[1] as LayerItem).Item.Value as ConnectorBaseViewModel).SnapPoint1VM.Value.IsSelected.Value = true;

            Assert.That(item1.ZIndex.Value, Is.EqualTo(0));
            Assert.That(item2.ZIndex.Value, Is.EqualTo(1));

            diagramVM.GroupCommand.Execute();

            Assert.That(diagramVM.Layers[0].Children, Has.Count.EqualTo(1));
            Assert.That(diagramVM.Layers[0].Children.Value[0].Name.Value, Is.EqualTo($"{Resources.Name_Item}3"));
            Assert.That(diagramVM.Layers[0].Children.Value[0].Children.Value[0].Name.Value, Is.EqualTo($"{Resources.Name_Item}1"));
            Assert.That(diagramVM.Layers[0].Children.Value[0].Children.Value[1].Name.Value, Is.EqualTo($"{Resources.Name_Item}2"));

            var group = diagramVM.AllItems.Value.First(x => x is GroupItemViewModel) as GroupItemViewModel;
            Assert.That(group, Is.TypeOf<GroupItemViewModel>());

            group.IsSelected.Value = true;

            Assert.That(item1.ZIndex.Value, Is.EqualTo(0));
            Assert.That(item2.ZIndex.Value, Is.EqualTo(1));
            Assert.That(group.ZIndex.Value, Is.EqualTo(2));

            diagramVM.DuplicateCommand.Execute();

            DisplayTree(diagramVM.Layers);

            Assert.That(diagramVM.Layers[0].Children, Has.Count.EqualTo(2));
            Assert.That(diagramVM.Layers[0].Children.Value[0].Name.Value, Is.EqualTo($"{Resources.Name_Item}3"));
            Assert.That((diagramVM.Layers[0].Children.Value[0] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(2));
            Assert.That(diagramVM.Layers[0].Children.Value[0].Children.Value[0].Name.Value, Is.EqualTo($"{Resources.Name_Item}1"));
            Assert.That((diagramVM.Layers[0].Children.Value[0].Children.Value[0] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(0));
            Assert.That(diagramVM.Layers[0].Children.Value[0].Children.Value[1].Name.Value, Is.EqualTo($"{Resources.Name_Item}2"));
            Assert.That((diagramVM.Layers[0].Children.Value[0].Children.Value[1] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(1));
            Assert.That(diagramVM.Layers[0].Children.Value[1].Name.Value, Is.EqualTo($"{Resources.Name_Item}4"));
            Assert.That((diagramVM.Layers[0].Children.Value[1] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(5));
            Assert.That(diagramVM.Layers[0].Children.Value[1].Children.Value[0].Name.Value, Is.EqualTo($"{Resources.Name_Item}1"));
            Assert.That((diagramVM.Layers[0].Children.Value[1].Children.Value[0] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(3));
            Assert.That(diagramVM.Layers[0].Children.Value[1].Children.Value[1].Name.Value, Is.EqualTo($"{Resources.Name_Item}2"));
            Assert.That((diagramVM.Layers[0].Children.Value[1].Children.Value[1] as LayerItem).Item.Value.ZIndex.Value, Is.EqualTo(4));
        }

        private void DisplayTree(ObservableCollection<LayerTreeViewItemBase> layers, int offset = 0)
        {
            foreach (var layer in layers)
            {
                string offsetStr = string.Empty;
                for (int i = 0; i < offset; i++)
                {
                    offsetStr += " ";
                }
                Console.WriteLine($"{offsetStr}{layer.ToString()}");
                DisplayTree(layer.Children.Value, offset + 2);
            }
        }
    }
}
