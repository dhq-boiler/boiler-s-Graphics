using boilersGraphics.Models.Animation;

namespace boilersGraphics.Helpers.Animation.Export;

/// <summary>
/// Phase 5.5-b: <see cref="EasingKind"/> × <see cref="EasingMode"/> を WPF Storyboard で使う
/// <c>EasingFunction</c> 要素の XAML 文字列に変換する pure helper。
/// 仕様書 Q-5 に従い、Phase 5 EasingKind は WPF 標準 Easing クラスと 1:1 同名対応:
///   Linear→null / SineEase / QuadraticEase / CubicEase / QuarticEase / QuinticEase /
///   ExponentialEase / CircleEase / PowerEase / ElasticEase / BackEase / BounceEase。
/// EasingMode は WPF <c>System.Windows.Media.Animation.EasingMode</c> の <c>EaseIn / EaseOut / EaseInOut</c>
/// と完全に同名なのでそのまま使う。
/// </summary>
public static class EasingToXamlMapper
{
    /// <summary>
    /// WPF Storyboard 用の <c>EasingFunction</c> XAML 要素を返す。
    /// LinearEase の場合は null を返す (EasingDoubleKeyFrame の EasingFunction を未指定にすると線形補間)。
    /// 戻り値は単一行の XAML 要素文字列で、外側からインデント整形して挿入する想定。
    /// </summary>
    public static string ToWpfEasingXaml(EasingKind kind, EasingMode mode)
    {
        var elementName = ToWpfElementName(kind);
        if (elementName is null) return null;
        return $"<{elementName} EasingMode=\"{mode}\" />";
    }

    /// <summary>
    /// 該当する WPF EasingFunction 要素名 (e.g. "SineEase")。LinearEase は null。
    /// </summary>
    public static string ToWpfElementName(EasingKind kind) => kind switch
    {
        EasingKind.LinearEase => null,
        EasingKind.SineEase => "SineEase",
        EasingKind.QuadraticEase => "QuadraticEase",
        EasingKind.CubicEase => "CubicEase",
        EasingKind.QuarticEase => "QuarticEase",
        EasingKind.QuinticEase => "QuinticEase",
        EasingKind.ExponentialEase => "ExponentialEase",
        EasingKind.CircleEase => "CircleEase",
        EasingKind.PowerEase => "PowerEase",
        EasingKind.ElasticEase => "ElasticEase",
        EasingKind.BackEase => "BackEase",
        EasingKind.BounceEase => "BounceEase",
        _ => null,
    };

    /// <summary>
    /// LinearEase のとき <c>LinearXxxKeyFrame</c>、それ以外は <c>EasingXxxKeyFrame</c> を使う。
    /// Phase 5.5-b 仕様 (Q-3 案 A): <c>*UsingKeyFrames</c> に統一、Easing なしは Linear 系、
    /// 有りは Easing 系 + EasingFunction プロパティ。
    /// </summary>
    public static bool RequiresEasingFunction(EasingKind kind) => kind != EasingKind.LinearEase;
}
