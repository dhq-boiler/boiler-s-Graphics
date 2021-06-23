using boiler_sGraphics.ViewModels;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace boiler_sGraphics.Test
{
    [TestFixture]
    public class SelectedItemsTest
    {
        [Test]
        public void Items1_Selected0()
        {
            var viewModel = new DiagramViewModel();
            var items = new SelectableDesignerItemViewModelBase[]
            {
                new NRectangleViewModel() { IsSelected = false }
            };
            items.ToList().ForEach(x => viewModel.Items.Add(x));

            Assert.That(viewModel.SelectedItems, Has.Count.EqualTo(0));
        }

        [Test]
        public void Items1_Selected1()
        {
            var viewModel = new DiagramViewModel();
            var items = new SelectableDesignerItemViewModelBase[]
            {
                new NRectangleViewModel() { IsSelected = true }
            };
            items.ToList().ForEach(x => viewModel.Items.Add(x));

            Assert.That(viewModel.SelectedItems, Has.Count.EqualTo(1));
        }

        [Test]
        public void Items1_Selected0_to_Item1_Selected1()
        {
            var viewModel = new DiagramViewModel();
            var items = new SelectableDesignerItemViewModelBase[]
            {
                new NRectangleViewModel() { IsSelected = false }
            };
            items.ToList().ForEach(x => viewModel.Items.Add(x));

            Assert.That(viewModel.SelectedItems, Has.Count.EqualTo(0));

            items[0].IsSelected = true;

            Assert.That(viewModel.SelectedItems, Has.Count.EqualTo(1));
        }

        [Test]
        public void Items1_Selected0_to_Item1_Selected1_to_Item1_Selected0()
        {
            var viewModel = new DiagramViewModel();
            var items = new SelectableDesignerItemViewModelBase[]
            {
                new NRectangleViewModel() { IsSelected = false }
            };
            items.ToList().ForEach(x => viewModel.Items.Add(x));

            Assert.That(viewModel.SelectedItems, Has.Count.EqualTo(0));

            items[0].IsSelected = true;

            Assert.That(viewModel.SelectedItems, Has.Count.EqualTo(1));

            items[0].IsSelected = false;

            Assert.That(viewModel.SelectedItems, Has.Count.EqualTo(0));
        }
    }
}
