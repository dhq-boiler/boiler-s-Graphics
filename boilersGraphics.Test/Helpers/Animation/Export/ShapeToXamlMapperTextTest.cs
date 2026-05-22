using boilersGraphics.Helpers.Animation.Export;
using boilersGraphics.Models.Text;
using boilersGraphics.ViewModels.Text;
using NUnit.Framework;
using System;
using System.Windows.Media;

namespace boilersGraphics.Test.Helpers.Animation.Export;

/// <summary>
/// Phase 6-b: ShapeToXamlMapper のテキスト系 (Mono/DataGen/NumSeq/TextMatrix/TextOnPath) 5 種に対する
/// WPF TextBlock / Canvas+TextBlock 群への変換を検証する。
/// </summary>
[TestFixture]
public class ShapeToXamlMapperTextTest
{
    [SetUp]
    public void SetUp()
    {
        boilersGraphics.App.IsTest = true;
    }

    private static readonly XamlExportSettings DefaultSettings = new();

    // ---- ShortenFontFamily (Q-2) ----

    [Test]
    public void ShortenFontFamily_packURI_は_最後の_hash_以降のみ()
    {
        var raw = "pack://application:,,,/boilersGraphics;component/Fonts/#JetBrains Mono";
        Assert.That(ShapeToXamlMapper.ShortenFontFamily(raw), Is.EqualTo("JetBrains Mono"));
    }

    [Test]
    public void ShortenFontFamily_hash_無しは_そのまま()
    {
        Assert.That(ShapeToXamlMapper.ShortenFontFamily("Arial"), Is.EqualTo("Arial"));
    }

    [Test]
    public void ShortenFontFamily_null_と_空文字_は_そのまま()
    {
        Assert.That(ShapeToXamlMapper.ShortenFontFamily(null), Is.Null);
        Assert.That(ShapeToXamlMapper.ShortenFontFamily(string.Empty), Is.EqualTo(string.Empty));
    }

    [Test]
    public void ShortenFontFamily_複数_hash_は_最後の_hash_以降()
    {
        Assert.That(ShapeToXamlMapper.ShortenFontFamily("##Bar"), Is.EqualTo("Bar"));
    }

    // ---- EncodeTextAttribute ----

    [Test]
    public void EncodeTextAttribute_改行は_HexLF_に()
    {
        Assert.That(ShapeToXamlMapper.EncodeTextAttribute("a\nb"), Is.EqualTo("a&#x0A;b"));
        Assert.That(ShapeToXamlMapper.EncodeTextAttribute("a\r\nb"), Is.EqualTo("a&#x0A;b"));
        Assert.That(ShapeToXamlMapper.EncodeTextAttribute("a\rb"), Is.EqualTo("a&#x0A;b"));
    }

    [Test]
    public void EncodeTextAttribute_XML_予約文字を_エスケープ()
    {
        Assert.That(ShapeToXamlMapper.EncodeTextAttribute("a & <b>"), Is.EqualTo("a &amp; &lt;b&gt;"));
        Assert.That(ShapeToXamlMapper.EncodeTextAttribute("\"q\""), Is.EqualTo("&quot;q&quot;"));
    }

    // ---- MonoTextBlock ----

    [Test]
    public void MonoText_最小構成は_TextBlock_self_closing()
    {
        var t = new MonoTextBlockViewModel
        {
            Left = { Value = 10 },
            Top = { Value = 20 },
            Width = { Value = 0 },
            Height = { Value = 0 },
        };
        t.ID = Guid.Parse("11111111-2222-3333-4444-555555555555");
        t.Text.Value = "Hello";

        var xaml = ShapeToXamlMapper.TryMapWpfShape(t, DefaultSettings);

        Assert.That(xaml, Does.Contain("<TextBlock x:Name=\"Item_11111111222233334444555555555555\""));
        Assert.That(xaml, Does.Contain("Canvas.Left=\"10\" Canvas.Top=\"20\""));
        Assert.That(xaml, Does.Contain("Text=\"Hello\""));
        Assert.That(xaml, Does.Contain("TextWrapping=\"NoWrap\""));
        Assert.That(xaml, Does.EndWith("/>"));
        Assert.That(xaml, Does.Not.Contain("<!-- Generator:"));
    }

    [Test]
    public void MonoText_Foreground_FontSize_FontFamily_TextOpacity_が_出力される()
    {
        var t = new MonoTextBlockViewModel();
        t.Foreground.Value = new SolidColorBrush(Color.FromArgb(0xFF, 0x12, 0x34, 0x56));
        t.FontSize.Value = 24;
        t.FontFamily.Value = "pack://application:,,,/x;component/Fonts/#JetBrains Mono";
        t.TextOpacity.Value = 0.5;
        t.Text.Value = "x";

        var xaml = ShapeToXamlMapper.TryMapWpfShape(t, DefaultSettings);

        Assert.That(xaml, Does.Contain("Foreground=\"#FF123456\""));
        Assert.That(xaml, Does.Contain("FontSize=\"24\""));
        Assert.That(xaml, Does.Contain("FontFamily=\"JetBrains Mono\""));
        Assert.That(xaml, Does.Contain("Opacity=\"0.5\""));
    }

