using boilersGraphics.Helpers.Animation;
using boilersGraphics.Models.Animation;
using NUnit.Framework;
using System;
using System.IO;

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
}
