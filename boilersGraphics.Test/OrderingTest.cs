using boilersGraphics.ViewModels;
using NUnit.Framework;
using System.Linq;

namespace boilersGraphics.Test
{
    [TestFixture]
    public class OrderingTest
    {
        [Test]
        public void BringForward()
        {
            var viewModel = new DiagramViewModel();
            var r0 = new NRectangleViewModel();
            var r1 = new NRectangleViewModel();
            var r2 = new NRectangleViewModel();
            var r3 = new NRectangleViewModel();
            var r4 = new NRectangleViewModel();

            r0.ZIndex.Value = 0;
            r1.ZIndex.Value = 1;
            r2.ZIndex.Value = 2;
            r3.ZIndex.Value = 3;
            r4.ZIndex.Value = 4;

            viewModel.Items.Add(r0);
            viewModel.Items.Add(r1);
            viewModel.Items.Add(r2);
            viewModel.Items.Add(r3);
            viewModel.Items.Add(r4);

            viewModel.SelectedItems.AddRangeOnScheduler(r2);

            viewModel.BringForwardCommand.Execute();

            Assert.That(r0.ZIndex.Value, Is.EqualTo(0));
            Assert.That(r1.ZIndex.Value, Is.EqualTo(1));
            Assert.That(r2.ZIndex.Value, Is.EqualTo(3));
            Assert.That(r3.ZIndex.Value, Is.EqualTo(2));
            Assert.That(r4.ZIndex.Value, Is.EqualTo(4));
        }

        [Test]
        public void BringForward_GroupIncluded()
        {
            var viewModel = new DiagramViewModel();
            var r0 = new NRectangleViewModel();
            var r1 = new NRectangleViewModel();
            var r2 = new NRectangleViewModel();
            var r3 = new NRectangleViewModel();
            var r4 = new NRectangleViewModel();
            var r5 = new NRectangleViewModel();
            var r6 = new NRectangleViewModel();

            r0.ZIndex.Value = 0;
            r1.ZIndex.Value = 1; //group
            r2.ZIndex.Value = 2; //group
            r3.ZIndex.Value = 3; //target
            r4.ZIndex.Value = 4; //group
            r5.ZIndex.Value = 5; //group
            r6.ZIndex.Value = 6;

            viewModel.Items.Add(r0);
            viewModel.Items.Add(r1);
            viewModel.Items.Add(r2);
            viewModel.Items.Add(r3);
            viewModel.Items.Add(r4);
            viewModel.Items.Add(r5);
            viewModel.Items.Add(r6);

            viewModel.SelectedItems.AddRangeOnScheduler(r1, r2);

            viewModel.GroupCommand.Execute();

            viewModel.SelectedItems.Clear();
            viewModel.SelectedItems.AddRangeOnScheduler(r4, r5);

            viewModel.GroupCommand.Execute();

            Assert.That(r0.ZIndex.Value, Is.EqualTo(0));
            Assert.That(r1.ZIndex.Value, Is.EqualTo(1)); //group
            Assert.That(r2.ZIndex.Value, Is.EqualTo(2)); //group
            Assert.That(viewModel.Items.ElementAt(7).ZIndex.Value, Is.EqualTo(3));
            Assert.That(r3.ZIndex.Value, Is.EqualTo(4)); //target
            Assert.That(r4.ZIndex.Value, Is.EqualTo(5)); //group
            Assert.That(r5.ZIndex.Value, Is.EqualTo(6)); //group
            Assert.That(viewModel.Items.ElementAt(8).ZIndex.Value, Is.EqualTo(7));
            Assert.That(r6.ZIndex.Value, Is.EqualTo(8));

            viewModel.SelectedItems.Clear();
            viewModel.SelectedItems.Add(r3);

            viewModel.BringForwardCommand.Execute();

            Assert.That(r0.ZIndex.Value, Is.EqualTo(0));
            Assert.That(r1.ZIndex.Value, Is.EqualTo(1)); //group
            Assert.That(r2.ZIndex.Value, Is.EqualTo(2)); //group
            Assert.That(viewModel.Items.ElementAt(7).ZIndex.Value, Is.EqualTo(3));
            Assert.That(r3.ZIndex.Value, Is.EqualTo(7)); //target
            Assert.That(r4.ZIndex.Value, Is.EqualTo(4)); //group
            Assert.That(r5.ZIndex.Value, Is.EqualTo(5)); //group
            Assert.That(viewModel.Items.ElementAt(8).ZIndex.Value, Is.EqualTo(6));
            Assert.That(r6.ZIndex.Value, Is.EqualTo(8));
        }

