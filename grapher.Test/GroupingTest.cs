using grapher.ViewModels;
using NUnit.Framework;
using System.Linq;

namespace grapher.Test
{
    [TestFixture]
    public class GroupingTest
    {
        [Test]
        public void Group_Duplicate_Ungroup_Group()
        {
            var viewModel = new DiagramViewModel();
            var r0 = new NRectangleViewModel();
            var r1 = new NRectangleViewModel();
            var r2 = new NRectangleViewModel();

            r0.ZIndex.Value = 0;
            r1.ZIndex.Value = 1;
            r2.ZIndex.Value = 2;

            viewModel.Items.Add(r0);
            viewModel.Items.Add(r1);
            viewModel.Items.Add(r2);

            r0.IsSelected = true;
            r1.IsSelected = true;
            r2.IsSelected = true;

            viewModel.GroupCommand.Execute();

            var group = viewModel.Items.Last();
            Assert.That(group, Is.TypeOf<GroupItemViewModel>());
            Assert.That(group.ZIndex.Value, Is.EqualTo(3));

            group.IsSelected = true;

            viewModel.DuplicateCommand.Execute();

            var secondGroup = viewModel.Items.Last();
            Assert.That(secondGroup, Is.TypeOf<GroupItemViewModel>());
            Assert.That(secondGroup.ZIndex.Value, Is.EqualTo(7));

            var secondGroupMembers = (from item in viewModel.Items
                                      where item.ParentID == secondGroup.ID
                                      select item).ToList();
            Assert.That(secondGroupMembers, Has.Count.EqualTo(3));
            Assert.That(secondGroupMembers.ElementAt(0).ZIndex.Value, Is.EqualTo(4));
            Assert.That(secondGroupMembers.ElementAt(1).ZIndex.Value, Is.EqualTo(5));
            Assert.That(secondGroupMembers.ElementAt(2).ZIndex.Value, Is.EqualTo(6));

            group.IsSelected = false;
            secondGroup.IsSelected = true;

            viewModel.UngroupCommand.Execute();

            Assert.That(viewModel.Items, Has.Count.EqualTo(7));
            Assert.That(viewModel.Items.ElementAt(4).ZIndex.Value, Is.EqualTo(4));
            Assert.That(viewModel.Items.ElementAt(5).ZIndex.Value, Is.EqualTo(5));
            Assert.That(viewModel.Items.ElementAt(6).ZIndex.Value, Is.EqualTo(6));

            secondGroupMembers.ForEach(x => x.IsSelected = true);

            viewModel.GroupCommand.Execute();

            secondGroup = viewModel.Items.Last();
            Assert.That(secondGroup, Is.TypeOf<GroupItemViewModel>());

            Assert.That(r0.ZIndex.Value, Is.EqualTo(0));
            Assert.That(r1.ZIndex.Value, Is.EqualTo(1));
            Assert.That(r2.ZIndex.Value, Is.EqualTo(2));
            Assert.That(group.ZIndex.Value, Is.EqualTo(3));

            Assert.That(secondGroupMembers.ElementAt(0).ZIndex.Value, Is.EqualTo(4));
            Assert.That(secondGroupMembers.ElementAt(1).ZIndex.Value, Is.EqualTo(5));
            Assert.That(secondGroupMembers.ElementAt(2).ZIndex.Value, Is.EqualTo(6));
            Assert.That(secondGroup.ZIndex.Value, Is.EqualTo(7));
        }
    }
}
