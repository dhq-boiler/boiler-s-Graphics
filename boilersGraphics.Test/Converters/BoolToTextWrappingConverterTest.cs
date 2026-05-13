using boilersGraphics.Converters;
using NUnit.Framework;
using System.Globalization;
using System.Windows;

namespace boilersGraphics.Test.Converters;

[TestFixture]
public class BoolToTextWrappingConverterTest
{
    [Test]
    public void Convert_true_はWrap()
    {
        var c = new BoolToTextWrappingConverter();
        Assert.That(c.Convert(true, typeof(TextWrapping), null, CultureInfo.InvariantCulture),
            Is.EqualTo(TextWrapping.Wrap));
    }

    [Test]
    public void Convert_false_はNoWrap()
    {
        var c = new BoolToTextWrappingConverter();
        Assert.That(c.Convert(false, typeof(TextWrapping), null, CultureInfo.InvariantCulture),
            Is.EqualTo(TextWrapping.NoWrap));
    }

    [Test]
    public void Convert_非bool値_はNoWrap()
    {
        var c = new BoolToTextWrappingConverter();
        Assert.That(c.Convert(null, typeof(TextWrapping), null, CultureInfo.InvariantCulture),
            Is.EqualTo(TextWrapping.NoWrap));
        Assert.That(c.Convert("true", typeof(TextWrapping), null, CultureInfo.InvariantCulture),
            Is.EqualTo(TextWrapping.NoWrap));
    }

    [Test]
    public void ConvertBack_Wrap_はtrue()
    {
        var c = new BoolToTextWrappingConverter();
        Assert.That(c.ConvertBack(TextWrapping.Wrap, typeof(bool), null, CultureInfo.InvariantCulture),
            Is.EqualTo(true));
    }

    [Test]
    public void ConvertBack_NoWrap_はfalse()
    {
        var c = new BoolToTextWrappingConverter();
        Assert.That(c.ConvertBack(TextWrapping.NoWrap, typeof(bool), null, CultureInfo.InvariantCulture),
            Is.EqualTo(false));
    }

    [Test]
    public void ConvertBack_WrapWithOverflow_はfalse()
    {
        var c = new BoolToTextWrappingConverter();
        Assert.That(c.ConvertBack(TextWrapping.WrapWithOverflow, typeof(bool), null, CultureInfo.InvariantCulture),
            Is.EqualTo(false));
    }
}
