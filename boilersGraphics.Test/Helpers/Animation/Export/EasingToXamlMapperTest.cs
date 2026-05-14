using boilersGraphics.Helpers.Animation.Export;
using boilersGraphics.Models.Animation;
using NUnit.Framework;

namespace boilersGraphics.Test.Helpers.Animation.Export;

[TestFixture]
public class EasingToXamlMapperTest
{
    private static readonly EasingKind[] AllKinds = (EasingKind[])System.Enum.GetValues(typeof(EasingKind));
    private static readonly EasingMode[] AllModes = (EasingMode[])System.Enum.GetValues(typeof(EasingMode));

    [Test]
    public void LinearEase_は_要素名_null()
    {
        Assert.That(EasingToXamlMapper.ToWpfElementName(EasingKind.LinearEase), Is.Null);
    }

    [Test]
    public void LinearEase_は_RequiresEasingFunction_false()
    {
        Assert.That(EasingToXamlMapper.RequiresEasingFunction(EasingKind.LinearEase), Is.False);
    }

    [Test]
    public void LinearEase_以外の_11_種は_RequiresEasingFunction_true()
    {
        foreach (var kind in AllKinds)
        {
            if (kind == EasingKind.LinearEase) continue;
            Assert.That(EasingToXamlMapper.RequiresEasingFunction(kind), Is.True, $"{kind} should require EasingFunction");
        }
    }

    [TestCase(EasingKind.SineEase, "SineEase")]
    [TestCase(EasingKind.QuadraticEase, "QuadraticEase")]
    [TestCase(EasingKind.CubicEase, "CubicEase")]
    [TestCase(EasingKind.QuarticEase, "QuarticEase")]
    [TestCase(EasingKind.QuinticEase, "QuinticEase")]
    [TestCase(EasingKind.ExponentialEase, "ExponentialEase")]
    [TestCase(EasingKind.CircleEase, "CircleEase")]
    [TestCase(EasingKind.PowerEase, "PowerEase")]
    [TestCase(EasingKind.ElasticEase, "ElasticEase")]
    [TestCase(EasingKind.BackEase, "BackEase")]
    [TestCase(EasingKind.BounceEase, "BounceEase")]
    public void WPF_要素名は_1対1_同名マップ(EasingKind kind, string expected)
    {
        Assert.That(EasingToXamlMapper.ToWpfElementName(kind), Is.EqualTo(expected));
    }

    [Test]
    public void LinearEase_の_XAML_要素文字列は_null()
    {
        Assert.That(EasingToXamlMapper.ToWpfEasingXaml(EasingKind.LinearEase, EasingMode.EaseInOut), Is.Null);
    }

    [TestCase(EasingMode.EaseIn)]
    [TestCase(EasingMode.EaseOut)]
    [TestCase(EasingMode.EaseInOut)]
    public void SineEase_の_XAML_要素は_EasingMode属性付き(EasingMode mode)
    {
        var actual = EasingToXamlMapper.ToWpfEasingXaml(EasingKind.SineEase, mode);
        Assert.That(actual, Is.EqualTo($"<SineEase EasingMode=\"{mode}\" />"));
    }

    [Test]
    public void 全種類_全モードで_LinearEase以外は_要素文字列を返す()
    {
        foreach (var kind in AllKinds)
        {
            foreach (var mode in AllModes)
            {
                var xaml = EasingToXamlMapper.ToWpfEasingXaml(kind, mode);
                if (kind == EasingKind.LinearEase)
                {
                    Assert.That(xaml, Is.Null);
                }
                else
                {
                    Assert.That(xaml, Does.StartWith("<").And.EndWith($" EasingMode=\"{mode}\" />"));
                }
            }
        }
    }
}
