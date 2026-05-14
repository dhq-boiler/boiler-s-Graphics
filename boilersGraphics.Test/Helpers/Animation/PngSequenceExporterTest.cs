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
public class PngSequenceExporterTest
{
    private static PngSequenceExportSettings Valid(double start = 0.0, double end = 2.0, int fps = 30,
        string outDir = "C:/tmp/out", string prefix = "frame_") => new()
    {
        Start = start,
        End = end,
        Fps = fps,
        OutputDirectory = outDir,
        FilenamePrefix = prefix,
    };

    // ----- Validate -----

    [Test]
    public void Validate_標準設定_は_IsValid_true()
    {
        var r = PngSequenceExporter.Validate(Valid());
        Assert.That(r.IsValid, Is.True);
        Assert.That(r.ErrorMessage, Is.Null);
    }

    [Test]
    public void Validate_settings_null_は_NG()
    {
        var r = PngSequenceExporter.Validate(null);
        Assert.That(r.IsValid, Is.False);
        Assert.That(r.ErrorMessage, Does.Contain("null"));
    }

    [Test]
    public void Validate_OutputDirectory_null_or_whitespace_は_NG()
    {
        Assert.That(PngSequenceExporter.Validate(Valid(outDir: null)).IsValid, Is.False);
        Assert.That(PngSequenceExporter.Validate(Valid(outDir: "")).IsValid, Is.False);
        Assert.That(PngSequenceExporter.Validate(Valid(outDir: "   ")).IsValid, Is.False);
    }

    [Test]
    public void Validate_FilenamePrefix_null_or_whitespace_は_NG()
    {
        Assert.That(PngSequenceExporter.Validate(Valid(prefix: null)).IsValid, Is.False);
        Assert.That(PngSequenceExporter.Validate(Valid(prefix: "")).IsValid, Is.False);
        Assert.That(PngSequenceExporter.Validate(Valid(prefix: "   ")).IsValid, Is.False);
    }

    [Test]
    public void Validate_Fps_0以下_は_NG()
    {
        Assert.That(PngSequenceExporter.Validate(Valid(fps: 0)).IsValid, Is.False);
        Assert.That(PngSequenceExporter.Validate(Valid(fps: -1)).IsValid, Is.False);
    }

    [Test]
    public void Validate_Fps_過大_は_NG()
    {
        Assert.That(PngSequenceExporter.Validate(Valid(fps: 1000)).IsValid, Is.False);
        Assert.That(PngSequenceExporter.Validate(Valid(fps: PngSequenceExporter.MaxAllowedFps + 1)).IsValid, Is.False);
    }

    [Test]
    public void Validate_Fps_境界値_は_OK()
    {
        Assert.That(PngSequenceExporter.Validate(Valid(fps: 1)).IsValid, Is.True);
        Assert.That(PngSequenceExporter.Validate(Valid(fps: PngSequenceExporter.MaxAllowedFps)).IsValid, Is.True);
    }

    [Test]
    public void Validate_Start_負数_は_NG()
    {
        Assert.That(PngSequenceExporter.Validate(Valid(start: -1.0)).IsValid, Is.False);
    }

    [Test]
    public void Validate_End_が_Start以下_は_NG()
    {
        Assert.That(PngSequenceExporter.Validate(Valid(start: 1.0, end: 1.0)).IsValid, Is.False);
        Assert.That(PngSequenceExporter.Validate(Valid(start: 2.0, end: 1.0)).IsValid, Is.False);
    }

    [Test]
    public void Validate_End_が_Duration_超え_は_NG()
    {
        Assert.That(PngSequenceExporter.Validate(Valid(end: 10.0), timelineDuration: 5.0).IsValid, Is.False);
    }

    [Test]
    public void Validate_Duration_0以下_は_範囲チェックスキップ()
    {
        // timelineDuration = 0 (= 未指定相当) なら End>Duration のチェックは行わない
        Assert.That(PngSequenceExporter.Validate(Valid(end: 100.0), timelineDuration: 0.0).IsValid, Is.True);
    }

    // ----- ComputeFrameCount -----

    [Test]
    public void ComputeFrameCount_Start_0_End_2_Fps_30_は_61()
    {
        Assert.That(PngSequenceExporter.ComputeFrameCount(Valid(0, 2, 30)), Is.EqualTo(61));
    }

    [Test]
    public void ComputeFrameCount_Start_1_End_2_Fps_30_は_31()
    {
        Assert.That(PngSequenceExporter.ComputeFrameCount(Valid(1, 2, 30)), Is.EqualTo(31));
    }

    [Test]
    public void ComputeFrameCount_End_eq_Start_は_0()
    {
        Assert.That(PngSequenceExporter.ComputeFrameCount(Valid(1, 1, 30)), Is.EqualTo(0));
    }

    [Test]
    public void ComputeFrameCount_Fps_0以下_は_0()
    {
        Assert.That(PngSequenceExporter.ComputeFrameCount(Valid(0, 2, 0)), Is.EqualTo(0));
    }

