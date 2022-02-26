using boilersGraphics.Models;
using boilersGraphics.Properties;
using boilersGraphics.ViewModels;
using Moq;
using NUnit.Framework;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace boilersGraphics.Test
{
    [TestFixture]
    public class ReactivePropertyTest
    {
        [Test]
        public void PictureDesignerItemViewModelからReactivePropertyを作る()
        {
            var picture = new PictureDesignerItemViewModel();
            var pictureRP = picture.ToReactiveProperty();
            Assert.That(pictureRP.Value, Is.Not.EqualTo(null));
        }

        [Test]
        public void NEllipseViewModelからReactivePropertyを作る()
        {
            var ellipse = new NEllipseViewModel();
            var ellipseRP = ellipse.ToReactiveProperty();
            Assert.That(ellipseRP.Value, Is.Not.EqualTo(null));
        }

        [Test]
        public void NPolygonViewModelからReactivePropertyを作る()
        {
            var polygon = new NPolygonViewModel();
            var polygonRP = polygon.ToReactiveProperty();
            Assert.That(polygonRP.Value, Is.Not.EqualTo(null));
        }

        [Test]
        public void NRectangleViewModelからReactivePropertyを作る()
        {
            var rectangle = new NRectangleViewModel();
            var rectangleRP = rectangle.ToReactiveProperty();
            Assert.That(rectangleRP.Value, Is.Not.EqualTo(null));
        }

        [Test]
        public void Children代入()
        {
            boilersGraphics.App.IsTest = true;
            var dlgService = new Mock<IDialogService>();
            MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(dlgService.Object);
            var diagramVM = new DiagramViewModel(mainWindowViewModel, 1000, 1000);
            diagramVM.Layers.Clear();
            var layer1 = new Layer();
            layer1.Name.Value = $"{Resources.Name_Layer}1";
            diagramVM.Layers.Add(layer1);
            layer1.IsSelected.Value = true; //レイヤー1を選択状態にする

            diagramVM.DisposeProperties();
            diagramVM.InitializeProperties_Layers(false);
            diagramVM.AddNewLayer(mainWindowViewModel, false);
            var firstSelectedLayer = diagramVM.SelectedLayers.Value.First();
            var item = new NRectangleViewModel();
            var layerItem = new LayerItem(item, firstSelectedLayer, "TEST");
            Assert.That(firstSelectedLayer.Name.Value, Is.EqualTo("レイヤー1"));
            diagramVM.InitializeProperties_Items(false);
            firstSelectedLayer.Children.Value = new System.Collections.ObjectModel.ObservableCollection<LayerTreeViewItemBase>(new LayerItem[] { layerItem });
            diagramVM.SetSubscribes(false);

            Assert.That(diagramVM.AllItems.Value, Has.Member(item));
        }

        [Test]
        public void ReactiveCollection初期化()
        {
            boilersGraphics.App.IsTest = true;
            var bag = new ConcurrentBag<SelectableDesignerItemViewModelBase>();
            const int count = 100000;
            Parallel.For(0, count, i =>
            {
                bag.Add(new NRectangleViewModel() { });
            });
            var l = new List<LayerTreeViewItemBase>();
            while (bag.TryTake(out var item))
            {
                var i = new LayerItem(item, null, null);
                l.Add(i);
            }
            var reactiveCollection = new ReactiveCollection<LayerTreeViewItemBase>(l.ToObservable());
            Assert.That(reactiveCollection, Has.Count.EqualTo(100000));
        }
    }
}
