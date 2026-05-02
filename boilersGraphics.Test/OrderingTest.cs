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
    }
}
