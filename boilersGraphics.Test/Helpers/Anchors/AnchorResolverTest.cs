using boilersGraphics.Helpers.Anchors;
using boilersGraphics.Models.Anchors;
using NUnit.Framework;
using System;

namespace boilersGraphics.Test.Helpers.Anchors;

[TestFixture]
public class AnchorResolverTest
{
    [Test]
    public void Resolve_diagramがnull_nullを返す()
    {
        var result = AnchorResolver.Resolve(null, "00000000-0000-0000-0000-000000000000");
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Resolve_anchorRefがnull_nullを返す()
    {
        var result = AnchorResolver.Resolve(null, null);
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Resolve_anchorRefが空文字_nullを返す()
    {
        var result = AnchorResolver.Resolve(null, string.Empty);
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Resolve_暗黙アンカーRefだがOwnerGuid不正_nullを返す()
    {
        // # を含むが手前が Guid でない
        var result = AnchorResolver.Resolve(null, "not-a-guid#tl");
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Resolve_暗黙アンカーRefだが予約語不正_nullを返す()
    {
        var guid = Guid.NewGuid();
        var result = AnchorResolver.Resolve(null, $"{guid}#xx");
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Resolve_明示AnchorRefだがGuidパース失敗_nullを返す()
    {
        var result = AnchorResolver.Resolve(null, "not-a-guid");
        Assert.That(result, Is.Null);
    }

    [Test]
    public void BuildImplicitRef_OwnerGuidと予約語の連結()
    {
        var guid = Guid.Parse("aabbccdd-1122-3344-5566-778899aabbcc");
        var refText = AnchorResolver.BuildImplicitRef(guid, AnchorPosition.TopLeft);
        Assert.That(refText, Is.EqualTo("aabbccdd-1122-3344-5566-778899aabbcc#tl"));
    }

    [Test]
    public void BuildImplicitRef_全9位置_予約語が正しく付与される()
    {
        var guid = Guid.NewGuid();
        var expected = new[]
        {
            (AnchorPosition.TopLeft, "tl"),
            (AnchorPosition.TopCenter, "tc"),
            (AnchorPosition.TopRight, "tr"),
            (AnchorPosition.LeftCenter, "lc"),
            (AnchorPosition.Center, "c"),
            (AnchorPosition.RightCenter, "rc"),
            (AnchorPosition.BottomLeft, "bl"),
            (AnchorPosition.BottomCenter, "bc"),
            (AnchorPosition.BottomRight, "br"),
        };
        foreach (var (pos, reserved) in expected)
        {
            var refText = AnchorResolver.BuildImplicitRef(guid, pos);
            Assert.That(refText, Is.EqualTo($"{guid}#{reserved}"));
        }
    }

    [Test]
    public void BuildExplicitRef_Guid文字列を返す()
    {
        var guid = Guid.Parse("aabbccdd-1122-3344-5566-778899aabbcc");
        Assert.That(AnchorResolver.BuildExplicitRef(guid), Is.EqualTo("aabbccdd-1122-3344-5566-778899aabbcc"));
    }
}