    [Test]
    public void ComputeFrameCount_設定null_は_0()
    {
        Assert.That(PngSequenceExporter.ComputeFrameCount(null), Is.EqualTo(0));
    }

    [Test]
    public void ComputeFrameCount_浮動小数の取りこぼし_は_補正される()
    {
        // 0.1 * 10 が浮動小数で 0.9999... になりがちなケース
        Assert.That(PngSequenceExporter.ComputeFrameCount(Valid(0, 0.1, 10)), Is.EqualTo(2));
    }

    // ----- GetFrameTime -----

    [Test]
    public void GetFrameTime_index_0_は_Start()
    {
        Assert.That(PngSequenceExporter.GetFrameTime(Valid(0.5, 2, 30), 0), Is.EqualTo(0.5));
    }

    [Test]
    public void GetFrameTime_index_30_Fps_30_は_Start_plus_1()
    {
        Assert.That(PngSequenceExporter.GetFrameTime(Valid(0.0, 5, 30), 30), Is.EqualTo(1.0).Within(1e-9));
    }

    [Test]
    public void GetFrameTime_最終フレーム_は_End_と一致()
    {
        var s = Valid(0, 2, 30);
        var n = PngSequenceExporter.ComputeFrameCount(s);
        Assert.That(PngSequenceExporter.GetFrameTime(s, n - 1), Is.EqualTo(2.0).Within(1e-9));
    }

    [Test]
    public void GetFrameTime_設定null_は_0()
    {
        Assert.That(PngSequenceExporter.GetFrameTime(null, 0), Is.EqualTo(0.0));
    }

    // ----- BuildFrameFilePath -----

    [Test]
    public void BuildFrameFilePath_標準_は_最小4桁ゼロパディング()
    {
        var path = PngSequenceExporter.BuildFrameFilePath(Valid(outDir: "C:/out", prefix: "frame_"), 3, totalFrames: 61);
        Assert.That(path, Is.EqualTo(Path.Combine("C:/out", "frame_0003.png")));
    }

    [Test]
    public void BuildFrameFilePath_totalFrames_5桁_は_5桁パディング()
    {
        var path = PngSequenceExporter.BuildFrameFilePath(Valid(outDir: "C:/out", prefix: "f"), 7, totalFrames: 12345);
        Assert.That(path, Is.EqualTo(Path.Combine("C:/out", "f00007.png")));
    }

    [Test]
    public void BuildFrameFilePath_totalFrames_1桁_でも_最小4桁を維持()
    {
        var path = PngSequenceExporter.BuildFrameFilePath(Valid(outDir: "C:/out", prefix: "f_"), 0, totalFrames: 5);
        Assert.That(path, Is.EqualTo(Path.Combine("C:/out", "f_0000.png")));
    }

