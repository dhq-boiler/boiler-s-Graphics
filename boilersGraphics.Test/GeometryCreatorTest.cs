using boilersGraphics.Helpers;
using NUnit.Framework;
using System.Windows;
using System.Windows.Media;

namespace boilersGraphics.Test
{
    [TestFixture]
    public class GeometryCreatorTest
    {
        private const double Tolerance = 1e-6;

        [Test]
        public void 四角形移動()
        {
            var geometry = PathGeometry.CreateFromGeometry(new RectangleGeometry(new Rect(0, 10, 20, 30)));
            geometry = GeometryCreator.Translate(geometry, 3, 6);
            Assert.That(geometry.Bounds.Left, Is.EqualTo(3));
            Assert.That(geometry.Bounds.Top, Is.EqualTo(16));
            Assert.That(geometry.Bounds.Width, Is.EqualTo(20));
            Assert.That(geometry.Bounds.Height, Is.EqualTo(30));
        }

        [Test]
        public void 四角形マイナス方向移動()
        {
            var geometry = PathGeometry.CreateFromGeometry(new RectangleGeometry(new Rect(10, 10, 20, 30)));
            geometry = GeometryCreator.Translate(geometry, -5, -7);
            Assert.That(geometry.Bounds.Left, Is.EqualTo(5));
            Assert.That(geometry.Bounds.Top, Is.EqualTo(3));
            Assert.That(geometry.Bounds.Width, Is.EqualTo(20));
            Assert.That(geometry.Bounds.Height, Is.EqualTo(30));
        }

        [Test]
        public void 楕円移動_Cコマンド分岐()
        {
            var geometry = PathGeometry.CreateFromGeometry(new EllipseGeometry(new Point(20, 30), 10, 15));
            geometry = GeometryCreator.Translate(geometry, 5, 7);
            // 元 Bounds: (10, 15, 20, 30)
            Assert.That(geometry.Bounds.Left, Is.EqualTo(15).Within(Tolerance));
            Assert.That(geometry.Bounds.Top, Is.EqualTo(22).Within(Tolerance));
            Assert.That(geometry.Bounds.Width, Is.EqualTo(20).Within(Tolerance));
            Assert.That(geometry.Bounds.Height, Is.EqualTo(30).Within(Tolerance));
        }

        [Test]
        public void 四角形拡大()
        {
            var geometry = PathGeometry.CreateFromGeometry(new RectangleGeometry(new Rect(0, 0, 10, 20)));
            geometry = GeometryCreator.Scale(geometry, 2.0, 3.0);
            Assert.That(geometry.Bounds.Left, Is.EqualTo(0));
            Assert.That(geometry.Bounds.Top, Is.EqualTo(0));
            Assert.That(geometry.Bounds.Width, Is.EqualTo(20).Within(Tolerance));
            Assert.That(geometry.Bounds.Height, Is.EqualTo(60).Within(Tolerance));
        }

        [Test]
        public void 四角形縮小()
        {
            var geometry = PathGeometry.CreateFromGeometry(new RectangleGeometry(new Rect(0, 0, 40, 80)));
            geometry = GeometryCreator.Scale(geometry, 0.5, 0.25);
            Assert.That(geometry.Bounds.Width, Is.EqualTo(20).Within(Tolerance));
            Assert.That(geometry.Bounds.Height, Is.EqualTo(20).Within(Tolerance));
        }

        [Test]
        public void 楕円拡大_Cコマンド分岐()
        {
            var geometry = PathGeometry.CreateFromGeometry(new EllipseGeometry(new Point(10, 15), 10, 15));
            geometry = GeometryCreator.Scale(geometry, 2.0, 2.0);
            Assert.That(geometry.Bounds.Width, Is.EqualTo(40).Within(1e-3));
            Assert.That(geometry.Bounds.Height, Is.EqualTo(60).Within(1e-3));
        }

        [Test]
        public void 四角形回転_90度()
        {
            var geometry = PathGeometry.CreateFromGeometry(new RectangleGeometry(new Rect(0, 0, 20, 10)));
            geometry = GeometryCreator.Rotate(geometry, 90, new Point(10, 5));
            // 中心(10,5)で90度回転すると、20x10 が 10x20 になる
            Assert.That(geometry.Bounds.Width, Is.EqualTo(10).Within(Tolerance));
            Assert.That(geometry.Bounds.Height, Is.EqualTo(20).Within(Tolerance));
        }

        [Test]
        public void 四角形回転_180度()
        {
            var geometry = PathGeometry.CreateFromGeometry(new RectangleGeometry(new Rect(0, 0, 20, 10)));
            geometry = GeometryCreator.Rotate(geometry, 180, new Point(10, 5));
            // 中心点で180度回転 → 同じ Bounds
            Assert.That(geometry.Bounds.Left, Is.EqualTo(0).Within(Tolerance));
            Assert.That(geometry.Bounds.Top, Is.EqualTo(0).Within(Tolerance));
            Assert.That(geometry.Bounds.Width, Is.EqualTo(20).Within(Tolerance));
            Assert.That(geometry.Bounds.Height, Is.EqualTo(10).Within(Tolerance));
        }

