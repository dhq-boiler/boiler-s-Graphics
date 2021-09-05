using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace boilersGraphics.Test
{
    [TestFixture]
    public class DiagramViewModelTest
    {
        [Test]
        public void Dispose()
        {
            boilersGraphics.App.IsTest = true;
            var mainWindowVM = new MainWindowViewModel(null);
            var viewModel = new DiagramViewModel(mainWindowVM, 1000, 1000);
            viewModel.Dispose();
            Assert.That(viewModel.Layers, Has.Count.EqualTo(0));
            Assert.That(viewModel.AllItems.Value, Has.Length.EqualTo(0));
            Assert.That(viewModel.SelectedItems.Value, Has.Length.EqualTo(0));
            Assert.That(viewModel.EdgeThickness.Value, Is.EqualTo(null));
            Assert.That(viewModel.EnableMiniMap.Value, Is.EqualTo(false));
            Assert.That(viewModel.FileName.Value, Is.EqualTo(null));
            Assert.That(viewModel.CanvasBackground.Value, Is.EqualTo(Color.FromArgb(0, 0, 0, 0)));
            Assert.That(viewModel.EnablePointSnap.Value, Is.EqualTo(false));
            Assert.That(() => viewModel.Dispose(), Throws.Nothing);
        }

        [Test]
        public void CanExecuteAlign()
        {
            boilersGraphics.App.IsTest = true;
            var mainWindowVM = new MainWindowViewModel(null);
            var viewModel = new DiagramViewModel(mainWindowVM, 1000, 1000);
            
            viewModel.Layers.Clear();
            var layer1 = new Layer();
            layer1.Name.Value = "レイヤー1";
            viewModel.Layers.Add(layer1);
            layer1.IsSelected.Value = true;

            Assert.That(viewModel.CanExecuteAlign(), Is.False);

            var item1 = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(item1);

            var item2 = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(item2);

            var item3 = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(item3);


            viewModel.Layers[0].Children[0].IsSelected.Value = true;
            viewModel.Layers[0].Children[1].IsSelected.Value = true;

            Assert.That(viewModel.CanExecuteAlign(), Is.True);
        }

        [Test]
        public void CanExecuteDistribute()
        {
            boilersGraphics.App.IsTest = true;
            var mainWindowVM = new MainWindowViewModel(null);
            var viewModel = new DiagramViewModel(mainWindowVM, 1000, 1000);

            viewModel.Layers.Clear();
            var layer1 = new Layer();
            layer1.Name.Value = "レイヤー1";
            viewModel.Layers.Add(layer1);
            layer1.IsSelected.Value = true;

            Assert.That(viewModel.CanExecuteDistribute(), Is.False);

            var item1 = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(item1);

            var item2 = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(item2);

            var item3 = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(item3);


            viewModel.Layers[0].Children[0].IsSelected.Value = true;
            viewModel.Layers[0].Children[1].IsSelected.Value = true;

            Assert.That(viewModel.CanExecuteDistribute(), Is.True);
        }

        [Test]
        public void CanExecuteClip()
        {
            boilersGraphics.App.IsTest = true;
            var mainWindowVM = new MainWindowViewModel(null);
            var viewModel = new DiagramViewModel(mainWindowVM, 1000, 1000);

            viewModel.Layers.Clear();
            var layer1 = new Layer();
            layer1.Name.Value = "レイヤー1";
            viewModel.Layers.Add(layer1);
            layer1.IsSelected.Value = true;

            Assert.That(viewModel.CanExecuteClip(), Is.False);

            var item1 = new PictureDesignerItemViewModel();
            viewModel.AddItemCommand.Execute(item1);

            var item2 = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(item2);

            var item3 = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(item3);


            viewModel.Layers[0].Children[0].IsSelected.Value = true;
            viewModel.Layers[0].Children[1].IsSelected.Value = true;

            Assert.That(viewModel.CanExecuteClip(), Is.True);
        }

        [Test]
        public void CanExecuteCopy()
        {
            boilersGraphics.App.IsTest = true;
            var mainWindowVM = new MainWindowViewModel(null);
            var viewModel = new DiagramViewModel(mainWindowVM, 1000, 1000);

            viewModel.Layers.Clear();
            var layer1 = new Layer();
            layer1.Name.Value = "レイヤー1";
            viewModel.Layers.Add(layer1);
            layer1.IsSelected.Value = true;

            Assert.That(viewModel.CanExecuteCopy(), Is.False);

            var item1 = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(item1);

            var item2 = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(item2);

            var item3 = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(item3);


            viewModel.Layers[0].Children[0].IsSelected.Value = true;

            Assert.That(viewModel.CanExecuteCopy(), Is.True);
        }
    }
}
