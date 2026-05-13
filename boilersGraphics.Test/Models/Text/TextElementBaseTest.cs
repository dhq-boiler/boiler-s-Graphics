using boilersGraphics.Models.Text;
using NUnit.Framework;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;

namespace boilersGraphics.Test.Models.Text;

[TestFixture]
public class TextElementBaseTest
{
    // 抽象クラスなので具象 MonoTextBlock で挙動を検証する。
    [Test]
    public void デフォルト値_Phase2b仕様に従う()
    {
        var m = new MonoTextBlock();

        Assert.That(m.Text, Is.EqualTo(string.Empty));
        Assert.That(m.FontFamily, Is.EqualTo(TextElementBase.DefaultFontFamily));
        Assert.That(m.FontFamily, Does.Contain("JetBrains Mono"));
        Assert.That(m.FontSize, Is.EqualTo(12));
        Assert.That(m.Foreground, Is.EqualTo(Brushes.White));
        Assert.That(m.Background, Is.Null);
        Assert.That(m.LineHeight, Is.Null);
        Assert.That(m.LetterSpacing, Is.EqualTo(0d));
        Assert.That(m.TextOpacity, Is.EqualTo(1.0));
        Assert.That(m.IsWordWrap, Is.False);
    }

    [Test]
    public void Text変更でPropertyChangedが発火する()
    {
        var m = new MonoTextBlock();
        var changed = new List<string>();
        ((INotifyPropertyChanged)m).PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        m.Text = "0x1234";

        Assert.That(changed, Does.Contain(nameof(MonoTextBlock.Text)));
        Assert.That(m.Text, Is.EqualTo("0x1234"));
    }

    [Test]
    public void FontFamily変更でPropertyChangedが発火する()
    {
        var m = new MonoTextBlock();
        var changed = new List<string>();
        ((INotifyPropertyChanged)m).PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        m.FontFamily = "JetBrains Mono";

        Assert.That(changed, Does.Contain(nameof(MonoTextBlock.FontFamily)));
        Assert.That(m.FontFamily, Is.EqualTo("JetBrains Mono"));
    }

    [Test]
    public void FontSizeとLetterSpacingを設定できる()
    {
        var m = new MonoTextBlock();
        m.FontSize = 24;
        m.LetterSpacing = 1.5;

        Assert.That(m.FontSize, Is.EqualTo(24));
        Assert.That(m.LetterSpacing, Is.EqualTo(1.5));
    }

    [Test]
    public void Backgroundはnull許容で透過を表現する()
    {
        var m = new MonoTextBlock();
        Assert.That(m.Background, Is.Null);

        m.Background = Brushes.Black;
        Assert.That(m.Background, Is.EqualTo(Brushes.Black));

        m.Background = null;
        Assert.That(m.Background, Is.Null);
    }

    [Test]
    public void LineHeightはnull許容でautoを表現する()
    {
        var m = new MonoTextBlock();
        Assert.That(m.LineHeight, Is.Null);

        m.LineHeight = 18.0;
        Assert.That(m.LineHeight, Is.EqualTo(18.0));

        m.LineHeight = null;
        Assert.That(m.LineHeight, Is.Null);
    }

    [Test]
    public void TextOpacityを設定できる()
    {
        var m = new MonoTextBlock();
        m.TextOpacity = 0.5;
        Assert.That(m.TextOpacity, Is.EqualTo(0.5));
    }

    [Test]
    public void IsWordWrapを設定できる()
    {
        var m = new MonoTextBlock();
        m.IsWordWrap = true;
        Assert.That(m.IsWordWrap, Is.True);
    }
}
