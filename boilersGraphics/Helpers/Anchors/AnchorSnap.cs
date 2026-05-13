using boilersGraphics.Models.Anchors;
using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.Anchors;
using System;
using System.Windows;
using ZLinq;

namespace boilersGraphics.Helpers.Anchors;

/// <summary>
/// Phase 3-e: ドラッグ確定時に「クリック点の近くのアンカー (暗黙 9 点 or 明示 AnchorViewModel)」を
/// 探して AnchorRef 文字列を返すヘルパ。Phase 3-i で <see cref="AnchorSnapSettings.SnapDistance"/>
/// (グローバル設定) を参照するようリファクタ済み。
/// </summary>
public static class AnchorSnap
{
    /// <summary>初期デフォルトの吸着距離 (px)。<see cref="AnchorSnapSettings.SnapDistance"/> の初期値と同期している必要あり。</summary>
    public const double DefaultSnapDistance = 10.0;

    /// <summary>
    /// クリック点に対し、<paramref name="threshold"/> 以内で最も近いアンカーの AnchorRef を返す。
    /// 該当なしの場合は null。<paramref name="threshold"/> 未指定なら現在の <see cref="AnchorSnapSettings.SnapDistance"/> を使う。
    /// </summary>
    public static string FindNearestAnchorRef(IDiagramViewModel diagram, Point click,
        double? threshold = null)
    {
        if (diagram is null) return null;
        var effectiveThreshold = threshold ?? AnchorSnapSettings.SnapDistance.Value;
        string bestRef = null;
        var bestDist = effectiveThreshold;

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
