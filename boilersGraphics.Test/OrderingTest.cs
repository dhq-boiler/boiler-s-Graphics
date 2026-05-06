using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using Moq;
using NUnit.Framework;
using Prism.Services.Dialogs;
using System.Linq;

namespace boilersGraphics.Test
{
    [TestFixture]
    public class OrderingTest
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
            // The LayerItem -> underlying-item IsSelected propagation does
            // not always fire (e.g. for groups), so set the inner item's
            // IsSelected explicitly too.
            if (item is LayerItem li && li.Item.Value != null)
                li.Item.Value.IsSelected.Value = false;
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

        private static GroupItemViewModel[] AllGroups(DiagramViewModel viewModel)
            => viewModel.AllItems.Value.OfType<GroupItemViewModel>().ToArray();

        [Test]
        public void BringForward()
        {
            var (viewModel, _) = CreateSingleLayerViewModel();
            var r = AddRectangles(viewModel, 5);

            viewModel.Layers[0].Children[2].IsSelected.Value = true;
            viewModel.BringForwardCommand.Execute();

            Assert.That(r[0].ZIndex.Value, Is.EqualTo(0));
            Assert.That(r[1].ZIndex.Value, Is.EqualTo(1));
            Assert.That(r[2].ZIndex.Value, Is.EqualTo(3));
            Assert.That(r[3].ZIndex.Value, Is.EqualTo(2));
            Assert.That(r[4].ZIndex.Value, Is.EqualTo(4));
        }

        [Test]
        public void BringForward_NoEffect()
        {
            var (viewModel, _) = CreateSingleLayerViewModel();
            var r = AddRectangles(viewModel, 5);

            viewModel.Layers[0].Children[4].IsSelected.Value = true;
            viewModel.BringForwardCommand.Execute();

            Assert.That(r[0].ZIndex.Value, Is.EqualTo(0));
            Assert.That(r[1].ZIndex.Value, Is.EqualTo(1));
            Assert.That(r[2].ZIndex.Value, Is.EqualTo(2));
            Assert.That(r[3].ZIndex.Value, Is.EqualTo(3));
            Assert.That(r[4].ZIndex.Value, Is.EqualTo(4));
        }

        [Test]
        public void BringForeground()
        {
            var (viewModel, _) = CreateSingleLayerViewModel();
            var r = AddRectangles(viewModel, 5);

            viewModel.Layers[0].Children[2].IsSelected.Value = true;
            viewModel.BringForegroundCommand.Execute();

            Assert.That(r[0].ZIndex.Value, Is.EqualTo(0));
            Assert.That(r[1].ZIndex.Value, Is.EqualTo(1));
            Assert.That(r[2].ZIndex.Value, Is.EqualTo(4));
            Assert.That(r[3].ZIndex.Value, Is.EqualTo(2));
            Assert.That(r[4].ZIndex.Value, Is.EqualTo(3));
        }

        [Test]
        public void BringForeground_NoEffect()
        {
            var (viewModel, _) = CreateSingleLayerViewModel();
            var r = AddRectangles(viewModel, 5);

            viewModel.Layers[0].Children[4].IsSelected.Value = true;
            viewModel.BringForegroundCommand.Execute();

            Assert.That(r[0].ZIndex.Value, Is.EqualTo(0));
            Assert.That(r[1].ZIndex.Value, Is.EqualTo(1));
            Assert.That(r[2].ZIndex.Value, Is.EqualTo(2));
            Assert.That(r[3].ZIndex.Value, Is.EqualTo(3));
            Assert.That(r[4].ZIndex.Value, Is.EqualTo(4));
        }

        [Test]
        public void SendBackward()
        {
            var (viewModel, _) = CreateSingleLayerViewModel();
            var r = AddRectangles(viewModel, 5);

            viewModel.Layers[0].Children[2].IsSelected.Value = true;
            viewModel.SendBackwardCommand.Execute();

            Assert.That(r[0].ZIndex.Value, Is.EqualTo(0));
            Assert.That(r[1].ZIndex.Value, Is.EqualTo(2));
            Assert.That(r[2].ZIndex.Value, Is.EqualTo(1));
            Assert.That(r[3].ZIndex.Value, Is.EqualTo(3));
            Assert.That(r[4].ZIndex.Value, Is.EqualTo(4));
        }

