using boilersGraphics.Models.Animation;

namespace boilersGraphics.Helpers.Animation.Export;

/// <summary>
/// Phase 5.5-d: <see cref="EasingKind"/> × <see cref="EasingMode"/> を MAUI コードビハインドで使う
/// <c>Microsoft.Maui.Easing</c> インスタンス参照の C# 式に変換する pure helper。
///
/// 仕様書 Q-5 案 A に従い、MAUI 標準 Easing で直接表現できる組合せは <c>Easing.SinIn</c> 等の参照、
/// 表現できないものは <c>new Easing(t =&gt; ...)</c> の inline ラムダを返す。
///
/// MAUI 標準 (Microsoft.Maui.Easing):
/// Linear / SinIn / SinOut / SinInOut / CubicIn / CubicOut / CubicInOut /
/// BounceIn / BounceOut / SpringIn / SpringOut。
/// </summary>
public static class MauiEasingToCSharpMapper
{
    /// <summary>
    /// MAUI Easing インスタンスの C# 式を返す。LinearEase は EasingMode を問わず <c>Easing.Linear</c>。
    /// </summary>
    public static string ToExpression(EasingKind kind, EasingMode mode)
    {
        if (kind == EasingKind.LinearEase) return "Easing.Linear";

        // MAUI 標準で In/Out/InOut 三方を持つもの
        if (kind == EasingKind.SineEase) return ModedStandard("Sin", mode);
        if (kind == EasingKind.CubicEase) return ModedStandard("Cubic", mode);

        // BounceEase / BackEase は In/Out のみ MAUI 標準。InOut はカスタム。
        if (kind == EasingKind.BounceEase) return BounceExpr(mode);
        if (kind == EasingKind.BackEase) return BackExpr(mode);

        // 残りはすべてカスタムラムダ
        return CustomLambda(kind, mode);
    }

    /// <summary>
    /// MAUI 標準で完全対応するかどうか。<c>true</c> なら <see cref="ToExpression"/> は
    /// <c>Easing.Xxx</c> の単純参照を返す。カスタムラムダなら <c>false</c>。
    /// </summary>
    public static bool IsStandard(EasingKind kind, EasingMode mode)
    {
        if (kind == EasingKind.LinearEase) return true;
        if (kind == EasingKind.SineEase || kind == EasingKind.CubicEase) return true;
        if ((kind == EasingKind.BounceEase || kind == EasingKind.BackEase) && mode != EasingMode.EaseInOut) return true;
        return false;
    }

    private static string ModedStandard(string prefix, EasingMode mode) => mode switch
    {
        EasingMode.EaseIn => $"Easing.{prefix}In",
        EasingMode.EaseOut => $"Easing.{prefix}Out",
        EasingMode.EaseInOut => $"Easing.{prefix}InOut",
        _ => $"Easing.{prefix}In",
    };

    private static string BounceExpr(EasingMode mode) => mode switch
    {
        EasingMode.EaseIn => "Easing.BounceIn",
        EasingMode.EaseOut => "Easing.BounceOut",
        EasingMode.EaseInOut => CustomLambda(EasingKind.BounceEase, EasingMode.EaseInOut),
        _ => "Easing.BounceIn",
    };

    private static string BackExpr(EasingMode mode) => mode switch
    {
        EasingMode.EaseIn => "Easing.SpringIn",
        EasingMode.EaseOut => "Easing.SpringOut",
        EasingMode.EaseInOut => CustomLambda(EasingKind.BackEase, EasingMode.EaseInOut),
        _ => "Easing.SpringIn",
    };

    /// <summary>
    /// EasingKind+EasingMode を Phase 5 <see cref="EasingFunctions"/> の式に沿って inline ラムダで返す。
    /// 数式は <see cref="EasingFunctions.Apply"/> の core formula と一致させる。
    /// </summary>
    public static string CustomLambda(EasingKind kind, EasingMode mode)
    {
        var core = CoreExpression(kind);
        return mode switch
        {
            EasingMode.EaseIn => $"new Easing(t => {core})",
            EasingMode.EaseOut => $"new Easing(t => 1 - ({CoreExpressionFor("1 - t", kind)}))",
            EasingMode.EaseInOut => $"new Easing(t => t < 0.5 ? ({CoreExpressionFor("2 * t", kind)}) / 2 : (2 - ({CoreExpressionFor("2 - 2 * t", kind)})) / 2)",
            _ => $"new Easing(t => {core})",
        };
    }

    /// <summary>core(t) — kind に対応する純粋な EaseIn 式 (引数 t)</summary>
    private static string CoreExpression(EasingKind kind) => CoreExpressionFor("t", kind);

    /// <summary>引数表現を <paramref name="arg"/> に差し替えた core 式。</summary>
    private static string CoreExpressionFor(string arg, EasingKind kind) => kind switch
    {
        EasingKind.LinearEase => arg,
        EasingKind.QuadraticEase => $"({arg}) * ({arg})",
        EasingKind.CubicEase => $"({arg}) * ({arg}) * ({arg})",
        EasingKind.QuarticEase => $"({arg}) * ({arg}) * ({arg}) * ({arg})",
        EasingKind.QuinticEase => $"({arg}) * ({arg}) * ({arg}) * ({arg}) * ({arg})",
        EasingKind.SineEase => $"1 - Math.Cos(({arg}) * Math.PI / 2)",
        EasingKind.ExponentialEase => $"(Math.Exp(2 * ({arg})) - 1) / (Math.Exp(2) - 1)",
        EasingKind.CircleEase => $"1 - Math.Sqrt(1 - ({arg}) * ({arg}))",
        EasingKind.PowerEase => $"Math.Pow({arg}, 2.0)",
        EasingKind.ElasticEase => $"-Math.Pow(2, 10 * (({arg}) - 1)) * Math.Sin((({arg}) - 1 - 0.3 / 4) * (2 * Math.PI) / 0.3)",
        EasingKind.BackEase => $"({arg}) * ({arg}) * ((1.70158 + 1) * ({arg}) - 1.70158)",
        EasingKind.BounceEase => $"1 - BoilersBounceOut(1 - ({arg}))",
        _ => arg,
    };

    /// <summary>
    /// BounceEase のラムダで参照する <c>BoilersBounceOut</c> ヘルパの C# ソース。
    /// Code-behind builder が EmitsBounce=true のときに class スコープに展開する。
    /// </summary>
    public static string BounceHelperSource =>
@"private static double BoilersBounceOut(double t)
{
    const double n1 = 7.5625;
    const double d1 = 2.75;
    if (t < 1 / d1) return n1 * t * t;
    if (t < 2 / d1) { t -= 1.5 / d1; return n1 * t * t + 0.75; }
    if (t < 2.5 / d1) { t -= 2.25 / d1; return n1 * t * t + 0.9375; }
    t -= 2.625 / d1;
    return n1 * t * t + 0.984375;
}";

    /// <summary>
    /// 出力 C# が <see cref="BounceHelperSource"/> を必要とするか (= BounceEase のカスタムラムダを使うか)。
    /// </summary>
    public static bool RequiresBounceHelper(EasingKind kind, EasingMode mode) =>
        kind == EasingKind.BounceEase && mode == EasingMode.EaseInOut;
}
