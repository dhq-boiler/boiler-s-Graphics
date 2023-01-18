using boilersGraphics.Controls;
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
    public class ResizeThumbTest
    {
        [Test]
        public void Vertical()
        {
            var picture = new PictureDesignerItemViewModel();
            picture.Left.Value = 10;
            picture.Top.Value = 20;
            picture.Width.Value = 100;
            picture.Height.Value = 200;
            ResizeThumb.AffectVertical(new System.Windows.Controls.Primitives.DragDeltaEventArgs(3, 3), System.Windows.VerticalAlignment.Top, 0, 10, picture);
            Assert.That(picture.Left.Value, Is.EqualTo(10));
            Assert.That(picture.Top.Value, Is.EqualTo(23));
            Assert.That(picture.Width.Value, Is.EqualTo(100));
            Assert.That(picture.Height.Value, Is.EqualTo(197));
        }

        [Test]
        public void Horizontal()
        {
            var picture = new PictureDesignerItemViewModel();
            picture.Left.Value = 10;
            picture.Top.Value = 20;
            picture.Width.Value = 100;
            picture.Height.Value = 200;
            ResizeThumb.AffectHorizontal(new System.Windows.Controls.Primitives.DragDeltaEventArgs(3, 3), System.Windows.HorizontalAlignment.Left, 0, 10, picture);
            Assert.That(picture.Left.Value, Is.EqualTo(13));
            Assert.That(picture.Top.Value, Is.EqualTo(20));
            Assert.That(picture.Width.Value, Is.EqualTo(97));
            Assert.That(picture.Height.Value, Is.EqualTo(200));
        }

        [Test]
        public void VerticalAndHorizontal()
        {
            var picture = new PictureDesignerItemViewModel();
            picture.Left.Value = 10;
            picture.Top.Value = 20;
            picture.Width.Value = 100;
            picture.Height.Value = 200;
            ResizeThumb.AffectVertical(new System.Windows.Controls.Primitives.DragDeltaEventArgs(3, 3), System.Windows.VerticalAlignment.Top, 0, 10, picture);
            ResizeThumb.AffectHorizontal(new System.Windows.Controls.Primitives.DragDeltaEventArgs(3, 3), System.Windows.HorizontalAlignment.Left, 0, 10, picture);
            Assert.That(picture.Left.Value, Is.EqualTo(13));
            Assert.That(picture.Top.Value, Is.EqualTo(23));
            Assert.That(picture.Width.Value, Is.EqualTo(97));
            Assert.That(picture.Height.Value, Is.EqualTo(197));
        }
    }
}
