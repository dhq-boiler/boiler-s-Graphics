using boilersGraphics.Converters;
using NUnit.Framework;
using System.Globalization;

namespace boilersGraphics.Test.Converters;

[TestFixture]
public class TimeToXConverterTest
{
    private const double Half = 4.0; // ダイヤ ◆ の半幅 (中央寄せのオフセット)

    private static object Convert(double time, double duration, double width)
    {
        var c = new TimeToXConverter();
        return c.Convert(new object[] { time, duration, width }, typeof(double), null, CultureInfo.InvariantCulture);
    }

    [Test]
    public void Convert_time_zero_は_left_offset_minus_half()
    {
        Assert.That(Convert(0.0, 10.0, 400.0), Is.EqualTo(-Half).Within(1e-9));
    }

    [Test]
    public void Convert_time_middle_は_中央付近()
    {
        // 5 / 10 * 400 = 200, minus half = 196
        Assert.That(Convert(5.0, 10.0, 400.0), Is.EqualTo(200.0 - Half).Within(1e-9));
    }

    [Test]
    public void Convert_time_end_は_右端_minus_half()
    {
        // 10 / 10 * 400 = 400, minus half = 396 (= width - half)
        Assert.That(Convert(10.0, 10.0, 400.0), Is.EqualTo(400.0 - Half).Within(1e-9));
    }

    [Test]
    public void Convert_time_negative_は_minus_half_にクランプ()
    {
        Assert.That(Convert(-3.0, 10.0, 400.0), Is.EqualTo(-Half).Within(1e-9));
    }

    [Test]
    public void Convert_time_over_duration_は_右端にクランプ()
    {
        Assert.That(Convert(20.0, 10.0, 400.0), Is.EqualTo(400.0 - Half).Within(1e-9));
    }

    [Test]
    public void Convert_duration_zero_は_0()
    {
        Assert.That(Convert(5.0, 0.0, 400.0), Is.EqualTo(0.0).Within(1e-9));
    }

    [Test]
    public void Convert_duration_negative_は_0()
    {
        Assert.That(Convert(5.0, -1.0, 400.0), Is.EqualTo(0.0).Within(1e-9));
    }

    [Test]
    public void Convert_width_zero_は_0()
    {
        Assert.That(Convert(5.0, 10.0, 0.0), Is.EqualTo(0.0).Within(1e-9));
    }

    [Test]
    public void Convert_values_null_は_0()
    {
        var c = new TimeToXConverter();
        Assert.That(c.Convert(null, typeof(double), null, CultureInfo.InvariantCulture), Is.EqualTo(0.0));
    }

    [Test]
    public void Convert_values_短すぎる_は_0()
    {
        var c = new TimeToXConverter();
        Assert.That(c.Convert(new object[] { 1.0, 10.0 }, typeof(double), null, CultureInfo.InvariantCulture), Is.EqualTo(0.0));
    }

    [Test]
    public void Convert_non_double_は_0()
    {
        var c = new TimeToXConverter();
        Assert.That(c.Convert(new object[] { "x", 10.0, 400.0 }, typeof(double), null, CultureInfo.InvariantCulture), Is.EqualTo(0.0));
        Assert.That(c.Convert(new object[] { 1.0, "x", 400.0 }, typeof(double), null, CultureInfo.InvariantCulture), Is.EqualTo(0.0));
        Assert.That(c.Convert(new object[] { 1.0, 10.0, "x" }, typeof(double), null, CultureInfo.InvariantCulture), Is.EqualTo(0.0));
    }

    [Test]
    public void Convert_quarter_position()
    {
        // 2.5 / 10 * 400 = 100, minus half = 96
        Assert.That(Convert(2.5, 10.0, 400.0), Is.EqualTo(100.0 - Half).Within(1e-9));
    }

    [Test]
    public void Convert_three_quarters_position()
    {
        // 7.5 / 10 * 400 = 300, minus half = 296
        Assert.That(Convert(7.5, 10.0, 400.0), Is.EqualTo(300.0 - Half).Within(1e-9));
    }
}
