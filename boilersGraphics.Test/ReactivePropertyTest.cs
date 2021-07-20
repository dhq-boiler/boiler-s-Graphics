using boilersGraphics.ViewModels;
using NUnit.Framework;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    }
}
