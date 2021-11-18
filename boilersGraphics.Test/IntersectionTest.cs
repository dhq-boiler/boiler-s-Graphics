using boilersGraphics.Helpers;
using boilersGraphics.ViewModels;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace boilersGraphics.Test
{

    /// <summary>
    /// http://csharphelper.com/blog/2017/08/calculate-where-a-line-segment-and-an-ellipse-intersect-in-c/
    /// </summary>
    [TestFixture]
    public class IntersectionTest
    {
        [Test]
        public void Basic()
        {
            boilersGraphics.App.IsTest = true;
            var mainWindowViewModel = new MainWindowViewModel(null);
            var diagramViewModel = new DiagramViewModel(mainWindowViewModel, 1000, 1000);
            var ellipse = new NEllipseViewModel(-4, 3, 8, 6);
            var line = new StraightConnectorViewModel(diagramViewModel, new Point(-4, -1));
            line.AddPointP2(diagramViewModel, new Point(-4, 3));

            var intersections = Intersection.FindEllipseSegmentIntersections(ellipse, line, false);

            Assert.That(intersections.Count(), Is.EqualTo(1));
            Assert.That(intersections.First().X, Is.EqualTo(-4).Within(0.00001));
            Assert.That(intersections.First().Y, Is.EqualTo(6).Within(0.00001));
        }
    }
}
