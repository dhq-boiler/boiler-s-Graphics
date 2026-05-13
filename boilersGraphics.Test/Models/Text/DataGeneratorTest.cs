using boilersGraphics.Models.Text;
using NUnit.Framework;
using System;

namespace boilersGraphics.Test.Models.Text;

[TestFixture]
public class DataGeneratorTest
{
    [Test]
    public void Hex_各要素は2文字の大文字16進()
    {
        var text = DataGenerator.Generate(DataGeneratorType.Hex, seed: 1, count: 4, separator: " ", DataGeneratorLayout.OneLine);
        var parts = text.Split(' ');
        Assert.That(parts.Length, Is.EqualTo(4));
        foreach (var p in parts)
            Assert.That(p, Does.Match("^[0-9A-F]{2}$"));
    }

    [Test]
    public void Binary_各要素は4ビット()
    {
        var text = DataGenerator.Generate(DataGeneratorType.Binary, seed: 1, count: 3, separator: " ", DataGeneratorLayout.OneLine);
        var parts = text.Split(' ');
        Assert.That(parts.Length, Is.EqualTo(3));
        foreach (var p in parts)
            Assert.That(p, Does.Match("^[01]{4}$"));
    }

    [Test]
    public void Ipv4_4オクテット形式()
    {
        var text = DataGenerator.Generate(DataGeneratorType.Ipv4Address, seed: 1, count: 1, separator: " ", DataGeneratorLayout.OneLine);
        Assert.That(text, Does.Match(@"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$"));
    }

    [Test]
    public void Ipv6_8グループ完全形()
    {
        var text = DataGenerator.Generate(DataGeneratorType.Ipv6Address, seed: 1, count: 1, separator: " ", DataGeneratorLayout.OneLine);
        Assert.That(text, Does.Match(@"^[0-9a-f]{4}:[0-9a-f]{4}:[0-9a-f]{4}:[0-9a-f]{4}:[0-9a-f]{4}:[0-9a-f]{4}:[0-9a-f]{4}:[0-9a-f]{4}$"));
    }

    [Test]
    public void Uuid_RFC4122_v4形式()
    {
        var text = DataGenerator.Generate(DataGeneratorType.Uuid, seed: 1, count: 1, separator: " ", DataGeneratorLayout.OneLine);
        Assert.That(Guid.TryParseExact(text, "D", out var g), Is.True);
        // v4 では variant の M (4) と N (8/9/A/B) がセットされる
        Assert.That(text[14], Is.EqualTo('4'));
        Assert.That("89ab", Does.Contain(text[19].ToString().ToLowerInvariant()));
    }

    [Test]
    public void Timestamp_ISO8601風UTC()
    {
        var text = DataGenerator.Generate(DataGeneratorType.Timestamp, seed: 1, count: 1, separator: " ", DataGeneratorLayout.OneLine);
        Assert.That(text, Does.Match(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}Z$"));
    }

    [Test]
    public void RandomCode_6文字_紛らわしい文字を含まない()
    {
        var text = DataGenerator.Generate(DataGeneratorType.RandomCode, seed: 1, count: 2, separator: " ", DataGeneratorLayout.OneLine);
        var parts = text.Split(' ');
        Assert.That(parts.Length, Is.EqualTo(2));
        foreach (var p in parts)
            Assert.That(p, Does.Match("^[A-HJ-NP-Z2-9]{6}$"));
    }

    [Test]
    public void LogLine_LEVEL_TIMESTAMP_MODULE_MESSAGE形式()
    {
        var text = DataGenerator.Generate(DataGeneratorType.LogLine, seed: 1, count: 1, separator: " ", DataGeneratorLayout.MultiLine);
        Assert.That(text, Does.Match(@"^\[(INFO|WARN|ERROR|DEBUG|TRACE)\] \d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2} \w+: .+$"));
    }

    [Test]
    public void Seed同じなら出力同じ_Reproducible()
    {
        var a = DataGenerator.Generate(DataGeneratorType.Hex, seed: 42, count: 16, separator: " ", DataGeneratorLayout.OneLine);
        var b = DataGenerator.Generate(DataGeneratorType.Hex, seed: 42, count: 16, separator: " ", DataGeneratorLayout.OneLine);
        Assert.That(a, Is.EqualTo(b));
    }

    [Test]
    public void Seed違えば高確率で出力が異なる()
    {
        var a = DataGenerator.Generate(DataGeneratorType.Hex, seed: 1, count: 16, separator: " ", DataGeneratorLayout.OneLine);
        var b = DataGenerator.Generate(DataGeneratorType.Hex, seed: 2, count: 16, separator: " ", DataGeneratorLayout.OneLine);
        Assert.That(a, Is.Not.EqualTo(b));
    }

    [Test]
    public void Layout_MultiLine_改行区切りで件数分の行になる()
    {
        var text = DataGenerator.Generate(DataGeneratorType.Hex, seed: 1, count: 3, separator: " ", DataGeneratorLayout.MultiLine);
        var lines = text.Split(Environment.NewLine);
        Assert.That(lines.Length, Is.EqualTo(3));
    }

    [Test]
    public void Layout_OneLine_separatorで結合される()
    {
        var text = DataGenerator.Generate(DataGeneratorType.Hex, seed: 1, count: 3, separator: ", ", DataGeneratorLayout.OneLine);
        Assert.That(text.Split(", ").Length, Is.EqualTo(3));
    }

    [Test]
    public void Count_0なら空文字列()
    {
        var text = DataGenerator.Generate(DataGeneratorType.Hex, seed: 1, count: 0, separator: " ", DataGeneratorLayout.OneLine);
        Assert.That(text, Is.EqualTo(string.Empty));
    }

    [Test]
    public void Count_負数も空文字列を返す()
    {
        var text = DataGenerator.Generate(DataGeneratorType.Hex, seed: 1, count: -5, separator: " ", DataGeneratorLayout.OneLine);
        Assert.That(text, Is.EqualTo(string.Empty));
    }

    [Test]
    public void Separator_nullでも例外にならない()
    {
        Assert.DoesNotThrow(() => DataGenerator.Generate(
            DataGeneratorType.Hex, seed: 1, count: 3, separator: null!, DataGeneratorLayout.OneLine));
    }

    [Test]
    public void 未定義のTypeでArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => DataGenerator.Generate(
            (DataGeneratorType)999, seed: 1, count: 1, separator: " ", DataGeneratorLayout.OneLine));
    }
}
