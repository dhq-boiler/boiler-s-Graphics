using boilersGraphics.Helpers.Animation.Export;
using NUnit.Framework;

namespace boilersGraphics.Test.Helpers.Animation.Export;

[TestFixture]
public class XamlExportSettingsTest
{
    [Test]
    public void 既定値()
    {
        var s = new XamlExportSettings();
        Assert.That(s.TargetNamespace, Is.EqualTo("MyApp.Animations"));
        Assert.That(s.ClassName, Is.EqualTo("FuiAnimation"));
        Assert.That(s.AccessModifier, Is.EqualTo("public"));
        Assert.That(s.GenerateCodeBehind, Is.True);
        Assert.That(s.IndentWidth, Is.EqualTo(4));
        Assert.That(s.NewLine, Is.EqualTo("\r\n"));
        Assert.That(s.IncludeHeaderComment, Is.True);
    }

    [Test]
    public void with_式で_部分上書き()
    {
        var s = new XamlExportSettings();
        var u = s with { IndentWidth = 2, NewLine = "\n", ClassName = "Foo" };
        Assert.That(u.IndentWidth, Is.EqualTo(2));
        Assert.That(u.NewLine, Is.EqualTo("\n"));
        Assert.That(u.ClassName, Is.EqualTo("Foo"));
        // 上書きしてない項目は既定値が残る
        Assert.That(u.TargetNamespace, Is.EqualTo("MyApp.Animations"));
        Assert.That(u.AccessModifier, Is.EqualTo("public"));
    }

    [Test]
    public void 等価性は_全フィールド一致()
    {
        var a = new XamlExportSettings { ClassName = "X", IndentWidth = 2 };
        var b = new XamlExportSettings { ClassName = "X", IndentWidth = 2 };
        Assert.That(a, Is.EqualTo(b));
        Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
    }
}
