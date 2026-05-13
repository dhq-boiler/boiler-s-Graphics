using R3;

namespace boilersGraphics.Helpers.Anchors;

/// <summary>
/// Phase 3-i §5 / Q-7 案 C: アンカー吸着距離のグローバル設定。
/// 設定ダイアログから書き込まれ、AnchorSnap.FindNearestAnchorRef で都度読み出される。
/// 起動毎にデフォルト 10.0 px で初期化される (Properties\Settings.settings への永続化は将来対応)。
/// </summary>
public static class AnchorSnapSettings
{
    /// <summary>吸着距離 (px)。最小 0、推奨 5〜30。設定ダイアログ経由で変更可能。</summary>
    public static BindableReactiveProperty<double> SnapDistance { get; } =
        new BindableReactiveProperty<double>(10.0);
}
