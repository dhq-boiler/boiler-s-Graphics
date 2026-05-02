using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using Moq;
using NUnit.Framework;
using Prism.Services.Dialogs;
using System.Linq;
using System.Threading;

namespace boilersGraphics.Test
{
    [TestFixture]
    public class ClipboardOpsTest
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

        [Test, RequiresThread(ApartmentState.STA)]
        public void Copy_Paste_AddsDuplicateItem()
        {
            var (viewModel, layer) = CreateSingleLayerViewModel();
            var item = new NRectangleViewModel();
            item.Left.Value = 10;
            item.Top.Value = 10;
            item.Width.Value = 20;
            item.Height.Value = 20;
            viewModel.AddItemCommand.Execute(item);
            viewModel.Layers[0].Children[0].IsSelected.Value = true;

            var beforeCount = viewModel.AllItems.Value.Length;
            viewModel.CopyCommand.Execute();
            viewModel.PasteCommand.Execute();

            Assert.That(viewModel.AllItems.Value.Length, Is.GreaterThan(beforeCount),
                "Paste should add at least one new item to AllItems");
            Assert.That(viewModel.AllItems.Value.OfType<NRectangleViewModel>().Count(),
                Is.EqualTo(2),
                "AllItems should contain the original rectangle plus the pasted clone");
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void Cut_RemovesOriginalAndPlacesOnClipboard()
        {
            var (viewModel, layer) = CreateSingleLayerViewModel();
            var item = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(item);
            viewModel.Layers[0].Children[0].IsSelected.Value = true;

            viewModel.CutCommand.Execute();

            Assert.That(layer.Children, Has.Count.EqualTo(0),
                "Cut should remove the original from the layer");
            Assert.That(viewModel.CanExecutePaste(), Is.True,
                "After Cut the clipboard should contain a paste-able payload");
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void Cut_Paste_MovesItemRatherThanDuplicating()
        {
            var (viewModel, layer) = CreateSingleLayerViewModel();
            var item = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(item);
            viewModel.Layers[0].Children[0].IsSelected.Value = true;

            viewModel.CutCommand.Execute();
            Assert.That(layer.Children, Has.Count.EqualTo(0));

            viewModel.PasteCommand.Execute();

            Assert.That(viewModel.AllItems.Value.OfType<NRectangleViewModel>().Count(),
                Is.EqualTo(1),
                "Cut+Paste should leave exactly one rectangle, not duplicate");
        }
    }
}
