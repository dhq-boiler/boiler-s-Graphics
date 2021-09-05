using boilersGraphics.Helpers;
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
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;

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

        [Test]
        public void CanExecuteCut()
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

            Assert.That(viewModel.CanExecuteCut(), Is.True);

            viewModel.Layers[0].Children[0].IsSelected.Value = false;

            Assert.That(viewModel.CanExecuteCut(), Is.True);

            layer1.IsSelected.Value = false;

            Assert.That(viewModel.CanExecuteCut(), Is.False);
        }

        [Test]
        public void CanExecuteDuplicate()
        {
            boilersGraphics.App.IsTest = true;
            var mainWindowVM = new MainWindowViewModel(null);
            var viewModel = new DiagramViewModel(mainWindowVM, 1000, 1000);

            viewModel.Layers.Clear();
            var layer1 = new Layer();
            layer1.Name.Value = "レイヤー1";
            viewModel.Layers.Add(layer1);
            layer1.IsSelected.Value = true;

            Assert.That(viewModel.CanExecuteDuplicate(), Is.False);

            var item1 = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(item1);

            var item2 = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(item2);

            var item3 = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(item3);


            viewModel.Layers[0].Children[0].IsSelected.Value = true;

            Assert.That(viewModel.CanExecuteDuplicate(), Is.True);
        }

        [Test]
        public void CanExecuteExclude()
        {
            boilersGraphics.App.IsTest = true;
            var mainWindowVM = new MainWindowViewModel(null);
            var viewModel = new DiagramViewModel(mainWindowVM, 1000, 1000);

            viewModel.Layers.Clear();
            var layer1 = new Layer();
            layer1.Name.Value = "レイヤー1";
            viewModel.Layers.Add(layer1);
            layer1.IsSelected.Value = true;

            Assert.That(viewModel.CanExecuteExclude(), Is.False);

            var item1 = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(item1);

            var item2 = new PictureDesignerItemViewModel();
            viewModel.AddItemCommand.Execute(item2);

            var item3 = new PictureDesignerItemViewModel();
            viewModel.AddItemCommand.Execute(item3);

            var item4 = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(item4);

            viewModel.Layers[0].Children[0].IsSelected.Value = true;

            Assert.That(viewModel.CanExecuteExclude(), Is.False);

            viewModel.Layers[0].Children[1].IsSelected.Value = true;
            viewModel.Layers[0].Children[2].IsSelected.Value = true;

            Assert.That(viewModel.CanExecuteExclude(), Is.False);

            viewModel.Layers[0].Children[0].IsSelected.Value = true;
            viewModel.Layers[0].Children[1].IsSelected.Value = false;
            viewModel.Layers[0].Children[2].IsSelected.Value = false;
            viewModel.Layers[0].Children[3].IsSelected.Value = true;

            Assert.That(viewModel.CanExecuteExclude(), Is.True);
        }

        [Test]
        public void CanExecuteGroup()
        {
            boilersGraphics.App.IsTest = true;
            var mainWindowVM = new MainWindowViewModel(null);
            var viewModel = new DiagramViewModel(mainWindowVM, 1000, 1000);

            viewModel.Layers.Clear();
            var layer1 = new Layer();
            layer1.Name.Value = "レイヤー1";
            viewModel.Layers.Add(layer1);
            layer1.IsSelected.Value = true;

            Assert.That(viewModel.CanExecuteGroup(), Is.False);

            var item1 = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(item1);

            var item2 = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(item2);

            var item3 = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(item3);

            viewModel.Layers[0].Children[0].IsSelected.Value = true;
            viewModel.Layers[0].Children[1].IsSelected.Value = true;

            Assert.That(viewModel.CanExecuteGroup(), Is.True);
        }

        [Test]
        public void CanExecuteIntersect()
        {
            boilersGraphics.App.IsTest = true;
            var mainWindowVM = new MainWindowViewModel(null);
            var viewModel = new DiagramViewModel(mainWindowVM, 1000, 1000);

            viewModel.Layers.Clear();
            var layer1 = new Layer();
            layer1.Name.Value = "レイヤー1";
            viewModel.Layers.Add(layer1);
            layer1.IsSelected.Value = true;

            Assert.That(viewModel.CanExecuteIntersect(), Is.False);

            var item1 = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(item1);

            var item2 = new PictureDesignerItemViewModel();
            viewModel.AddItemCommand.Execute(item2);

            var item3 = new PictureDesignerItemViewModel();
            viewModel.AddItemCommand.Execute(item3);

            var item4 = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(item4);

            viewModel.Layers[0].Children[0].IsSelected.Value = true;

            Assert.That(viewModel.CanExecuteIntersect(), Is.False);

            viewModel.Layers[0].Children[1].IsSelected.Value = true;
            viewModel.Layers[0].Children[2].IsSelected.Value = true;

            Assert.That(viewModel.CanExecuteIntersect(), Is.False);

            viewModel.Layers[0].Children[0].IsSelected.Value = true;
            viewModel.Layers[0].Children[1].IsSelected.Value = false;
            viewModel.Layers[0].Children[2].IsSelected.Value = false;
            viewModel.Layers[0].Children[3].IsSelected.Value = true;

            Assert.That(viewModel.CanExecuteIntersect(), Is.True);
        }

        [Test]
        public void CanExecuteOrder()
        {
            boilersGraphics.App.IsTest = true;
            var mainWindowVM = new MainWindowViewModel(null);
            var viewModel = new DiagramViewModel(mainWindowVM, 1000, 1000);

            viewModel.Layers.Clear();
            var layer1 = new Layer();
            layer1.Name.Value = "レイヤー1";
            viewModel.Layers.Add(layer1);
            layer1.IsSelected.Value = true;

            Assert.That(viewModel.CanExecuteOrder(), Is.False);

            var item1 = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(item1);

            var item2 = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(item2);

            var item3 = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(item3);


            viewModel.Layers[0].Children[0].IsSelected.Value = true;
            viewModel.Layers[0].Children[1].IsSelected.Value = true;

            Assert.That(viewModel.CanExecuteOrder(), Is.True);
        }


        [Test, RequiresSTA]
        public void CanExecutePaste_False()
        {
            boilersGraphics.App.IsTest = true;
            var mainWindowVM = new MainWindowViewModel(null);
            var viewModel = new DiagramViewModel(mainWindowVM, 1000, 1000);

            viewModel.Layers.Clear();
            var layer1 = new Layer();
            layer1.Name.Value = "レイヤー1";
            viewModel.Layers.Add(layer1);
            layer1.IsSelected.Value = true;

            Assert.That(viewModel.CanExecutePaste(), Is.False);
        }

        [Test, RequiresSTA]
        public void CanExecutePaste_True()
        {
            boilersGraphics.App.IsTest = true;
            var mainWindowVM = new MainWindowViewModel(null);
            var viewModel = new DiagramViewModel(mainWindowVM, 1000, 1000);

            viewModel.Layers.Clear();
            var layer1 = new Layer();
            layer1.Name.Value = "レイヤー1";
            viewModel.Layers.Add(layer1);
            layer1.IsSelected.Value = true;

            var item1 = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(item1);

            var item2 = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(item2);

            var item3 = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(item3);


            viewModel.Layers[0].Children[0].IsSelected.Value = true;
            viewModel.Layers[0].Children[1].IsSelected.Value = true;

            viewModel.CopyCommand.Execute();

            Assert.That(viewModel.CanExecutePaste(), Is.True);
        }

        [Test, RequiresSTA]
        public void CanExecutePaste_False_RootElementMissing()
        {
            boilersGraphics.App.IsTest = true;
            var mainWindowVM = new MainWindowViewModel(null);
            var viewModel = new DiagramViewModel(mainWindowVM, 1000, 1000);

            viewModel.Layers.Clear();
            var layer1 = new Layer();
            layer1.Name.Value = "レイヤー1";
            viewModel.Layers.Add(layer1);
            layer1.IsSelected.Value = true;

            var root = new XElement("boilersGraphic");
            var copyObj = new XElement("CopyObjects");
            root.Add(copyObj);
            Clipboard.SetDataObject(new ClipboardDTO(root.ToString()), false);

            Assert.That(viewModel.CanExecutePaste(), Is.False);
        }

        [Test, RequiresSTA]
        public void CanExecutePaste_False_RootDoesnothaveCopyObjects()
        {
            boilersGraphics.App.IsTest = true;
            var mainWindowVM = new MainWindowViewModel(null);
            var viewModel = new DiagramViewModel(mainWindowVM, 1000, 1000);

            viewModel.Layers.Clear();
            var layer1 = new Layer();
            layer1.Name.Value = "レイヤー1";
            viewModel.Layers.Add(layer1);
            layer1.IsSelected.Value = true;

            var root = new XElement("boilersGraphics");
            root.Add(new XElement("Dummy"));
            Clipboard.SetDataObject(new ClipboardDTO(root.ToString()), false);

            Assert.That(viewModel.CanExecutePaste(), Is.False);
        }

        [Test, RequiresSTA]
        public void CanExecutePaste_True_Layers()
        {
            boilersGraphics.App.IsTest = true;
            var mainWindowVM = new MainWindowViewModel(null);
            var viewModel = new DiagramViewModel(mainWindowVM, 1000, 1000);

            viewModel.Layers.Clear();
            var layer1 = new Layer();
            layer1.Name.Value = "レイヤー1";
            viewModel.Layers.Add(layer1);
            layer1.IsSelected.Value = true;

            viewModel.CopyCommand.Execute();
            Assert.That(viewModel.CanExecutePaste(), Is.True);
        }

        [Test, RequiresSTA]
        public void CanExecutePaste_False_XmlException()
        {
            boilersGraphics.App.IsTest = true;
            var mainWindowVM = new MainWindowViewModel(null);
            var viewModel = new DiagramViewModel(mainWindowVM, 1000, 1000);

            viewModel.Layers.Clear();
            var layer1 = new Layer();
            layer1.Name.Value = "レイヤー1";
            viewModel.Layers.Add(layer1);
            layer1.IsSelected.Value = true;

            var root = new XElement("boilersGraphics");
            Clipboard.SetDataObject(new ClipboardDTO(root.ToString().Substring(0, root.ToString().Length - 1)), false);

            Assert.That(viewModel.CanExecutePaste(), Is.False);
        }

        [Test, RequiresSTA]
        public void CanExecutePaste_False_CopyObjectsDontHaveAnyElement()
        {
            boilersGraphics.App.IsTest = true;
            var mainWindowVM = new MainWindowViewModel(null);
            var viewModel = new DiagramViewModel(mainWindowVM, 1000, 1000);

            viewModel.Layers.Clear();
            var layer1 = new Layer();
            layer1.Name.Value = "レイヤー1";
            viewModel.Layers.Add(layer1);
            layer1.IsSelected.Value = true;

            var root = new XElement("boilersGraphics");
            var copyObj = new XElement("CopyObjects");
            root.Add(copyObj);
            Clipboard.SetDataObject(new ClipboardDTO(root.ToString()), false);

            Assert.That(viewModel.CanExecutePaste(), Is.False);
        }

        [Test]
        public void CanExecuteRedo()
        {
            boilersGraphics.App.IsTest = true;
            var mainWindowVM = new MainWindowViewModel(null);
            var viewModel = new DiagramViewModel(mainWindowVM, 1000, 1000);

            viewModel.Layers.Clear();
            var layer1 = new Layer();
            layer1.Name.Value = "レイヤー1";
            viewModel.Layers.Add(layer1);
            layer1.IsSelected.Value = true;

            Assert.That(viewModel.CanExecuteRedo(), Is.EqualTo(false));
        }

        [Test]
        public void CanExecuteUndo()
        {
            boilersGraphics.App.IsTest = true;
            var mainWindowVM = new MainWindowViewModel(null);
            var viewModel = new DiagramViewModel(mainWindowVM, 1000, 1000);

            viewModel.Layers.Clear();
            var layer1 = new Layer();
            layer1.Name.Value = "レイヤー1";
            viewModel.Layers.Add(layer1);
            layer1.IsSelected.Value = true;

            Assert.That(viewModel.CanExecuteUndo(), Is.EqualTo(false));
        }

        [Test]
        public void CanExecuteUngroup()
        {
            boilersGraphics.App.IsTest = true;
            var mainWindowVM = new MainWindowViewModel(null);
            var viewModel = new DiagramViewModel(mainWindowVM, 1000, 1000);

            viewModel.Layers.Clear();
            var layer1 = new Layer();
            layer1.Name.Value = "レイヤー1";
            viewModel.Layers.Add(layer1);
            layer1.IsSelected.Value = true;

            var item1 = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(item1);

            var item2 = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(item2);

            var item3 = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(item3);

            viewModel.Layers[0].Children[0].IsSelected.Value = true;
            viewModel.Layers[0].Children[1].IsSelected.Value = true;

            viewModel.GroupCommand.Execute();

            viewModel.Layers[0].Children[0].IsSelected.Value = true;

            Assert.That(viewModel.CanExecuteUngroup(), Is.EqualTo(true));
        }

        [Test]
        public void CanExecuteUniform()
        {
            boilersGraphics.App.IsTest = true;
            var mainWindowVM = new MainWindowViewModel(null);
            var viewModel = new DiagramViewModel(mainWindowVM, 1000, 1000);

            viewModel.Layers.Clear();
            var layer1 = new Layer();
            layer1.Name.Value = "レイヤー1";
            viewModel.Layers.Add(layer1);
            layer1.IsSelected.Value = true;

            Assert.That(viewModel.CanExecuteUniform(), Is.False);

            var item1 = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(item1);

            var item2 = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(item2);

            var item3 = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(item3);

            viewModel.Layers[0].Children[0].IsSelected.Value = true;
            viewModel.Layers[0].Children[1].IsSelected.Value = true;

            Assert.That(viewModel.CanExecuteUniform(), Is.True);
        }

        [Test]
        public void CanExecuteUnion()
        {
            boilersGraphics.App.IsTest = true;
            var mainWindowVM = new MainWindowViewModel(null);
            var viewModel = new DiagramViewModel(mainWindowVM, 1000, 1000);

            viewModel.Layers.Clear();
            var layer1 = new Layer();
            layer1.Name.Value = "レイヤー1";
            viewModel.Layers.Add(layer1);
            layer1.IsSelected.Value = true;

            Assert.That(viewModel.CanExecuteUnion(), Is.False);

            var item1 = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(item1);

            var item2 = new PictureDesignerItemViewModel();
            viewModel.AddItemCommand.Execute(item2);

            var item3 = new PictureDesignerItemViewModel();
            viewModel.AddItemCommand.Execute(item3);

            var item4 = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(item4);

            viewModel.Layers[0].Children[0].IsSelected.Value = true;

            Assert.That(viewModel.CanExecuteUnion(), Is.False);

            viewModel.Layers[0].Children[1].IsSelected.Value = true;
            viewModel.Layers[0].Children[2].IsSelected.Value = true;

            Assert.That(viewModel.CanExecuteUnion(), Is.False);

            viewModel.Layers[0].Children[0].IsSelected.Value = true;
            viewModel.Layers[0].Children[1].IsSelected.Value = false;
            viewModel.Layers[0].Children[2].IsSelected.Value = false;
            viewModel.Layers[0].Children[3].IsSelected.Value = true;

            Assert.That(viewModel.CanExecuteUnion(), Is.True);
        }

        [Test]
        public void CanExecuteXor()
        {
            boilersGraphics.App.IsTest = true;
            var mainWindowVM = new MainWindowViewModel(null);
            var viewModel = new DiagramViewModel(mainWindowVM, 1000, 1000);

            viewModel.Layers.Clear();
            var layer1 = new Layer();
            layer1.Name.Value = "レイヤー1";
            viewModel.Layers.Add(layer1);
            layer1.IsSelected.Value = true;

            Assert.That(viewModel.CanExecuteXor(), Is.False);

            var item1 = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(item1);

            var item2 = new PictureDesignerItemViewModel();
            viewModel.AddItemCommand.Execute(item2);

            var item3 = new PictureDesignerItemViewModel();
            viewModel.AddItemCommand.Execute(item3);

            var item4 = new NRectangleViewModel();
            viewModel.AddItemCommand.Execute(item4);

            viewModel.Layers[0].Children[0].IsSelected.Value = true;

            Assert.That(viewModel.CanExecuteXor(), Is.False);

            viewModel.Layers[0].Children[1].IsSelected.Value = true;
            viewModel.Layers[0].Children[2].IsSelected.Value = true;

            Assert.That(viewModel.CanExecuteXor(), Is.False);

            viewModel.Layers[0].Children[0].IsSelected.Value = true;
            viewModel.Layers[0].Children[1].IsSelected.Value = false;
            viewModel.Layers[0].Children[2].IsSelected.Value = false;
            viewModel.Layers[0].Children[3].IsSelected.Value = true;

            Assert.That(viewModel.CanExecuteXor(), Is.True);
        }
    }
}