        [Test]
        public void SendBackward_NoEffect()
        {
            var (viewModel, _) = CreateSingleLayerViewModel();
            var r = AddRectangles(viewModel, 5);

            viewModel.Layers[0].Children[0].IsSelected.Value = true;
            viewModel.SendBackwardCommand.Execute();

            Assert.That(r[0].ZIndex.Value, Is.EqualTo(0));
            Assert.That(r[1].ZIndex.Value, Is.EqualTo(1));
            Assert.That(r[2].ZIndex.Value, Is.EqualTo(2));
            Assert.That(r[3].ZIndex.Value, Is.EqualTo(3));
            Assert.That(r[4].ZIndex.Value, Is.EqualTo(4));
        }

        [Test]
        public void SendBackground()
        {
            var (viewModel, _) = CreateSingleLayerViewModel();
            var r = AddRectangles(viewModel, 5);

            viewModel.Layers[0].Children[2].IsSelected.Value = true;
            viewModel.SendBackgroundCommand.Execute();

            Assert.That(r[0].ZIndex.Value, Is.EqualTo(1));
            Assert.That(r[1].ZIndex.Value, Is.EqualTo(2));
            Assert.That(r[2].ZIndex.Value, Is.EqualTo(0));
            Assert.That(r[3].ZIndex.Value, Is.EqualTo(3));
            Assert.That(r[4].ZIndex.Value, Is.EqualTo(4));
        }

        [Test]
        public void SendBackground_NoEffect()
        {
            var (viewModel, _) = CreateSingleLayerViewModel();
            var r = AddRectangles(viewModel, 5);

            viewModel.Layers[0].Children[0].IsSelected.Value = true;
            viewModel.SendBackgroundCommand.Execute();

            Assert.That(r[0].ZIndex.Value, Is.EqualTo(0));
            Assert.That(r[1].ZIndex.Value, Is.EqualTo(1));
            Assert.That(r[2].ZIndex.Value, Is.EqualTo(2));
            Assert.That(r[3].ZIndex.Value, Is.EqualTo(3));
            Assert.That(r[4].ZIndex.Value, Is.EqualTo(4));
        }

        // Wave 2: Z-order on an item that lives between two groups.
        // Under the "swap with the next/previous top-level neighbour"
        // design, groups and ungrouped items at the layer's top level
        // form one ordering. Children inside a group keep their ZIndex
        // values intact when their parent moves.

        [Test]
        public void BringForward_GroupIncluded()
        {
            var (viewModel, _) = CreateSingleLayerViewModel();
            var r = AddRectangles(viewModel, 7);

            SelectByItem(viewModel, r[1]);
            SelectByItem(viewModel, r[2]);
            viewModel.GroupCommand.Execute();
            var groupA = AllGroups(viewModel).Single();
            DeselectAll(viewModel);

            SelectByItem(viewModel, r[4]);
            SelectByItem(viewModel, r[5]);
            viewModel.GroupCommand.Execute();
            var groupB = AllGroups(viewModel).Single(g => g != groupA);
            DeselectAll(viewModel);

            // Top-level ZIndex order: r0(0), r3(3), r6(6), groupA(7), groupB(8)
            SelectByItem(viewModel, r[3]);
            viewModel.BringForwardCommand.Execute();

            // r3 swaps ZIndex with the next top-level neighbour above it (r6).
            Assert.That(r[0].ZIndex.Value, Is.EqualTo(0));
            Assert.That(r[1].ZIndex.Value, Is.EqualTo(1));
            Assert.That(r[2].ZIndex.Value, Is.EqualTo(2));
            Assert.That(r[6].ZIndex.Value, Is.EqualTo(3));
            Assert.That(r[4].ZIndex.Value, Is.EqualTo(4));
            Assert.That(r[5].ZIndex.Value, Is.EqualTo(5));
            Assert.That(r[3].ZIndex.Value, Is.EqualTo(6));
            Assert.That(groupA.ZIndex.Value, Is.EqualTo(7));
            Assert.That(groupB.ZIndex.Value, Is.EqualTo(8));
        }

