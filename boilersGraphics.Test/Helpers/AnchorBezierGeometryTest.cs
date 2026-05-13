using boilersGraphics.Helpers;
using NUnit.Framework;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace boilersGraphics.Test.Helpers;

[TestFixture]
public class AnchorBezierGeometryTest
{
    [Test, Apartment(ApartmentState.STA)]
    public void CreateAnchorBezier_4点で1本のBezierSegmentが作られる()
    {
        var geom = GeometryCreator.CreateAnchorBezier(
            begin: new Point(0, 0),
            beginControl: new Point(20, 50),
            endControl: new Point(80, 50),
            end: new Point(100, 0));

        Assert.That(geom.Figures.Count, Is.EqualTo(1));
        var fig = geom.Figures[0];
        Assert.That(fig.StartPoint, Is.EqualTo(new Point(0, 0)));
        Assert.That(fig.IsClosed, Is.False);
        Assert.That(fig.IsFilled, Is.False);
        Assert.That(fig.Segments.Count, Is.EqualTo(1));
        Assert.That(fig.Segments[0], Is.TypeOf<BezierSegment>());

        var seg = (BezierSegment)fig.Segments[0];
        Assert.That(seg.Point1, Is.EqualTo(new Point(20, 50)));
        Assert.That(seg.Point2, Is.EqualTo(new Point(80, 50)));
        Assert.That(seg.Point3, Is.EqualTo(new Point(100, 0)));
    }

    [Test, Apartment(ApartmentState.STA)]
    public void CreateAnchorBezier_Begin等しいEnd_退化ベジエでも例外なく生成()
    {
        var geom = GeometryCreator.CreateAnchorBezier(
            begin: new Point(50, 50),
            beginControl: new Point(50, 50),
            endControl: new Point(50, 50),
            end: new Point(50, 50));
        Assert.That(geom.Figures.Count, Is.EqualTo(1));
    }
}
