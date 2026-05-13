using boilersGraphics.Models.Text;
using NUnit.Framework;
using System;

namespace boilersGraphics.Test.Models.Text;

[TestFixture]
public class TextMatrixGeneratorTest
{
    [Test]
    public void Sequential_2x3_行優先連番_空文字書式()
    {
        var text = TextMatrixGenerator.Generate(
            rows: 2, columns: 3, mode: TextMatrixCellMode.Sequential, separator: " ",
            sequenceStart: 0, sequenceFormat: string.Empty,
            dataGenType: DataGeneratorType.Hex, dataGenSeed: 0, customItems: string.Empty);
        var lines = text.Split(Environment.NewLine);
        Assert.That(lines, Is.EqualTo(new[] { "0 1 2", "3 4 5" }));
    }

    [Test]
    public void Sequential_SequenceStart_開始値オフセット()
    {
        var text = TextMatrixGenerator.Generate(
            rows: 2, columns: 2, mode: TextMatrixCellMode.Sequential, separator: ",",
            sequenceStart: 10, sequenceFormat: string.Empty,
            dataGenType: DataGeneratorType.Hex, dataGenSeed: 0, customItems: string.Empty);
        var lines = text.Split(Environment.NewLine);
        Assert.That(lines, Is.EqualTo(new[] { "10,11", "12,13" }));
    }

    [Test]
    public void Sequential_D3_3桁0パディング整数書式()
    {
        var text = TextMatrixGenerator.Generate(
            rows: 1, columns: 4, mode: TextMatrixCellMode.Sequential, separator: " ",
            sequenceStart: 0, sequenceFormat: "D3",
            dataGenType: DataGeneratorType.Hex, dataGenSeed: 0, customItems: string.Empty);
        Assert.That(text, Is.EqualTo("000 001 002 003"));
    }

    [Test]
    public void Sequential_X2_16進2桁書式()
    {
        var text = TextMatrixGenerator.Generate(
            rows: 1, columns: 3, mode: TextMatrixCellMode.Sequential, separator: " ",
            sequenceStart: 254, sequenceFormat: "X2",
            dataGenType: DataGeneratorType.Hex, dataGenSeed: 0, customItems: string.Empty);
        Assert.That(text, Is.EqualTo("FE FF 100"));
    }

    [Test]
    public void DataGenerator_Hex_同じrootSeedなら同じ出力()
    {
        var t1 = TextMatrixGenerator.Generate(
            rows: 2, columns: 2, mode: TextMatrixCellMode.DataGenerator, separator: " ",
            sequenceStart: 0, sequenceFormat: string.Empty,
            dataGenType: DataGeneratorType.Hex, dataGenSeed: 42, customItems: string.Empty);
        var t2 = TextMatrixGenerator.Generate(
            rows: 2, columns: 2, mode: TextMatrixCellMode.DataGenerator, separator: " ",
            sequenceStart: 0, sequenceFormat: string.Empty,
            dataGenType: DataGeneratorType.Hex, dataGenSeed: 42, customItems: string.Empty);
        Assert.That(t1, Is.EqualTo(t2));
        // Hex は 2 桁
        var lines = t1.Split(Environment.NewLine);
        Assert.That(lines.Length, Is.EqualTo(2));
        foreach (var line in lines)
        {
            var cells = line.Split(' ');
            Assert.That(cells.Length, Is.EqualTo(2));
            foreach (var cell in cells)
                Assert.That(cell, Has.Length.EqualTo(2));
        }
    }