        [Test]
        public void BringForeground_GroupIncluded()
        {
            var (viewModel, _) = CreateSingleLayerViewModel();
            var r = AddRectangles(viewModel, 7);

            SelectByItem(viewModel, r[1]);
            SelectByItem(viewModel, r[2]);
            viewModel.GroupCommand.Execute();
            var groupA = AllGroups(viewModel).Single();
            DeselectAll(viewModel);

            SelectByItem(viewModel, r[4]);
            SelectByItem(viewModel, r[5]);
            viewModel.GroupCommand.Execute();
            var groupB = AllGroups(viewModel).Single(g => g != groupA);
            DeselectAll(viewModel);

            SelectByItem(viewModel, r[3]);
            viewModel.BringForegroundCommand.Execute();

            // r3 walks past r6, groupA and groupB one swap at a time and
            // lands at ZIndex 8; each neighbour drops one slot.
            Assert.That(r[0].ZIndex.Value, Is.EqualTo(0));
            Assert.That(r[1].ZIndex.Value, Is.EqualTo(1));
            Assert.That(r[2].ZIndex.Value, Is.EqualTo(2));
            Assert.That(r[6].ZIndex.Value, Is.EqualTo(3));
            Assert.That(r[4].ZIndex.Value, Is.EqualTo(4));
            Assert.That(r[5].ZIndex.Value, Is.EqualTo(5));
            Assert.That(groupA.ZIndex.Value, Is.EqualTo(6));
            Assert.That(groupB.ZIndex.Value, Is.EqualTo(7));
            Assert.That(r[3].ZIndex.Value, Is.EqualTo(8));
        }

        [Test]
        public void SendBackward_GroupIncluded()
        {
            var (viewModel, _) = CreateSingleLayerViewModel();
            var r = AddRectangles(viewModel, 7);

            SelectByItem(viewModel, r[1]);
            SelectByItem(viewModel, r[2]);
            viewModel.GroupCommand.Execute();
            var groupA = AllGroups(viewModel).Single();
            DeselectAll(viewModel);

            SelectByItem(viewModel, r[4]);
            SelectByItem(viewModel, r[5]);
            viewModel.GroupCommand.Execute();
            var groupB = AllGroups(viewModel).Single(g => g != groupA);
            DeselectAll(viewModel);

            SelectByItem(viewModel, r[3]);
            viewModel.SendBackwardCommand.Execute();

            // r3 swaps with r0 (the next top-level item below it).
            Assert.That(r[3].ZIndex.Value, Is.EqualTo(0));
            Assert.That(r[1].ZIndex.Value, Is.EqualTo(1));
            Assert.That(r[2].ZIndex.Value, Is.EqualTo(2));
            Assert.That(r[0].ZIndex.Value, Is.EqualTo(3));
            Assert.That(r[4].ZIndex.Value, Is.EqualTo(4));
            Assert.That(r[5].ZIndex.Value, Is.EqualTo(5));
            Assert.That(r[6].ZIndex.Value, Is.EqualTo(6));
            Assert.That(groupA.ZIndex.Value, Is.EqualTo(7));
            Assert.That(groupB.ZIndex.Value, Is.EqualTo(8));
        }

        [Test]
        public void SendBackground_GroupIncluded()
        {
            var (viewModel, _) = CreateSingleLayerViewModel();
            var r = AddRectangles(viewModel, 7);

            SelectByItem(viewModel, r[1]);
            SelectByItem(viewModel, r[2]);
            viewModel.GroupCommand.Execute();
            var groupA = AllGroups(viewModel).Single();
            DeselectAll(viewModel);

            SelectByItem(viewModel, r[4]);
            SelectByItem(viewModel, r[5]);
            viewModel.GroupCommand.Execute();
            var groupB = AllGroups(viewModel).Single(g => g != groupA);
            DeselectAll(viewModel);

            SelectByItem(viewModel, r[3]);
            viewModel.SendBackgroundCommand.Execute();

            // Only r0 sits below r3 at top level, so SendBackground reduces
            // to a single swap with r0 — same final state as SendBackward.
            Assert.That(r[3].ZIndex.Value, Is.EqualTo(0));
            Assert.That(r[1].ZIndex.Value, Is.EqualTo(1));
            Assert.That(r[2].ZIndex.Value, Is.EqualTo(2));
            Assert.That(r[0].ZIndex.Value, Is.EqualTo(3));
            Assert.That(r[4].ZIndex.Value, Is.EqualTo(4));
            Assert.That(r[5].ZIndex.Value, Is.EqualTo(5));
            Assert.That(r[6].ZIndex.Value, Is.EqualTo(6));
            Assert.That(groupA.ZIndex.Value, Is.EqualTo(7));
            Assert.That(groupB.ZIndex.Value, Is.EqualTo(8));
        }