        [Test]
        public void BringForward_NoEffect()
        {
            var viewModel = new DiagramViewModel();
            var r0 = new NRectangleViewModel();
            var r1 = new NRectangleViewModel();
            var r2 = new NRectangleViewModel();
            var r3 = new NRectangleViewModel();
            var r4 = new NRectangleViewModel();

            r0.ZIndex.Value = 0;
            r1.ZIndex.Value = 1;
            r2.ZIndex.Value = 2;
            r3.ZIndex.Value = 3;
            r4.ZIndex.Value = 4;

            viewModel.Items.Add(r0);
            viewModel.Items.Add(r1);
            viewModel.Items.Add(r2);
            viewModel.Items.Add(r3);
            viewModel.Items.Add(r4);

            viewModel.SelectedItems.AddRangeOnScheduler(r4);

            viewModel.BringForwardCommand.Execute();

            Assert.That(r0.ZIndex.Value, Is.EqualTo(0));
            Assert.That(r1.ZIndex.Value, Is.EqualTo(1));
            Assert.That(r2.ZIndex.Value, Is.EqualTo(2));
            Assert.That(r3.ZIndex.Value, Is.EqualTo(3));
            Assert.That(r4.ZIndex.Value, Is.EqualTo(4));
        }

        [Test]
        public void BringForeground()
        {
            var viewModel = new DiagramViewModel();
            var r0 = new NRectangleViewModel();
            var r1 = new NRectangleViewModel();
            var r2 = new NRectangleViewModel();
            var r3 = new NRectangleViewModel();
            var r4 = new NRectangleViewModel();

            r0.ZIndex.Value = 0;
            r1.ZIndex.Value = 1;
            r2.ZIndex.Value = 2;
            r3.ZIndex.Value = 3;
            r4.ZIndex.Value = 4;

            viewModel.Items.Add(r0);
            viewModel.Items.Add(r1);
            viewModel.Items.Add(r2);
            viewModel.Items.Add(r3);
            viewModel.Items.Add(r4);

            viewModel.SelectedItems.AddRangeOnScheduler(r2);

            viewModel.BringForegroundCommand.Execute();

            Assert.That(r0.ZIndex.Value, Is.EqualTo(0));
            Assert.That(r1.ZIndex.Value, Is.EqualTo(1));
            Assert.That(r2.ZIndex.Value, Is.EqualTo(4));
            Assert.That(r3.ZIndex.Value, Is.EqualTo(2));
            Assert.That(r4.ZIndex.Value, Is.EqualTo(3));
        }

        [Test]
        public void BringForeground_GroupIncluded()
        {
            var viewModel = new DiagramViewModel();
            var r0 = new NRectangleViewModel();
            var r1 = new NRectangleViewModel();
            var r2 = new NRectangleViewModel();
            var r3 = new NRectangleViewModel();
            var r4 = new NRectangleViewModel();
            var r5 = new NRectangleViewModel();
            var r6 = new NRectangleViewModel();

            r0.ZIndex.Value = 0;
            r1.ZIndex.Value = 1; //group
            r2.ZIndex.Value = 2; //group
            r3.ZIndex.Value = 3; //target
            r4.ZIndex.Value = 4; //group
            r5.ZIndex.Value = 5; //group
            r6.ZIndex.Value = 6;

            viewModel.Items.Add(r0);
            viewModel.Items.Add(r1);
            viewModel.Items.Add(r2);
            viewModel.Items.Add(r3);
            viewModel.Items.Add(r4);
            viewModel.Items.Add(r5);
            viewModel.Items.Add(r6);

            viewModel.SelectedItems.AddRangeOnScheduler(r1, r2);

            viewModel.GroupCommand.Execute();

            viewModel.SelectedItems.Clear();
            viewModel.SelectedItems.AddRangeOnScheduler(r4, r5);

            viewModel.GroupCommand.Execute();

            Assert.That(r0.ZIndex.Value, Is.EqualTo(0));
            Assert.That(r1.ZIndex.Value, Is.EqualTo(1)); //group
            Assert.That(r2.ZIndex.Value, Is.EqualTo(2)); //group
            Assert.That(viewModel.Items.ElementAt(7).ZIndex.Value, Is.EqualTo(3));
            Assert.That(r3.ZIndex.Value, Is.EqualTo(4)); //target
            Assert.That(r4.ZIndex.Value, Is.EqualTo(5)); //group
            Assert.That(r5.ZIndex.Value, Is.EqualTo(6)); //group
            Assert.That(viewModel.Items.ElementAt(8).ZIndex.Value, Is.EqualTo(7));
            Assert.That(r6.ZIndex.Value, Is.EqualTo(8));

            viewModel.SelectedItems.Clear();
            viewModel.SelectedItems.Add(r3);

            viewModel.BringForegroundCommand.Execute();

            Assert.That(r0.ZIndex.Value, Is.EqualTo(0));
            Assert.That(r1.ZIndex.Value, Is.EqualTo(1)); //group
            Assert.That(r2.ZIndex.Value, Is.EqualTo(2)); //group
            Assert.That(viewModel.Items.ElementAt(7).ZIndex.Value, Is.EqualTo(3));
            Assert.That(r3.ZIndex.Value, Is.EqualTo(8)); //target
            Assert.That(r4.ZIndex.Value, Is.EqualTo(4)); //group
            Assert.That(r5.ZIndex.Value, Is.EqualTo(5)); //group
            Assert.That(viewModel.Items.ElementAt(8).ZIndex.Value, Is.EqualTo(6));
            Assert.That(r6.ZIndex.Value, Is.EqualTo(7));
        }

