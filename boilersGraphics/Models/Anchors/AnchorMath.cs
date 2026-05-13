using System;
using System.Windows;

namespace boilersGraphics.Models.Anchors;

/// <summary>
/// Phase 3-a §3.3.4: DesignerItem の Bounds と RotationAngle から、
/// 相対座標 (0..1) のアンカー絶対座標を計算する純関数群。
/// </summary>
public static class AnchorMath
{
    /// <summary>
    /// 図形の Left/Top/Width/Height/RotationAngle (度数法、図形中心を回転原点) から、
    /// 相対 (relativeX, relativeY) アンカーのワールド座標を返す。
    /// </summary>
    public static Point ToWorld(double left, double top, double width, double height, double rotationDegrees,
        double relativeX, double relativeY)
    {
        // 回転前のローカル座標 (相対 0..1 を Bounds で物理座標へ展開)
        var localX = left + width * relativeX;
        var localY = top + height * relativeY;

        if (rotationDegrees == 0) return new Point(localX, localY);

        // 回転原点は図形中心
        var centerX = left + width / 2.0;
        var centerY = top + height / 2.0;

        var rad = rotationDegrees * Math.PI / 180.0;
        var cos = Math.Cos(rad);
        var sin = Math.Sin(rad);

        var dx = localX - centerX;
        var dy = localY - centerY;

        return new Point(centerX + dx * cos - dy * sin, centerY + dx * sin + dy * cos);
    }

    /// <summary>
    /// 暗黙 9 点アンカー (<see cref="AnchorPosition"/>) の相対座標 (RelativeX, RelativeY) を返す。
    /// </summary>
    public static (double RelativeX, double RelativeY) RelativeOf(AnchorPosition position)
    {
        return position switch
        {
            AnchorPosition.TopLeft => (0.0, 0.0),
            AnchorPosition.TopCenter => (0.5, 0.0),
            AnchorPosition.TopRight => (1.0, 0.0),
            AnchorPosition.LeftCenter => (0.0, 0.5),
            AnchorPosition.Center => (0.5, 0.5),
            AnchorPosition.RightCenter => (1.0, 0.5),
            AnchorPosition.BottomLeft => (0.0, 1.0),
            AnchorPosition.BottomCenter => (0.5, 1.0),
            AnchorPosition.BottomRight => (1.0, 1.0),
            _ => throw new ArgumentOutOfRangeException(nameof(position), position, null),
        };
    }

    /// <summary>
    /// AnchorRef 文字列の予約語部分 ("tl" / "tc" / ... / "br") を <see cref="AnchorPosition"/> にパースする。
    /// 不正な予約語の場合は null を返す。
    /// </summary>
    public static AnchorPosition? ParseReserved(string reserved)
    {
        if (string.IsNullOrEmpty(reserved)) return null;
        return reserved switch
        {
            "tl" => AnchorPosition.TopLeft,
            "tc" => AnchorPosition.TopCenter,
            "tr" => AnchorPosition.TopRight,
            "lc" => AnchorPosition.LeftCenter,
            "c" => AnchorPosition.Center,
            "rc" => AnchorPosition.RightCenter,
            "bl" => AnchorPosition.BottomLeft,
            "bc" => AnchorPosition.BottomCenter,
            "br" => AnchorPosition.BottomRight,
            _ => null,
        };
    }

    /// <summary><see cref="AnchorPosition"/> を AnchorRef 文字列の予約語に変換する。</summary>
    public static string ToReserved(AnchorPosition position)
    {
        return position switch
        {
            AnchorPosition.TopLeft => "tl",
            AnchorPosition.TopCenter => "tc",
            AnchorPosition.TopRight => "tr",
            AnchorPosition.LeftCenter => "lc",
            AnchorPosition.Center => "c",
            AnchorPosition.RightCenter => "rc",
            AnchorPosition.BottomLeft => "bl",
            AnchorPosition.BottomCenter => "bc",
            AnchorPosition.BottomRight => "br",
            _ => throw new ArgumentOutOfRangeException(nameof(position), position, null),
        };
    }
}
