using boilersGraphics.Helpers.Animation.Export;
using NUnit.Framework;

namespace boilersGraphics.Test.Helpers.Animation.Export;

[TestFixture]
public class PropertyToXamlMapperTest
{
    [TestCase("Left.Value", "(Canvas.Left)")]
    [TestCase("Top.Value", "(Canvas.Top)")]
    [TestCase("Width.Value", "Width")]
    [TestCase("Height.Value", "Height")]
    [TestCase("RotationAngle.Value", "(UIElement.RenderTransform).(RotateTransform.Angle)")]
    [TestCase("EdgeThickness.Value", "StrokeThickness")]
    [TestCase("GlowRadius.Value", "(UIElement.Effect).(DropShadowEffect.BlurRadius)")]
    [TestCase("GlowIntensity.Value", "(UIElement.Effect).(DropShadowEffect.Opacity)")]
    public void Double系プロパティの_TargetProperty(string path, string expectedTarget)
    {
        var m = PropertyToXamlMapper.TryMapWpf(path);
        Assert.That(m, Is.Not.Null);
        Assert.That(m!.TargetProperty, Is.EqualTo(expectedTarget));
        Assert.That(m.AnimationElementName, Is.EqualTo("DoubleAnimationUsingKeyFrames"));
        Assert.That(m.EasingKeyFrameElementName, Is.EqualTo("EasingDoubleKeyFrame"));
        Assert.That(m.LinearKeyFrameElementName, Is.EqualTo("LinearDoubleKeyFrame"));
    }

    [TestCase("EdgeBrush.Value", "(Shape.Stroke).(SolidColorBrush.Color)")]
    [TestCase("FillBrush.Value", "(Shape.Fill).(SolidColorBrush.Color)")]
    [TestCase("GlowColor.Value", "(UIElement.Effect).(DropShadowEffect.Color)")]
    public void Color系プロパティの_TargetProperty(string path, string expectedTarget)
    {
        var m = PropertyToXamlMapper.TryMapWpf(path);
        Assert.That(m, Is.Not.Null);
        Assert.That(m!.TargetProperty, Is.EqualTo(expectedTarget));
        Assert.That(m.AnimationElementName, Is.EqualTo("ColorAnimationUsingKeyFrames"));
        Assert.That(m.EasingKeyFrameElementName, Is.EqualTo("EasingColorKeyFrame"));
        Assert.That(m.LinearKeyFrameElementName, Is.EqualTo("LinearColorKeyFrame"));
    }

    [TestCase("Unknown.Value")]
    [TestCase("")]
    [TestCase("ExposedProperties[xxx]")]  // 展開後に正規パスへ解決される前提
    public void 未対応パスは_null(string path)
    {
        Assert.That(PropertyToXamlMapper.TryMapWpf(path), Is.Null);
        Assert.That(PropertyToXamlMapper.IsSupportedWpf(path), Is.False);
    }

    [Test]
    public void null_でも_NRE_せず_null_を返す()
    {
        Assert.That(PropertyToXamlMapper.TryMapWpf(null), Is.Null);
        Assert.That(PropertyToXamlMapper.IsSupportedWpf(null), Is.False);
    }

    [Test]
    public void IsSupportedWpf_は_対応パスで_true()
    {
        Assert.That(PropertyToXamlMapper.IsSupportedWpf("Left.Value"), Is.True);
        Assert.That(PropertyToXamlMapper.IsSupportedWpf("EdgeBrush.Value"), Is.True);
    }

    [Test]
    public void PropertyApplier対応の_11パスすべて_TryMapWpfで非null()
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
            Assert.That(PropertyToXamlMapper.TryMapWpf(p), Is.Not.Null, $"{p} should be mapped");
        }
    }

    // ---- Phase 6: テキスト系プロパティのアニメ対応 ----

    [Test]
    public void Phase6_FontSize_は_FontSize_DoubleAnimation()
    {
        var m = PropertyToXamlMapper.TryMapWpf("FontSize.Value");
        Assert.That(m, Is.Not.Null);
        Assert.That(m.TargetProperty, Is.EqualTo("FontSize"));
        Assert.That(m.AnimationElementName, Is.EqualTo("DoubleAnimationUsingKeyFrames"));
    }

    [Test]
    public void Phase6_Foreground_は_TextBlock_Foreground_ColorAnimation()
    {
        var m = PropertyToXamlMapper.TryMapWpf("Foreground.Value");
        Assert.That(m, Is.Not.Null);
        Assert.That(m.TargetProperty, Is.EqualTo("(TextBlock.Foreground).(SolidColorBrush.Color)"));
        Assert.That(m.AnimationElementName, Is.EqualTo("ColorAnimationUsingKeyFrames"));
    }

    [Test]
    public void Phase6_TextOpacity_は_Opacity_DoubleAnimation()
    {
        var m = PropertyToXamlMapper.TryMapWpf("TextOpacity.Value");
        Assert.That(m, Is.Not.Null);
        Assert.That(m.TargetProperty, Is.EqualTo("Opacity"));
        Assert.That(m.AnimationElementName, Is.EqualTo("DoubleAnimationUsingKeyFrames"));
    }
}
