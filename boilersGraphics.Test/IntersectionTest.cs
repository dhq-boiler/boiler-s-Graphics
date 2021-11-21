using boilersGraphics.Controls;
using boilersGraphics.Helpers;
using boilersGraphics.ViewModels;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
            var ellipse = new NEllipseViewModel(-4, 3, 8, 6);

            var intersections = Intersection.FindEllipseSegmentIntersections(ellipse, new Point(-4, -1), new Point(-4, 3), false);

            Assert.That(intersections.Item1.Count(), Is.EqualTo(1));
            Assert.That(intersections.Item1.First().X, Is.EqualTo(-4).Within(0.00001));
            Assert.That(intersections.Item1.First().Y, Is.EqualTo(6).Within(0.00001));
        }


        [Test]
        public void 回転0度の円()
        {
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
            var ellipse = new NEllipseViewModel(-4, -4, 8, 8);

            Assert.That(ellipse.CenterX.Value, Is.EqualTo(0));
            Assert.That(ellipse.CenterY.Value, Is.EqualTo(0));

            var intersections = Intersection.FindEllipseSegmentIntersectionsSupportRotation(ellipse, new Point(-4, -10), new Point(-4, 10), false);

            Assert.That(intersections.Item1.Count(), Is.EqualTo(1));
            Assert.That(intersections.Item1.First().X, Is.EqualTo(-4).Within(0.00001));
            Assert.That(intersections.Item1.First().Y, Is.EqualTo(0).Within(0.00001));
        }

        [Test]
        public void 回転90度の楕円_回転対応版()
        {
            var ellipse = new NEllipseViewModel(-3, -2, 6, 4);
            ellipse.RotationAngle.Value = 90;

            Assert.That(ellipse.CenterX.Value, Is.EqualTo(0));
            Assert.That(ellipse.CenterY.Value, Is.EqualTo(0));

            var intersections = Intersection.FindEllipseSegmentIntersectionsSupportRotation(ellipse, new Point(-10, 3), new Point(0, 3), false);

            Assert.That(intersections.Item1.Count(), Is.EqualTo(1));
            Assert.That(intersections.Item1.First().X, Is.EqualTo(0).Within(0.00001));
            Assert.That(intersections.Item1.First().Y, Is.EqualTo(3).Within(0.00001));
        }

        [Test]
        public void ベクトル2つの角度の確認()
        {
            Vector vec1, vec2;
            double angle;
            vec1 = new Vector(0, -10);
            //時計回りに45°
            vec2 = new Vector(10, -10);
            angle = Vector.AngleBetween(vec1, vec2);
            if (angle < 0)
            {
                angle += 360;
            }
            Assert.That(angle, Is.EqualTo(45));
            
            //時計回りに90°
            vec2 = new Vector(10, 0);
            angle = Vector.AngleBetween(vec1, vec2);
            if (angle < 0)
            {
                angle += 360;
            }
            Assert.That(angle, Is.EqualTo(90));

            //時計回りに135°
            vec2 = new Vector(10, 10);
            angle = Vector.AngleBetween(vec1, vec2);
            if (angle < 0)
            {
                angle += 360;
            }
            Assert.That(angle, Is.EqualTo(135));

            //時計回りに180°
            vec2 = new Vector(0, 10);
            angle = Vector.AngleBetween(vec1, vec2);
            if (angle < 0)
            {
                angle += 360;
            }
            Assert.That(angle, Is.EqualTo(180));

            //時計回りに225°/反時計回りに135°
            vec2 = new Vector(-10, 10);
            angle = Vector.AngleBetween(vec1, vec2);
            if (angle < 0)
            {
                angle += 360;
            }
            Assert.That(angle, Is.EqualTo(225));

            //時計回りに270°/反時計回りに90°
            vec2 = new Vector(-10, 0);
            angle = Vector.AngleBetween(vec1, vec2);
            if (angle < 0)
            {
                angle += 360;
            }
            Assert.That(angle, Is.EqualTo(270));

            //時計回りに315°/反時計回りに45°
            vec2 = new Vector(-10, -10);
            angle = Vector.AngleBetween(vec1, vec2);
            if (angle < 0)
            {
                angle += 360;
            }
            Assert.That(angle, Is.EqualTo(315));
        }
    }
}
