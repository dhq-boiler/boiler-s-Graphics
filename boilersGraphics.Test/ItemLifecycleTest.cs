using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using Moq;
using NUnit.Framework;
using Prism.Services.Dialogs;
using System.Linq;

namespace boilersGraphics.Test
{
    [TestFixture]
    public class ItemLifecycleTest
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

        [Test]
        public void AddItem_AppearsInLayerChildren()
        {
            var (viewModel, layer) = CreateSingleLayerViewModel();
            var r = new NRectangleViewModel();

            viewModel.AddItemCommand.Execute(r);

            Assert.That(layer.Children, Has.Count.EqualTo(1));
            Assert.That((layer.Children[0] as LayerItem)?.Item.Value, Is.EqualTo(r));
        }

        [Test]
        public void AddItem_AssignsZIndexInOrder()
        {
            var (viewModel, _) = CreateSingleLayerViewModel();

            var r0 = new NRectangleViewModel();
            var r1 = new NRectangleViewModel();
            var r2 = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(r0);
            viewModel.AddItemCommand.Execute(r1);
            viewModel.AddItemCommand.Execute(r2);

            Assert.That(r0.ZIndex.Value, Is.EqualTo(0));
            Assert.That(r1.ZIndex.Value, Is.EqualTo(1));
            Assert.That(r2.ZIndex.Value, Is.EqualTo(2));
        }

        [Test]
        public void RemoveItem_RemovesFromLayer()
        {
            var (viewModel, layer) = CreateSingleLayerViewModel();
            var r = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(r);
            Assert.That(layer.Children, Has.Count.EqualTo(1));

            viewModel.RemoveItemCommand.Execute(r);

            Assert.That(layer.Children, Has.Count.EqualTo(0));
            Assert.That(viewModel.AllItems.Value, Does.Not.Contain(r));
        }

        [Test]
        public void RemoveItem_OnlyOneOfMany_LeavesRest()
        {
            var (viewModel, layer) = CreateSingleLayerViewModel();
            var r0 = new NRectangleViewModel();
            var r1 = new NRectangleViewModel();
            var r2 = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(r0);
            viewModel.AddItemCommand.Execute(r1);
            viewModel.AddItemCommand.Execute(r2);

            viewModel.RemoveItemCommand.Execute(r1);

            Assert.That(layer.Children, Has.Count.EqualTo(2));
            Assert.That(viewModel.AllItems.Value, Does.Contain(r0));
            Assert.That(viewModel.AllItems.Value, Does.Not.Contain(r1));
            Assert.That(viewModel.AllItems.Value, Does.Contain(r2));
        }

        [Test]
        public void Visibility_TogglingItem_UpdatesIsVisible()
        {
            var (viewModel, layer) = CreateSingleLayerViewModel();
            var r = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(r);

            Assert.That(r.IsVisible.Value, Is.True);

            r.IsVisible.Value = false;
            Assert.That(r.IsVisible.Value, Is.False);

            r.IsVisible.Value = true;
            Assert.That(r.IsVisible.Value, Is.True);
        }

        [Test]
        public void Visibility_ItemAndLayerIndependent()
        {
            var (viewModel, layer) = CreateSingleLayerViewModel();
            var r = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(r);

            layer.IsVisible.Value = false;
            Assert.That(layer.IsVisible.Value, Is.False);

            layer.IsVisible.Value = true;
            Assert.That(layer.IsVisible.Value, Is.True);
            Assert.That(r.IsVisible.Value, Is.True);
        }
    }
}
