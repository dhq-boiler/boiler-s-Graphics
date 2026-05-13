using boilersGraphics.Helpers;
using NUnit.Framework;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace boilersGraphics.Test.Helpers;

[TestFixture]
public class OrthogonalGeometryTest
{
    [Test, Apartment(ApartmentState.STA)]
    public void CreateOrthogonal_中間点なし_2点直線になる()
    {
        var geom = GeometryCreator.CreateOrthogonal(new Point(0, 0), null, new Point(100, 50), 0);
        Assert.That(geom.Figures.Count, Is.EqualTo(1));
        var fig = geom.Figures[0];
        Assert.That(fig.StartPoint, Is.EqualTo(new Point(0, 0)));
        Assert.That(fig.Segments.Count, Is.EqualTo(1));
        Assert.That(fig.Segments[0], Is.TypeOf<LineSegment>());
        Assert.That(((LineSegment)fig.Segments[0]).Point, Is.EqualTo(new Point(100, 50)));
    }

    [Test, Apartment(ApartmentState.STA)]
    public void CreateOrthogonal_HFirst中間点_3点折れ線()
    {
        // HFirst: Begin(0,0) → Mid(100, 0) → End(100, 50)
        var geom = GeometryCreator.CreateOrthogonal(
            new Point(0, 0),
            new[] { new Point(100, 0) },
            new Point(100, 50),
            cornerRadius: 0);
        var fig = geom.Figures[0];
        Assert.That(fig.StartPoint, Is.EqualTo(new Point(0, 0)));
        Assert.That(fig.Segments.Count, Is.EqualTo(2));
        Assert.That(((LineSegment)fig.Segments[0]).Point, Is.EqualTo(new Point(100, 0)));
        Assert.That(((LineSegment)fig.Segments[1]).Point, Is.EqualTo(new Point(100, 50)));
    }

    [Test, Apartment(ApartmentState.STA)]
    public void CreateOrthogonal_CornerRadius0_全部LineSegment()
    {
        var geom = GeometryCreator.CreateOrthogonal(
            new Point(0, 0),
            new[] { new Point(100, 0), new Point(100, 50) },
            new Point(200, 50),
            cornerRadius: 0);
        var fig = geom.Figures[0];
        Assert.That(fig.Segments.All(s => s is LineSegment), Is.True);
        Assert.That(fig.Segments.Count, Is.EqualTo(3));
    }

    [Test, Apartment(ApartmentState.STA)]
    public void CreateOrthogonal_CornerRadius正_各折れ点がArcSegmentで丸まる()
    {
        // 中間点 1 つ + CornerRadius=10
        // 期待: LineSegment(pIn) + ArcSegment(pOut) + LineSegment(End)
        var geom = GeometryCreator.CreateOrthogonal(
            new Point(0, 0),
            new[] { new Point(100, 0) },
            new Point(100, 50),
            cornerRadius: 10);
        var fig = geom.Figures[0];
        Assert.That(fig.Segments.Count, Is.EqualTo(3));
        Assert.That(fig.Segments[0], Is.TypeOf<LineSegment>(), "pIn まで線");
        Assert.That(fig.Segments[1], Is.TypeOf<ArcSegment>(), "折れ点 ArcSegment");
        Assert.That(fig.Segments[2], Is.TypeOf<LineSegment>(), "終点まで線");

        // pIn = (90, 0), pOut = (100, 10)
        Assert.That(((LineSegment)fig.Segments[0]).Point, Is.EqualTo(new Point(90, 0)));
        var arc = (ArcSegment)fig.Segments[1];
        Assert.That(arc.Point, Is.EqualTo(new Point(100, 10)));
        Assert.That(arc.Size, Is.EqualTo(new Size(10, 10)));
    }

    [Test, Apartment(ApartmentState.STA)]
    public void CreateOrthogonal_CornerRadius大なる_辺の半分にクランプ()
    {
        // 辺長 100 と 50 の折れ点で CornerRadius=200 を指定 → 50/2=25 にクランプ
        var geom = GeometryCreator.CreateOrthogonal(
            new Point(0, 0),
            new[] { new Point(100, 0) },
            new Point(100, 50),
            cornerRadius: 200);
        var fig = geom.Figures[0];
        var arc = (ArcSegment)fig.Segments[1];
        Assert.That(arc.Size, Is.EqualTo(new Size(25, 25)));
    }

    [Test, Apartment(ApartmentState.STA)]
    public void CreateOrthogonal_中間点複数_全て折れる()
    {
        // 0,0 → 100,0 → 100,50 → 200,50 のジグザグ + 角丸 5
        var geom = GeometryCreator.CreateOrthogonal(
            new Point(0, 0),
            new[] { new Point(100, 0), new Point(100, 50) },
            new Point(200, 50),
            cornerRadius: 5);
        var fig = geom.Figures[0];
        // 2 折れ点 → 各折れ点で Line+Arc + 最後の End まで Line = 5 セグメント
        Assert.That(fig.Segments.Count, Is.EqualTo(5));
        Assert.That(fig.Segments[1], Is.TypeOf<ArcSegment>());
        Assert.That(fig.Segments[3], Is.TypeOf<ArcSegment>());
    }
}
