using boilersGraphics.Helpers.Animation.Export;
using NUnit.Framework;

namespace boilersGraphics.Test.Helpers.Animation.Export;

[TestFixture]
public class MauiPropertyToCSharpMapperTest
{
    [Test]
    public void Left_は_AbsoluteLayout_LayoutBoundsのX成分を更新()
    {
        var m = MauiPropertyToCSharpMapper.TryMap("Left.Value");
        Assert.That(m, Is.Not.Null);
        Assert.That(m!.Kind, Is.EqualTo(MauiAnimationKind.Double));
        Assert.That(m.DoubleCallbackTemplate, Does.Contain("AbsoluteLayout.GetLayoutBounds({0})"));
        Assert.That(m.DoubleCallbackTemplate, Does.Contain("new Rect(d, __b.Y"));
        Assert.That(m.ColorCallbackTemplate, Is.Null);
    }

    [Test]
    public void Top_は_AbsoluteLayout_LayoutBoundsのY成分を更新()
    {
        var m = MauiPropertyToCSharpMapper.TryMap("Top.Value");
        Assert.That(m!.DoubleCallbackTemplate, Does.Contain("new Rect(__b.X, d"));
    }

    [TestCase("Width.Value", "{0}.WidthRequest = d;")]
    [TestCase("Height.Value", "{0}.HeightRequest = d;")]
    [TestCase("RotationAngle.Value", "{0}.Rotation = d;")]
    [TestCase("EdgeThickness.Value", "{0}.StrokeThickness = d;")]
    public void Double系プロパティ_Template一致(string path, string expected)
    {
        var m = MauiPropertyToCSharpMapper.TryMap(path);
        Assert.That(m, Is.Not.Null);
        Assert.That(m!.Kind, Is.EqualTo(MauiAnimationKind.Double));
        Assert.That(m.DoubleCallbackTemplate, Is.EqualTo(expected));
        Assert.That(m.ColorCallbackTemplate, Is.Null);
    }

    [Test]
    public void Glow系は_Shadow_nullガード付き()
    {
        var r = MauiPropertyToCSharpMapper.TryMap("GlowRadius.Value");
        Assert.That(r!.DoubleCallbackTemplate, Does.StartWith("if ({0}.Shadow != null)"));
        Assert.That(r.DoubleCallbackTemplate, Does.Contain("{0}.Shadow.Radius = d;"));

        var i = MauiPropertyToCSharpMapper.TryMap("GlowIntensity.Value");
        Assert.That(i!.DoubleCallbackTemplate, Does.Contain("{0}.Shadow.Opacity = (float)d;"));

        var c = MauiPropertyToCSharpMapper.TryMap("GlowColor.Value");
        Assert.That(c!.Kind, Is.EqualTo(MauiAnimationKind.Color));
        Assert.That(c.ColorCallbackTemplate, Does.Contain("{0}.Shadow.Brush = new SolidColorBrush(c);"));
    }

    [TestCase("EdgeBrush.Value", "{0}.Stroke = new SolidColorBrush(c);")]
    [TestCase("FillBrush.Value", "{0}.Fill = new SolidColorBrush(c);")]
    public void Color系プロパティ(string path, string expected)
    {
        var m = MauiPropertyToCSharpMapper.TryMap(path);
        Assert.That(m, Is.Not.Null);
        Assert.That(m!.Kind, Is.EqualTo(MauiAnimationKind.Color));
        Assert.That(m.ColorCallbackTemplate, Is.EqualTo(expected));
        Assert.That(m.DoubleCallbackTemplate, Is.Null);
    }

    [TestCase("Unknown.Value")]
    [TestCase("")]
    [TestCase("ExposedProperties[xxx]")]
    public void 未対応パスは_null(string path)
    {
        Assert.That(MauiPropertyToCSharpMapper.TryMap(path), Is.Null);
        Assert.That(MauiPropertyToCSharpMapper.IsSupported(path), Is.False);
    }

    [Test]
    public void null_でも_NRE_せず_null()
    {
        Assert.That(MauiPropertyToCSharpMapper.TryMap(null), Is.Null);
    }

    [Test]
    public void IsSupported_対応パスでtrue()
    {
        Assert.That(MauiPropertyToCSharpMapper.IsSupported("Left.Value"), Is.True);
        Assert.That(MauiPropertyToCSharpMapper.IsSupported("EdgeBrush.Value"), Is.True);
    }

    [Test]
    public void PropertyApplier対応の_11パス全件_TryMapで非null()
    {
        string[] paths =
        {
            "Left.Value", "Top.Value", "Width.Value", "Height.Value",
            "RotationAngle.Value", "EdgeBrush.Value", "FillBrush.Value",
            "EdgeThickness.Value", "GlowRadius.Value", "GlowIntensity.Value",
            "GlowColor.Value",
        };
        foreach (var p in paths)
        {
            Assert.That(MauiPropertyToCSharpMapper.TryMap(p), Is.Not.Null, $"{p} should be mapped");
        }
    }
}
