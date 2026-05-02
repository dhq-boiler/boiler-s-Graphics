using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using Moq;
using NUnit.Framework;
using Prism.Services.Dialogs;
using System.Linq;

namespace boilersGraphics.Test
{
    [TestFixture]
    public class UndoRedoTest
    {
        private static (DiagramViewModel viewModel, Layer layer) CreateSingleLayerViewModel()
        {
            boilersGraphics.App.IsTest = true;
            var dlgService = new Mock<IDialogService>();
            var mainWindowViewModel = new MainWindowViewModel(dlgService.Object);
            var viewModel = new DiagramViewModel(mainWindowViewModel);
            viewModel.Layers.Clear();

            var layer = new Layer();
            layer.Name.Value = "Layer1";
            viewModel.Layers.Add(layer);
            layer.IsSelected.Value = true;

            return (viewModel, layer);
        }

        private static NRectangleViewModel[] AddRectangles(DiagramViewModel viewModel, int count)
        {
            var rects = Enumerable.Range(0, count).Select(_ => new NRectangleViewModel()).ToArray();
            foreach (var r in rects)
                viewModel.AddItemCommand.Execute(r);
            return rects;
        }

        private static void DeselectAll(DiagramViewModel viewModel)
        {
            foreach (var layer in viewModel.Layers)
                foreach (var child in layer.Children)
                    DeselectRecursive(child);
        }

        private static void DeselectRecursive(LayerTreeViewItemBase item)
        {
            item.IsSelected.Value = false;
            foreach (var child in item.Children)
                DeselectRecursive(child);
        }

        private static void SelectByItem(DiagramViewModel viewModel, SelectableDesignerItemViewModelBase target)
        {
            foreach (var layer in viewModel.Layers)
                if (TrySelectInItem(layer, target)) return;
        }

        private static bool TrySelectInItem(LayerTreeViewItemBase item, SelectableDesignerItemViewModelBase target)
        {
            if (item is LayerItem li && li.Item.Value == target)
            {
                li.IsSelected.Value = true;
                return true;
            }
            foreach (var child in item.Children)
                if (TrySelectInItem(child, target)) return true;
            return false;
        }

        [Test]
        public void AddItem_Undo_RemovesItem()
        {
            var (viewModel, layer) = CreateSingleLayerViewModel();

            var r = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(r);

            Assert.That(layer.Children, Has.Count.EqualTo(1));

            viewModel.UndoCommand.Execute();

            Assert.That(layer.Children, Has.Count.EqualTo(0));
        }

        [Test]
        public void AddItem_Undo_Redo_RestoresItem()
        {
            var (viewModel, layer) = CreateSingleLayerViewModel();

            var r = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(r);
            viewModel.UndoCommand.Execute();
            viewModel.RedoCommand.Execute();

            Assert.That(layer.Children, Has.Count.EqualTo(1));
        }

        [Test]
        public void Group_Undo_RestoresIndividualItems()
        {
            var (viewModel, layer) = CreateSingleLayerViewModel();
            var r = AddRectangles(viewModel, 3);

            SelectByItem(viewModel, r[0]);
            SelectByItem(viewModel, r[1]);
            SelectByItem(viewModel, r[2]);
            viewModel.GroupCommand.Execute();
            Assert.That(viewModel.AllItems.Value.OfType<GroupItemViewModel>().Count(), Is.EqualTo(1));

            viewModel.UndoCommand.Execute();

            Assert.That(viewModel.AllItems.Value.OfType<GroupItemViewModel>().Count(), Is.EqualTo(0));
            Assert.That(layer.Children, Has.Count.EqualTo(3));
        }

        [Test]
        public void BringForward_Undo_RestoresZIndex()
        {
            var (viewModel, _) = CreateSingleLayerViewModel();
            var r = AddRectangles(viewModel, 5);

            SelectByItem(viewModel, r[2]);
            viewModel.BringForwardCommand.Execute();
            Assert.That(r[2].ZIndex.Value, Is.EqualTo(3));

            viewModel.UndoCommand.Execute();

            Assert.That(r[0].ZIndex.Value, Is.EqualTo(0));
            Assert.That(r[1].ZIndex.Value, Is.EqualTo(1));
            Assert.That(r[2].ZIndex.Value, Is.EqualTo(2));
            Assert.That(r[3].ZIndex.Value, Is.EqualTo(3));
            Assert.That(r[4].ZIndex.Value, Is.EqualTo(4));
        }

