using boilersGraphics.Models.Connectors;
using NUnit.Framework;
using System.Windows;

namespace boilersGraphics.Test.Models.Connectors;

[TestFixture]
public class OrthogonalRouterTest
{
    [Test]
    public void Auto_水平差大なる_HFirstと同じ中間点()
    {
        // (0,0) -> (100, 20): |dx|=100 >= |dy|=20 → HFirst (横→縦) → mid = (100, 0)
        var mid = OrthogonalRouter.ComputeMidPoints(
            new Point(0, 0), new Point(100, 20), OrthogonalRoutingMode.Auto, null);
        Assert.That(mid, Has.Count.EqualTo(1));
        Assert.That(mid[0], Is.EqualTo(new Point(100, 0)));
    }

    [Test]
    public void Auto_垂直差大なる_VFirstと同じ中間点()
    {
        // (0,0) -> (20, 100): |dx|=20 < |dy|=100 → VFirst (縦→横) → mid = (0, 100)
        var mid = OrthogonalRouter.ComputeMidPoints(
            new Point(0, 0), new Point(20, 100), OrthogonalRoutingMode.Auto, null);
        Assert.That(mid, Has.Count.EqualTo(1));
        Assert.That(mid[0], Is.EqualTo(new Point(0, 100)));
    }

    [Test]
    public void Auto_差分が等しい_HFirstを優先()
    {
        // dx == dy のときは HFirst (>=)
        var mid = OrthogonalRouter.ComputeMidPoints(
            new Point(0, 0), new Point(50, 50), OrthogonalRoutingMode.Auto, null);
        Assert.That(mid[0], Is.EqualTo(new Point(50, 0)));
    }

    [Test]
    public void HFirst_中間点はEndX_BeginY()
    {
        var mid = OrthogonalRouter.ComputeMidPoints(
            new Point(10, 20), new Point(80, 90), OrthogonalRoutingMode.HFirst, null);
        Assert.That(mid, Has.Count.EqualTo(1));
        Assert.That(mid[0], Is.EqualTo(new Point(80, 20)));
    }

    [Test]
    public void VFirst_中間点はBeginX_EndY()
    {
        var mid = OrthogonalRouter.ComputeMidPoints(
            new Point(10, 20), new Point(80, 90), OrthogonalRoutingMode.VFirst, null);
        Assert.That(mid, Has.Count.EqualTo(1));
        Assert.That(mid[0], Is.EqualTo(new Point(10, 90)));
    }

    [Test]
    public void Manual_ユーザMidPointsをコピーして返す()
    {
        var input = new[] { new Point(10, 0), new Point(20, 50), new Point(30, 50) };
        var mid = OrthogonalRouter.ComputeMidPoints(
            new Point(0, 0), new Point(40, 50), OrthogonalRoutingMode.Manual, input);
        Assert.That(mid, Has.Count.EqualTo(3));
        Assert.That(mid[0], Is.EqualTo(new Point(10, 0)));
        Assert.That(mid[1], Is.EqualTo(new Point(20, 50)));
        Assert.That(mid[2], Is.EqualTo(new Point(30, 50)));
    }

    [Test]
    public void Manual_nullMidPoints_空配列を返す()
    {
        var mid = OrthogonalRouter.ComputeMidPoints(
            new Point(0, 0), new Point(40, 50), OrthogonalRoutingMode.Manual, null);
        Assert.That(mid, Is.Empty);
    }

    [Test]
    public void Auto_BeginとEndが同一_HFirst扱いで中間点はEnd相当()
    {
        // dx=dy=0 → HFirst → mid = (End.X, Begin.Y) = (0,0)
        var mid = OrthogonalRouter.ComputeMidPoints(
            new Point(5, 5), new Point(5, 5), OrthogonalRoutingMode.Auto, null);
        Assert.That(mid[0], Is.EqualTo(new Point(5, 5)));
    }
}
