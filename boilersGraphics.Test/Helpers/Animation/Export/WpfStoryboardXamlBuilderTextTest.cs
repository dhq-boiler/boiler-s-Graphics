using boilersGraphics.Helpers.Animation.Export;
using boilersGraphics.Models.Animation;
using boilersGraphics.Models.Text;
using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.Animation;
using boilersGraphics.ViewModels.Text;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace boilersGraphics.Test.Helpers.Animation.Export;

/// <summary>
/// Phase 6-c: WpfStoryboardXamlBuilder がテキスト系図形 (MonoText / DataGen / NumSeq /
/// TextMatrix / TextOnPath) を AppendShapes 経由で正しく XAML 出力できることを検証する。
/// </summary>
[TestFixture]
public class WpfStoryboardXamlBuilderTextTest
{
    [SetUp]
    public void SetUp()
    {
        boilersGraphics.App.IsTest = true;
    }

    private static readonly XamlExportSettings DefaultSettings = new();

    private static TimelineViewModel NewEmptyTimeline() => new(2.0, 30);

    [Test]
    public void MonoText_を_含むキャンバスは_TextBlockが_出力される()
    {
        var t = new MonoTextBlockViewModel();
        t.ID = Guid.Parse("12345678-1234-1234-1234-123456789abc");
        t.Text.Value = "BG";
        t.Left.Value = 10;
        t.Top.Value = 20;

        var items = new SelectableDesignerItemViewModelBase[] { t };
        var xaml = WpfStoryboardXamlBuilder.Build(NewEmptyTimeline(), items, DefaultSettings);

        Assert.That(xaml, Does.Contain("<TextBlock x:Name=\"Item_12345678123412341234123456789abc\""));
        Assert.That(xaml, Does.Contain("Text=\"BG\""));
    }

    [Test]
    public void TextOnPath_Placements_あり_は_Canvas_と_子_TextBlock_群が_展開()
    {
        var top = new TextOnPathBlockViewModel();
        top.FontSize.Value = 16;
        top.Placements.Add(new TextOnPathCharPlacement { Char = "A", X = 1, Y = 2, Angle = 0 });
        top.Placements.Add(new TextOnPathCharPlacement { Char = "B", X = 3, Y = 4, Angle = 10 });

        var items = new SelectableDesignerItemViewModelBase[] { top };
        var xaml = WpfStoryboardXamlBuilder.Build(NewEmptyTimeline(), items, DefaultSettings);

        Assert.That(xaml, Does.Contain("<Canvas x:Name=\"Item_"));
        Assert.That(xaml, Does.Contain("Text=\"A\""));
        Assert.That(xaml, Does.Contain("Text=\"B\""));
        Assert.That(xaml, Does.Contain("<RotateTransform Angle=\"10\""));
    }

    [Test]
    public void DataGen_を_含むキャンバスは_GeneratorコメントとTextBlock_が_出力()
    {
        var t = new DataGeneratorTextBlockViewModel();
        t.Type.Value = DataGeneratorType.Hex;
        t.Seed.Value = 42;
        t.Count.Value = 2;

        var items = new SelectableDesignerItemViewModelBase[] { t };
        var xaml = WpfStoryboardXamlBuilder.Build(NewEmptyTimeline(), items, DefaultSettings);

        Assert.That(xaml, Does.Contain("<!-- Generator: DataGenerator (Type=Hex, Seed=42"));
        Assert.That(xaml, Does.Contain("<TextBlock x:Name=\"Item_"));
        // Skipped コメント (テキスト系未対応のレガシー) は出力されないこと
        Assert.That(xaml, Does.Not.Contain("Skipped unsupported item: DataGeneratorTextBlockViewModel"));
    }

    [Test]
    public void テキスト系の_FontSize_アニメ_は_Storyboardに_DoubleAnimation()
    {
        var t = new MonoTextBlockViewModel();
        t.ID = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
        var timeline = NewEmptyTimeline();
        var track = new AnimationTrack(new PropertyRef(t.ID, "FontSize.Value", AnimatedValueType.Double));
        track.Keyframes.Add(new Keyframe { Time = { Value = 0 }, Value = { Value = 10.0 } });
        track.Keyframes.Add(new Keyframe { Time = { Value = 1 }, Value = { Value = 32.0 } });
        timeline.Tracks.Add(track);

        var items = new SelectableDesignerItemViewModelBase[] { t };
        var xaml = WpfStoryboardXamlBuilder.Build(timeline, items, DefaultSettings);

        Assert.That(xaml, Does.Contain("<DoubleAnimationUsingKeyFrames"));
        Assert.That(xaml, Does.Contain("Storyboard.TargetName=\"Item_aaaaaaaabbbbccccddddeeeeeeeeeeee\""));
        Assert.That(xaml, Does.Contain("Storyboard.TargetProperty=\"FontSize\""));
        Assert.That(xaml, Does.Contain("Value=\"10\""));
        Assert.That(xaml, Does.Contain("Value=\"32\""));
    }

    [Test]
    public void テキスト系の_Foreground_アニメ_は_Storyboardに_ColorAnimation()
    {
        var t = new MonoTextBlockViewModel();
        t.ID = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
        var timeline = NewEmptyTimeline();
        var track = new AnimationTrack(new PropertyRef(t.ID, "Foreground.Value", AnimatedValueType.Color));
        track.Keyframes.Add(new Keyframe { Time = { Value = 0 }, Value = { Value = Colors.Red } });
        track.Keyframes.Add(new Keyframe { Time = { Value = 1 }, Value = { Value = Colors.Blue } });
        timeline.Tracks.Add(track);

        var items = new SelectableDesignerItemViewModelBase[] { t };
        var xaml = WpfStoryboardXamlBuilder.Build(timeline, items, DefaultSettings);

        Assert.That(xaml, Does.Contain("<ColorAnimationUsingKeyFrames"));
        Assert.That(xaml, Does.Contain("Storyboard.TargetProperty=\"(TextBlock.Foreground).(SolidColorBrush.Color)\""));
    }
}
