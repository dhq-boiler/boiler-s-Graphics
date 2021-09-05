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
    public class HelpersTest
    {
        [Test]
        public void GeometryCreator_CreateEllipse()
        {
            var ellipse = new NEllipseViewModel();
            ellipse.Left.Value = 10;
            ellipse.Top.Value = 10;
            ellipse.Width.Value = 10;
            ellipse.Height.Value = 10;
            var pg = GeometryCreator.CreateEllipse(ellipse);
            Assert.That(pg.Bounds.Left, Is.EqualTo(10));
            Assert.That(pg.Bounds.Top, Is.EqualTo(10));
            Assert.That(pg.Bounds.Width, Is.EqualTo(10));
            Assert.That(pg.Bounds.Height, Is.EqualTo(10));
        }
        [Test]
        public void GeometryCreator_CreateEllipse_Angle()
        {
            var ellipse = new NEllipseViewModel();
            ellipse.Left.Value = 10;
            ellipse.Top.Value = 10;
            ellipse.Width.Value = 20;
            ellipse.Height.Value = 10;
            var pg = GeometryCreator.CreateEllipse(ellipse);
            Assert.That(pg.Bounds.Left, Is.EqualTo(10));
            Assert.That(pg.Bounds.Top, Is.EqualTo(10));
            Assert.That(pg.Bounds.Width, Is.EqualTo(20));
            Assert.That(pg.Bounds.Height, Is.EqualTo(10));
            pg = GeometryCreator.CreateEllipse(ellipse, 90);
            Assert.That(pg.Bounds.Left, Is.EqualTo(15));
            Assert.That(pg.Bounds.Top, Is.EqualTo(5));
            Assert.That(pg.Bounds.Width, Is.EqualTo(10));
            Assert.That(pg.Bounds.Height, Is.EqualTo(20));
        }
    }
}