        [Test]
        public void BringForeground_NoEffect()
        {
            var viewModel = new DiagramViewModel();
            var r0 = new NRectangleViewModel();
            var r1 = new NRectangleViewModel();
            var r2 = new NRectangleViewModel();
            var r3 = new NRectangleViewModel();
            var r4 = new NRectangleViewModel();

            r0.ZIndex.Value = 0;
            r1.ZIndex.Value = 1;
            r2.ZIndex.Value = 2;
            r3.ZIndex.Value = 3;
            r4.ZIndex.Value = 4;

            viewModel.Items.Add(r0);
            viewModel.Items.Add(r1);
            viewModel.Items.Add(r2);
            viewModel.Items.Add(r3);
            viewModel.Items.Add(r4);

            viewModel.SelectedItems.AddRangeOnScheduler(r4);

            viewModel.BringForegroundCommand.Execute();

            Assert.That(r0.ZIndex.Value, Is.EqualTo(0));
            Assert.That(r1.ZIndex.Value, Is.EqualTo(1));
            Assert.That(r2.ZIndex.Value, Is.EqualTo(2));
            Assert.That(r3.ZIndex.Value, Is.EqualTo(3));
            Assert.That(r4.ZIndex.Value, Is.EqualTo(4));
        }

        [Test]
        public void SendBackward()
        {
            var viewModel = new DiagramViewModel();
            var r0 = new NRectangleViewModel();
            var r1 = new NRectangleViewModel();
            var r2 = new NRectangleViewModel();
            var r3 = new NRectangleViewModel();
            var r4 = new NRectangleViewModel();

            r0.ZIndex.Value = 0;
            r1.ZIndex.Value = 1;
            r2.ZIndex.Value = 2;
            r3.ZIndex.Value = 3;
            r4.ZIndex.Value = 4;

            viewModel.Items.Add(r0);
            viewModel.Items.Add(r1);
            viewModel.Items.Add(r2);
            viewModel.Items.Add(r3);
            viewModel.Items.Add(r4);

            viewModel.SelectedItems.AddRangeOnScheduler(r2);

            viewModel.SendBackwardCommand.Execute();

            Assert.That(r0.ZIndex.Value, Is.EqualTo(0));
            Assert.That(r1.ZIndex.Value, Is.EqualTo(2));
            Assert.That(r2.ZIndex.Value, Is.EqualTo(1));
            Assert.That(r3.ZIndex.Value, Is.EqualTo(3));
            Assert.That(r4.ZIndex.Value, Is.EqualTo(4));
        }

