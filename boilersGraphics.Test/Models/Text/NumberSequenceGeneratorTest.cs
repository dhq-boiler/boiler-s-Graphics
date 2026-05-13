using boilersGraphics.Models.Text;
using NUnit.Framework;
using System;
using System.Linq;

namespace boilersGraphics.Test.Models.Text;

[TestFixture]
public class NumberSequenceGeneratorTest
{
    [Test]
    public void Horizontal_整数列_separator結合()
    {
        var text = NumberSequenceGenerator.Generate(0, 5, 1, format: string.Empty, separator: " ",
            NumberSequenceDirection.Horizontal, gridRows: 1, gridColumns: 1);
        Assert.That(text, Is.EqualTo("0 1 2 3 4 5"));
    }

    [Test]
    public void Vertical_整数列_改行結合()
    {
        var text = NumberSequenceGenerator.Generate(1, 3, 1, format: string.Empty, separator: " ",
            NumberSequenceDirection.Vertical, gridRows: 1, gridColumns: 1);
        var lines = text.Split(Environment.NewLine);
        Assert.That(lines, Is.EqualTo(new[] { "1", "2", "3" }));
    }

    [Test]
    public void Grid_3x2_行毎にseparator結合()
    {
        var text = NumberSequenceGenerator.Generate(0, 5, 1, format: string.Empty, separator: ",",
            NumberSequenceDirection.Grid, gridRows: 3, gridColumns: 2);
        var lines = text.Split(Environment.NewLine);
        Assert.That(lines, Is.EqualTo(new[] { "0,1", "2,3", "4,5" }));
    }

    [Test]
    public void Grid_セル数より値が少ない場合_余りは空文字列()
    {
        var text = NumberSequenceGenerator.Generate(0, 2, 1, format: string.Empty, separator: "-",
            NumberSequenceDirection.Grid, gridRows: 2, gridColumns: 3);
        var lines = text.Split(Environment.NewLine);
        Assert.That(lines, Is.EqualTo(new[] { "0-1-2", "--" }));
    }

    [Test]
    public void Format_D2_2桁0パディング整数()
    {
        var text = NumberSequenceGenerator.Generate(0, 3, 1, format: "D2", separator: " ",
            NumberSequenceDirection.Horizontal, gridRows: 1, gridColumns: 1);
        Assert.That(text, Is.EqualTo("00 01 02 03"));
    }

    [Test]
    public void Format_X4_4桁0パディング16進大文字()
    {
        var text = NumberSequenceGenerator.Generate(254, 256, 1, format: "X4", separator: " ",
            NumberSequenceDirection.Horizontal, gridRows: 1, gridColumns: 1);
        Assert.That(text, Is.EqualTo("00FE 00FF 0100"));
    }

    [Test]
    public void Format_F2_小数2桁InvariantCulture()
    {
        var text = NumberSequenceGenerator.Generate(0, 1, 0.5, format: "F2", separator: ";",
            NumberSequenceDirection.Horizontal, gridRows: 1, gridColumns: 1);
        // Invariant では小数点が "."、ja-JP の "," にならないこと
        Assert.That(text, Is.EqualTo("0.00;0.50;1.00"));
    }

    [Test]
    public void Step_小数で正確な件数を生成()
    {
        var text = NumberSequenceGenerator.Generate(0, 1, 0.1, format: "F1", separator: " ",
            NumberSequenceDirection.Horizontal, gridRows: 1, gridColumns: 1);
        var parts = text.Split(' ');
        Assert.That(parts.Length, Is.EqualTo(11));
        Assert.That(parts[0], Is.EqualTo("0.0"));
        Assert.That(parts[10], Is.EqualTo("1.0"));
    }

    [Test]
    public void Step_負数で降順()
    {
        var values = NumberSequenceGenerator.Enumerate(5, 0, -1).ToList();
        Assert.That(values, Is.EqualTo(new[] { 5.0, 4.0, 3.0, 2.0, 1.0, 0.0 }));
    }

    [Test]
    public void Step_0_空列()
    {
        var values = NumberSequenceGenerator.Enumerate(0, 10, 0).ToList();
        Assert.That(values, Is.Empty);
    }

    [Test]
    public void Step方向不一致_空列()
    {
        // start < end なのに step が負
        var v1 = NumberSequenceGenerator.Enumerate(0, 10, -1).ToList();
        Assert.That(v1, Is.Empty);

        // start > end なのに step が正
        var v2 = NumberSequenceGenerator.Enumerate(10, 0, 1).ToList();
        Assert.That(v2, Is.Empty);
    }

    [Test]
    public void Separator_nullでもクラッシュしない()
    {
        Assert.DoesNotThrow(() => NumberSequenceGenerator.Generate(0, 3, 1, format: string.Empty, separator: null!,
            NumberSequenceDirection.Horizontal, gridRows: 1, gridColumns: 1));
    }

    [Test]
    public void Grid_rows0以下_空文字列()
    {
        var text = NumberSequenceGenerator.Generate(0, 5, 1, format: string.Empty, separator: " ",
            NumberSequenceDirection.Grid, gridRows: 0, gridColumns: 3);
        Assert.That(text, Is.EqualTo(string.Empty));
    }
}
