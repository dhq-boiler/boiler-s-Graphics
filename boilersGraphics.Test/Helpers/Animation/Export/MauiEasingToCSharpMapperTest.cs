using boilersGraphics.Helpers.Animation.Export;
using boilersGraphics.Models.Animation;
using NUnit.Framework;

namespace boilersGraphics.Test.Helpers.Animation.Export;

[TestFixture]
public class MauiEasingToCSharpMapperTest
{
    [Test]
    public void LinearEase_は_モードによらず_Easing_Linear()
    {
        foreach (EasingMode m in System.Enum.GetValues(typeof(EasingMode)))
        {
            Assert.That(MauiEasingToCSharpMapper.ToExpression(EasingKind.LinearEase, m),
                Is.EqualTo("Easing.Linear"));
            Assert.That(MauiEasingToCSharpMapper.IsStandard(EasingKind.LinearEase, m), Is.True);
        }
    }

    [TestCase(EasingMode.EaseIn, "Easing.SinIn")]
    [TestCase(EasingMode.EaseOut, "Easing.SinOut")]
    [TestCase(EasingMode.EaseInOut, "Easing.SinInOut")]
    public void SineEase_は_モードごとに_標準Easing(EasingMode mode, string expected)
    {
        Assert.That(MauiEasingToCSharpMapper.ToExpression(EasingKind.SineEase, mode), Is.EqualTo(expected));
        Assert.That(MauiEasingToCSharpMapper.IsStandard(EasingKind.SineEase, mode), Is.True);
    }

    [TestCase(EasingMode.EaseIn, "Easing.CubicIn")]
    [TestCase(EasingMode.EaseOut, "Easing.CubicOut")]
    [TestCase(EasingMode.EaseInOut, "Easing.CubicInOut")]
    public void CubicEase_は_モードごとに_標準Easing(EasingMode mode, string expected)
    {
        Assert.That(MauiEasingToCSharpMapper.ToExpression(EasingKind.CubicEase, mode), Is.EqualTo(expected));
        Assert.That(MauiEasingToCSharpMapper.IsStandard(EasingKind.CubicEase, mode), Is.True);
    }

    [TestCase(EasingMode.EaseIn, "Easing.BounceIn")]
    [TestCase(EasingMode.EaseOut, "Easing.BounceOut")]
    public void BounceEase_InとOutは標準_InOutはカスタム(EasingMode mode, string expected)
    {
        Assert.That(MauiEasingToCSharpMapper.ToExpression(EasingKind.BounceEase, mode), Is.EqualTo(expected));
        Assert.That(MauiEasingToCSharpMapper.IsStandard(EasingKind.BounceEase, mode), Is.True);
    }

    [Test]
    public void BounceEase_InOutは_カスタムラムダ()
    {
        var expr = MauiEasingToCSharpMapper.ToExpression(EasingKind.BounceEase, EasingMode.EaseInOut);
        Assert.That(expr, Does.StartWith("new Easing(t =>"));
        Assert.That(expr, Does.Contain("BoilersBounceOut"));
        Assert.That(MauiEasingToCSharpMapper.IsStandard(EasingKind.BounceEase, EasingMode.EaseInOut), Is.False);
        Assert.That(MauiEasingToCSharpMapper.RequiresBounceHelper(EasingKind.BounceEase, EasingMode.EaseInOut), Is.True);
    }

    [TestCase(EasingMode.EaseIn, "Easing.SpringIn")]
    [TestCase(EasingMode.EaseOut, "Easing.SpringOut")]
    public void BackEase_InOutは_SpringIn_SpringOut(EasingMode mode, string expected)
    {
        Assert.That(MauiEasingToCSharpMapper.ToExpression(EasingKind.BackEase, mode), Is.EqualTo(expected));
        Assert.That(MauiEasingToCSharpMapper.IsStandard(EasingKind.BackEase, mode), Is.True);
    }

    [Test]
    public void BackEase_InOutは_カスタムラムダ()
    {
        var expr = MauiEasingToCSharpMapper.ToExpression(EasingKind.BackEase, EasingMode.EaseInOut);
        Assert.That(expr, Does.StartWith("new Easing(t =>"));
        Assert.That(MauiEasingToCSharpMapper.IsStandard(EasingKind.BackEase, EasingMode.EaseInOut), Is.False);
    }

    [TestCase(EasingKind.QuadraticEase)]
    [TestCase(EasingKind.QuarticEase)]
    [TestCase(EasingKind.QuinticEase)]
    [TestCase(EasingKind.ExponentialEase)]
    [TestCase(EasingKind.CircleEase)]
    [TestCase(EasingKind.PowerEase)]
    [TestCase(EasingKind.ElasticEase)]
    public void 残りはカスタムラムダ_modeごとに違う式(EasingKind kind)
    {
        var inExpr = MauiEasingToCSharpMapper.ToExpression(kind, EasingMode.EaseIn);
        var outExpr = MauiEasingToCSharpMapper.ToExpression(kind, EasingMode.EaseOut);
        var inOutExpr = MauiEasingToCSharpMapper.ToExpression(kind, EasingMode.EaseInOut);
        Assert.That(inExpr, Does.StartWith("new Easing(t =>"));
        Assert.That(outExpr, Does.StartWith("new Easing(t =>"));
        Assert.That(inOutExpr, Does.StartWith("new Easing(t =>"));
        Assert.That(inExpr, Is.Not.EqualTo(outExpr));
        Assert.That(inExpr, Is.Not.EqualTo(inOutExpr));
        Assert.That(MauiEasingToCSharpMapper.IsStandard(kind, EasingMode.EaseIn), Is.False);
    }

    [Test]
    public void QuadraticEase_In_は_t_x_t()
    {
        var expr = MauiEasingToCSharpMapper.ToExpression(EasingKind.QuadraticEase, EasingMode.EaseIn);
        Assert.That(expr, Is.EqualTo("new Easing(t => (t) * (t))"));
    }

    [Test]
    public void CubicEase_カスタムは_出ない_標準のみ()
    {
        // CubicEase はすべての EasingMode で標準対応されているので、カスタムラムダは生まれない
        foreach (EasingMode m in System.Enum.GetValues(typeof(EasingMode)))
        {
            Assert.That(MauiEasingToCSharpMapper.ToExpression(EasingKind.CubicEase, m),
                Does.Not.StartWith("new Easing"));
        }
    }

    [Test]
    public void RequiresBounceHelper_BounceEaseInOutのみtrue()
    {
        Assert.That(MauiEasingToCSharpMapper.RequiresBounceHelper(EasingKind.BounceEase, EasingMode.EaseInOut), Is.True);
        Assert.That(MauiEasingToCSharpMapper.RequiresBounceHelper(EasingKind.BounceEase, EasingMode.EaseIn), Is.False);
        Assert.That(MauiEasingToCSharpMapper.RequiresBounceHelper(EasingKind.BounceEase, EasingMode.EaseOut), Is.False);
        Assert.That(MauiEasingToCSharpMapper.RequiresBounceHelper(EasingKind.BackEase, EasingMode.EaseInOut), Is.False);
        Assert.That(MauiEasingToCSharpMapper.RequiresBounceHelper(EasingKind.LinearEase, EasingMode.EaseIn), Is.False);
    }

    [Test]
    public void BounceHelperSource_は_BoilersBounceOut_メソッド定義を含む()
    {
        Assert.That(MauiEasingToCSharpMapper.BounceHelperSource, Does.Contain("BoilersBounceOut"));
        Assert.That(MauiEasingToCSharpMapper.BounceHelperSource, Does.Contain("private static double"));
    }
}
