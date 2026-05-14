using boilersGraphics.Models.Animation;
using System;
using IoPath = System.IO.Path;

namespace boilersGraphics.Helpers.Animation;

/// <summary>
/// Phase 5-f-1: PNG 連番書出の純粋計算ヘルパ。
/// <list type="bullet">
/// <item>設定値の形式バリデーション (副作用なし、ディレクトリ存在チェックは含めない)</item>
/// <item>書出フレーム数 / 各フレームの時刻 / 各フレームのファイルパス算出</item>
/// </list>
/// 実 Renderer 呼び出しや I/O は Phase 5-f-2 で別 helper / Command に乗せる。
/// </summary>
public static class PngSequenceExporter
{
    /// <summary>ファイル名のゼロパディング桁数の下限 (Phase 5-a memo: 0001.png〜)。</summary>
    public const int MinFilenameDigits = 4;

    /// <summary>許容する Fps の最大値。Phase 5-f-1 では実用範囲のみ。</summary>
    public const int MaxAllowedFps = 240;

    public readonly record struct ValidationResult(bool IsValid, string ErrorMessage)
    {
        public static ValidationResult Ok() => new(true, null);
        public static ValidationResult Fail(string message) => new(false, message);
    }

    /// <summary>
    /// 形式チェック (副作用なし)。OutputDirectory のディレクトリ存在チェックは含まない。
    /// timelineDuration を渡すと End &gt; Duration の場合に NG にする (0 以下なら範囲チェックスキップ)。
    /// </summary>
    public static ValidationResult Validate(PngSequenceExportSettings settings, double timelineDuration = 0.0)
    {
        if (settings is null) return ValidationResult.Fail("Settings が null です。");
        if (string.IsNullOrWhiteSpace(settings.OutputDirectory))
            return ValidationResult.Fail("出力ディレクトリが指定されていません。");
        if (string.IsNullOrWhiteSpace(settings.FilenamePrefix))
            return ValidationResult.Fail("ファイル名プレフィックスが指定されていません。");
        if (settings.Fps <= 0 || settings.Fps > MaxAllowedFps)
            return ValidationResult.Fail($"Fps は 1〜{MaxAllowedFps} の範囲で指定してください。");
        if (settings.Start < 0)
            return ValidationResult.Fail("開始時刻は 0 以上である必要があります。");
        if (settings.End <= settings.Start)
            return ValidationResult.Fail("終了時刻は開始時刻より大きい必要があります。");
        if (timelineDuration > 0 && settings.End > timelineDuration + 1e-9)
            return ValidationResult.Fail($"終了時刻が Timeline.Duration ({timelineDuration:F2} s) を超えています。");
        return ValidationResult.Ok();
    }

    /// <summary>
    /// Start から End まで Fps 間隔でサンプリングするフレーム数。End ちょうども 1 枚として含める。
    /// 例: Start=0, End=2, Fps=30 → 2*30+1 = 61 frames。
    /// 不正設定 (Fps≤0 / End&lt;=Start) では 0 を返す。
    /// </summary>
    public static int ComputeFrameCount(PngSequenceExportSettings settings)
    {
        if (settings is null) return 0;
        if (settings.Fps <= 0) return 0;
        if (settings.End <= settings.Start) return 0;
        var span = settings.End - settings.Start;
        // 浮動小数の丸めで 1 フレーム取りこぼすのを避けるため、span に小さなマージンを足してから切り捨て + 1。
        var count = (int)Math.Floor(span * settings.Fps + 1e-9) + 1;
        return Math.Max(1, count);
    }

    /// <summary>
    /// frameIndex 番目 (0-origin) のフレーム時刻。Start + frameIndex / Fps。
    /// 範囲外の frameIndex でもクランプはせず計算結果をそのまま返す (呼び出し側の責任)。
    /// </summary>
    public static double GetFrameTime(PngSequenceExportSettings settings, int frameIndex)
    {
        if (settings is null) return 0.0;
        if (settings.Fps <= 0) return settings?.Start ?? 0.0;
        return settings.Start + (double)frameIndex / settings.Fps;
    }

    /// <summary>
    /// frameIndex 番目のフレームのファイルパス。
    /// ゼロパディング桁数は <c>max(MinFilenameDigits, totalFrames.ToString().Length)</c>。
    /// 例: prefix="frame_", totalFrames=61, frameIndex=3 → "{dir}/frame_0003.png"。
    /// </summary>
    public static string BuildFrameFilePath(PngSequenceExportSettings settings, int frameIndex, int totalFrames)
    {
        if (settings is null) throw new ArgumentNullException(nameof(settings));
        var digits = Math.Max(MinFilenameDigits, totalFrames.ToString().Length);
        var fileName = $"{settings.FilenamePrefix}{frameIndex.ToString().PadLeft(digits, '0')}.png";
        var dir = settings.OutputDirectory ?? string.Empty;
        return IoPath.Combine(dir, fileName);
    }
}
