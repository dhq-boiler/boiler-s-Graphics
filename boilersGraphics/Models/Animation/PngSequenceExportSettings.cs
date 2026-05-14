namespace boilersGraphics.Models.Animation;

/// <summary>
/// Phase 5-f: PNG 連番書出の設定。
/// ダイアログ ViewModel から値を取り出して <see cref="boilersGraphics.Helpers.Animation.PngSequenceExporter"/> に渡す DTO。
/// 不変オブジェクトとして扱うため <c>record</c> + <c>init</c>。
/// </summary>
public sealed record class PngSequenceExportSettings
{
    /// <summary>書出開始時刻 (秒)。Timeline 上で何秒目から書き出すか。</summary>
    public double Start { get; init; }

    /// <summary>書出終了時刻 (秒)。Timeline 上で何秒目まで書き出すか (この時刻も含む)。</summary>
    public double End { get; init; }

    /// <summary>サンプリングレート (fps)。1 秒あたり何枚書き出すか。</summary>
    public int Fps { get; init; }

    /// <summary>出力ディレクトリの絶対パス。</summary>
    public string OutputDirectory { get; init; }

    /// <summary>出力ファイル名のプレフィックス。例: "frame_" → "frame_0001.png"。</summary>
    public string FilenamePrefix { get; init; } = "frame_";
}