    [Test]
    public void MonoText_IsWordWrap_true_で_Wrap()
    {
        var t = new MonoTextBlockViewModel();
        t.IsWordWrap.Value = true;
        t.Text.Value = "x";
        var xaml = ShapeToXamlMapper.TryMapWpfShape(t, DefaultSettings);
        Assert.That(xaml, Does.Contain("TextWrapping=\"Wrap\""));
    }

    // ---- DataGeneratorTextBlock (Generator コメント) ----

    [Test]
    public void DataGen_は_Generator_コメントを_前置()
    {
        var t = new DataGeneratorTextBlockViewModel();
        t.Type.Value = DataGeneratorType.Hex;
        t.Seed.Value = 12345;
        t.Count.Value = 4;
        t.Separator.Value = " ";
        t.Layout.Value = DataGeneratorLayout.OneLine;

        var xaml = ShapeToXamlMapper.TryMapWpfShape(t, DefaultSettings);

        Assert.That(xaml, Does.Contain("<!-- Generator: DataGenerator (Type=Hex, Seed=12345, Count=4, Separator=\" \", Layout=OneLine) -->"));
        Assert.That(xaml, Does.Contain("<TextBlock x:Name=\"Item_"));
    }

    // ---- NumberSequenceBlock (Generator コメント) ----

    [Test]
    public void NumberSequence_は_Generator_コメントを_前置()
    {
        var t = new NumberSequenceBlockViewModel();
        t.Start.Value = 0;
        t.End.Value = 10;
        t.Step.Value = 1;
        t.Format.Value = "D2";
        t.Direction.Value = NumberSequenceDirection.Horizontal;

        var xaml = ShapeToXamlMapper.TryMapWpfShape(t, DefaultSettings);

        Assert.That(xaml, Does.Contain("<!-- Generator: NumberSequence (Start=0, End=10, Step=1"));
        Assert.That(xaml, Does.Contain("Direction=Horizontal"));
    }

    // ---- TextMatrixBlock (改行エンコード + Generator コメント) ----

    [Test]
    public void TextMatrix_は_改行_HexLF_に_エンコード_されてる()
    {
        var t = new TextMatrixBlockViewModel
        {
            Rows = { Value = 2 },
            Columns = { Value = 2 },
        };
        var xaml = ShapeToXamlMapper.TryMapWpfShape(t, DefaultSettings);
        Assert.That(xaml, Does.Contain("<!-- Generator: TextMatrix"));
        Assert.That(xaml, Does.Contain("&#x0A;"));
    }

    // ---- TextOnPathBlock (Canvas + 個別 TextBlock) ----

    [Test]
    public void TextOnPath_Placements_空_は_Canvas_self_closing()
    {
        var top = new TextOnPathBlockViewModel
        {
            Left = { Value = 5 },
            Top = { Value = 10 },
            Width = { Value = 0 },
            Height = { Value = 0 },
        };
        top.ID = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

        var xaml = ShapeToXamlMapper.TryMapWpfShape(top, DefaultSettings);

        Assert.That(xaml, Does.Contain("<!-- Generator: TextOnPath"));
        Assert.That(xaml, Does.Contain("<Canvas x:Name=\"Item_aaaaaaaabbbbccccddddeeeeeeeeeeee\""));
        Assert.That(xaml, Does.EndWith("/>"));
        Assert.That(xaml, Does.Not.Contain("<TextBlock"));
    }

    [Test]
    public void TextOnPath_Placements_あり_は_個別_TextBlock_を_展開()
    {
        var top = new TextOnPathBlockViewModel();
        top.FontSize.Value = 16;
        top.Foreground.Value = new SolidColorBrush(Colors.Red);
        top.Placements.Add(new TextOnPathCharPlacement { Char = "A", X = 1, Y = 2, Angle = 0 });
        top.Placements.Add(new TextOnPathCharPlacement { Char = "B", X = 10, Y = 20, Angle = 45 });

        var xaml = ShapeToXamlMapper.TryMapWpfShape(top, DefaultSettings);

        Assert.That(xaml, Does.Contain("<Canvas x:Name=\"Item_"));
        Assert.That(xaml, Does.Contain("Text=\"A\""));
        Assert.That(xaml, Does.Contain("Text=\"B\""));
        Assert.That(xaml, Does.Contain("Canvas.Left=\"1\" Canvas.Top=\"2\""));
        Assert.That(xaml, Does.Contain("Canvas.Left=\"10\" Canvas.Top=\"20\""));
        Assert.That(xaml, Does.Contain("<RotateTransform Angle=\"45\""));
        Assert.That(xaml, Does.EndWith("</Canvas>"));
    }
}
