using boilersGraphics.Helpers.Animation.Export;
using boilersGraphics.Models.Animation;
using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.Animation;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace boilersGraphics.Test.Helpers.Animation.Export;

[TestFixture]
public class MauiAnimationXamlExporterTest
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
    public void Metadata_は_MAUI_Animation_XAML()
    {
        var ex = new MauiAnimationXamlExporter(NoItems());
        Assert.That(ex.FormatName, Is.EqualTo("MAUI Animation XAML"));
        Assert.That(ex.DefaultFileExtension, Is.EqualTo(".xaml"));
        Assert.That(ex.IsMultiFile, Is.False);
    }

    [Test]
    public void Ctor_allItems_null_は_ArgumentNullException()
    {
        Assert.That(() => new MauiAnimationXamlExporter(null),
            Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void Export_outputPath空は_ArgumentException()
    {
        var ex = new MauiAnimationXamlExporter(NoItems(), writeAllText: (_, _) => { });
        Assert.That(() => ex.Export(NewEmptyTimeline(), "", null, null),
            Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Export_は_xaml_と_xaml_cs_を必ず両方書き出す_戻り値2()
    {
        var writes = new List<(string path, string content)>();
        var ex = new MauiAnimationXamlExporter(NoItems(), writeAllText: (p, c) => writes.Add((p, c)));

        var written = ex.Export(NewEmptyTimeline(), "C:/tmp/Foo.xaml", null, null);

        Assert.That(written, Is.EqualTo(2));
        Assert.That(writes.Count, Is.EqualTo(2));
        Assert.That(writes[0].path, Is.EqualTo("C:/tmp/Foo.xaml"));
        Assert.That(writes[0].content, Does.Contain("<ContentView x:Class="));
        Assert.That(writes[1].path, Is.EqualTo("C:/tmp/Foo.xaml.cs"));
        Assert.That(writes[1].content, Does.Contain("public partial class FuiAnimation : ContentView"));
    }

    [Test]
    public void Export_optionsで_namespaceとclassNameを反映()
    {
        var writes = new List<(string path, string content)>();
        var ex = new MauiAnimationXamlExporter(NoItems(), writeAllText: (p, c) => writes.Add((p, c)));
        var options = new Dictionary<string, object>
        {
            { "TargetNamespace", "Acme.MAUI" },
            { "ClassName", "MySpin" },
        };
        ex.Export(NewEmptyTimeline(), "C:/tmp/MySpin.xaml", null, options);
        Assert.That(writes[0].content, Does.Contain("x:Class=\"Acme.MAUI.MySpin\""));
        Assert.That(writes[1].content, Does.Contain("namespace Acme.MAUI;"));
        Assert.That(writes[1].content, Does.Contain("public partial class MySpin : ContentView"));
    }

    [Test]
    public void Export_Track1個ありで_xaml_csにAnimationコードが出る()
    {
        var rect = new NRectangleViewModel(10, 20, 30, 40);
        rect.EdgeBrush.Value = null;
        rect.FillBrush.Value = null;
        var t = NewEmptyTimeline();
        var track = new AnimationTrack(new PropertyRef(rect.ID, "Width.Value", AnimatedValueType.Double));
        track.Keyframes.Add(new Keyframe(0.0, 30.0, EasingKind.LinearEase, EasingMode.EaseIn));
        track.Keyframes.Add(new Keyframe(1.0, 100.0, EasingKind.LinearEase, EasingMode.EaseIn));
        t.Tracks.Add(track);

        var writes = new List<(string path, string content)>();
        var ex = new MauiAnimationXamlExporter(new[] { rect }, writeAllText: (p, c) => writes.Add((p, c)));
        ex.Export(t, "C:/tmp/A.xaml", null, null);

        Assert.That(writes[0].content, Does.Contain("<Rectangle x:Name=\"Item_"));
        Assert.That(writes[1].content, Does.Contain(".WidthRequest = d;"));
        Assert.That(writes[1].content, Does.Contain(".Commit(this, \"Item_"));
    }
}
