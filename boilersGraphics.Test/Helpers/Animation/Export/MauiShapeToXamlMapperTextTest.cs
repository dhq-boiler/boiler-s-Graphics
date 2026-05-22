using boilersGraphics.Helpers.Animation.Export;
using boilersGraphics.Models.Text;
using boilersGraphics.ViewModels.Text;
using NUnit.Framework;
using System;
using System.Windows.Media;

namespace boilersGraphics.Test.Helpers.Animation.Export;

/// <summary>
/// Phase 6-b: MauiShapeToXamlMapper のテキスト系 5 種に対する MAUI Label / AbsoluteLayout+Label 群への
/// 変換を検証する。
/// </summary>
[TestFixture]
public class MauiShapeToXamlMapperTextTest
{
    [SetUp]
    public void SetUp()
    {
        boilersGraphics.App.IsTest = true;
    }

    private static readonly XamlExportSettings DefaultSettings = new();

    // ---- MonoTextBlock (Label) ----

    [Test]
    public void MonoText_最小構成は_Label_with_LayoutBounds()
    {
        var t = new MonoTextBlockViewModel
        {
            Left = { Value = 10 },
            Top = { Value = 20 },
            Width = { Value = 100 },
            Height = { Value = 30 },
        };
        t.ID = Guid.Parse("11111111-2222-3333-4444-555555555555");
        t.Text.Value = "Hello";

        var xaml = MauiShapeToXamlMapper.TryMapMauiShape(t, DefaultSettings);

        Assert.That(xaml, Does.Contain("<Label x:Name=\"Item_11111111222233334444555555555555\""));
        Assert.That(xaml, Does.Contain("AbsoluteLayout.LayoutBounds=\"10,20,100,30\""));
        Assert.That(xaml, Does.Contain("AbsoluteLayout.LayoutFlags=\"None\""));
        Assert.That(xaml, Does.Contain("Text=\"Hello\""));
        Assert.That(xaml, Does.Contain("LineBreakMode=\"NoWrap\""));
        Assert.That(xaml, Does.EndWith("/>"));
    }

    [Test]
    public void MonoText_Foreground_は_TextColor_に_マップ()
    {
        var t = new MonoTextBlockViewModel();
        t.Foreground.Value = new SolidColorBrush(Color.FromArgb(0xFF, 0xAB, 0xCD, 0xEF));
        t.Text.Value = "x";
        var xaml = MauiShapeToXamlMapper.TryMapMauiShape(t, DefaultSettings);
        Assert.That(xaml, Does.Contain("TextColor=\"#FFABCDEF\""));
    }

    [Test]
    public void MonoText_IsWordWrap_true_で_WordWrap()
    {
        var t = new MonoTextBlockViewModel();
        t.IsWordWrap.Value = true;
        t.Text.Value = "x";
        var xaml = MauiShapeToXamlMapper.TryMapMauiShape(t, DefaultSettings);
        Assert.That(xaml, Does.Contain("LineBreakMode=\"WordWrap\""));
    }

    [Test]
    public void MonoText_LetterSpacing_は_CharacterSpacing_に()
    {
        var t = new MonoTextBlockViewModel();
        t.LetterSpacing.Value = 1.5;
        t.Text.Value = "x";
        var xaml = MauiShapeToXamlMapper.TryMapMauiShape(t, DefaultSettings);
        Assert.That(xaml, Does.Contain("CharacterSpacing=\"1.5\""));
    }

    // ---- DataGen / NumSeq (Generator コメント) ----

    [Test]
    public void DataGen_は_Generator_コメントを_前置_MAUI()
    {
        var t = new DataGeneratorTextBlockViewModel();
        t.Type.Value = DataGeneratorType.Uuid;
        t.Seed.Value = 999;
        var xaml = MauiShapeToXamlMapper.TryMapMauiShape(t, DefaultSettings);
        Assert.That(xaml, Does.Contain("<!-- Generator: DataGenerator (Type=Uuid, Seed=999"));
        Assert.That(xaml, Does.Contain("<Label x:Name=\"Item_"));
    }

    // ---- TextOnPathBlock (AbsoluteLayout + 個別 Label) ----

    [Test]
    public void TextOnPath_Placements_空_は_AbsoluteLayout_self_closing()
    {
        var top = new TextOnPathBlockViewModel();
        top.ID = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
        var xaml = MauiShapeToXamlMapper.TryMapMauiShape(top, DefaultSettings);
        Assert.That(xaml, Does.Contain("<!-- Generator: TextOnPath"));
        Assert.That(xaml, Does.Contain("<AbsoluteLayout x:Name=\"Item_aaaaaaaabbbbccccddddeeeeeeeeeeee\""));
        Assert.That(xaml, Does.EndWith("/>"));
    }

    [Test]
    public void TextOnPath_Placements_あり_は_個別_Label_を_展開_MAUI()
    {
        var top = new TextOnPathBlockViewModel();
        top.FontSize.Value = 12;
        top.Foreground.Value = new SolidColorBrush(Colors.Blue);
        top.Placements.Add(new TextOnPathCharPlacement { Char = "X", X = 3, Y = 4, Angle = 0 });
        top.Placements.Add(new TextOnPathCharPlacement { Char = "Y", X = 30, Y = 40, Angle = 30 });

        var xaml = MauiShapeToXamlMapper.TryMapMauiShape(top, DefaultSettings);

        Assert.That(xaml, Does.Contain("<AbsoluteLayout x:Name=\"Item_"));
        Assert.That(xaml, Does.Contain("AbsoluteLayout.LayoutBounds=\"3,4,AutoSize,AutoSize\""));
        Assert.That(xaml, Does.Contain("AbsoluteLayout.LayoutBounds=\"30,40,AutoSize,AutoSize\""));
        Assert.That(xaml, Does.Contain("Text=\"X\""));
        Assert.That(xaml, Does.Contain("Text=\"Y\""));
        Assert.That(xaml, Does.Contain("Rotation=\"30\""));
        Assert.That(xaml, Does.EndWith("</AbsoluteLayout>"));
    }
}