        [Test]
        public void Group_3items_inLayerWith4()
        {
            var (viewModel, _) = CreateSingleLayerViewModel();
            var r = AddRectangles(viewModel, 4);

            SelectByItem(viewModel, r[0]);
            SelectByItem(viewModel, r[1]);
            SelectByItem(viewModel, r[2]);
            viewModel.GroupCommand.Execute();
            var group = AllGroups(viewModel).Single();

            Assert.That(r[0].ZIndex.Value, Is.EqualTo(0));
            Assert.That(r[1].ZIndex.Value, Is.EqualTo(1));
            Assert.That(r[2].ZIndex.Value, Is.EqualTo(2));
            Assert.That(r[3].ZIndex.Value, Is.EqualTo(3));
            Assert.That(group.ZIndex.Value, Is.EqualTo(4));
        }

        [Test]
        public void Group_middleItems_inLayerWith7()
        {
            var (viewModel, _) = CreateSingleLayerViewModel();
            var r = AddRectangles(viewModel, 7);

            SelectByItem(viewModel, r[2]);
            SelectByItem(viewModel, r[3]);
            SelectByItem(viewModel, r[4]);
            viewModel.GroupCommand.Execute();
            var group = AllGroups(viewModel).Single();

            Assert.That(r[0].ZIndex.Value, Is.EqualTo(0));
            Assert.That(r[1].ZIndex.Value, Is.EqualTo(1));
            Assert.That(r[2].ZIndex.Value, Is.EqualTo(2));
            Assert.That(r[3].ZIndex.Value, Is.EqualTo(3));
            Assert.That(r[4].ZIndex.Value, Is.EqualTo(4));
            Assert.That(r[5].ZIndex.Value, Is.EqualTo(5));
            Assert.That(r[6].ZIndex.Value, Is.EqualTo(6));
            Assert.That(group.ZIndex.Value, Is.EqualTo(7));
        }

        [Test]
        public void Group_discontinuousItems()
        {
            var (viewModel, _) = CreateSingleLayerViewModel();
            var r = AddRectangles(viewModel, 5);

            SelectByItem(viewModel, r[0]);
            SelectByItem(viewModel, r[1]);
            SelectByItem(viewModel, r[3]);
            viewModel.GroupCommand.Execute();
            var group = AllGroups(viewModel).Single();

            Assert.That(r[0].ZIndex.Value, Is.EqualTo(0));
            Assert.That(r[1].ZIndex.Value, Is.EqualTo(1));
            Assert.That(r[2].ZIndex.Value, Is.EqualTo(2));
            Assert.That(r[3].ZIndex.Value, Is.EqualTo(3));
            Assert.That(r[4].ZIndex.Value, Is.EqualTo(4));
            Assert.That(group.ZIndex.Value, Is.EqualTo(5));
        }

        [Test]
        public void Ungroup_restoresChildrenToTopLevel()
        {
            var (viewModel, _) = CreateSingleLayerViewModel();
            var r = AddRectangles(viewModel, 5);

            SelectByItem(viewModel, r[0]);
            SelectByItem(viewModel, r[1]);
            SelectByItem(viewModel, r[3]);
            viewModel.GroupCommand.Execute();
            var group = AllGroups(viewModel).Single();
            Assert.That(group, Is.Not.Null);

            DeselectAll(viewModel);
            SelectByItem(viewModel, group);
            viewModel.UngroupCommand.Execute();

            Assert.That(AllGroups(viewModel), Is.Empty);
            Assert.That(r[0].ZIndex.Value, Is.EqualTo(0));
            Assert.That(r[1].ZIndex.Value, Is.EqualTo(1));
            Assert.That(r[2].ZIndex.Value, Is.EqualTo(2));
            Assert.That(r[3].ZIndex.Value, Is.EqualTo(3));
            Assert.That(r[4].ZIndex.Value, Is.EqualTo(4));
        }

        // Wave 4: Z-order applied TO a group as the target. Per the
        // "swap with the next/previous top-level neighbour" design,
        // moving a group only changes the group's own ZIndex; the
        // ZIndex of items inside the group stays untouched. When the
        // group already sits at the top (which is where Group leaves
        // it) BringForward / BringForeground are no-ops.

