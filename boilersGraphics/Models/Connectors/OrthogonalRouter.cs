using System.Collections.Generic;
using System.Windows;

namespace boilersGraphics.Models.Connectors;

/// <summary>
/// Phase 3-a §3.1: 始点・終点・<see cref="OrthogonalRoutingMode"/> から L 字コネクタの
/// MidPoints を返す純関数。Manual モードはユーザの MidPoints をそのまま返す。
/// </summary>
public static class OrthogonalRouter
{
    /// <summary>
    /// Auto/HFirst/VFirst モードでは 1 中間点を内部計算して返す。
    /// Manual モードは <paramref name="manualMidPoints"/> をそのままコピーして返す。
    /// </summary>
    public static IReadOnlyList<Point> ComputeMidPoints(
        Point begin, Point end, OrthogonalRoutingMode mode, IReadOnlyList<Point> manualMidPoints)
    {
        switch (mode)
        {
            case OrthogonalRoutingMode.Manual:
                if (manualMidPoints is null) return System.Array.Empty<Point>();
                var copy = new Point[manualMidPoints.Count];
                for (var i = 0; i < manualMidPoints.Count; i++) copy[i] = manualMidPoints[i];
                return copy;

            case OrthogonalRoutingMode.HFirst:
                return new[] { new Point(end.X, begin.Y) };

            case OrthogonalRoutingMode.VFirst:
                return new[] { new Point(begin.X, end.Y) };

            case OrthogonalRoutingMode.Auto:
            default:
                // Q-2 案 A: 水平差 ≥ 垂直差なら HFirst、それ以外 VFirst
                var dx = System.Math.Abs(end.X - begin.X);
                var dy = System.Math.Abs(end.Y - begin.Y);
                return dx >= dy
                    ? new[] { new Point(end.X, begin.Y) }
                    : new[] { new Point(begin.X, end.Y) };
        }
    }
}