        [Test]
        public void SendBackward_GroupIncluded()
        {
            var viewModel = new DiagramViewModel();
            var r0 = new NRectangleViewModel();
            var r1 = new NRectangleViewModel();
            var r2 = new NRectangleViewModel();
            var r3 = new NRectangleViewModel();
            var r4 = new NRectangleViewModel();
            var r5 = new NRectangleViewModel();
            var r6 = new NRectangleViewModel();

            r0.ZIndex.Value = 0;
            r1.ZIndex.Value = 1; //group
            r2.ZIndex.Value = 2; //group
            r3.ZIndex.Value = 3; //target
            r4.ZIndex.Value = 4; //group
            r5.ZIndex.Value = 5; //group
            r6.ZIndex.Value = 6;

            viewModel.Items.Add(r0);
            viewModel.Items.Add(r1);
            viewModel.Items.Add(r2);
            viewModel.Items.Add(r3);
            viewModel.Items.Add(r4);
            viewModel.Items.Add(r5);
            viewModel.Items.Add(r6);

            viewModel.SelectedItems.AddRangeOnScheduler(r1, r2);

            viewModel.GroupCommand.Execute();

            viewModel.SelectedItems.Clear();
            viewModel.SelectedItems.AddRangeOnScheduler(r4, r5);

            viewModel.GroupCommand.Execute();

            Assert.That(r0.ZIndex.Value, Is.EqualTo(0));
            Assert.That(r1.ZIndex.Value, Is.EqualTo(1)); //group
            Assert.That(r2.ZIndex.Value, Is.EqualTo(2)); //group
            Assert.That(viewModel.Items.ElementAt(7).ZIndex.Value, Is.EqualTo(3));
            Assert.That(r3.ZIndex.Value, Is.EqualTo(4)); //target
            Assert.That(r4.ZIndex.Value, Is.EqualTo(5)); //group
            Assert.That(r5.ZIndex.Value, Is.EqualTo(6)); //group
            Assert.That(viewModel.Items.ElementAt(8).ZIndex.Value, Is.EqualTo(7));
            Assert.That(r6.ZIndex.Value, Is.EqualTo(8));

            viewModel.SelectedItems.Clear();
            viewModel.SelectedItems.Add(r3);

            viewModel.SendBackwardCommand.Execute();

            Assert.That(r0.ZIndex.Value, Is.EqualTo(0));
            Assert.That(r1.ZIndex.Value, Is.EqualTo(2)); //group
            Assert.That(r2.ZIndex.Value, Is.EqualTo(3)); //group
            Assert.That(viewModel.Items.ElementAt(7).ZIndex.Value, Is.EqualTo(4));
            Assert.That(r3.ZIndex.Value, Is.EqualTo(1)); //target
            Assert.That(r4.ZIndex.Value, Is.EqualTo(5)); //group
            Assert.That(r5.ZIndex.Value, Is.EqualTo(6)); //group
            Assert.That(viewModel.Items.ElementAt(8).ZIndex.Value, Is.EqualTo(7));
            Assert.That(r6.ZIndex.Value, Is.EqualTo(8));
        }

        [Test]
        public void SendBackward_NoEffect()
        {
            var viewModel = new DiagramViewModel();
            var r0 = new NRectangleViewModel();
            var r1 = new NRectangleViewModel();
            var r2 = new NRectangleViewModel();
            var r3 = new NRectangleViewModel();
            var r4 = new NRectangleViewModel();

            r0.ZIndex.Value = 0;
            r1.ZIndex.Value = 1;
            r2.ZIndex.Value = 2;
            r3.ZIndex.Value = 3;
            r4.ZIndex.Value = 4;

            viewModel.Items.Add(r0);
            viewModel.Items.Add(r1);
            viewModel.Items.Add(r2);
            viewModel.Items.Add(r3);
            viewModel.Items.Add(r4);

            viewModel.SelectedItems.AddRangeOnScheduler(r0);

            viewModel.SendBackwardCommand.Execute();

            Assert.That(r0.ZIndex.Value, Is.EqualTo(0));
            Assert.That(r1.ZIndex.Value, Is.EqualTo(1));
            Assert.That(r2.ZIndex.Value, Is.EqualTo(2));
            Assert.That(r3.ZIndex.Value, Is.EqualTo(3));
            Assert.That(r4.ZIndex.Value, Is.EqualTo(4));
        }

        [Test]
        public void SendBackground()
        {
            var viewModel = new DiagramViewModel();
            var r0 = new NRectangleViewModel();
            var r1 = new NRectangleViewModel();
            var r2 = new NRectangleViewModel();
            var r3 = new NRectangleViewModel();
            var r4 = new NRectangleViewModel();

            r0.ZIndex.Value = 0;
            r1.ZIndex.Value = 1;
            r2.ZIndex.Value = 2;
            r3.ZIndex.Value = 3;
            r4.ZIndex.Value = 4;

            viewModel.Items.Add(r0);
            viewModel.Items.Add(r1);
            viewModel.Items.Add(r2);
            viewModel.Items.Add(r3);
            viewModel.Items.Add(r4);

            viewModel.SelectedItems.AddRangeOnScheduler(r2);

            viewModel.SendBackgroundCommand.Execute();

            Assert.That(r0.ZIndex.Value, Is.EqualTo(1));
            Assert.That(r1.ZIndex.Value, Is.EqualTo(2));
            Assert.That(r2.ZIndex.Value, Is.EqualTo(0));
            Assert.That(r3.ZIndex.Value, Is.EqualTo(3));
            Assert.That(r4.ZIndex.Value, Is.EqualTo(4));
        }

