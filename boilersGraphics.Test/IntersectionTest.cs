﻿using boilersGraphics.Helpers;
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

            var intersections = Intersection.FindEllipseSegmentIntersections(ellipse, new Point(-4, -1), new Point(-4, 3), false);

            Assert.That(intersections.Item1.Count(), Is.EqualTo(1));
            Assert.That(intersections.Item1.First().X, Is.EqualTo(-4).Within(0.00001));
            Assert.That(intersections.Item1.First().Y, Is.EqualTo(6).Within(0.00001));
        }


        [Test]
        public void 回転0度の円()
        {
            boilersGraphics.App.IsTest = true;
            var mainWindowViewModel = new MainWindowViewModel(null);
            var diagramViewModel = new DiagramViewModel(mainWindowViewModel, 1000, 1000);
            var ellipse = new NEllipseViewModel(-4, -4, 8, 8);

            Assert.That(ellipse.CenterX.Value, Is.EqualTo(0));
            Assert.That(ellipse.CenterY.Value, Is.EqualTo(0));

            var intersections = Intersection.FindEllipseSegmentIntersections(ellipse, new Point(-4, -10), new Point(-4, 10), false);

            Assert.That(intersections.Item1.Count(), Is.EqualTo(1));
            Assert.That(intersections.Item1.First().X, Is.EqualTo(-4).Within(0.00001));
            Assert.That(intersections.Item1.First().Y, Is.EqualTo(0).Within(0.00001));
        }

        [Test]
        public void 回転0度の円_回転対応版()
        {
            boilersGraphics.App.IsTest = true;
            var mainWindowViewModel = new MainWindowViewModel(null);
            var diagramViewModel = new DiagramViewModel(mainWindowViewModel, 1000, 1000);
            var ellipse = new NEllipseViewModel(-4, -4, 8, 8);

            Assert.That(ellipse.CenterX.Value, Is.EqualTo(0));
            Assert.That(ellipse.CenterY.Value, Is.EqualTo(0));

            var intersections = Intersection.FindEllipseSegmentIntersectionsSupportRotation(ellipse, new Point(-4, -10), new Point(-4, 10), false);

            Assert.That(intersections.Item1.Count(), Is.EqualTo(1));
            Assert.That(intersections.Item1.First().X, Is.EqualTo(-4).Within(0.00001));
            Assert.That(intersections.Item1.First().Y, Is.EqualTo(0).Within(0.00001));
        }

        [Test]
        public void 回転90度の円_回転対応版()
        {
            boilersGraphics.App.IsTest = true;
            var mainWindowViewModel = new MainWindowViewModel(null);
            var diagramViewModel = new DiagramViewModel(mainWindowViewModel, 1000, 1000);
            var ellipse = new NEllipseViewModel(-4, -4, 8, 8);
            ellipse.RotationAngle.Value = 90;

            Assert.That(ellipse.CenterX.Value, Is.EqualTo(0));
            Assert.That(ellipse.CenterY.Value, Is.EqualTo(0));

            var intersections = Intersection.FindEllipseSegmentIntersectionsSupportRotation(ellipse, new Point(-4, -10), new Point(-4, 10), false);

            Assert.That(intersections.Item1.Count(), Is.EqualTo(1));
            Assert.That(intersections.Item1.First().X, Is.EqualTo(-4).Within(0.00001));
            Assert.That(intersections.Item1.First().Y, Is.EqualTo(0).Within(0.00001));
        }
    }
}
