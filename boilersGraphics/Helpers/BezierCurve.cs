using System;
using System.Collections.Generic;
using System.Windows;
using ZLinq;

namespace boilersGraphics.Helpers;

/// <summary>
///     [数学] 3D空間にベジェ曲線を描く
///     Author: @edo_m18
///     https://qiita.com/edo_m18/items/643512f27c2b083b47ac
///     で掲載されているソースコードを改変して実装
/// </summary>
public static class BezierCurve
{
    /// <summary>
    ///     ベジェ曲線関数
    /// </summary>
    public static Point Evaluate(double t, IEnumerable<Point> points)
    {
        var result = new Point();
        var n = points.AsValueEnumerable().Count();
        for (var i = 0; i < n; i++)
        {
            result.X += points.AsValueEnumerable().ElementAt(i).X * Bernstein(n - 1, i, t);
            result.Y += points.AsValueEnumerable().ElementAt(i).Y * Bernstein(n - 1, i, t);
        }

        return result;
    }

    /// <summary>
    ///     バーンスタイン基底関数
    /// </summary>
    private static double Bernstein(int n, int i, double t)
    {
        return Binomial(n, i) * Math.Pow(t, i) * Math.Pow(1 - t, n - i);
    }

    /// <summary>
    ///     二項係数を計算する
    /// </summary>
    private static double Binomial(int n, int k)
    {
        return Factorial(n) / (Factorial(k) * Factorial(n - k));
    }

    /// <summary>
    ///     階乗を計算する
    /// </summary>
    private static double Factorial(int a)
    {
        var result = 1d;
        for (var i = 2; i <= a; i++) result *= i;

        return result;
    }
}