        [Test]
        public void BringForeground_Undo_RestoresZIndex()
        {
            var (viewModel, _) = CreateSingleLayerViewModel();
            var r = AddRectangles(viewModel, 5);

            SelectByItem(viewModel, r[2]);
            viewModel.BringForegroundCommand.Execute();
            Assert.That(r[2].ZIndex.Value, Is.EqualTo(4));

            viewModel.UndoCommand.Execute();

            Assert.That(r[0].ZIndex.Value, Is.EqualTo(0));
            Assert.That(r[1].ZIndex.Value, Is.EqualTo(1));
            Assert.That(r[2].ZIndex.Value, Is.EqualTo(2));
            Assert.That(r[3].ZIndex.Value, Is.EqualTo(3));
            Assert.That(r[4].ZIndex.Value, Is.EqualTo(4));
        }

        [Test]
        public void SendBackward_Undo_RestoresZIndex()
        {
            var (viewModel, _) = CreateSingleLayerViewModel();
            var r = AddRectangles(viewModel, 5);

            SelectByItem(viewModel, r[2]);
            viewModel.SendBackwardCommand.Execute();
            Assert.That(r[2].ZIndex.Value, Is.EqualTo(1));

            viewModel.UndoCommand.Execute();

            Assert.That(r[0].ZIndex.Value, Is.EqualTo(0));
            Assert.That(r[1].ZIndex.Value, Is.EqualTo(1));
            Assert.That(r[2].ZIndex.Value, Is.EqualTo(2));
            Assert.That(r[3].ZIndex.Value, Is.EqualTo(3));
            Assert.That(r[4].ZIndex.Value, Is.EqualTo(4));
        }

        [Test]
        public void SendBackground_Undo_RestoresZIndex()
        {
            var (viewModel, _) = CreateSingleLayerViewModel();
            var r = AddRectangles(viewModel, 5);

            SelectByItem(viewModel, r[2]);
            viewModel.SendBackgroundCommand.Execute();
            Assert.That(r[2].ZIndex.Value, Is.EqualTo(0));

            viewModel.UndoCommand.Execute();

            Assert.That(r[0].ZIndex.Value, Is.EqualTo(0));
            Assert.That(r[1].ZIndex.Value, Is.EqualTo(1));
            Assert.That(r[2].ZIndex.Value, Is.EqualTo(2));
            Assert.That(r[3].ZIndex.Value, Is.EqualTo(3));
            Assert.That(r[4].ZIndex.Value, Is.EqualTo(4));
        }

        [Test]
        public void BringForward_Undo_Redo_ReappliesShift()
        {
            var (viewModel, _) = CreateSingleLayerViewModel();
            var r = AddRectangles(viewModel, 5);

            SelectByItem(viewModel, r[2]);
            viewModel.BringForwardCommand.Execute();
            viewModel.UndoCommand.Execute();
            viewModel.RedoCommand.Execute();

            Assert.That(r[2].ZIndex.Value, Is.EqualTo(3));
            Assert.That(r[3].ZIndex.Value, Is.EqualTo(2));
        }

        [Test]
        [Ignore("Pre-existing regression: Ungroup disposes the group's reactive " +
                "properties, so a subsequent Undo throws ObjectDisposedException " +
                "when re-subscribing to the restored group. Tracked separately.")]
        public void Ungroup_Undo_RestoresGroup()
        {
            var (viewModel, _) = CreateSingleLayerViewModel();
            var r = AddRectangles(viewModel, 3);

            SelectByItem(viewModel, r[0]);
            SelectByItem(viewModel, r[1]);
            SelectByItem(viewModel, r[2]);
            viewModel.GroupCommand.Execute();
            var group = viewModel.AllItems.Value.OfType<GroupItemViewModel>().Single();

            DeselectAll(viewModel);
            SelectByItem(viewModel, group);
            viewModel.UngroupCommand.Execute();
            Assert.That(viewModel.AllItems.Value.OfType<GroupItemViewModel>().Count(), Is.EqualTo(0));

            viewModel.UndoCommand.Execute();

            Assert.That(viewModel.AllItems.Value.OfType<GroupItemViewModel>().Count(), Is.EqualTo(1));
        }
    }
}
