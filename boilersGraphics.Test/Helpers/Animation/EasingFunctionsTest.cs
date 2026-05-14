using boilersGraphics.Helpers.Animation;
using boilersGraphics.Models.Animation;
using NUnit.Framework;

namespace boilersGraphics.Test.Helpers.Animation;

[TestFixture]
public class EasingFunctionsTest
{
    private static readonly EasingKind[] AllKinds = (EasingKind[])System.Enum.GetValues(typeof(EasingKind));
    private static readonly EasingMode[] AllModes = (EasingMode[])System.Enum.GetValues(typeof(EasingMode));

    [Test]
    public void Apply_at_zero_returns_zero_for_all_combinations()
    {
        foreach (var kind in AllKinds)
        {
            foreach (var mode in AllModes)
            {
                Assert.That(EasingFunctions.Apply(kind, mode, 0.0), Is.EqualTo(0.0),
                    $"Apply({kind}, {mode}, 0.0) must be 0");
            }
        }
    }

    [Test]
    public void Apply_at_one_returns_one_for_all_combinations()
    {
        foreach (var kind in AllKinds)
        {
            foreach (var mode in AllModes)
            {
                Assert.That(EasingFunctions.Apply(kind, mode, 1.0), Is.EqualTo(1.0),
                    $"Apply({kind}, {mode}, 1.0) must be 1");
            }
        }
    }

    [Test]
    public void Apply_below_zero_clamps_to_zero()
    {
        Assert.That(EasingFunctions.Apply(EasingKind.CubicEase, EasingMode.EaseIn, -10.0), Is.EqualTo(0.0));
    }

    [Test]
    public void Apply_above_one_clamps_to_one()
    {
        Assert.That(EasingFunctions.Apply(EasingKind.CubicEase, EasingMode.EaseIn, 5.0), Is.EqualTo(1.0));
    }

    [Test]
    public void LinearEase_EaseIn_is_identity()
    {
        Assert.That(EasingFunctions.Apply(EasingKind.LinearEase, EasingMode.EaseIn, 0.25), Is.EqualTo(0.25));
        Assert.That(EasingFunctions.Apply(EasingKind.LinearEase, EasingMode.EaseIn, 0.5), Is.EqualTo(0.5));
        Assert.That(EasingFunctions.Apply(EasingKind.LinearEase, EasingMode.EaseIn, 0.75), Is.EqualTo(0.75));
    }

    [Test]
    public void LinearEase_EaseOut_is_identity()
    {
        Assert.That(EasingFunctions.Apply(EasingKind.LinearEase, EasingMode.EaseOut, 0.25), Is.EqualTo(0.25));
        Assert.That(EasingFunctions.Apply(EasingKind.LinearEase, EasingMode.EaseOut, 0.5), Is.EqualTo(0.5));
    }

    [Test]
    public void LinearEase_EaseInOut_is_identity()
    {
        Assert.That(EasingFunctions.Apply(EasingKind.LinearEase, EasingMode.EaseInOut, 0.25), Is.EqualTo(0.25));
        Assert.That(EasingFunctions.Apply(EasingKind.LinearEase, EasingMode.EaseInOut, 0.5), Is.EqualTo(0.5));
        Assert.That(EasingFunctions.Apply(EasingKind.LinearEase, EasingMode.EaseInOut, 0.75), Is.EqualTo(0.75));
    }

    [Test]
    public void CubicEase_EaseIn_t05_is_0125()
    {
        // f(t) = t^3、f(0.5) = 0.125
        Assert.That(EasingFunctions.Apply(EasingKind.CubicEase, EasingMode.EaseIn, 0.5), Is.EqualTo(0.125).Within(1e-9));
    }

    [Test]
    public void CubicEase_EaseOut_t05_is_0875()
    {
        // 1 - f(1 - t) = 1 - (1 - 0.5)^3 = 1 - 0.125 = 0.875
        Assert.That(EasingFunctions.Apply(EasingKind.CubicEase, EasingMode.EaseOut, 0.5), Is.EqualTo(0.875).Within(1e-9));
    }

    [Test]
    public void CubicEase_EaseInOut_t05_is_05()
    {
        // 対称イージングは EaseInOut の中点で 0.5 を返す (連続性)
        Assert.That(EasingFunctions.Apply(EasingKind.CubicEase, EasingMode.EaseInOut, 0.5), Is.EqualTo(0.5).Within(1e-9));
    }

    [Test]
    public void QuadraticEase_EaseIn_t05_is_025()
    {
        Assert.That(EasingFunctions.Apply(EasingKind.QuadraticEase, EasingMode.EaseIn, 0.5), Is.EqualTo(0.25).Within(1e-9));
    }

