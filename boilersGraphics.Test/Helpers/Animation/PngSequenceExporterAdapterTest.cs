using boilersGraphics.Helpers.Animation;
using boilersGraphics.Models.Animation;
using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.Animation;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace boilersGraphics.Test.Helpers.Animation;

[TestFixture]
public class PngSequenceExporterAdapterTest
{
    [SetUp]
    public void SetUp()
    {
        boilersGraphics.App.IsTest = true;
    }

    private static (TimelineViewModel tl, NRectangleViewModel rect) NewTimeline()
    {
        var tl = new TimelineViewModel(5.0, 30);
        var rect = new NRectangleViewModel();
        var track = new AnimationTrack(new PropertyRef(rect.ID, "Left.Value", AnimatedValueType.Double));
        track.Keyframes.Add(new Keyframe(0.0, 0.0, EasingKind.LinearEase, EasingMode.EaseIn));
        track.Keyframes.Add(new Keyframe(2.0, 100.0, EasingKind.LinearEase, EasingMode.EaseIn));
        tl.Tracks.Add(track);
        return (tl, rect);
    }

    [Test]
    public void Metadata_は_PNGSequence_を表す()
    {
        var ad = new PngSequenceExporterAdapter((_, _) => { });
        Assert.That(ad.FormatName, Is.EqualTo("PNG Sequence"));
        Assert.That(ad.DefaultFileExtension, Is.EqualTo(".png"));
        Assert.That(ad.IsMultiFile, Is.True);
    }

    [Test]
    public void Ctor_renderAndSaveFrame_null_は_ArgumentNullException()
    {
        Assert.That(() => new PngSequenceExporterAdapter(null),
            Throws.TypeOf<ArgumentNullException>());
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Export_options_から_Settings_を組立てて_PngSequenceExporter_Export_に橋渡し()
    {
        var (tl, rect) = NewTimeline();
        var calls = new List<(double time, string path)>();
        var ad = new PngSequenceExporterAdapter((t, p) => calls.Add((t, p)));

        var options = new Dictionary<string, object>
        {
            { "Start", 0.0 },
            { "End", 1.0 },
            { "Fps", 5 },
            { "FilenamePrefix", "out_" },
        };

        var saved = ad.Export(tl, "C:/outdir", g => g == rect.ID ? rect : null, options);

        Assert.That(saved, Is.EqualTo(6)); // 0..1s @5fps = 6 frames
        Assert.That(calls.Count, Is.EqualTo(6));
        Assert.That(calls[0].path, Is.EqualTo(Path.Combine("C:/outdir", "out_0000.png")));
        Assert.That(calls[5].path, Is.EqualTo(Path.Combine("C:/outdir", "out_0005.png")));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Export_FilenamePrefix_省略_は_デフォルト_frame_()
    {
        var (tl, rect) = NewTimeline();
        var calls = new List<string>();
        var ad = new PngSequenceExporterAdapter((_, p) => calls.Add(p));

        ad.Export(tl, "D:/o", g => g == rect.ID ? rect : null,
            new Dictionary<string, object> { { "Start", 0.0 }, { "End", 0.4 }, { "Fps", 5 } });

        Assert.That(calls[0], Is.EqualTo(Path.Combine("D:/o", "frame_0000.png")));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Export_Start_必須_未指定_は_ArgumentException()
    {
        var (tl, _) = NewTimeline();
        var ad = new PngSequenceExporterAdapter((_, _) => { });
        var options = new Dictionary<string, object> { { "End", 1.0 }, { "Fps", 30 } };

        Assert.That(() => ad.Export(tl, "C:/o", null, options),
            Throws.TypeOf<ArgumentException>().With.Message.Contains("Start"));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Export_End_必須_未指定_は_ArgumentException()
    {
        var (tl, _) = NewTimeline();
        var ad = new PngSequenceExporterAdapter((_, _) => { });
        var options = new Dictionary<string, object> { { "Start", 0.0 }, { "Fps", 30 } };

        Assert.That(() => ad.Export(tl, "C:/o", null, options),
            Throws.TypeOf<ArgumentException>().With.Message.Contains("End"));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Export_Fps_必須_未指定_は_ArgumentException()
    {
        var (tl, _) = NewTimeline();
        var ad = new PngSequenceExporterAdapter((_, _) => { });
        var options = new Dictionary<string, object> { { "Start", 0.0 }, { "End", 1.0 } };

        Assert.That(() => ad.Export(tl, "C:/o", null, options),
            Throws.TypeOf<ArgumentException>().With.Message.Contains("Fps"));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Export_型不一致_は_ArgumentException()
    {
        var (tl, _) = NewTimeline();
        var ad = new PngSequenceExporterAdapter((_, _) => { });
        var options = new Dictionary<string, object>
        {
            { "Start", "not-a-number" }, // ← string で double に変換不可
            { "End", 1.0 },
            { "Fps", 30 },
        };

        Assert.That(() => ad.Export(tl, "C:/o", null, options),
            Throws.TypeOf<ArgumentException>());
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Export_timeline_null_は_ArgumentNullException()
    {
        var ad = new PngSequenceExporterAdapter((_, _) => { });
        var options = new Dictionary<string, object> { { "Start", 0.0 }, { "End", 1.0 }, { "Fps", 30 } };
        Assert.That(() => ad.Export(null, "C:/o", null, options),
            Throws.TypeOf<ArgumentNullException>());
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Export_outputPath_空_は_ArgumentException()
    {
        var (tl, _) = NewTimeline();
        var ad = new PngSequenceExporterAdapter((_, _) => { });
        var options = new Dictionary<string, object> { { "Start", 0.0 }, { "End", 1.0 }, { "Fps", 30 } };

        Assert.That(() => ad.Export(tl, "", null, options),
            Throws.TypeOf<ArgumentException>());
        Assert.That(() => ad.Export(tl, null, null, options),
            Throws.TypeOf<ArgumentException>());
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Export_options_null_は_ArgumentNullException()
    {
        var (tl, _) = NewTimeline();
        var ad = new PngSequenceExporterAdapter((_, _) => { });
        Assert.That(() => ad.Export(tl, "C:/o", null, null),
            Throws.TypeOf<ArgumentNullException>());
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Export_int_を_要求するキーに_long_を渡すと_変換成功()
    {
        var (tl, rect) = NewTimeline();
        var calls = 0;
        var ad = new PngSequenceExporterAdapter((_, _) => calls++);
        var options = new Dictionary<string, object>
        {
            { "Start", 0.0 },
            { "End", 0.4 },
            { "Fps", 5L }, // long → int に Convert.ChangeType で OK
        };

        Assert.DoesNotThrow(() => ad.Export(tl, "C:/o", g => g == rect.ID ? rect : null, options));
        Assert.That(calls, Is.EqualTo(3)); // 0..0.4s @5fps = 3 frames
    }
}