        [Test]
        public void Group_BringForward()
        {
            var (viewModel, _) = CreateSingleLayerViewModel();
            var r = AddRectangles(viewModel, 4);

            SelectByItem(viewModel, r[0]);
            SelectByItem(viewModel, r[1]);
            SelectByItem(viewModel, r[2]);
            viewModel.GroupCommand.Execute();
            var group = AllGroups(viewModel).Single();
            DeselectAll(viewModel);

            // After grouping: r0=0, r1=1, r2=2 (in group), r3=3, group=4.
            // Group already sits on top of the only top-level neighbour (r3).
            SelectByItem(viewModel, group);
            viewModel.BringForwardCommand.Execute();

            Assert.That(r[0].ZIndex.Value, Is.EqualTo(0));
            Assert.That(r[1].ZIndex.Value, Is.EqualTo(1));
            Assert.That(r[2].ZIndex.Value, Is.EqualTo(2));
            Assert.That(r[3].ZIndex.Value, Is.EqualTo(3));
            Assert.That(group.ZIndex.Value, Is.EqualTo(4));
        }

        [Test]
        public void Group_BringForward_2()
        {
            var (viewModel, _) = CreateSingleLayerViewModel();
            var r = AddRectangles(viewModel, 5);

            SelectByItem(viewModel, r[0]);
            SelectByItem(viewModel, r[1]);
            SelectByItem(viewModel, r[2]);
            viewModel.GroupCommand.Execute();
            var group = AllGroups(viewModel).Single();
            DeselectAll(viewModel);

            // Group sits on top after creation (group=5), no neighbour
            // above to swap with.
            SelectByItem(viewModel, group);
            viewModel.BringForwardCommand.Execute();

            Assert.That(r[0].ZIndex.Value, Is.EqualTo(0));
            Assert.That(r[1].ZIndex.Value, Is.EqualTo(1));
            Assert.That(r[2].ZIndex.Value, Is.EqualTo(2));
            Assert.That(r[3].ZIndex.Value, Is.EqualTo(3));
            Assert.That(r[4].ZIndex.Value, Is.EqualTo(4));
            Assert.That(group.ZIndex.Value, Is.EqualTo(5));
        }

        [Test]
        public void Group_BringForward_NoEffect()
        {
            var (viewModel, _) = CreateSingleLayerViewModel();
            var r = AddRectangles(viewModel, 4);

            SelectByItem(viewModel, r[1]);
            SelectByItem(viewModel, r[2]);
            SelectByItem(viewModel, r[3]);
            viewModel.GroupCommand.Execute();
            var group = AllGroups(viewModel).Single();
            DeselectAll(viewModel);

            // Group already at the top, BringForward is a no-op.
            SelectByItem(viewModel, group);
            viewModel.BringForwardCommand.Execute();

            Assert.That(r[0].ZIndex.Value, Is.EqualTo(0));
            Assert.That(r[1].ZIndex.Value, Is.EqualTo(1));
            Assert.That(r[2].ZIndex.Value, Is.EqualTo(2));
            Assert.That(r[3].ZIndex.Value, Is.EqualTo(3));
            Assert.That(group.ZIndex.Value, Is.EqualTo(4));
        }

        [Test]
        public void Group_BringForeground()
        {
            var (viewModel, _) = CreateSingleLayerViewModel();
            var r = AddRectangles(viewModel, 6);

            SelectByItem(viewModel, r[0]);
            SelectByItem(viewModel, r[1]);
            SelectByItem(viewModel, r[2]);
            viewModel.GroupCommand.Execute();
            var group = AllGroups(viewModel).Single();
            DeselectAll(viewModel);

            // Group already at the top (group=6), BringForeground is a no-op.
            SelectByItem(viewModel, group);
            viewModel.BringForegroundCommand.Execute();

            Assert.That(r[0].ZIndex.Value, Is.EqualTo(0));
            Assert.That(r[1].ZIndex.Value, Is.EqualTo(1));
            Assert.That(r[2].ZIndex.Value, Is.EqualTo(2));
            Assert.That(r[3].ZIndex.Value, Is.EqualTo(3));
            Assert.That(r[4].ZIndex.Value, Is.EqualTo(4));
            Assert.That(r[5].ZIndex.Value, Is.EqualTo(5));
            Assert.That(group.ZIndex.Value, Is.EqualTo(6));
        }

