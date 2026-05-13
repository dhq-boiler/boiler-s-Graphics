using boilersGraphics.Models.Text;
using NUnit.Framework;
using System.Collections.Generic;
using System.ComponentModel;

namespace boilersGraphics.Test.Models.Text;

[TestFixture]
public class NumberSequenceBlockTest
{
    [Test]
    public void デフォルト値_Phase2d仕様に従う()
    {
        var m = new NumberSequenceBlock();
        Assert.That(m.Start, Is.EqualTo(0));
        Assert.That(m.End, Is.EqualTo(10));
        Assert.That(m.Step, Is.EqualTo(1));
        Assert.That(m.Format, Is.EqualTo(string.Empty));
        Assert.That(m.Separator, Is.EqualTo(" "));
        Assert.That(m.Direction, Is.EqualTo(NumberSequenceDirection.Horizontal));
        Assert.That(m.GridRows, Is.EqualTo(1));
        Assert.That(m.GridColumns, Is.EqualTo(1));
        Assert.That(m.FontFamily, Is.EqualTo(TextElementBase.DefaultFontFamily));
    }

    [Test]
    public void Start_End_Step変更でPropertyChanged()
    {
        var m = new NumberSequenceBlock();
        var changed = new List<string?>();
        ((INotifyPropertyChanged)m).PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        m.Start = -5;
        m.End = 100;
        m.Step = 0.5;

        Assert.That(changed, Does.Contain(nameof(NumberSequenceBlock.Start)));
        Assert.That(changed, Does.Contain(nameof(NumberSequenceBlock.End)));
        Assert.That(changed, Does.Contain(nameof(NumberSequenceBlock.Step)));
    }

    [Test]
    public void Format_Separator_Direction変更でPropertyChanged()
    {
        var m = new NumberSequenceBlock();
        var changed = new List<string?>();
        ((INotifyPropertyChanged)m).PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        m.Format = "D3";
        m.Separator = ", ";
        m.Direction = NumberSequenceDirection.Grid;

        Assert.That(changed, Does.Contain(nameof(NumberSequenceBlock.Format)));
        Assert.That(changed, Does.Contain(nameof(NumberSequenceBlock.Separator)));
        Assert.That(changed, Does.Contain(nameof(NumberSequenceBlock.Direction)));
    }

    [Test]
    public void GridRows_GridColumns変更でPropertyChanged()
    {
        var m = new NumberSequenceBlock();
        var changed = new List<string?>();
        ((INotifyPropertyChanged)m).PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        m.GridRows = 4;
        m.GridColumns = 5;

        Assert.That(changed, Does.Contain(nameof(NumberSequenceBlock.GridRows)));
        Assert.That(changed, Does.Contain(nameof(NumberSequenceBlock.GridColumns)));
    }
}
