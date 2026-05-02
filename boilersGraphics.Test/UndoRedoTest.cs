using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using Moq;
using NUnit.Framework;
using Prism.Services.Dialogs;
using System.Linq;
using TsOperationHistory.Extensions;

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
        public void AlignLeft_Undo_RestoresPositions()
        {
            var (viewModel, _) = CreateSingleLayerViewModel();

            var item1 = new NRectangleViewModel();
            item1.Left.Value = 10; item1.Top.Value = 10;
            item1.Width.Value = 20; item1.Height.Value = 20;
            viewModel.AddItemCommand.Execute(item1);

            var item2 = new NRectangleViewModel();
            item2.Left.Value = 50; item2.Top.Value = 30;
            item2.Width.Value = 20; item2.Height.Value = 20;
            viewModel.AddItemCommand.Execute(item2);

            SelectByItem(viewModel, item1);
            SelectByItem(viewModel, item2);

            viewModel.AlignLeftCommand.Execute();
            Assert.That(item2.Left.Value, Is.EqualTo(10));

            viewModel.UndoCommand.Execute();

            Assert.That(item1.Left.Value, Is.EqualTo(10));
            Assert.That(item2.Left.Value, Is.EqualTo(50));
        }

        [Test]
        public void AlignTop_Undo_RestoresPositions()
        {
            var (viewModel, _) = CreateSingleLayerViewModel();

            var item1 = new NRectangleViewModel();
            item1.Left.Value = 10; item1.Top.Value = 10;
            item1.Width.Value = 20; item1.Height.Value = 20;
            viewModel.AddItemCommand.Execute(item1);

            var item2 = new NRectangleViewModel();
            item2.Left.Value = 30; item2.Top.Value = 50;
            item2.Width.Value = 20; item2.Height.Value = 20;
            viewModel.AddItemCommand.Execute(item2);

            SelectByItem(viewModel, item1);
            SelectByItem(viewModel, item2);

            viewModel.AlignTopCommand.Execute();
            Assert.That(item2.Top.Value, Is.EqualTo(10));

            viewModel.UndoCommand.Execute();

            Assert.That(item1.Top.Value, Is.EqualTo(10));
            Assert.That(item2.Top.Value, Is.EqualTo(50));
        }

        [Test]
        public void Union_Undo_RemovesCombinedGeometry()
        {
            var (viewModel, layer) = CreateSingleLayerViewModel();

            var item1 = new NRectangleViewModel();
            item1.Left.Value = 10; item1.Top.Value = 10;
            item1.Width.Value = 20; item1.Height.Value = 20;
            viewModel.AddItemCommand.Execute(item1);

            var item2 = new NRectangleViewModel();
            item2.Left.Value = 20; item2.Top.Value = 20;
            item2.Width.Value = 20; item2.Height.Value = 20;
            viewModel.AddItemCommand.Execute(item2);

            SelectByItem(viewModel, item1);
            SelectByItem(viewModel, item2);

            var beforeCount = viewModel.AllItems.Value.Length;
            viewModel.UnionCommand.Execute();
            Assert.That(viewModel.AllItems.Value.OfType<CombineGeometryViewModel>().Count(),
                Is.GreaterThan(0));

            viewModel.UndoCommand.Execute();

            Assert.That(viewModel.AllItems.Value.OfType<CombineGeometryViewModel>().Count(),
                Is.EqualTo(0));
        }

        [Test]
        public void Duplicate_Undo_RemovesCopies()
        {
            var (viewModel, layer) = CreateSingleLayerViewModel();
            var r = AddRectangles(viewModel, 2);

            SelectByItem(viewModel, r[0]);
            SelectByItem(viewModel, r[1]);

            var beforeCount = layer.Children.Count;
            viewModel.DuplicateCommand.Execute();
            Assert.That(layer.Children.Count, Is.GreaterThan(beforeCount));

            viewModel.UndoCommand.Execute();

            Assert.That(layer.Children.Count, Is.EqualTo(beforeCount));
        }

        [Test]
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

        // Property-level Undo coverage: properties commonly mutated through
        // Recorder.Current.ExecuteSetProperty (resize, rotate, edge / fill
        // changes). These guard the property-set + Undo round-trip without
        // requiring the UI command surface that drives them in production.

        [Test]
        public void Resize_Width_ExecuteSetProperty_Undo_RestoresOriginalWidth()
        {
            var (viewModel, _) = CreateSingleLayerViewModel();
            var r = new NRectangleViewModel();
            r.Width.Value = 30;
            viewModel.AddItemCommand.Execute(r);

            var mainWindowVM = viewModel.MainWindowVM;
            mainWindowVM.Recorder.BeginRecode();
            mainWindowVM.Recorder.Current.ExecuteSetProperty(r, "Width.Value", 80.0);
            mainWindowVM.Recorder.EndRecode();
            Assert.That(r.Width.Value, Is.EqualTo(80));

            viewModel.UndoCommand.Execute();
            Assert.That(r.Width.Value, Is.EqualTo(30));
        }

        [Test]
        public void Resize_Height_ExecuteSetProperty_Undo_RestoresOriginalHeight()
        {
            var (viewModel, _) = CreateSingleLayerViewModel();
            var r = new NRectangleViewModel();
            r.Height.Value = 25;
            viewModel.AddItemCommand.Execute(r);

            var mainWindowVM = viewModel.MainWindowVM;
            mainWindowVM.Recorder.BeginRecode();
            mainWindowVM.Recorder.Current.ExecuteSetProperty(r, "Height.Value", 90.0);
            mainWindowVM.Recorder.EndRecode();
            Assert.That(r.Height.Value, Is.EqualTo(90));

            viewModel.UndoCommand.Execute();
            Assert.That(r.Height.Value, Is.EqualTo(25));
        }

        [Test]
        public void Rotate_RotationAngle_ExecuteSetProperty_Undo_RestoresOriginalAngle()
        {
            var (viewModel, _) = CreateSingleLayerViewModel();
            var r = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(r);
            Assert.That(r.RotationAngle.Value, Is.EqualTo(0));

            var mainWindowVM = viewModel.MainWindowVM;
            mainWindowVM.Recorder.BeginRecode();
            mainWindowVM.Recorder.Current.ExecuteSetProperty(r, "RotationAngle.Value", 45.0);
            mainWindowVM.Recorder.EndRecode();
            Assert.That(r.RotationAngle.Value, Is.EqualTo(45));

            viewModel.UndoCommand.Execute();
            Assert.That(r.RotationAngle.Value, Is.EqualTo(0));
        }

        [Test]
        public void EdgeThickness_ExecuteSetProperty_Undo_RestoresOriginalThickness()
        {
            var (viewModel, _) = CreateSingleLayerViewModel();
            var r = new NRectangleViewModel();
            r.EdgeThickness.Value = 1.0;
            viewModel.AddItemCommand.Execute(r);

            var mainWindowVM = viewModel.MainWindowVM;
            mainWindowVM.Recorder.BeginRecode();
            mainWindowVM.Recorder.Current.ExecuteSetProperty(r, "EdgeThickness.Value", 5.0);
            mainWindowVM.Recorder.EndRecode();
            Assert.That(r.EdgeThickness.Value, Is.EqualTo(5));

            viewModel.UndoCommand.Execute();
            Assert.That(r.EdgeThickness.Value, Is.EqualTo(1));
        }

        [Test]
        public void EdgeBrush_ExecuteSetProperty_Undo_RestoresOriginalBrush()
        {
            var (viewModel, _) = CreateSingleLayerViewModel();
            var r = new NRectangleViewModel();
            var original = r.EdgeBrush.Value;
            viewModel.AddItemCommand.Execute(r);

            var newBrush = System.Windows.Media.Brushes.Crimson;
            var mainWindowVM = viewModel.MainWindowVM;
            mainWindowVM.Recorder.BeginRecode();
            mainWindowVM.Recorder.Current.ExecuteSetProperty(r, "EdgeBrush.Value", newBrush);
            mainWindowVM.Recorder.EndRecode();
            Assert.That(r.EdgeBrush.Value, Is.SameAs(newBrush));

            viewModel.UndoCommand.Execute();
            Assert.That(r.EdgeBrush.Value, Is.SameAs(original));
        }

        [Test]
        public void FillBrush_ExecuteSetProperty_Undo_RestoresOriginalBrush()
        {
            var (viewModel, _) = CreateSingleLayerViewModel();
            var r = new NRectangleViewModel();
            var original = r.FillBrush.Value;
            viewModel.AddItemCommand.Execute(r);

            var newBrush = System.Windows.Media.Brushes.SkyBlue;
            var mainWindowVM = viewModel.MainWindowVM;
            mainWindowVM.Recorder.BeginRecode();
            mainWindowVM.Recorder.Current.ExecuteSetProperty(r, "FillBrush.Value", newBrush);
            mainWindowVM.Recorder.EndRecode();
            Assert.That(r.FillBrush.Value, Is.SameAs(newBrush));

            viewModel.UndoCommand.Execute();
            Assert.That(r.FillBrush.Value, Is.SameAs(original));
        }
    }
}