    [Test]
    public void BuildFrameFilePath_settings_null_は_ArgumentNullException()
    {
        Assert.That(() => PngSequenceExporter.BuildFrameFilePath(null, 0, 1),
            Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void BuildFrameFilePath_OutputDirectory_null_でも_例外無し_ファイル名のみ返る()
    {
        var s = new PngSequenceExportSettings
        {
            Start = 0,
            End = 2,
            Fps = 30,
            OutputDirectory = null,
            FilenamePrefix = "f_",
        };
        var path = PngSequenceExporter.BuildFrameFilePath(s, 1, 2);
        Assert.That(path, Is.EqualTo("f_0001.png"));
    }

    // ----- DTO record 等価性 -----

    [Test]
    public void Settings_record_は_値等価()
    {
        var a = Valid();
        var b = Valid();
        Assert.That(a, Is.EqualTo(b));
    }

    [Test]
    public void Settings_FilenamePrefix_デフォルトは_frame_()
    {
        var s = new PngSequenceExportSettings { Start = 0, End = 1, Fps = 30, OutputDirectory = "C:/tmp" };
        Assert.That(s.FilenamePrefix, Is.EqualTo("frame_"));
    }

    // ----- Export (Phase 5-f-2) -----

    private static (TimelineViewModel tl, NRectangleViewModel rect) NewTimelineWithTrack()
    {
        boilersGraphics.App.IsTest = true;
        var tl = new TimelineViewModel(5.0, 30);
        var rect = new NRectangleViewModel();
        var track = new AnimationTrack(new PropertyRef(rect.ID, "Left.Value", AnimatedValueType.Double));
        track.Keyframes.Add(new Keyframe(0.0, 100.0, EasingKind.LinearEase, EasingMode.EaseIn));
        track.Keyframes.Add(new Keyframe(2.0, 200.0, EasingKind.LinearEase, EasingMode.EaseIn));
        tl.Tracks.Add(track);
        return (tl, rect);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Export_は_フレーム数分_renderAndSaveFrame_を呼ぶ()
    {
        var (tl, rect) = NewTimelineWithTrack();
        var settings = Valid(0, 2, 30, outDir: "C:/tmp");
        var calls = new List<(double time, string path)>();

        var saved = PngSequenceExporter.Export(tl, settings, g => g == rect.ID ? rect : null,
            (t, p) => calls.Add((t, p)));

        var expected = PngSequenceExporter.ComputeFrameCount(settings);
        Assert.That(saved, Is.EqualTo(expected));
        Assert.That(calls.Count, Is.EqualTo(expected));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Export_は_各フレーム時刻_と_ファイル名_を_順に渡す()
    {
        var (tl, rect) = NewTimelineWithTrack();
        var settings = Valid(0, 0.1, 10, outDir: "C:/out", prefix: "f_");
        var calls = new List<(double time, string path)>();

        PngSequenceExporter.Export(tl, settings, g => g == rect.ID ? rect : null,
            (t, p) => calls.Add((t, p)));

        // 0.1 * 10 + 1 = 2 frames: t = 0.0, 0.1
        Assert.That(calls.Count, Is.EqualTo(2));
        Assert.That(calls[0].time, Is.EqualTo(0.0).Within(1e-9));
        Assert.That(calls[1].time, Is.EqualTo(0.1).Within(1e-9));
        Assert.That(calls[0].path, Is.EqualTo(Path.Combine("C:/out", "f_0000.png")));
        Assert.That(calls[1].path, Is.EqualTo(Path.Combine("C:/out", "f_0001.png")));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Export_は_各フレームで_PlaybackEngine_ApplyAt_を呼んで_アイテム値が変動()
    {
        var (tl, rect) = NewTimelineWithTrack();
        // 0..2s @ 1fps = 3 frames (t=0,1,2)。Track は (0->100, 2->200) リニア → 100, 150, 200。
        var settings = Valid(0, 2, 1, outDir: "C:/out");
        var observed = new List<double>();

        PngSequenceExporter.Export(tl, settings, g => g == rect.ID ? rect : null,
            (t, _) => observed.Add(rect.Left.Value));

        Assert.That(observed.Count, Is.EqualTo(3));
        Assert.That(observed[0], Is.EqualTo(100.0));
        Assert.That(observed[1], Is.EqualTo(150.0));
        Assert.That(observed[2], Is.EqualTo(200.0));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Export_完了時_は_Snapshot_を_Restore_して_元値に戻る()
    {
        var (tl, rect) = NewTimelineWithTrack();
        rect.Left.Value = 100.0;
        var settings = Valid(0, 2, 1, outDir: "C:/out");

        PngSequenceExporter.Export(tl, settings, g => g == rect.ID ? rect : null,
            (_, _) => { });

        Assert.That(rect.Left.Value, Is.EqualTo(100.0), "Export 終了時に Snapshot で元の値に戻ること");
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Export_途中で_例外_でも_Restore_は_必ず実行()
    {
        var (tl, rect) = NewTimelineWithTrack();
        rect.Left.Value = 100.0;
        var settings = Valid(0, 2, 1, outDir: "C:/out");
        var calls = 0;

        Assert.Throws<InvalidOperationException>(() =>
        {
            PngSequenceExporter.Export(tl, settings, g => g == rect.ID ? rect : null,
                (_, _) =>
                {
                    calls++;
                    if (calls == 2) throw new InvalidOperationException("boom");
                });
        });

        Assert.That(rect.Left.Value, Is.EqualTo(100.0), "例外時も Restore 実行");
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Export_resolver_null_でも_動く_アイテム値は変更されず_renderAndSave_は呼ばれる()
    {
        var (tl, _) = NewTimelineWithTrack();
        var settings = Valid(0, 1, 5, outDir: "C:/out");
        var calls = 0;

        var saved = PngSequenceExporter.Export(tl, settings, resolver: null,
            (_, _) => calls++);

        Assert.That(saved, Is.GreaterThan(0));
        Assert.That(calls, Is.EqualTo(saved));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Export_timeline_null_は_ArgumentNullException()
    {
        Assert.That(() => PngSequenceExporter.Export(null, Valid(), null, (_, _) => { }),
            Throws.TypeOf<ArgumentNullException>());
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Export_settings_null_は_ArgumentNullException()
    {
        var (tl, _) = NewTimelineWithTrack();
        Assert.That(() => PngSequenceExporter.Export(tl, null, null, (_, _) => { }),
            Throws.TypeOf<ArgumentNullException>());
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Export_renderAndSaveFrame_null_は_ArgumentNullException()
    {
        var (tl, _) = NewTimelineWithTrack();
        Assert.That(() => PngSequenceExporter.Export(tl, Valid(), null, null),
            Throws.TypeOf<ArgumentNullException>());
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Export_invalid_settings_は_ArgumentException()
    {
        var (tl, _) = NewTimelineWithTrack();
        var settings = Valid(start: 0, end: 0, fps: 30); // End <= Start で NG
        Assert.That(() => PngSequenceExporter.Export(tl, settings, null, (_, _) => { }),
            Throws.TypeOf<ArgumentException>());
    }
}