    [Test]
    public void QuarticEase_EaseIn_t05_is_00625()
    {
        // 0.5^4 = 0.0625
        Assert.That(EasingFunctions.Apply(EasingKind.QuarticEase, EasingMode.EaseIn, 0.5), Is.EqualTo(0.0625).Within(1e-9));
    }

    [Test]
    public void SineEase_EaseIn_t05_is_about_0293()
    {
        // 1 - cos(π/4) = 1 - √2/2 ≈ 0.2928932
        var expected = 1 - System.Math.Cos(System.Math.PI / 4);
        Assert.That(EasingFunctions.Apply(EasingKind.SineEase, EasingMode.EaseIn, 0.5), Is.EqualTo(expected).Within(1e-9));
    }

    [Test]
    public void CircleEase_EaseIn_t05_is_about_0134()
    {
        // 1 - √(1 - 0.25) = 1 - √0.75 ≈ 0.1339745
        var expected = 1 - System.Math.Sqrt(0.75);
        Assert.That(EasingFunctions.Apply(EasingKind.CircleEase, EasingMode.EaseIn, 0.5), Is.EqualTo(expected).Within(1e-9));
    }

    [Test]
    public void PowerEase_EaseIn_t05_is_025()
    {
        // power = 2 既定、0.5^2 = 0.25
        Assert.That(EasingFunctions.Apply(EasingKind.PowerEase, EasingMode.EaseIn, 0.5), Is.EqualTo(0.25).Within(1e-9));
    }

    [Test]
    public void All_kinds_EaseInOut_midpoint_is_05_for_symmetric_functions()
    {
        // Linear / Cubic / Quadratic / Quartic / Quintic / Sine / Circle / Power / Exponential は対称
        var symmetric = new[]
        {
            EasingKind.LinearEase, EasingKind.CubicEase, EasingKind.QuadraticEase,
            EasingKind.QuarticEase, EasingKind.QuinticEase, EasingKind.SineEase,
            EasingKind.CircleEase, EasingKind.PowerEase, EasingKind.ExponentialEase,
        };
        foreach (var kind in symmetric)
        {
            Assert.That(EasingFunctions.Apply(kind, EasingMode.EaseInOut, 0.5), Is.EqualTo(0.5).Within(1e-9),
                $"{kind} EaseInOut at 0.5 must be exactly 0.5");
        }
    }

    [Test]
    public void ExponentialEase_EaseIn_t05_is_positive_less_than_05()
    {
        // (e^1 - 1) / (e^2 - 1) ≈ (1.718) / (6.389) ≈ 0.269
        var v = EasingFunctions.Apply(EasingKind.ExponentialEase, EasingMode.EaseIn, 0.5);
        Assert.That(v, Is.GreaterThan(0).And.LessThan(0.5));
    }

    [Test]
    public void BackEase_BounceEase_ElasticEase_satisfy_endpoint_invariants()
    {
        // 非対称・overshoot 系も端点不変式 (t=0->0, t=1->1) は満たすこと
        foreach (var kind in new[] { EasingKind.BackEase, EasingKind.BounceEase, EasingKind.ElasticEase })
        {
            foreach (var mode in AllModes)
            {
                Assert.That(EasingFunctions.Apply(kind, mode, 0.0), Is.EqualTo(0.0), $"{kind} {mode} at 0");
                Assert.That(EasingFunctions.Apply(kind, mode, 1.0), Is.EqualTo(1.0), $"{kind} {mode} at 1");
            }
        }
    }

    [Test]
    public void EaseOut_is_symmetric_to_EaseIn_for_monotonic_functions()
    {
        // EaseOut(t) = 1 - EaseIn(1 - t) なので、特に t=0.5 では: EaseOut(0.5) + EaseIn(0.5) は 1 になる
        // ただし対称関数限定。Cubic/Quadratic/Sine 等で検証。
        foreach (var kind in new[] { EasingKind.CubicEase, EasingKind.QuadraticEase, EasingKind.SineEase, EasingKind.CircleEase })
        {
            var easeIn = EasingFunctions.Apply(kind, EasingMode.EaseIn, 0.5);
            var easeOut = EasingFunctions.Apply(kind, EasingMode.EaseOut, 0.5);
            Assert.That(easeIn + easeOut, Is.EqualTo(1.0).Within(1e-9), $"{kind}: EaseIn(0.5) + EaseOut(0.5) must be 1");
        }
    }
}