        [Test]
        public void SendBackgroud_GroupIncluded()
        {
            var viewModel = new DiagramViewModel();
            var r0 = new NRectangleViewModel();
            var r1 = new NRectangleViewModel();
            var r2 = new NRectangleViewModel();
            var r3 = new NRectangleViewModel();
            var r4 = new NRectangleViewModel();
            var r5 = new NRectangleViewModel();
            var r6 = new NRectangleViewModel();

            r0.ZIndex.Value = 0;
            r1.ZIndex.Value = 1; //group
            r2.ZIndex.Value = 2; //group
            r3.ZIndex.Value = 3; //target
            r4.ZIndex.Value = 4; //group
            r5.ZIndex.Value = 5; //group
            r6.ZIndex.Value = 6;

            viewModel.Items.Add(r0);
            viewModel.Items.Add(r1);
            viewModel.Items.Add(r2);
            viewModel.Items.Add(r3);
            viewModel.Items.Add(r4);
            viewModel.Items.Add(r5);
            viewModel.Items.Add(r6);

            viewModel.SelectedItems.AddRangeOnScheduler(r1, r2);

            viewModel.GroupCommand.Execute();

            viewModel.SelectedItems.Clear();
            viewModel.SelectedItems.AddRangeOnScheduler(r4, r5);

            viewModel.GroupCommand.Execute();

            Assert.That(r0.ZIndex.Value, Is.EqualTo(0));
            Assert.That(r1.ZIndex.Value, Is.EqualTo(1)); //group
            Assert.That(r2.ZIndex.Value, Is.EqualTo(2)); //group
            Assert.That(viewModel.Items.ElementAt(7).ZIndex.Value, Is.EqualTo(3));
            Assert.That(r3.ZIndex.Value, Is.EqualTo(4)); //target
            Assert.That(r4.ZIndex.Value, Is.EqualTo(5)); //group
            Assert.That(r5.ZIndex.Value, Is.EqualTo(6)); //group
            Assert.That(viewModel.Items.ElementAt(8).ZIndex.Value, Is.EqualTo(7));
            Assert.That(r6.ZIndex.Value, Is.EqualTo(8));

            viewModel.SelectedItems.Clear();
            viewModel.SelectedItems.Add(r3);

            viewModel.SendBackgroundCommand.Execute();

            Assert.That(r0.ZIndex.Value, Is.EqualTo(1));
            Assert.That(r1.ZIndex.Value, Is.EqualTo(2)); //group
            Assert.That(r2.ZIndex.Value, Is.EqualTo(3)); //group
            Assert.That(viewModel.Items.ElementAt(7).ZIndex.Value, Is.EqualTo(4));
            Assert.That(r3.ZIndex.Value, Is.EqualTo(0)); //target
            Assert.That(r4.ZIndex.Value, Is.EqualTo(5)); //group
            Assert.That(r5.ZIndex.Value, Is.EqualTo(6)); //group
            Assert.That(viewModel.Items.ElementAt(8).ZIndex.Value, Is.EqualTo(7));
            Assert.That(r6.ZIndex.Value, Is.EqualTo(8));
        }

