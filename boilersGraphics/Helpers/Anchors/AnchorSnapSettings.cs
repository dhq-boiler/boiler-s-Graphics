using R3;

namespace boilersGraphics.Helpers.Anchors;

/// <summary>
/// Phase 3-i §5 / Q-7 案 C: アンカー吸着距離のグローバル設定。
/// 設定ダイアログから書き込まれ、AnchorSnap.FindNearestAnchorRef で都度読み出される。
/// Phase 3.5: 値は Properties\Settings.settings (User scope) に永続化される。
/// アプリ起動時に Default から読み込み、変更時に Default 経由で保存。
/// </summary>
public static class AnchorSnapSettings
{
    private const double DefaultDistance = 10.0;
    private static bool _persistEnabled = true;

    static AnchorSnapSettings()
    {
        // 起動時に永続化された値を読み込み (テスト環境では App.IsTest=true で永続化スキップ)
        var initial = TryLoadInitial();
        SnapDistance = new BindableReactiveProperty<double>(initial);

        // 変更時に Properties.Settings へ書き戻し (テスト時/アプリ初期化前は skip)
        SnapDistance.Skip(1).Subscribe(value =>
        {
            if (!_persistEnabled) return;
            try
            {
                boilersGraphics.Properties.Settings.Default.AnchorSnapDistance = value;
                boilersGraphics.Properties.Settings.Default.Save();
            }
            catch (System.Configuration.ConfigurationErrorsException)
            {
                // 設定ファイルが壊れている / 書込権限なし等は無視 (UI で警告は出さない)
            }
        });
    }

    /// <summary>吸着距離 (px)。最小 0、推奨 5〜30。設定ダイアログ経由で変更可能。</summary>
    public static BindableReactiveProperty<double> SnapDistance { get; }

    /// <summary>テスト用: 永続化を無効化する。プロセス内のみ反映されることになる。</summary>
    public static void DisablePersistenceForTests() => _persistEnabled = false;

    private static double TryLoadInitial()
    {
        try
        {
            // App.IsTest 経由でテスト環境を判定: テスト時は Settings に触らずデフォルト値で初期化。
            if (boilersGraphics.App.IsTest) return DefaultDistance;
            var stored = boilersGraphics.Properties.Settings.Default.AnchorSnapDistance;
            return stored > 0 ? stored : DefaultDistance;
        }
        catch (System.Configuration.ConfigurationErrorsException)
        {
            return DefaultDistance;
        }
    }
}
