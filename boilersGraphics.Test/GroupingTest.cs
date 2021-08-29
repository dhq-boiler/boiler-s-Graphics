using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using NUnit.Framework;
using System.Linq;
using System.Reactive.Linq;

namespace boilersGraphics.Test
{
    [TestFixture]
    public class GroupingTest
    {
        [Test]
        public void Group_Move()
        {
            boilersGraphics.App.IsTest = true;
            var mainWindowViewModel = new MainWindowViewModel(null);
            var diagramVM = new DiagramViewModel(mainWindowViewModel, 1000, 1000);
            diagramVM.Layers.Clear();
            var layer1 = new Layer();
            layer1.Name.Value = "レイヤー1";
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

            diagramVM.Layers[0].Children[0].IsSelected.Value = true;
            diagramVM.Layers[0].Children[1].IsSelected.Value = true;
            diagramVM.Layers[0].Children[2].IsSelected.Value = true;

            diagramVM.GroupCommand.Execute();

            var group = diagramVM.AllItems.Value.First(x => x is GroupItemViewModel) as GroupItemViewModel;
            Assert.That(group, Is.TypeOf<GroupItemViewModel>());

            group.IsSelected.Value = true;
            group.Left.Value += 10;
            group.Top.Value += 10;

            diagramVM.Layers[0].Children[0].IsSelected.Value = true;

            diagramVM.UngroupCommand.Execute();

            Assert.That(item1.Left.Value, Is.EqualTo(20));
            Assert.That(item1.Top.Value, Is.EqualTo(20));

            Assert.That(item2.Left.Value, Is.EqualTo(30));
            Assert.That(item2.Top.Value, Is.EqualTo(30));

            Assert.That(item3.Left.Value, Is.EqualTo(40));
            Assert.That(item3.Top.Value, Is.EqualTo(40));
        }
    }
}
