using boilersGraphics.Models.Anchors;
using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.Anchors;
using System;
using System.Windows;
using ZLinq;

namespace boilersGraphics.Helpers.Anchors;

/// <summary>
/// Phase 3-e: ドラッグ確定時に「クリック点の近くのアンカー (暗黙 9 点 or 明示 AnchorViewModel)」を
/// 探して AnchorRef 文字列を返すヘルパ。Phase 3-i でグローバル設定の吸着距離を使うようリファクタ予定。
/// </summary>
public static class AnchorSnap
{
    /// <summary>デフォルトの吸着距離 (px)。Phase 3-i で Settings 経由に切り替え予定 (Q-7 案 C)。</summary>
    public const double DefaultSnapDistance = 10.0;

    /// <summary>
    /// クリック点に対し、<paramref name="threshold"/> 以内で最も近いアンカーの AnchorRef を返す。
    /// 該当なしの場合は null。
    /// </summary>
    public static string FindNearestAnchorRef(IDiagramViewModel diagram, Point click,
        double threshold = DefaultSnapDistance)
    {
        if (diagram is null) return null;
        string bestRef = null;
        var bestDist = threshold;

        foreach (var item in diagram.AllItems.Value.AsValueEnumerable())
        {
            switch (item)
            {
                case AnchorViewModel a:
                {
                    var d = Distance(new Point(a.Left.Value, a.Top.Value), click);
                    if (d <= bestDist)
                    {
                        bestDist = d;
                        bestRef = AnchorResolver.BuildExplicitRef(a.ID);
                    }
                    break;
                }
                case DesignerItemViewModelBase shape:
                {
                    foreach (AnchorPosition pos in Enum.GetValues<AnchorPosition>())
                    {
                        var (rx, ry) = AnchorMath.RelativeOf(pos);
                        var worldP = AnchorMath.ToWorld(shape.Left.Value, shape.Top.Value, shape.Width.Value,
                            shape.Height.Value, shape.RotationAngle.Value, rx, ry);
                        var d = Distance(worldP, click);
                        if (d <= bestDist)
                        {
                            bestDist = d;
                            bestRef = AnchorResolver.BuildImplicitRef(shape.ID, pos);
                        }
                    }
                    break;
                }
            }
        }
        return bestRef;
    }

    private static double Distance(Point a, Point b)
    {
        var dx = b.X - a.X;
        var dy = b.Y - a.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}
