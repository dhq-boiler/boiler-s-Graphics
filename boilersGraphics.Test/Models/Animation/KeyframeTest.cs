using boilersGraphics.Models.Animation;
using NUnit.Framework;

namespace boilersGraphics.Test.Models.Animation;

[TestFixture]
public class KeyframeTest
{
    [Test]
    public void DefaultConstructor_initializes_to_zero_and_LinearEase_EaseIn()
    {
        var kf = new Keyframe();
        Assert.That(kf.Time.Value, Is.EqualTo(0.0));
        Assert.That(kf.Value.Value, Is.Null);
        Assert.That(kf.Easing.Value, Is.EqualTo(EasingKind.LinearEase));
        Assert.That(kf.Mode.Value, Is.EqualTo(EasingMode.EaseIn));
    }

    [Test]
    public void Parameterized_constructor_sets_all_four_properties()
    {
        var kf = new Keyframe(1.5, 42.0, EasingKind.CubicEase, EasingMode.EaseInOut);
        Assert.That(kf.Time.Value, Is.EqualTo(1.5));
        Assert.That(kf.Value.Value, Is.EqualTo(42.0));
        Assert.That(kf.Easing.Value, Is.EqualTo(EasingKind.CubicEase));
        Assert.That(kf.Mode.Value, Is.EqualTo(EasingMode.EaseInOut));
    }

    [Test]
    public void Property_values_are_mutable_via_BindableReactiveProperty()
    {
        var kf = new Keyframe();
        kf.Time.Value = 2.5;
        kf.Value.Value = "hello";
        kf.Easing.Value = EasingKind.BounceEase;
        kf.Mode.Value = EasingMode.EaseOut;

        Assert.That(kf.Time.Value, Is.EqualTo(2.5));
        Assert.That(kf.Value.Value, Is.EqualTo("hello"));
        Assert.That(kf.Easing.Value, Is.EqualTo(EasingKind.BounceEase));
        Assert.That(kf.Mode.Value, Is.EqualTo(EasingMode.EaseOut));
    }

    [Test]
    public void Dispose_does_not_throw()
    {
        var kf = new Keyframe(1.0, 100.0, EasingKind.LinearEase, EasingMode.EaseIn);
        Assert.DoesNotThrow(() => kf.Dispose());
    }
}
