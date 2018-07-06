using grapher.ViewModels;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace grapher.Test
{
    [TestFixture]
    public class OrderingTest
    {
        [Test]
        public void Group()
        {
            var viewModel = new DiagramViewModel();
            var r1 = new NRectangleViewModel();
            var r2 = new NRectangleViewModel();
            var r3 = new NRectangleViewModel();
            var r4 = new NRectangleViewModel();

            r1.ZIndex.Value = 0;
            r2.ZIndex.Value = 1;
            r3.ZIndex.Value = 2;
            r4.ZIndex.Value = 3;

            viewModel.Items.Add(r1);
            viewModel.Items.Add(r2);
            viewModel.Items.Add(r3);
            viewModel.Items.Add(r4);

            viewModel.SelectedItems.AddRangeOnScheduler(r1, r2, r3);

            viewModel.GroupCommand.Execute();

            Assert.That(r1.ZIndex.Value, Is.EqualTo(0));
            Assert.That(r2.ZIndex.Value, Is.EqualTo(1));
            Assert.That(r3.ZIndex.Value, Is.EqualTo(2));
            Assert.That(viewModel.Items.ElementAt(4).ZIndex.Value, Is.EqualTo(3));
            Assert.That(r4.ZIndex.Value, Is.EqualTo(4));
        }
        [Test]
        public void Group_Discontinuous()
        {
            var viewModel = new DiagramViewModel();
            var r1 = new NRectangleViewModel();
            var r2 = new NRectangleViewModel();
            var r3 = new NRectangleViewModel();
            var r4 = new NRectangleViewModel();
            var r5 = new NRectangleViewModel();

            r1.ZIndex.Value = 0;
            r2.ZIndex.Value = 1;
            r3.ZIndex.Value = 2;
            r4.ZIndex.Value = 3;
            r5.ZIndex.Value = 4;

            viewModel.Items.Add(r1);
            viewModel.Items.Add(r2);
            viewModel.Items.Add(r3);
            viewModel.Items.Add(r4);
            viewModel.Items.Add(r5);

            viewModel.SelectedItems.AddRangeOnScheduler(r1, r2, r4);

            viewModel.GroupCommand.Execute();

            Assert.That(r1.ZIndex.Value, Is.EqualTo(1));
            Assert.That(r2.ZIndex.Value, Is.EqualTo(2));
            Assert.That(r3.ZIndex.Value, Is.EqualTo(0));
            Assert.That(r4.ZIndex.Value, Is.EqualTo(3));
            Assert.That(viewModel.Items.ElementAt(5).ZIndex.Value, Is.EqualTo(4));
            Assert.That(r5.ZIndex.Value, Is.EqualTo(5));
        }

        [Test]
        public void Ungroup()
        {
            var viewModel = new DiagramViewModel();
            var r1 = new NRectangleViewModel();
            var r2 = new NRectangleViewModel();
            var r3 = new NRectangleViewModel();
            var r4 = new NRectangleViewModel();
            var r5 = new NRectangleViewModel();

            r1.ZIndex.Value = 0;
            r2.ZIndex.Value = 1;
            r3.ZIndex.Value = 2;
            r4.ZIndex.Value = 3;
            r5.ZIndex.Value = 4;

            viewModel.Items.Add(r1);
            viewModel.Items.Add(r2);
            viewModel.Items.Add(r3);
            viewModel.Items.Add(r4);
            viewModel.Items.Add(r5);

            viewModel.SelectedItems.AddRangeOnScheduler(r1, r2, r4);

            viewModel.GroupCommand.Execute();

            viewModel.UngroupCommand.Execute();

            Assert.That(r1.ZIndex.Value, Is.EqualTo(1));
            Assert.That(r2.ZIndex.Value, Is.EqualTo(2));
            Assert.That(r3.ZIndex.Value, Is.EqualTo(0));
            Assert.That(r4.ZIndex.Value, Is.EqualTo(3));
            Assert.That(r5.ZIndex.Value, Is.EqualTo(4));
        }

        [Test]
        public void Group_BringForward()
        {
            var viewModel = new DiagramViewModel();
            var r1 = new NRectangleViewModel();
            var r2 = new NRectangleViewModel();
            var r3 = new NRectangleViewModel();
            var r4 = new NRectangleViewModel();

            r1.ZIndex.Value = 0;
            r2.ZIndex.Value = 1;
            r3.ZIndex.Value = 2;
            r4.ZIndex.Value = 3;

            viewModel.Items.Add(r1);
            viewModel.Items.Add(r2);
            viewModel.Items.Add(r3);
            viewModel.Items.Add(r4);

            viewModel.SelectedItems.AddRangeOnScheduler(r1, r2, r3);

            viewModel.GroupCommand.Execute();

            viewModel.SelectedItems.Clear();
            viewModel.SelectedItems.Add(viewModel.Items.OfType<GroupItemViewModel>().Single());

            viewModel.BringForwardCommand.Execute();

            Assert.That(viewModel.Items.ElementAt(0).ZIndex.Value, Is.EqualTo(1));
            Assert.That(viewModel.Items.ElementAt(1).ZIndex.Value, Is.EqualTo(2));
            Assert.That(viewModel.Items.ElementAt(2).ZIndex.Value, Is.EqualTo(3));
            Assert.That(viewModel.Items.ElementAt(3).ZIndex.Value, Is.EqualTo(0));
            Assert.That(viewModel.Items.ElementAt(4).ZIndex.Value, Is.EqualTo(4));
        }

        [Test]
        public void Group_SendBackward()
        {
            var viewModel = new DiagramViewModel();
            var r1 = new NRectangleViewModel() { Name = "r1" };
            var r2 = new NRectangleViewModel() { Name = "r2" };
            var r3 = new NRectangleViewModel() { Name = "r3" };
            var r4 = new NRectangleViewModel() { Name = "r4" };

            r1.ZIndex.Value = 0;
            r2.ZIndex.Value = 1;
            r3.ZIndex.Value = 2;
            r4.ZIndex.Value = 3;

            viewModel.Items.Add(r1);
            viewModel.Items.Add(r2);
            viewModel.Items.Add(r3);
            viewModel.Items.Add(r4);

            viewModel.SelectedItems.AddRangeOnScheduler(r2, r3, r4);

            viewModel.GroupCommand.Execute();

            viewModel.SelectedItems.Clear();
            viewModel.SelectedItems.Add(viewModel.Items.OfType<GroupItemViewModel>().Single());

            viewModel.SendBackwardCommand.Execute();

            Assert.That(viewModel.Items.ElementAt(0).ZIndex.Value, Is.EqualTo(4));
            Assert.That(viewModel.Items.ElementAt(1).ZIndex.Value, Is.EqualTo(0));
            Assert.That(viewModel.Items.ElementAt(2).ZIndex.Value, Is.EqualTo(1));
            Assert.That(viewModel.Items.ElementAt(3).ZIndex.Value, Is.EqualTo(2));
            Assert.That(viewModel.Items.ElementAt(4).ZIndex.Value, Is.EqualTo(3));
        }
    }
}
