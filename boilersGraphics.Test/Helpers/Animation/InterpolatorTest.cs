using boilersGraphics.Helpers.Animation;
using boilersGraphics.Models.Animation;
using NUnit.Framework;
using System.Windows;
using System.Windows.Media;

namespace boilersGraphics.Test.Helpers.Animation;

[TestFixture]
public class InterpolatorTest
{
    [Test]
    public void Double_lerp_at_0_returns_from()
    {
        var v = (double)Interpolator.Interpolate(AnimatedValueType.Double, 10.0, 20.0, 0.0);
        Assert.That(v, Is.EqualTo(10.0));
    }

    [Test]
    public void Double_lerp_at_1_returns_to()
    {
        var v = (double)Interpolator.Interpolate(AnimatedValueType.Double, 10.0, 20.0, 1.0);
        Assert.That(v, Is.EqualTo(20.0));
    }

    [Test]
    public void Double_lerp_at_05_returns_midpoint()
    {
        var v = (double)Interpolator.Interpolate(AnimatedValueType.Double, 10.0, 20.0, 0.5);
        Assert.That(v, Is.EqualTo(15.0));
    }

    [Test]
    public void Int_lerp_rounds_to_nearest()
    {
        // 1 .. 10、t=0.5 → 5.5 → Round to 6 (Banker's rounding: .5 → even, so 6)
        var v = (int)Interpolator.Interpolate(AnimatedValueType.Int, 1, 10, 0.5);
        Assert.That(v, Is.EqualTo(6));

        // 0 .. 100、t=0.25 → 25
        var v2 = (int)Interpolator.Interpolate(AnimatedValueType.Int, 0, 100, 0.25);
        Assert.That(v2, Is.EqualTo(25));
    }

    [Test]
    public void Color_lerp_at_05_returns_channelwise_midpoint()
    {
        var from = Color.FromArgb(255, 0, 0, 0);
        var to = Color.FromArgb(255, 200, 100, 50);
        var v = (Color)Interpolator.Interpolate(AnimatedValueType.Color, from, to, 0.5);
        Assert.That(v.A, Is.EqualTo(255));
        Assert.That(v.R, Is.EqualTo(100));
        Assert.That(v.G, Is.EqualTo(50));
        Assert.That(v.B, Is.EqualTo(25));
    }

    [Test]
    public void Color_lerp_clamps_alpha_to_byte_range()
    {
        var from = Color.FromArgb(0, 255, 255, 255);
        var to = Color.FromArgb(255, 0, 0, 0);
        var v = (Color)Interpolator.Interpolate(AnimatedValueType.Color, from, to, 0.5);
        Assert.That(v.A, Is.EqualTo(128).Or.EqualTo(127));
    }

    [Test]
    public void Point_lerp_handles_x_and_y_independently()
    {
        var from = new Point(0, 100);
        var to = new Point(200, 50);
        var v = (Point)Interpolator.Interpolate(AnimatedValueType.Point, from, to, 0.5);
        Assert.That(v.X, Is.EqualTo(100));
        Assert.That(v.Y, Is.EqualTo(75));
    }

    [Test]
    public void Brush_solid_to_solid_returns_solid_with_lerped_color()
    {
        var from = new SolidColorBrush(Color.FromRgb(0, 0, 0));
        var to = new SolidColorBrush(Color.FromRgb(200, 100, 50));
        var v = Interpolator.Interpolate(AnimatedValueType.Brush, from, to, 0.5) as SolidColorBrush;
        Assert.That(v, Is.Not.Null);
        Assert.That(v!.Color.R, Is.EqualTo(100));
        Assert.That(v.Color.G, Is.EqualTo(50));
        Assert.That(v.Color.B, Is.EqualTo(25));
    }

    [Test]
    public void Brush_non_solid_falls_back_to_discrete_jump()
    {
        var from = new LinearGradientBrush();
        var to = new SolidColorBrush(Colors.Red);
        var midBefore = Interpolator.Interpolate(AnimatedValueType.Brush, from, to, 0.5);
        Assert.That(midBefore, Is.SameAs(from)); // t<1 で from を維持

        var midAtOne = Interpolator.Interpolate(AnimatedValueType.Brush, from, to, 1.0);
        Assert.That(midAtOne, Is.SameAs(to));
    }

    [Test]
    public void Boolean_jumps_discretely_at_t_equals_one()
    {
        Assert.That(Interpolator.Interpolate(AnimatedValueType.Boolean, false, true, 0.0), Is.EqualTo(false));
        Assert.That(Interpolator.Interpolate(AnimatedValueType.Boolean, false, true, 0.5), Is.EqualTo(false));
        Assert.That(Interpolator.Interpolate(AnimatedValueType.Boolean, false, true, 0.999), Is.EqualTo(false));
        Assert.That(Interpolator.Interpolate(AnimatedValueType.Boolean, false, true, 1.0), Is.EqualTo(true));
    }

    [Test]
    public void String_jumps_discretely_at_t_equals_one()
    {
        Assert.That(Interpolator.Interpolate(AnimatedValueType.String, "before", "after", 0.5), Is.EqualTo("before"));
        Assert.That(Interpolator.Interpolate(AnimatedValueType.String, "before", "after", 1.0), Is.EqualTo("after"));
    }

    [Test]
    public void Enum_jumps_discretely_at_t_equals_one()
    {
        // Enum も離散
        Assert.That(Interpolator.Interpolate(AnimatedValueType.Enum, "A", "B", 0.3), Is.EqualTo("A"));
        Assert.That(Interpolator.Interpolate(AnimatedValueType.Enum, "A", "B", 1.0), Is.EqualTo("B"));
    }

    [Test]
    public void From_null_returns_to()
    {
        var v = Interpolator.Interpolate(AnimatedValueType.Double, null, 42.0, 0.5);
        Assert.That(v, Is.EqualTo(42.0));
    }

    [Test]
    public void To_null_returns_from()
    {
        var v = Interpolator.Interpolate(AnimatedValueType.Double, 42.0, null, 0.5);
        Assert.That(v, Is.EqualTo(42.0));
    }

    [Test]
    public void LerpDouble_pure_helper_is_linear()
    {
        Assert.That(Interpolator.LerpDouble(0, 100, 0), Is.EqualTo(0));
        Assert.That(Interpolator.LerpDouble(0, 100, 0.5), Is.EqualTo(50));
        Assert.That(Interpolator.LerpDouble(0, 100, 1), Is.EqualTo(100));
        Assert.That(Interpolator.LerpDouble(-10, 10, 0.5), Is.EqualTo(0));
    }
}
