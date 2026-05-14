using boilersGraphics.Models.Animation;
using System;

namespace boilersGraphics.Helpers.Animation;

/// <summary>
/// Phase 5-c (Q-5 案 A): WPF 標準互換の 12 種 × 3 モード イージング関数の pure helper。
/// 入力 t は [0,1] にクランプされ、出力も [0,1] (BackEase / ElasticEase は overshoot で 0..1 を超えるが、
/// 補間時に値が外挿される効果を狙う、これは WPF 同様)。
/// Phase 5.5 で WPF Storyboard に変換するときは、`EasingKind` を `EasingFunctionBase` 派生インスタンスに、
/// `EasingMode` を `System.Windows.Media.Animation.EasingMode` にマップするだけで意味的に一致する。
/// </summary>
public static class EasingFunctions
{
    public static double Apply(EasingKind kind, EasingMode mode, double t)
    {
        if (t <= 0) return 0;
        if (t >= 1) return 1;

        return mode switch
        {
            EasingMode.EaseIn => Core(kind, t),
            EasingMode.EaseOut => 1 - Core(kind, 1 - t),
            EasingMode.EaseInOut =>
                t < 0.5
                    ? Core(kind, 2 * t) / 2
                    : (2 - Core(kind, 2 - 2 * t)) / 2,
            _ => Core(kind, t),
        };
    }

    private static double Core(EasingKind kind, double t) => kind switch
    {
        EasingKind.LinearEase => t,
        EasingKind.CubicEase => t * t * t,
        EasingKind.QuadraticEase => t * t,
        EasingKind.QuarticEase => t * t * t * t,
        EasingKind.QuinticEase => t * t * t * t * t,
        EasingKind.SineEase => 1 - Math.Cos(t * Math.PI / 2),
        EasingKind.ExponentialEase => ExponentialCore(t),
        EasingKind.CircleEase => 1 - Math.Sqrt(1 - t * t),
        EasingKind.PowerEase => Math.Pow(t, 2.0),
        EasingKind.ElasticEase => ElasticCore(t),
        EasingKind.BackEase => BackCore(t),
        EasingKind.BounceEase => BounceCore(t),
        _ => t,
    };

    private static double ExponentialCore(double t)
    {
        // WPF ExponentialEase: (e^(exponent * t) - 1) / (e^exponent - 1)、exponent=2 既定
        const double exponent = 2.0;
        return (Math.Exp(exponent * t) - 1) / (Math.Exp(exponent) - 1);
    }

    private static double BackCore(double t)
    {
        // Robert Penner BackIn 風: f(t) = t^2 * ((s+1) * t - s), s = 1.70158
        const double amplitude = 1.70158;
        return t * t * ((amplitude + 1) * t - amplitude);
    }

    private static double ElasticCore(double t)
    {
        // Robert Penner ElasticIn (簡易、振動 + 減衰)
        if (t == 0 || t == 1) return t;
        const double p = 0.3;
        return -Math.Pow(2, 10 * (t - 1)) * Math.Sin((t - 1 - p / 4) * (2 * Math.PI) / p);
    }

    private static double BounceCore(double t)
    {
        // BounceIn = 1 - BounceOut(1 - t)
        return 1 - BounceOut(1 - t);
    }

    private static double BounceOut(double t)
    {
        const double n1 = 7.5625;
        const double d1 = 2.75;

        if (t < 1 / d1) return n1 * t * t;
        if (t < 2 / d1) { t -= 1.5 / d1; return n1 * t * t + 0.75; }
        if (t < 2.5 / d1) { t -= 2.25 / d1; return n1 * t * t + 0.9375; }
        t -= 2.625 / d1;
        return n1 * t * t + 0.984375;
    }
}
