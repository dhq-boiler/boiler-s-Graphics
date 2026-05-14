using boilersGraphics.Helpers.Animation.Export;
using boilersGraphics.Models.Animation;
using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.Animation;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace boilersGraphics.Test.Helpers.Animation.Export;

[TestFixture]
public class WpfStoryboardXamlExporterTest
{
    [SetUp]
    public void SetUp()
    {
        boilersGraphics.App.IsTest = true;
    }

    private static IReadOnlyList<SelectableDesignerItemViewModelBase> NoItems() =>
        Array.Empty<SelectableDesignerItemViewModelBase>();

    private static TimelineViewModel NewEmptyTimeline() => new(1.0, 30);

    [Test]
    public void Metadata_は_WPF_Storyboard_XAML_を表す()
    {
        var ex = new WpfStoryboardXamlExporter(NoItems());
        Assert.That(ex.FormatName, Is.EqualTo("WPF Storyboard XAML"));
        Assert.That(ex.DefaultFileExtension, Is.EqualTo(".xaml"));
        Assert.That(ex.IsMultiFile, Is.False);
    }

    [Test]
    public void Ctor_allItems_null_は_ArgumentNullException()
    {
        Assert.That(() => new WpfStoryboardXamlExporter(null),
            Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void Export_outputPath_空は_ArgumentException()
    {
        var ex = new WpfStoryboardXamlExporter(NoItems(), writeAllText: (_, _) => { });
        Assert.That(() => ex.Export(NewEmptyTimeline(), "", null, null),
            Throws.TypeOf<ArgumentException>());
        Assert.That(() => ex.Export(NewEmptyTimeline(), null, null, null),
            Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Export_timeline_null_は_ArgumentNullException()
    {
        var ex = new WpfStoryboardXamlExporter(NoItems(), writeAllText: (_, _) => { });
        Assert.That(() => ex.Export(null, "C:/tmp/a.xaml", null, null),
            Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void Export_デフォルトオプションで_XAML_と_CodeBehind_両方書き出し戻り値2()
    {
        var writes = new List<(string path, string content)>();
        var exporter = new WpfStoryboardXamlExporter(NoItems(), writeAllText: (p, c) => writes.Add((p, c)));

        var written = exporter.Export(NewEmptyTimeline(), "C:/tmp/Foo.xaml", null, null);

        Assert.That(written, Is.EqualTo(2));
        Assert.That(writes.Count, Is.EqualTo(2));
        Assert.That(writes[0].path, Is.EqualTo("C:/tmp/Foo.xaml"));
        Assert.That(writes[0].content, Does.Contain("<UserControl x:Class=\"MyApp.Animations.FuiAnimation\""));
        Assert.That(writes[1].path, Is.EqualTo("C:/tmp/Foo.xaml.cs"));
        Assert.That(writes[1].content, Does.Contain("public partial class FuiAnimation : UserControl"));
    }

    [Test]
    public void Export_GenerateCodeBehind_false_で_XAMLのみ_戻り値1()
    {
        var writes = new List<(string path, string content)>();
        var exporter = new WpfStoryboardXamlExporter(NoItems(), writeAllText: (p, c) => writes.Add((p, c)));
        var options = new Dictionary<string, object> { { "GenerateCodeBehind", false } };

        var written = exporter.Export(NewEmptyTimeline(), "C:/tmp/Bar.xaml", null, options);

        Assert.That(written, Is.EqualTo(1));
        Assert.That(writes.Count, Is.EqualTo(1));
        Assert.That(writes[0].path, Is.EqualTo("C:/tmp/Bar.xaml"));
    }

    [Test]
    public void Export_options_で_TargetNamespace_ClassName_が_XAMLとCodeBehindに反映()
    {
        var writes = new List<(string path, string content)>();
        var exporter = new WpfStoryboardXamlExporter(NoItems(), writeAllText: (p, c) => writes.Add((p, c)));
        var options = new Dictionary<string, object>
        {
            { "TargetNamespace", "Acme.Animations" },
            { "ClassName", "MySpin" },
        };

        exporter.Export(NewEmptyTimeline(), "C:/tmp/MySpin.xaml", null, options);

        Assert.That(writes[0].content, Does.Contain("x:Class=\"Acme.Animations.MySpin\""));
        Assert.That(writes[1].content, Does.Contain("namespace Acme.Animations;"));
        Assert.That(writes[1].content, Does.Contain("public partial class MySpin : UserControl"));
    }

    [Test]
    public void BuildSettings_optionsがnullでもデフォルトを返す()
    {
        var s = WpfStoryboardXamlExporter.BuildSettings(null);
        Assert.That(s.TargetNamespace, Is.EqualTo("MyApp.Animations"));
        Assert.That(s.ClassName, Is.EqualTo("FuiAnimation"));
    }

    [Test]
    public void BuildSettings_長整数の数値はConvertChangeTypeで型強制()
    {
        var options = new Dictionary<string, object> { { "IndentWidth", 2L } };
        var s = WpfStoryboardXamlExporter.BuildSettings(options);
        Assert.That(s.IndentWidth, Is.EqualTo(2));
    }

    [Test]
    public void BuildSettings_変換失敗のキーはスルーしてデフォルト維持()
    {
        var options = new Dictionary<string, object> { { "IndentWidth", "not-a-number" } };
        var s = WpfStoryboardXamlExporter.BuildSettings(options);
        Assert.That(s.IndentWidth, Is.EqualTo(4));
    }

    [Test]
    public void Export_でTimelineと図形が反映される()
    {
        var rect = new NRectangleViewModel(10, 20, 30, 40);
        rect.EdgeBrush.Value = null;
        rect.FillBrush.Value = null;
        var t = NewEmptyTimeline();
        var track = new AnimationTrack(new PropertyRef(rect.ID, "Left.Value", AnimatedValueType.Double));
        track.Keyframes.Add(new Keyframe(0.0, 0.0, EasingKind.LinearEase, EasingMode.EaseIn));
        track.Keyframes.Add(new Keyframe(1.0, 100.0, EasingKind.LinearEase, EasingMode.EaseIn));
        t.Tracks.Add(track);

        var writes = new List<(string path, string content)>();
        var exporter = new WpfStoryboardXamlExporter(new[] { rect }, writeAllText: (p, c) => writes.Add((p, c)));
        exporter.Export(t, "C:/tmp/A.xaml", null, new Dictionary<string, object> { { "GenerateCodeBehind", false } });

        var xaml = writes[0].content;
        Assert.That(xaml, Does.Contain("<Rectangle x:Name=\"Item_"));
        Assert.That(xaml, Does.Contain("Canvas.Left=\"10\""));
        Assert.That(xaml, Does.Contain("<DoubleAnimationUsingKeyFrames"));
        Assert.That(xaml, Does.Contain("Storyboard.TargetProperty=\"(Canvas.Left)\""));
    }
}
