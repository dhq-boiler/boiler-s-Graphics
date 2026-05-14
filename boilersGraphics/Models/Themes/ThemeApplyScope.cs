namespace boilersGraphics.Models.Themes;

/// <summary>
/// Phase 4-a §3.4 / Q-10 案 C: テーマ適用の対象範囲を表す。
/// </summary>
public enum ThemeApplyScope
{
    /// <summary>現在選択中の図形のみ。</summary>
    SelectedItems,

    /// <summary>アクティブレイヤー全体。</summary>
    ActiveLayer,

    /// <summary>プロジェクト全体 (全レイヤーの全図形)。</summary>
    EntireProject,
}

/// <summary>
/// Phase 4-c: パレット適用の書き換え対象 (線 / 塗り / 両方)。
/// </summary>
public enum ThemeApplyTarget
{
    /// <summary>線色 (EdgeBrush) のみ書き換え。</summary>
    EdgeOnly,

    /// <summary>塗り色 (FillBrush) のみ書き換え。</summary>
    FillOnly,

    /// <summary>線と塗りの両方 (Edge=primary, Fill=background) を書き換え。</summary>
    Both,
}