        [Test]
        public void Group_SendBackward()
        {
            var (viewModel, _) = CreateSingleLayerViewModel();
            var r = AddRectangles(viewModel, 4);

            SelectByItem(viewModel, r[1]);
            SelectByItem(viewModel, r[2]);
            SelectByItem(viewModel, r[3]);
            viewModel.GroupCommand.Execute();
            var group = AllGroups(viewModel).Single();
            DeselectAll(viewModel);

            // Setup: r0=0, r1=1, r2=2, r3=3 (group's children), group=4.
            // Top-level: r0, group. SendBackward swaps group with r0;
            // children stay at 1, 2, 3.
            SelectByItem(viewModel, group);
            viewModel.SendBackwardCommand.Execute();

            Assert.That(group.ZIndex.Value, Is.EqualTo(0));
            Assert.That(r[1].ZIndex.Value, Is.EqualTo(1));
            Assert.That(r[2].ZIndex.Value, Is.EqualTo(2));
            Assert.That(r[3].ZIndex.Value, Is.EqualTo(3));
            Assert.That(r[0].ZIndex.Value, Is.EqualTo(4));
        }

        [Test]
        public void Group_SendBackward_2()
        {
            var (viewModel, _) = CreateSingleLayerViewModel();
            var r = AddRectangles(viewModel, 5);

            SelectByItem(viewModel, r[2]);
            SelectByItem(viewModel, r[3]);
            SelectByItem(viewModel, r[4]);
            viewModel.GroupCommand.Execute();
            var group = AllGroups(viewModel).Single();
            DeselectAll(viewModel);

            // Setup: r0=0, r1=1, r2=2, r3=3, r4=4 (group's children),
            // group=5. Top-level: r0, r1, group. SendBackward on group
            // swaps with r1.
            SelectByItem(viewModel, group);
            viewModel.SendBackwardCommand.Execute();

            Assert.That(r[0].ZIndex.Value, Is.EqualTo(0));
            Assert.That(group.ZIndex.Value, Is.EqualTo(1));
            Assert.That(r[2].ZIndex.Value, Is.EqualTo(2));
            Assert.That(r[3].ZIndex.Value, Is.EqualTo(3));
            Assert.That(r[4].ZIndex.Value, Is.EqualTo(4));
            Assert.That(r[1].ZIndex.Value, Is.EqualTo(5));
        }

        [Test]
        public void Group_SendBackward_OneStep()
        {
            var (viewModel, _) = CreateSingleLayerViewModel();
            var r = AddRectangles(viewModel, 4);

            SelectByItem(viewModel, r[0]);
            SelectByItem(viewModel, r[1]);
            SelectByItem(viewModel, r[2]);
            viewModel.GroupCommand.Execute();
            var group = AllGroups(viewModel).Single();
            DeselectAll(viewModel);

            // Setup: r0=0, r1=1, r2=2 (group's children), r3=3, group=4.
            // Top-level: r3, group. SendBackward swaps group with r3.
            SelectByItem(viewModel, group);
            viewModel.SendBackwardCommand.Execute();

            Assert.That(r[0].ZIndex.Value, Is.EqualTo(0));
            Assert.That(r[1].ZIndex.Value, Is.EqualTo(1));
            Assert.That(r[2].ZIndex.Value, Is.EqualTo(2));
            Assert.That(group.ZIndex.Value, Is.EqualTo(3));
            Assert.That(r[3].ZIndex.Value, Is.EqualTo(4));
        }

        [Test]
        public void Group_SendBackground()
        {
            var (viewModel, _) = CreateSingleLayerViewModel();
            var r = AddRectangles(viewModel, 6);

            SelectByItem(viewModel, r[3]);
            SelectByItem(viewModel, r[4]);
            SelectByItem(viewModel, r[5]);
            viewModel.GroupCommand.Execute();
            var group = AllGroups(viewModel).Single();
            DeselectAll(viewModel);

            // Setup: r0=0, r1=1, r2=2, r3=3, r4=4, r5=5 (group's children),
            // group=6. Top-level: r0, r1, r2, group. SendBackground walks
            // the group past r2, r1, r0 in turn — each gets bumped up by
            // exactly one slot.
            SelectByItem(viewModel, group);
            viewModel.SendBackgroundCommand.Execute();

            Assert.That(group.ZIndex.Value, Is.EqualTo(0));
            Assert.That(r[0].ZIndex.Value, Is.EqualTo(1));
            Assert.That(r[1].ZIndex.Value, Is.EqualTo(2));
            Assert.That(r[3].ZIndex.Value, Is.EqualTo(3));
            Assert.That(r[4].ZIndex.Value, Is.EqualTo(4));
            Assert.That(r[5].ZIndex.Value, Is.EqualTo(5));
            Assert.That(r[2].ZIndex.Value, Is.EqualTo(6));
        }
    }
}
