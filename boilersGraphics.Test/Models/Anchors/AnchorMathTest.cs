using boilersGraphics.Models.Anchors;
using NUnit.Framework;
using System;
using System.Threading;

namespace boilersGraphics.Test.Models.Anchors;

[TestFixture]
public class AnchorMathTest
{
    [Test, Apartment(ApartmentState.STA)]
    public void ToWorld_回転0_RelativeX0Y0_左上が原点に()
    {
        var p = AnchorMath.ToWorld(left: 10, top: 20, width: 100, height: 50, rotationDegrees: 0,
            relativeX: 0, relativeY: 0);
        Assert.That(p.X, Is.EqualTo(10).Within(1e-6));
        Assert.That(p.Y, Is.EqualTo(20).Within(1e-6));
    }

    [Test, Apartment(ApartmentState.STA)]
    public void ToWorld_回転0_RelativeX1Y1_右下が右下に()
    {
        var p = AnchorMath.ToWorld(left: 10, top: 20, width: 100, height: 50, rotationDegrees: 0,
            relativeX: 1, relativeY: 1);
        Assert.That(p.X, Is.EqualTo(110).Within(1e-6));
        Assert.That(p.Y, Is.EqualTo(70).Within(1e-6));
    }

    [Test, Apartment(ApartmentState.STA)]
    public void ToWorld_回転0_中心は中心()
    {
        var p = AnchorMath.ToWorld(left: 0, top: 0, width: 100, height: 100, rotationDegrees: 0,
            relativeX: 0.5, relativeY: 0.5);
        Assert.That(p.X, Is.EqualTo(50).Within(1e-6));
        Assert.That(p.Y, Is.EqualTo(50).Within(1e-6));
    }

    [Test, Apartment(ApartmentState.STA)]
    public void ToWorld_90度回転_中心点は不動()
    {
        // 中心点 (RelativeX=0.5, Y=0.5) は回転に無関係
        var p = AnchorMath.ToWorld(left: 0, top: 0, width: 100, height: 100, rotationDegrees: 90,
            relativeX: 0.5, relativeY: 0.5);
        Assert.That(p.X, Is.EqualTo(50).Within(1e-6));
        Assert.That(p.Y, Is.EqualTo(50).Within(1e-6));
    }

    [Test, Apartment(ApartmentState.STA)]
    public void ToWorld_90度回転_左上は左下へ()
    {
        // 中心 (50,50) で 90 度回転 → ローカル (0,0) は (100, 0)
        // 詳細: dx=0-50=-50, dy=0-50=-50; cos90=0, sin90=1
        // worldX = 50 + (-50)*0 - (-50)*1 = 50 + 50 = 100
        // worldY = 50 + (-50)*1 + (-50)*0 = 50 - 50 = 0
        var p = AnchorMath.ToWorld(left: 0, top: 0, width: 100, height: 100, rotationDegrees: 90,
            relativeX: 0, relativeY: 0);
        Assert.That(p.X, Is.EqualTo(100).Within(1e-6));
        Assert.That(p.Y, Is.EqualTo(0).Within(1e-6));
    }

    [Test, Apartment(ApartmentState.STA)]
    public void ToWorld_180度回転_左上は右下へ()
    {
        var p = AnchorMath.ToWorld(left: 0, top: 0, width: 100, height: 100, rotationDegrees: 180,
            relativeX: 0, relativeY: 0);
        Assert.That(p.X, Is.EqualTo(100).Within(1e-6));
        Assert.That(p.Y, Is.EqualTo(100).Within(1e-6));
    }

    [Test]
    public void RelativeOf_全9点_予約座標と一致()
    {
        Assert.That(AnchorMath.RelativeOf(AnchorPosition.TopLeft), Is.EqualTo((0.0, 0.0)));
        Assert.That(AnchorMath.RelativeOf(AnchorPosition.TopCenter), Is.EqualTo((0.5, 0.0)));
        Assert.That(AnchorMath.RelativeOf(AnchorPosition.TopRight), Is.EqualTo((1.0, 0.0)));
        Assert.That(AnchorMath.RelativeOf(AnchorPosition.LeftCenter), Is.EqualTo((0.0, 0.5)));
        Assert.That(AnchorMath.RelativeOf(AnchorPosition.Center), Is.EqualTo((0.5, 0.5)));
        Assert.That(AnchorMath.RelativeOf(AnchorPosition.RightCenter), Is.EqualTo((1.0, 0.5)));
        Assert.That(AnchorMath.RelativeOf(AnchorPosition.BottomLeft), Is.EqualTo((0.0, 1.0)));
        Assert.That(AnchorMath.RelativeOf(AnchorPosition.BottomCenter), Is.EqualTo((0.5, 1.0)));
        Assert.That(AnchorMath.RelativeOf(AnchorPosition.BottomRight), Is.EqualTo((1.0, 1.0)));
    }

    [Test]
    public void ParseReserved_全9予約語_正しくパース()
    {
        Assert.That(AnchorMath.ParseReserved("tl"), Is.EqualTo(AnchorPosition.TopLeft));
        Assert.That(AnchorMath.ParseReserved("tc"), Is.EqualTo(AnchorPosition.TopCenter));
        Assert.That(AnchorMath.ParseReserved("tr"), Is.EqualTo(AnchorPosition.TopRight));
        Assert.That(AnchorMath.ParseReserved("lc"), Is.EqualTo(AnchorPosition.LeftCenter));
        Assert.That(AnchorMath.ParseReserved("c"), Is.EqualTo(AnchorPosition.Center));
        Assert.That(AnchorMath.ParseReserved("rc"), Is.EqualTo(AnchorPosition.RightCenter));
        Assert.That(AnchorMath.ParseReserved("bl"), Is.EqualTo(AnchorPosition.BottomLeft));
        Assert.That(AnchorMath.ParseReserved("bc"), Is.EqualTo(AnchorPosition.BottomCenter));
        Assert.That(AnchorMath.ParseReserved("br"), Is.EqualTo(AnchorPosition.BottomRight));
    }

    [Test]
    public void ParseReserved_未知予約語_nullを返す()
    {
        Assert.That(AnchorMath.ParseReserved("xx"), Is.Null);
        Assert.That(AnchorMath.ParseReserved(string.Empty), Is.Null);
        Assert.That(AnchorMath.ParseReserved(null), Is.Null);
    }

    [Test]
    public void ToReserved_全9点_予約語と一致()
    {
        foreach (AnchorPosition pos in Enum.GetValues<AnchorPosition>())
        {
            var reserved = AnchorMath.ToReserved(pos);
            var roundtrip = AnchorMath.ParseReserved(reserved);
            Assert.That(roundtrip, Is.EqualTo(pos), $"ToReserved/ParseReserved roundtrip failed for {pos}");
        }
    }
}