        [Test]
        public void SendBackground_NoEffect()
        {
            var viewModel = new DiagramViewModel();
            var r0 = new NRectangleViewModel();
            var r1 = new NRectangleViewModel();
            var r2 = new NRectangleViewModel();
            var r3 = new NRectangleViewModel();
            var r4 = new NRectangleViewModel();

            r0.ZIndex.Value = 0;
            r1.ZIndex.Value = 1;
            r2.ZIndex.Value = 2;
            r3.ZIndex.Value = 3;
            r4.ZIndex.Value = 4;

            viewModel.Items.Add(r0);
            viewModel.Items.Add(r1);
            viewModel.Items.Add(r2);
            viewModel.Items.Add(r3);
            viewModel.Items.Add(r4);

            viewModel.SelectedItems.AddRangeOnScheduler(r0);

            viewModel.SendBackgroundCommand.Execute();

            Assert.That(r0.ZIndex.Value, Is.EqualTo(0));
            Assert.That(r1.ZIndex.Value, Is.EqualTo(1));
            Assert.That(r2.ZIndex.Value, Is.EqualTo(2));
            Assert.That(r3.ZIndex.Value, Is.EqualTo(3));
            Assert.That(r4.ZIndex.Value, Is.EqualTo(4));
        }

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
        public void Group_2()
        {
            var viewModel = new DiagramViewModel();
            var r0 = new NRectangleViewModel() { Name = "r0" };
            var r1 = new NRectangleViewModel() { Name = "r1" };
            var r2 = new NRectangleViewModel() { Name = "r2" }; //group
            var r3 = new NRectangleViewModel() { Name = "r3" }; //group
            var r4 = new NRectangleViewModel() { Name = "r4" }; //group
            var r5 = new NRectangleViewModel() { Name = "r5" };
            var r6 = new NRectangleViewModel() { Name = "r6" };

            r0.ZIndex.Value = 0;
            r1.ZIndex.Value = 1;
            r2.ZIndex.Value = 2; //group
            r3.ZIndex.Value = 3; //group
            r4.ZIndex.Value = 4; //group
            r5.ZIndex.Value = 5;
            r6.ZIndex.Value = 6;

            viewModel.Items.Add(r0);
            viewModel.Items.Add(r1);
            viewModel.Items.Add(r2); //group
            viewModel.Items.Add(r3); //group
            viewModel.Items.Add(r4); //group
            viewModel.Items.Add(r5);
            viewModel.Items.Add(r6);

            viewModel.SelectedItems.AddRangeOnScheduler(r2, r3, r4);

            viewModel.GroupCommand.Execute();

            Assert.That(r0.ZIndex.Value, Is.EqualTo(0));
            Assert.That(r1.ZIndex.Value, Is.EqualTo(1));
            Assert.That(r2.ZIndex.Value, Is.EqualTo(2));
            Assert.That(r3.ZIndex.Value, Is.EqualTo(3));
            Assert.That(r4.ZIndex.Value, Is.EqualTo(4));
            Assert.That(viewModel.Items.ElementAt(7).ZIndex.Value, Is.EqualTo(5));
            Assert.That(r5.ZIndex.Value, Is.EqualTo(6));
            Assert.That(r6.ZIndex.Value, Is.EqualTo(7));
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
        public void Group_BringForward_2()
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

            viewModel.SelectedItems.AddRangeOnScheduler(r1, r2, r3);

            viewModel.GroupCommand.Execute();

            viewModel.SelectedItems.Clear();
            viewModel.SelectedItems.Add(viewModel.Items.OfType<GroupItemViewModel>().Single());

            viewModel.BringForwardCommand.Execute();

            Assert.That(viewModel.Items.ElementAt(0).ZIndex.Value, Is.EqualTo(1));
            Assert.That(viewModel.Items.ElementAt(1).ZIndex.Value, Is.EqualTo(2));
            Assert.That(viewModel.Items.ElementAt(2).ZIndex.Value, Is.EqualTo(3));
            Assert.That(viewModel.Items.ElementAt(3).ZIndex.Value, Is.EqualTo(0));
            Assert.That(viewModel.Items.ElementAt(4).ZIndex.Value, Is.EqualTo(5));
            Assert.That(viewModel.Items.ElementAt(5).ZIndex.Value, Is.EqualTo(4));
        }

        [Test]
        public void Group_BringForward_NoEffect()
        {
            var viewModel = new DiagramViewModel();
            var r1 = new NRectangleViewModel();
            var r2 = new NRectangleViewModel();
            var r3 = new NRectangleViewModel();
            var r4 = new NRectangleViewModel();

            r1.ZIndex.Value = 0;
            r2.ZIndex.Value = 1; //group
            r3.ZIndex.Value = 2; //group
            r4.ZIndex.Value = 3; //group

            viewModel.Items.Add(r1);
            viewModel.Items.Add(r2);
            viewModel.Items.Add(r3);
            viewModel.Items.Add(r4);

            viewModel.SelectedItems.AddRangeOnScheduler(r2, r3, r4);

            viewModel.GroupCommand.Execute();

            viewModel.SelectedItems.Clear();
            viewModel.SelectedItems.Add(viewModel.Items.OfType<GroupItemViewModel>().Single());

            viewModel.BringForwardCommand.Execute();

            Assert.That(viewModel.Items.ElementAt(0).ZIndex.Value, Is.EqualTo(0));
            Assert.That(viewModel.Items.ElementAt(1).ZIndex.Value, Is.EqualTo(1));
            Assert.That(viewModel.Items.ElementAt(2).ZIndex.Value, Is.EqualTo(2));
            Assert.That(viewModel.Items.ElementAt(3).ZIndex.Value, Is.EqualTo(3));
            Assert.That(viewModel.Items.ElementAt(4).ZIndex.Value, Is.EqualTo(4));
        }

