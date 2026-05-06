using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using Moq;
using NUnit.Framework;
using Prism.Services.Dialogs;
using System.Linq;

namespace boilersGraphics.Test
{
    [TestFixture]
    public class ArrangementTest
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

        private static NRectangleViewModel AddRectangle(DiagramViewModel viewModel,
            double left, double top, double width, double height)
        {
            var r = new NRectangleViewModel();
            r.Left.Value = left;
            r.Top.Value = top;
            r.Width.Value = width;
            r.Height.Value = height;
            viewModel.AddItemCommand.Execute(r);
            return r;
        }

        private static void SelectAllRectangles(DiagramViewModel viewModel)
        {
            foreach (var layer in viewModel.Layers)
                foreach (var child in layer.Children)
                    child.IsSelected.Value = true;
        }

        [Test]
        public void DistributeHorizontal_3Items_EvenSpacing()
        {
            var (viewModel, _) = CreateSingleLayerViewModel();
            var r0 = AddRectangle(viewModel, 0, 0, 20, 20);
            var r1 = AddRectangle(viewModel, 50, 0, 20, 20);
            var r2 = AddRectangle(viewModel, 200, 0, 20, 20);
            SelectAllRectangles(viewModel);

            viewModel.DistributeHorizontalCommand.Execute();

            // Total range 0..220, sum widths 60, distance = (220-60)/2 = 80
            Assert.That(r0.Left.Value, Is.EqualTo(0));
            Assert.That(r1.Left.Value, Is.EqualTo(100));
            Assert.That(r2.Left.Value, Is.EqualTo(200));
        }

        [Test]
        public void DistributeVertical_3Items_EvenSpacing()
        {
            var (viewModel, _) = CreateSingleLayerViewModel();
            var r0 = AddRectangle(viewModel, 0, 0, 20, 20);
            var r1 = AddRectangle(viewModel, 0, 50, 20, 20);
            var r2 = AddRectangle(viewModel, 0, 200, 20, 20);
            SelectAllRectangles(viewModel);

            viewModel.DistributeVerticalCommand.Execute();

            Assert.That(r0.Top.Value, Is.EqualTo(0));
            Assert.That(r1.Top.Value, Is.EqualTo(100));
            Assert.That(r2.Top.Value, Is.EqualTo(200));
        }

        [Test]
        public void UniformWidth_AppliesFirstWidthToAll()
        {
            var (viewModel, _) = CreateSingleLayerViewModel();
            var r0 = AddRectangle(viewModel, 0, 0, 20, 30);
            var r1 = AddRectangle(viewModel, 50, 0, 40, 30);
            var r2 = AddRectangle(viewModel, 100, 0, 60, 30);
            SelectAllRectangles(viewModel);

            viewModel.UniformWidthCommand.Execute();

            var firstWidth = r0.Width.Value;
            Assert.That(r1.Width.Value, Is.EqualTo(firstWidth));
            Assert.That(r2.Width.Value, Is.EqualTo(firstWidth));
        }

        [Test]
        public void UniformHeight_AppliesFirstHeightToAll()
        {
            var (viewModel, _) = CreateSingleLayerViewModel();
            var r0 = AddRectangle(viewModel, 0, 0, 30, 20);
            var r1 = AddRectangle(viewModel, 0, 50, 30, 40);
            var r2 = AddRectangle(viewModel, 0, 100, 30, 60);
            SelectAllRectangles(viewModel);

            viewModel.UniformHeightCommand.Execute();

            var firstHeight = r0.Height.Value;
            Assert.That(r1.Height.Value, Is.EqualTo(firstHeight));
            Assert.That(r2.Height.Value, Is.EqualTo(firstHeight));
        }

        [Test]
        public void UniformWidth_Undo_RestoresOriginalWidths()
        {
            var (viewModel, _) = CreateSingleLayerViewModel();
            var r0 = AddRectangle(viewModel, 0, 0, 20, 30);
            var r1 = AddRectangle(viewModel, 50, 0, 40, 30);
            var r2 = AddRectangle(viewModel, 100, 0, 60, 30);
            SelectAllRectangles(viewModel);

            viewModel.UniformWidthCommand.Execute();
            viewModel.UndoCommand.Execute();

            Assert.That(r0.Width.Value, Is.EqualTo(20));
            Assert.That(r1.Width.Value, Is.EqualTo(40));
            Assert.That(r2.Width.Value, Is.EqualTo(60));
        }

        [Test]
        public void DistributeHorizontal_Undo_RestoresPositions()
        {
            var (viewModel, _) = CreateSingleLayerViewModel();
            var r0 = AddRectangle(viewModel, 0, 0, 20, 20);
            var r1 = AddRectangle(viewModel, 50, 0, 20, 20);
            var r2 = AddRectangle(viewModel, 200, 0, 20, 20);
            SelectAllRectangles(viewModel);

            viewModel.DistributeHorizontalCommand.Execute();
            viewModel.UndoCommand.Execute();

            Assert.That(r0.Left.Value, Is.EqualTo(0));
            Assert.That(r1.Left.Value, Is.EqualTo(50));
            Assert.That(r2.Left.Value, Is.EqualTo(200));
        }
    }
}
