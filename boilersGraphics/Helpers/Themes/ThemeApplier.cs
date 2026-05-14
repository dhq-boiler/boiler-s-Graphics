using boilersGraphics.Models.Themes;
using System.Collections.Generic;
using System.Windows.Media;

namespace boilersGraphics.Helpers.Themes;

/// <summary>
/// Phase 4-c / Q-3 案 A: テーマのパレット色を図形の EdgeBrush / FillBrush に直接書き換える pure ロジック。
/// セマンティックスロットキー (primary/accent/warning/info/background) と Theme を受け取り、
/// SolidColorBrush を返す。WPF VM への依存はインターフェース経由 (テスト容易)。
/// </summary>
public static class ThemeApplier
{
    /// <summary>
    /// テーマの semantic スロットを SolidColorBrush に変換する。
    /// スロット未登録 / インデックス範囲外なら null を返す。
    /// </summary>
    public static SolidColorBrush ToSolidColorBrush(Theme theme, string semanticKey)
    {
        if (theme == null || theme.Palette == null) return null;
        var color = theme.Palette.GetSemanticColor(semanticKey);
        if (!color.HasValue) return null;
        var brush = new SolidColorBrush(color.Value);
        brush.Freeze();
        return brush;
    }

    /// <summary>
    /// <see cref="ThemeApplyTarget"/> に応じて、テーマから (edgeBrush, fillBrush) のペアを決定する。
    /// 書き換え不要な側は null。
    /// 既定マッピング: Edge=primary, Fill=background。
    /// </summary>
    public static (SolidColorBrush edge, SolidColorBrush fill) ResolveBrushes(
        Theme theme,
        ThemeApplyTarget target)
    {
        if (theme == null) return (null, null);
        var primary = ToSolidColorBrush(theme, SemanticSlotKeys.Primary);
        var background = ToSolidColorBrush(theme, SemanticSlotKeys.Background);
        return target switch
        {
            ThemeApplyTarget.EdgeOnly => (primary, null),
            ThemeApplyTarget.FillOnly => (null, background),
            ThemeApplyTarget.Both => (primary, background),
            _ => (null, null),
        };
    }

    /// <summary>
    /// 適用範囲 <see cref="ThemeApplyScope"/> に応じて、対象アイテム ID リストを返す pure 関数。
    /// 引数の Selected / ActiveLayer / All は呼び出し側で取得して渡す (WPF / R3 依存を排除)。
    /// </summary>
    public static IReadOnlyList<T> ResolveScope<T>(
        ThemeApplyScope scope,
        IReadOnlyList<T> selected,
        IReadOnlyList<T> activeLayer,
        IReadOnlyList<T> all)
    {
        return scope switch
        {
            ThemeApplyScope.SelectedItems => selected ?? new List<T>(),
            ThemeApplyScope.ActiveLayer => activeLayer ?? new List<T>(),
            ThemeApplyScope.EntireProject => all ?? new List<T>(),
            _ => new List<T>(),
        };
    }

    /// <summary>
    /// Phase 4-d: 線種プリセットから、StrokeDashArray の Frozen 化済みコピーを返す pure 関数。
    /// 元の <see cref="LineStyle.StrokeDashArray"/> はテーマ側の共有参照なので、書換時は必ずコピーする。
    /// </summary>
    public static DoubleCollection CopyDashArray(LineStyle style)
    {
        if (style?.StrokeDashArray == null) return new DoubleCollection();
        var copy = new DoubleCollection();
        foreach (var v in style.StrokeDashArray)
        {
            copy.Add(v);
        }
        return copy;
    }

    /// <summary>
    /// Phase 4-e: テーマのデフォルトグロー設定を (radius, intensity, color) のタプルに変換する pure 関数。
    /// テーマ / DefaultGlow が null なら 0/0/null を返す。
    /// </summary>
    public static (double radius, double intensity, System.Windows.Media.Color? color) ResolveGlow(Theme theme)
    {
        if (theme?.DefaultGlow == null) return (0, 0, null);
        var g = theme.DefaultGlow;
        return (g.Radius, g.Intensity, g.Color);
    }

    /// <summary>
    /// Phase 4-e: ぼかし半径 (px) から OpenCV GaussianBlur 用の kernel サイズ (奇数) を返す pure 関数。
    /// radius 0 の場合は 1 (kernel 1x1、実質処理なし)。
    /// </summary>
    public static int ResolveKernelSize(double radius)
    {
        var raw = (int)System.Math.Max(0, System.Math.Round(radius));
        var size = raw * 2 + 1;
        if (size < 1) size = 1;
        if (size % 2 == 0) size++;
        return size;
    }
}
