using boilersGraphics.Helpers;
using boilersGraphics.ViewModels;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace boilersGraphics.Test
{
    [TestFixture]
    public class MeasureTest
    {
        [Test]
        public void GetWidthメソッドをテスト()
        {
            var item = new NRectangleViewModel();
            item.Left.Value = 10;
            item.Top.Value = 10;
            item.Width.Value = 20;
            item.Height.Value = 20;
            double minX, maxX;
            int width = Measure.GetWidth(new List<SelectableDesignerItemViewModelBase>() { item }, out minX, out maxX);
            Assert.That(width, Is.EqualTo(20));
            Assert.That(minX, Is.EqualTo(10));
            Assert.That(maxX, Is.EqualTo(30));
        }

        [Test]
        public void GetWidthメソッドを複数アイテムでテスト()
        {
            var item = new NRectangleViewModel();
            item.Left.Value = 10;
            item.Top.Value = 10;
            item.Width.Value = 20;
            item.Height.Value = 20;
            var item2 = new NRectangleViewModel();
            item2.Left.Value = 100;
            item2.Top.Value = 100;
            item2.Width.Value = 200;
            item2.Height.Value = 200;
            double minX, maxX;
            int width = Measure.GetWidth(new List<SelectableDesignerItemViewModelBase>() { item, item2 }, out minX, out maxX);
            Assert.That(width, Is.EqualTo(290));
            Assert.That(minX, Is.EqualTo(10));
            Assert.That(maxX, Is.EqualTo(300));
        }

        [Test]
        public void GetHeightメソッドをテスト()
        {
            var item = new NRectangleViewModel();
            item.Left.Value = 10;
            item.Top.Value = 10;
            item.Width.Value = 20;
            item.Height.Value = 20;
            double minY, maxY;
            int height = Measure.GetHeight(new List<SelectableDesignerItemViewModelBase>() { item }, out minY, out maxY);
            Assert.That(height, Is.EqualTo(20));
            Assert.That(minY, Is.EqualTo(10));
            Assert.That(maxY, Is.EqualTo(30));
        }

        [Test]
        public void GetHeightメソッドを複数アイテムでテスト()
        {
            var item = new NRectangleViewModel();
            item.Left.Value = 10;
            item.Top.Value = 10;
            item.Width.Value = 20;
            item.Height.Value = 20;
            var item2 = new NRectangleViewModel();
            item2.Left.Value = 100;
            item2.Top.Value = 100;
            item2.Width.Value = 200;
            item2.Height.Value = 200;
            double minY, maxY;
            int height = Measure.GetHeight(new List<SelectableDesignerItemViewModelBase>() { item, item2 }, out minY, out maxY);
            Assert.That(height, Is.EqualTo(290));
            Assert.That(minY, Is.EqualTo(10));
            Assert.That(maxY, Is.EqualTo(300));
        }
    }
}