        [Test]
        public void Group_BringForeground()
        {
            var viewModel = new DiagramViewModel();
            var r0 = new NRectangleViewModel() { Name = "r0" }; //group
            var r1 = new NRectangleViewModel() { Name = "r1" }; //group
            var r2 = new NRectangleViewModel() { Name = "r2" }; //group
            var r3 = new NRectangleViewModel() { Name = "r3" };
            var r4 = new NRectangleViewModel() { Name = "r4" };
            var r5 = new NRectangleViewModel() { Name = "r5" };

            r0.ZIndex.Value = 0; //group
            r1.ZIndex.Value = 1; //group
            r2.ZIndex.Value = 2; //group
            r3.ZIndex.Value = 3;
            r4.ZIndex.Value = 4;
            r5.ZIndex.Value = 5;

            viewModel.Items.Add(r0); //group
            viewModel.Items.Add(r1); //group
            viewModel.Items.Add(r2); //group
            viewModel.Items.Add(r3);
            viewModel.Items.Add(r4);
            viewModel.Items.Add(r5);

            viewModel.SelectedItems.AddRangeOnScheduler(r0, r1, r2);

            viewModel.GroupCommand.Execute();

            Assert.That(r0.ZIndex.Value, Is.EqualTo(0));
            Assert.That(r1.ZIndex.Value, Is.EqualTo(1));
            Assert.That(r2.ZIndex.Value, Is.EqualTo(2));
            Assert.That(viewModel.Items.ElementAt(6).ZIndex.Value, Is.EqualTo(3));
            Assert.That(r3.ZIndex.Value, Is.EqualTo(4));
            Assert.That(r4.ZIndex.Value, Is.EqualTo(5));
            Assert.That(r5.ZIndex.Value, Is.EqualTo(6));

            viewModel.SelectedItems.Clear();
            viewModel.SelectedItems.Add(viewModel.Items.OfType<GroupItemViewModel>().Single());

            viewModel.BringForegroundCommand.Execute();

            Assert.That(r0.ZIndex.Value, Is.EqualTo(3));
            Assert.That(r1.ZIndex.Value, Is.EqualTo(4));
            Assert.That(r2.ZIndex.Value, Is.EqualTo(5));
            Assert.That(viewModel.Items.ElementAt(6).ZIndex.Value, Is.EqualTo(6));
            Assert.That(r3.ZIndex.Value, Is.EqualTo(0));
            Assert.That(r4.ZIndex.Value, Is.EqualTo(1));
            Assert.That(r5.ZIndex.Value, Is.EqualTo(2));
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

        [Test]
        public void Group_SendBackward_2()
        {
            var viewModel = new DiagramViewModel();
            var r0 = new NRectangleViewModel() { Name = "r0" };
            var r1 = new NRectangleViewModel() { Name = "r1" };
            var r2 = new NRectangleViewModel() { Name = "r2" };
            var r3 = new NRectangleViewModel() { Name = "r3" };
            var r4 = new NRectangleViewModel() { Name = "r4" };
            var r5 = new NRectangleViewModel() { Name = "r5" };
            var r6 = new NRectangleViewModel() { Name = "r6" };
            var r7 = new NRectangleViewModel() { Name = "r7" };

            r0.ZIndex.Value = 0;
            r1.ZIndex.Value = 1;
            r2.ZIndex.Value = 2; //group
            r3.ZIndex.Value = 3; //group
            r4.ZIndex.Value = 4; //group
            r5.ZIndex.Value = 5;
            r6.ZIndex.Value = 6;
            r7.ZIndex.Value = 7;

            viewModel.Items.Add(r0);
            viewModel.Items.Add(r1);
            viewModel.Items.Add(r2);
            viewModel.Items.Add(r3);
            viewModel.Items.Add(r4);
            viewModel.Items.Add(r5);
            viewModel.Items.Add(r6);
            viewModel.Items.Add(r7);

            viewModel.SelectedItems.AddRangeOnScheduler(r2, r3, r4);

            viewModel.GroupCommand.Execute();

            viewModel.SelectedItems.Clear();
            viewModel.SelectedItems.Add(viewModel.Items.OfType<GroupItemViewModel>().Single());

            viewModel.SendBackwardCommand.Execute();

            Assert.That(viewModel.Items.ElementAt(0).ZIndex.Value, Is.EqualTo(0));
            Assert.That(viewModel.Items.ElementAt(1).ZIndex.Value, Is.EqualTo(5));
            Assert.That(viewModel.Items.ElementAt(2).ZIndex.Value, Is.EqualTo(1)); //group
            Assert.That(viewModel.Items.ElementAt(3).ZIndex.Value, Is.EqualTo(2)); //group
            Assert.That(viewModel.Items.ElementAt(4).ZIndex.Value, Is.EqualTo(3)); //group
            Assert.That(viewModel.Items.ElementAt(5).ZIndex.Value, Is.EqualTo(6));
            Assert.That(viewModel.Items.ElementAt(6).ZIndex.Value, Is.EqualTo(7));
            Assert.That(viewModel.Items.ElementAt(7).ZIndex.Value, Is.EqualTo(8));
            Assert.That(viewModel.Items.ElementAt(8).ZIndex.Value, Is.EqualTo(4)); //group
        }

        [Test]
        public void Group_SendBackward_NoEffect()
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

            viewModel.SelectedItems.AddRangeOnScheduler(r1, r2, r3);

            viewModel.GroupCommand.Execute();

            viewModel.SelectedItems.Clear();
            viewModel.SelectedItems.Add(viewModel.Items.OfType<GroupItemViewModel>().Single());

            viewModel.SendBackwardCommand.Execute();

            Assert.That(viewModel.Items.ElementAt(0).ZIndex.Value, Is.EqualTo(0));
            Assert.That(viewModel.Items.ElementAt(1).ZIndex.Value, Is.EqualTo(1));
            Assert.That(viewModel.Items.ElementAt(2).ZIndex.Value, Is.EqualTo(2));
            Assert.That(viewModel.Items.ElementAt(3).ZIndex.Value, Is.EqualTo(4));
            Assert.That(viewModel.Items.ElementAt(4).ZIndex.Value, Is.EqualTo(3));
        }

