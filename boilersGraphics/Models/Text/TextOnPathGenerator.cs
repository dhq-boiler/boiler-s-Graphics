using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace boilersGraphics.Models.Text;

/// <summary>
/// Phase 2.5-b §3.4: PathGeometry を入力に取り、Text の各文字をパスに沿って配置するための
/// <see cref="TextOnPathCharPlacement"/> リストを生成する純関数。
/// Generator 自体は WPF 依存だが、TextElementBase 既存 (Brush) と同レベルなので Models に置く。
/// </summary>
public static class TextOnPathGenerator
{
    /// <summary>等幅フォント前提の概算文字幅係数 (FontSize × 係数 = 1 文字あたりの進む距離)。</summary>
    public const double CharWidthRatio = 0.6;

    public static IReadOnlyList<TextOnPathCharPlacement> Generate(
        string text,
        PathGeometry path,
        double startOffset,
        double spacing,
        TextOnPathSide side,
        TextOnPathRotation rotation,
        double fontSize)
    {
        if (string.IsNullOrEmpty(text) || path is null) return Array.Empty<TextOnPathCharPlacement>();

        var totalLength = ComputePathLength(path);
        if (totalLength <= 0) return Array.Empty<TextOnPathCharPlacement>();

        var step = fontSize * CharWidthRatio + spacing;
        // 法線方向のオフセット (Above/On/Below)
        var sideOffset = side switch
        {
            TextOnPathSide.Above => -fontSize / 2.0,
            TextOnPathSide.Below => fontSize / 2.0,
            _ => 0.0,
        };

        var placements = new List<TextOnPathCharPlacement>(text.Length);
        for (var i = 0; i < text.Length; i++)
        {
            // 各文字の中心相当の累積距離 (px)
            var cumulativeLength = i * step;
            var fraction = startOffset + cumulativeLength / totalLength;
            if (fraction < 0 || fraction > 1) continue;

            path.GetPointAtFractionLength(fraction, out var point, out var tangent);

            var tangentLen = Math.Sqrt(tangent.X * tangent.X + tangent.Y * tangent.Y);
            double normalX = 0, normalY = 0;
            if (tangentLen > 0)
            {
                // 単位法線 (接線を 90 度回転)
                normalX = -tangent.Y / tangentLen;
                normalY = tangent.X / tangentLen;
            }

            var x = point.X + normalX * sideOffset;
            var y = point.Y + normalY * sideOffset;
            var angle = rotation == TextOnPathRotation.Tangent
                ? Math.Atan2(tangent.Y, tangent.X) * 180.0 / Math.PI
                : 0.0;

            placements.Add(new TextOnPathCharPlacement
            {
                Char = text[i].ToString(),
                X = x,
                Y = y,
                Angle = angle,
            });
        }
        return placements;
    }

    /// <summary>
    /// PathGeometry を直線近似 (Flatten) してセグメント長を合計することで全長を求める。
    /// PathGeometry には直接の長さ取得 API が無いため、WPF 標準の <see cref="PathGeometry.GetFlattenedPathGeometry()"/> を使う。
    /// </summary>
    public static double ComputePathLength(PathGeometry path)
    {
        if (path is null) return 0;
        PathGeometry flat;
        try
        {
            flat = path.GetFlattenedPathGeometry();
        }
        catch (InvalidOperationException)
        {
            return 0;
        }

        double total = 0;
        foreach (var figure in flat.Figures)
        {
            var prev = figure.StartPoint;
            foreach (var segment in figure.Segments)
            {
                switch (segment)
                {
                    case PolyLineSegment poly:
                        foreach (var p in poly.Points)
                        {
                            total += Distance(prev, p);
                            prev = p;
                        }
                        break;
                    case LineSegment line:
                        total += Distance(prev, line.Point);
                        prev = line.Point;
                        break;
                }
            }
        }
        return total;
    }

    private static double Distance(Point a, Point b)
    {
        var dx = b.X - a.X;
        var dy = b.Y - a.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}
