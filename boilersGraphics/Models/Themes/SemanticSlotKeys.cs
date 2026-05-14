namespace boilersGraphics.Models.Themes;

/// <summary>
/// Phase 4-a §3.1.2 / Q-2 案 A: カラーパレットの固定 5 セマンティックスロットキー。
/// </summary>
public static class SemanticSlotKeys
{
    public const string Primary = "primary";
    public const string Accent = "accent";
    public const string Warning = "warning";
    public const string Info = "info";
    public const string Background = "background";

    /// <summary>全 5 スロットを宣言順で返す。テーマプリセット定義 / UI 並び順で使用。</summary>
    public static readonly string[] All =
    {
        Primary,
        Accent,
        Warning,
        Info,
        Background,
    };
}
