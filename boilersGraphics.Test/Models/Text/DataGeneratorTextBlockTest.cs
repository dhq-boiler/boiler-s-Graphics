using boilersGraphics.Models.Text;
using NUnit.Framework;
using System.Collections.Generic;
using System.ComponentModel;

namespace boilersGraphics.Test.Models.Text;

[TestFixture]
public class DataGeneratorTextBlockTest
{
    [Test]
    public void デフォルト値_Phase2c仕様に従う()
    {
        var m = new DataGeneratorTextBlock();
        Assert.That(m.Type, Is.EqualTo(DataGeneratorType.Hex));
        Assert.That(m.IsSeedLocked, Is.False);
        Assert.That(m.Count, Is.EqualTo(8));
        Assert.That(m.Separator, Is.EqualTo(" "));
        Assert.That(m.Layout, Is.EqualTo(DataGeneratorLayout.OneLine));
        // TextElementBase デフォルトを継承
        Assert.That(m.FontFamily, Is.EqualTo(TextElementBase.DefaultFontFamily));
    }

    [Test]
    public void Seed_新規インスタンスごとに別の値()
    {
        // 連続で複数生成して、すべて同じ値ということがない
        var seeds = new HashSet<int>();
        for (var i = 0; i < 5; i++)
            seeds.Add(new DataGeneratorTextBlock().Seed);
        Assert.That(seeds.Count, Is.GreaterThan(1));
    }

    [Test]
    public void Type変更でPropertyChangedが発火する()
    {
        var m = new DataGeneratorTextBlock();
        var changed = new List<string?>();
        ((INotifyPropertyChanged)m).PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        m.Type = DataGeneratorType.Uuid;

        Assert.That(changed, Does.Contain(nameof(DataGeneratorTextBlock.Type)));
        Assert.That(m.Type, Is.EqualTo(DataGeneratorType.Uuid));
    }

    [Test]
    public void Seed変更でPropertyChangedが発火する()
    {
        var m = new DataGeneratorTextBlock();
        var changed = new List<string?>();
        ((INotifyPropertyChanged)m).PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        m.Seed = 12345;

        Assert.That(changed, Does.Contain(nameof(DataGeneratorTextBlock.Seed)));
    }

    [Test]
    public void IsSeedLockedを切替できる()
    {
        var m = new DataGeneratorTextBlock();
        Assert.That(m.IsSeedLocked, Is.False);
        m.IsSeedLocked = true;
        Assert.That(m.IsSeedLocked, Is.True);
    }

    [Test]
    public void Count変更でPropertyChangedが発火する()
    {
        var m = new DataGeneratorTextBlock();
        var changed = new List<string?>();
        ((INotifyPropertyChanged)m).PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        m.Count = 32;

        Assert.That(changed, Does.Contain(nameof(DataGeneratorTextBlock.Count)));
    }

    [Test]
    public void Separator変更でPropertyChangedが発火する()
    {
        var m = new DataGeneratorTextBlock();
        var changed = new List<string?>();
        ((INotifyPropertyChanged)m).PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        m.Separator = ", ";

        Assert.That(changed, Does.Contain(nameof(DataGeneratorTextBlock.Separator)));
    }

    [Test]
    public void Layout変更でPropertyChangedが発火する()
    {
        var m = new DataGeneratorTextBlock();
        var changed = new List<string?>();
        ((INotifyPropertyChanged)m).PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        m.Layout = DataGeneratorLayout.MultiLine;

        Assert.That(changed, Does.Contain(nameof(DataGeneratorTextBlock.Layout)));
    }
}