        [Test]
        public void Group_SendBackground()
        {
            var viewModel = new DiagramViewModel();
            var r0 = new NRectangleViewModel() { Name = "r0" };
            var r1 = new NRectangleViewModel() { Name = "r1" };
            var r2 = new NRectangleViewModel() { Name = "r2" };
            var r3 = new NRectangleViewModel() { Name = "r3" }; //group
            var r4 = new NRectangleViewModel() { Name = "r4" }; //group
            var r5 = new NRectangleViewModel() { Name = "r5" }; //group

            r0.ZIndex.Value = 0;
            r1.ZIndex.Value = 1;
            r2.ZIndex.Value = 2;
            r3.ZIndex.Value = 3; //group
            r4.ZIndex.Value = 4; //group
            r5.ZIndex.Value = 5; //group

            viewModel.Items.Add(r0);
            viewModel.Items.Add(r1);
            viewModel.Items.Add(r2);
            viewModel.Items.Add(r3); //group
            viewModel.Items.Add(r4); //group
            viewModel.Items.Add(r5); //group

            viewModel.SelectedItems.AddRangeOnScheduler(r3, r4, r5);

            viewModel.GroupCommand.Execute();

            Assert.That(r0.ZIndex.Value, Is.EqualTo(0));
            Assert.That(r1.ZIndex.Value, Is.EqualTo(1));
            Assert.That(r2.ZIndex.Value, Is.EqualTo(2));
            Assert.That(r3.ZIndex.Value, Is.EqualTo(3));
            Assert.That(r4.ZIndex.Value, Is.EqualTo(4));
            Assert.That(r5.ZIndex.Value, Is.EqualTo(5));
            Assert.That(viewModel.Items.ElementAt(6).ZIndex.Value, Is.EqualTo(6));

            viewModel.SelectedItems.Clear();
            viewModel.SelectedItems.Add(viewModel.Items.OfType<GroupItemViewModel>().Single());

            viewModel.SendBackgroundCommand.Execute();

            Assert.That(r0.ZIndex.Value, Is.EqualTo(4));
            Assert.That(r1.ZIndex.Value, Is.EqualTo(5));
            Assert.That(r2.ZIndex.Value, Is.EqualTo(6));
            Assert.That(r3.ZIndex.Value, Is.EqualTo(0));
            Assert.That(r4.ZIndex.Value, Is.EqualTo(1));
            Assert.That(r5.ZIndex.Value, Is.EqualTo(2));
            Assert.That(viewModel.Items.ElementAt(6).ZIndex.Value, Is.EqualTo(3));
        }
    }
}
