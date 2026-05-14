using boilersGraphics.Models.Animation;
using System;
using System.Windows;
using System.Windows.Media;

namespace boilersGraphics.Helpers.Animation;

/// <summary>
/// Phase 5-c (Q-4 案): 型ごとのキーフレーム補間ロジック (pure)。
/// normalizedT は [0,1]、すでに <see cref="EasingFunctions.Apply"/> によりイージング適用済みである前提。
/// </summary>
public static class Interpolator
{
    public static object Interpolate(AnimatedValueType type, object from, object to, double normalizedT)
    {
        if (from is null) return to;
        if (to is null) return from;

        return type switch
        {
            AnimatedValueType.Double => LerpDouble(Convert.ToDouble(from), Convert.ToDouble(to), normalizedT),
            AnimatedValueType.Int => (int)Math.Round(LerpDouble(Convert.ToDouble(from), Convert.ToDouble(to), normalizedT)),
            AnimatedValueType.Color => LerpColor(ToColor(from), ToColor(to), normalizedT),
            AnimatedValueType.Point => LerpPoint(ToPoint(from), ToPoint(to), normalizedT),
            AnimatedValueType.Brush => LerpBrush(from, to, normalizedT),
            // Phase 5-a Q-4: 離散ジャンプ (次キーフレームの時刻で切替)
            AnimatedValueType.Boolean => normalizedT < 1.0 ? from : to,
            AnimatedValueType.String => normalizedT < 1.0 ? from : to,
            AnimatedValueType.Enum => normalizedT < 1.0 ? from : to,
            _ => normalizedT < 1.0 ? from : to,
        };
    }

    public static double LerpDouble(double a, double b, double t) => a + (b - a) * t;

    public static Color LerpColor(Color a, Color b, double t)
    {
        byte Lerp(byte x, byte y) => (byte)Math.Clamp((int)Math.Round(x + (y - x) * t), 0, 255);
        return Color.FromArgb(Lerp(a.A, b.A), Lerp(a.R, b.R), Lerp(a.G, b.G), Lerp(a.B, b.B));
    }

    public static Point LerpPoint(Point a, Point b, double t) =>
        new(LerpDouble(a.X, b.X, t), LerpDouble(a.Y, b.Y, t));

    /// <summary>
    /// Phase 5-a Q-4: SolidColorBrush 同士のみ Color として補間。それ以外 (グラデーション等) は離散ジャンプ。
    /// </summary>
    public static object LerpBrush(object from, object to, double t)
    {
        if (from is SolidColorBrush f && to is SolidColorBrush g)
        {
            return new SolidColorBrush(LerpColor(f.Color, g.Color, t));
        }
        return t < 1.0 ? from : to;
    }

    private static Color ToColor(object o) => o switch
    {
        Color c => c,
        SolidColorBrush scb => scb.Color,
        _ => Colors.Transparent,
    };

    private static Point ToPoint(object o) => o switch
    {
        Point p => p,
        _ => new Point(0, 0),
    };
}
