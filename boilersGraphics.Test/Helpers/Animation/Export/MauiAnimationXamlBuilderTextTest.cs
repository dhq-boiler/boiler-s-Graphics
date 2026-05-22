using boilersGraphics.Helpers.Animation.Export;
using boilersGraphics.Models.Text;
using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.Animation;
using boilersGraphics.ViewModels.Text;
using NUnit.Framework;
using System;
using System.Windows.Media;

namespace boilersGraphics.Test.Helpers.Animation.Export;

/// <summary>
/// Phase 6-d: MauiAnimationXamlBuilder がテキスト系図形 5 種を AppendShapes 経由で
/// AbsoluteLayout + Label / 個別 Label 群として正しく XAML 出力できることを検証する。
/// </summary>
[TestFixture]
public class MauiAnimationXamlBuilderTextTest
{
    [SetUp]
    public void SetUp()
    {
        boilersGraphics.App.IsTest = true;
    }

    private static readonly XamlExportSettings DefaultSettings = new();

    private static TimelineViewModel NewEmptyTimeline() => new(2.0, 30);

    [Test]
    public void MonoText_を_含むキャンバスは_Label要素が_出力される_MAUI()
    {
        var t = new MonoTextBlockViewModel();
        t.ID = Guid.Parse("12345678-1234-1234-1234-123456789abc");
        t.Text.Value = "BG";
        t.Left.Value = 10;
        t.Top.Value = 20;
        t.Width.Value = 80;
        t.Height.Value = 30;

        var items = new SelectableDesignerItemViewModelBase[] { t };
        var xaml = MauiAnimationXamlBuilder.Build(NewEmptyTimeline(), items, DefaultSettings);

        Assert.That(xaml, Does.Contain("<Label x:Name=\"Item_12345678123412341234123456789abc\""));
        Assert.That(xaml, Does.Contain("AbsoluteLayout.LayoutBounds=\"10,20,80,30\""));
        Assert.That(xaml, Does.Contain("Text=\"BG\""));
    }

    [Test]
    public void TextOnPath_Placements_あり_は_AbsoluteLayoutと_子_Label群が_展開_MAUI()
    {
        var top = new TextOnPathBlockViewModel();
        top.FontSize.Value = 16;
        top.Placements.Add(new TextOnPathCharPlacement { Char = "A", X = 1, Y = 2, Angle = 0 });
        top.Placements.Add(new TextOnPathCharPlacement { Char = "B", X = 3, Y = 4, Angle = 10 });

        var items = new SelectableDesignerItemViewModelBase[] { top };
        var xaml = MauiAnimationXamlBuilder.Build(NewEmptyTimeline(), items, DefaultSettings);

        Assert.That(xaml, Does.Contain("<AbsoluteLayout x:Name=\"Item_"));
        Assert.That(xaml, Does.Contain("Text=\"A\""));
        Assert.That(xaml, Does.Contain("Text=\"B\""));
        Assert.That(xaml, Does.Contain("AbsoluteLayout.LayoutBounds=\"1,2,AutoSize,AutoSize\""));
        Assert.That(xaml, Does.Contain("Rotation=\"10\""));
    }

    [Test]
    public void NumSeq_を_含むキャンバスは_GeneratorコメントとLabel_が_出力_MAUI()
    {
        var t = new NumberSequenceBlockViewModel();
        t.Start.Value = 0;
        t.End.Value = 5;
        t.Step.Value = 1;

        var items = new SelectableDesignerItemViewModelBase[] { t };
        var xaml = MauiAnimationXamlBuilder.Build(NewEmptyTimeline(), items, DefaultSettings);

        Assert.That(xaml, Does.Contain("<!-- Generator: NumberSequence (Start=0, End=5, Step=1"));
        Assert.That(xaml, Does.Contain("<Label x:Name=\"Item_"));
    }

    [Test]
    public void テキスト系の_FontSize_アニメ_は_xaml_cs_にFontSize_callback_MAUI()
    {
        // MAUI 側は Builder.Build() が xaml のみ返す。code-behind は別 Builder (MauiAnimationCodeBehindBuilder)。
        // ここでは Build(xaml) で Label が出ることのみ確認 (FontSize 代入は code-behind 側の責務)。
        var t = new MonoTextBlockViewModel();
        t.ID = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
        var timeline = NewEmptyTimeline();
        var track = new boilersGraphics.Models.Animation.AnimationTrack(
            new boilersGraphics.Models.Animation.PropertyRef(t.ID, "FontSize.Value",
                boilersGraphics.Models.Animation.AnimatedValueType.Double));
        timeline.Tracks.Add(track);

        var items = new SelectableDesignerItemViewModelBase[] { t };
        var xaml = MauiAnimationXamlBuilder.Build(timeline, items, DefaultSettings);

        // XAML 側は Label が出てる
        Assert.That(xaml, Does.Contain("<Label x:Name=\"Item_aaaaaaaabbbbccccddddeeeeeeeeeeee\""));
    }
}