        [Test]
        public void 四角形回転は元を破壊しない()
        {
            var original = PathGeometry.CreateFromGeometry(new RectangleGeometry(new Rect(0, 0, 20, 10)));
            var rotated = GeometryCreator.Rotate(original, 90, new Point(10, 5));
            // Rotate は Clone するので、元 geometry の Bounds は変わらない
            Assert.That(original.Bounds.Width, Is.EqualTo(20));
            Assert.That(original.Bounds.Height, Is.EqualTo(10));
            Assert.That(rotated, Is.Not.SameAs(original));
        }

        [Test]
        public void Pie_90度時計回り_閉じた1Figure3Segment()
        {
            var pg = GeometryCreator.CreatePie(new Point(0, 0), 100, 0, 90, SweepDirection.Clockwise);
            Assert.That(pg.Figures.Count, Is.EqualTo(1));
            var fig = pg.Figures[0];
            Assert.That(fig.IsClosed, Is.True);
            // ArcSegment + LineSegment(中心へ) + LineSegment(始点へ)
            Assert.That(fig.Segments.Count, Is.EqualTo(3));
            Assert.That(fig.Segments[0], Is.InstanceOf<ArcSegment>());
            Assert.That(fig.Segments[1], Is.InstanceOf<LineSegment>());
            Assert.That(fig.Segments[2], Is.InstanceOf<LineSegment>());
            var arc = (ArcSegment)fig.Segments[0];
            // 90度なので isLargeArc は false
            Assert.That(arc.IsLargeArc, Is.False);
            Assert.That(arc.SweepDirection, Is.EqualTo(SweepDirection.Clockwise));
        }

        [Test]
        public void Pie_270度時計回りはIsLargeArcがtrue()
        {
            var pg = GeometryCreator.CreatePie(new Point(0, 0), 100, 0, 270, SweepDirection.Clockwise);
            var arc = (ArcSegment)pg.Figures[0].Segments[0];
            Assert.That(arc.IsLargeArc, Is.True);
        }

        [Test]
        public void Pie_反時計回り180度未満はIsLargeArcがfalse()
        {
            // Counterclockwise で 90 → 0 は 90度 → isLargeArc false
            var pg = GeometryCreator.CreatePie(new Point(0, 0), 100, 90, 0, SweepDirection.Counterclockwise);
            var arc = (ArcSegment)pg.Figures[0].Segments[0];
            Assert.That(arc.IsLargeArc, Is.False);
            Assert.That(arc.SweepDirection, Is.EqualTo(SweepDirection.Counterclockwise));
        }

        [Test]
        public void Donut_時計回り_2ArcSegmentと2LineSegment()
        {
            var pg = GeometryCreator.CreateDonut(new Point(0, 0), width: 20, distance: 100, startDeg: 0, stopDeg: 90, SweepDirection.Clockwise);
            Assert.That(pg.Figures.Count, Is.EqualTo(1));
            var fig = pg.Figures[0];
            Assert.That(fig.IsClosed, Is.True);
            Assert.That(fig.Segments.Count, Is.EqualTo(4));
            Assert.That(fig.Segments[0], Is.InstanceOf<ArcSegment>());
            Assert.That(fig.Segments[1], Is.InstanceOf<LineSegment>());
            Assert.That(fig.Segments[2], Is.InstanceOf<ArcSegment>());
            Assert.That(fig.Segments[3], Is.InstanceOf<LineSegment>());

            var outer = (ArcSegment)fig.Segments[0];
            var inner = (ArcSegment)fig.Segments[2];
            Assert.That(outer.SweepDirection, Is.EqualTo(SweepDirection.Clockwise));
            // 内側は外側と逆方向
            Assert.That(inner.SweepDirection, Is.EqualTo(SweepDirection.Counterclockwise));
            // 外側半径は distance、内側半径は distance - width
            Assert.That(outer.Size.Width, Is.EqualTo(100).Within(Tolerance));
            Assert.That(inner.Size.Width, Is.EqualTo(80).Within(Tolerance));
        }

        [Test]
        public void Donut_270度はIsLargeArcがtrue()
        {
            var pg = GeometryCreator.CreateDonut(new Point(0, 0), width: 10, distance: 50, startDeg: 0, stopDeg: 270, SweepDirection.Clockwise);
            var outer = (ArcSegment)pg.Figures[0].Segments[0];
            var inner = (ArcSegment)pg.Figures[0].Segments[2];
            Assert.That(outer.IsLargeArc, Is.True);
            Assert.That(inner.IsLargeArc, Is.True);
        }
    }
}
