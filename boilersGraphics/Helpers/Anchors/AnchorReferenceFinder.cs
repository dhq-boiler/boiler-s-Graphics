using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.Connectors;
using System;
using System.Collections.Generic;

namespace boilersGraphics.Helpers.Anchors;

/// <summary>
/// Phase 3.5 / Q-8 案 B: 指定 AnchorViewModel.Id を「明示 Guid 参照」している
/// OrthogonalConnector / AnchorBezierConnector を列挙する pure helper。
/// 暗黙 9 点参照 (`{ownerId}#{position}`) は対象外 (オーナー DesignerItem 削除時に別経路で回収)。
/// </summary>
public static class AnchorReferenceFinder
{
    /// <summary>
    /// <paramref name="all"/> の中から、AnchorRef が <paramref name="anchorId"/> を指す
    /// コネクタ (Orthogonal / AnchorBezier) を一通り返す。順序は <paramref name="all"/> 準拠。
    /// </summary>
    public static IEnumerable<SelectableDesignerItemViewModelBase> FindReferring(
        IEnumerable<SelectableDesignerItemViewModelBase> all,
        Guid anchorId)
    {
        if (all is null) yield break;
        var idStr = anchorId.ToString();
        foreach (var item in all)
        {
            if (item is OrthogonalConnectorViewModel oc &&
                (oc.BeginAnchorRef.Value == idStr || oc.EndAnchorRef.Value == idStr))
            {
                yield return oc;
            }
            else if (item is AnchorBezierConnectorViewModel ab &&
                (ab.BeginAnchorRef.Value == idStr || ab.EndAnchorRef.Value == idStr))
            {
                yield return ab;
            }
        }
    }
}