    [Test]
    public void DataGenerator_異なるrootSeed_異なる出力()
    {
        var t1 = TextMatrixGenerator.Generate(
            rows: 3, columns: 3, mode: TextMatrixCellMode.DataGenerator, separator: " ",
            sequenceStart: 0, sequenceFormat: string.Empty,
            dataGenType: DataGeneratorType.Hex, dataGenSeed: 1, customItems: string.Empty);
        var t2 = TextMatrixGenerator.Generate(
            rows: 3, columns: 3, mode: TextMatrixCellMode.DataGenerator, separator: " ",
            sequenceStart: 0, sequenceFormat: string.Empty,
            dataGenType: DataGeneratorType.Hex, dataGenSeed: 2, customItems: string.Empty);
        Assert.That(t1, Is.Not.EqualTo(t2));
    }

    [Test]
    public void CustomList_LF区切り_順番に展開()
    {
        var text = TextMatrixGenerator.Generate(
            rows: 2, columns: 2, mode: TextMatrixCellMode.CustomList, separator: " ",
            sequenceStart: 0, sequenceFormat: string.Empty,
            dataGenType: DataGeneratorType.Hex, dataGenSeed: 0,
            customItems: "foo\nbar\nbaz\nqux");
        var lines = text.Split(Environment.NewLine);
        Assert.That(lines, Is.EqualTo(new[] { "foo bar", "baz qux" }));
    }

    [Test]
    public void CustomList_CRLF区切り_順番に展開()
    {
        var text = TextMatrixGenerator.Generate(
            rows: 1, columns: 3, mode: TextMatrixCellMode.CustomList, separator: ",",
            sequenceStart: 0, sequenceFormat: string.Empty,
            dataGenType: DataGeneratorType.Hex, dataGenSeed: 0,
            customItems: "a\r\nb\r\nc");
        Assert.That(text, Is.EqualTo("a,b,c"));
    }

    [Test]
    public void CustomList_項目数不足_余りは空文字列()
    {
        var text = TextMatrixGenerator.Generate(
            rows: 2, columns: 2, mode: TextMatrixCellMode.CustomList, separator: "-",
            sequenceStart: 0, sequenceFormat: string.Empty,
            dataGenType: DataGeneratorType.Hex, dataGenSeed: 0,
            customItems: "x\ny");
        var lines = text.Split(Environment.NewLine);
        Assert.That(lines, Is.EqualTo(new[] { "x-y", "-" }));
    }

    [Test]
    public void CustomList_空文字列_全セル空()
    {
        var text = TextMatrixGenerator.Generate(
            rows: 2, columns: 2, mode: TextMatrixCellMode.CustomList, separator: "|",
            sequenceStart: 0, sequenceFormat: string.Empty,
            dataGenType: DataGeneratorType.Hex, dataGenSeed: 0, customItems: string.Empty);
        var lines = text.Split(Environment.NewLine);
        Assert.That(lines, Is.EqualTo(new[] { "|", "|" }));
    }

    [Test]
    public void Rows0_空文字列()
    {
        var text = TextMatrixGenerator.Generate(
            rows: 0, columns: 3, mode: TextMatrixCellMode.Sequential, separator: " ",
            sequenceStart: 0, sequenceFormat: string.Empty,
            dataGenType: DataGeneratorType.Hex, dataGenSeed: 0, customItems: string.Empty);
        Assert.That(text, Is.EqualTo(string.Empty));
    }

    [Test]
    public void Columns0_空文字列()
    {
        var text = TextMatrixGenerator.Generate(
            rows: 3, columns: 0, mode: TextMatrixCellMode.Sequential, separator: " ",
            sequenceStart: 0, sequenceFormat: string.Empty,
            dataGenType: DataGeneratorType.Hex, dataGenSeed: 0, customItems: string.Empty);
        Assert.That(text, Is.EqualTo(string.Empty));
    }

    [Test]
    public void Separator_null_空文字扱い()
    {
        Assert.DoesNotThrow(() => TextMatrixGenerator.Generate(
            rows: 2, columns: 2, mode: TextMatrixCellMode.Sequential, separator: null!,
            sequenceStart: 0, sequenceFormat: string.Empty,
            dataGenType: DataGeneratorType.Hex, dataGenSeed: 0, customItems: string.Empty));
    }
}
