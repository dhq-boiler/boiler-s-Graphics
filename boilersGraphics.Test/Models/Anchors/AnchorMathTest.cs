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

    // Phase 3.5: ToRelative (ToWorld の逆変換) のテスト

    [Test, Apartment(ApartmentState.STA)]
    public void ToRelative_回転0_左上ワールドはRelative0_0()
    {
        var (rx, ry) = AnchorMath.ToRelative(left: 10, top: 20, width: 100, height: 50,
            rotationDegrees: 0, worldX: 10, worldY: 20);
        Assert.That(rx, Is.EqualTo(0).Within(1e-6));
        Assert.That(ry, Is.EqualTo(0).Within(1e-6));
    }

    [Test, Apartment(ApartmentState.STA)]
    public void ToRelative_回転0_右下ワールドはRelative1_1()
    {
        var (rx, ry) = AnchorMath.ToRelative(left: 10, top: 20, width: 100, height: 50,
            rotationDegrees: 0, worldX: 110, worldY: 70);
        Assert.That(rx, Is.EqualTo(1).Within(1e-6));
        Assert.That(ry, Is.EqualTo(1).Within(1e-6));
    }

    [Test, Apartment(ApartmentState.STA)]
    public void ToRelative_90度回転_中心点ワールド50_50はRelative0_5()
    {
        // ToWorld(rot=90, rel=0.5,0.5) = (50,50) の逆
        var (rx, ry) = AnchorMath.ToRelative(left: 0, top: 0, width: 100, height: 100,
            rotationDegrees: 90, worldX: 50, worldY: 50);
        Assert.That(rx, Is.EqualTo(0.5).Within(1e-6));
        Assert.That(ry, Is.EqualTo(0.5).Within(1e-6));
    }

    [Test, Apartment(ApartmentState.STA)]
    public void ToRelative_90度回転_ワールド100_0はRelative0_0()
    {
        // ToWorld(rot=90, rel=0,0) = (100,0) の逆。左上が右上へ移動するパターン。
        var (rx, ry) = AnchorMath.ToRelative(left: 0, top: 0, width: 100, height: 100,
            rotationDegrees: 90, worldX: 100, worldY: 0);
        Assert.That(rx, Is.EqualTo(0).Within(1e-6));
        Assert.That(ry, Is.EqualTo(0).Within(1e-6));
    }

    [Test, Apartment(ApartmentState.STA)]
    public void ToRelative_180度回転_ワールド100_100はRelative0_0()
    {
        // ToWorld(rot=180, rel=0,0) = (100,100) の逆
        var (rx, ry) = AnchorMath.ToRelative(left: 0, top: 0, width: 100, height: 100,
            rotationDegrees: 180, worldX: 100, worldY: 100);
        Assert.That(rx, Is.EqualTo(0).Within(1e-6));
        Assert.That(ry, Is.EqualTo(0).Within(1e-6));
    }

    [Test, Apartment(ApartmentState.STA)]
    public void ToWorld_ToRelative_は互いに逆関数_45度回転()
    {
        // 任意の (relX, relY) を ToWorld → ToRelative して戻ること
        const double left = 30, top = 40, width = 80, height = 60, rot = 45;
        foreach (var (rx, ry) in new[] { (0.0, 0.0), (1.0, 0.0), (0.0, 1.0), (1.0, 1.0), (0.5, 0.5), (0.25, 0.75) })
        {
            var w = AnchorMath.ToWorld(left, top, width, height, rot, rx, ry);
            var (rx2, ry2) = AnchorMath.ToRelative(left, top, width, height, rot, w.X, w.Y);
            Assert.That(rx2, Is.EqualTo(rx).Within(1e-6), $"RelativeX roundtrip ({rx},{ry})");
            Assert.That(ry2, Is.EqualTo(ry).Within(1e-6), $"RelativeY roundtrip ({rx},{ry})");
        }
    }

    [Test, Apartment(ApartmentState.STA)]
    public void ToRelative_width0_退化図形は0_0()
    {
        var (rx, ry) = AnchorMath.ToRelative(left: 0, top: 0, width: 0, height: 100,
            rotationDegrees: 0, worldX: 50, worldY: 50);
        Assert.That(rx, Is.EqualTo(0));
        Assert.That(ry, Is.EqualTo(0));
    }

    [Test, Apartment(ApartmentState.STA)]
    public void ToRelative_height負値も退化扱い()
    {
        var (rx, ry) = AnchorMath.ToRelative(left: 0, top: 0, width: 100, height: -5,
            rotationDegrees: 0, worldX: 50, worldY: 50);
        Assert.That(rx, Is.EqualTo(0));
        Assert.That(ry, Is.EqualTo(0));
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
