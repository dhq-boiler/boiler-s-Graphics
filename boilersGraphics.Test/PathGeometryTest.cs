using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace boilersGraphics.Test
{
    [TestFixture]
    public class PathGeometryTest
    {
        [Test]
        public void A()
        {
            var a = PathGeometry.CreateFromGeometry(new RectangleGeometry(new Rect(new Point(0, 0), new Point(1000, 1000))));
            a.FillRule = FillRule.Nonzero;
            var b = PathGeometry.CreateFromGeometry(new RectangleGeometry(new Rect(new Point(0, 0), new Point(1000, 1000))));
            b.FillRule = FillRule.Nonzero;
            Assert.AreEqual(a.ToString(), b.ToString());
            var c = Geometry.Combine(a, b, GeometryCombineMode.Intersect, null);
            Assert.AreEqual(a.ToString(), c.ToString());
        }
    }
}
