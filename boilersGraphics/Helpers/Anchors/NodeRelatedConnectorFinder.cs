using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.Anchors;
using boilersGraphics.ViewModels.Connectors;
using System;
using System.Collections.Generic;
using System.Linq;
using ZLinq;

namespace boilersGraphics.Helpers.Anchors;

/// <summary>
/// Phase 3-g: 指定の DesignerItem (IsNode=true) と紐づく Orthogonal/AnchorBezier コネクタを列挙する。
/// 「紐づく」= コネクタの AnchorRef が暗黙 ref (ownerGuid#xx) で当該 ID を指す、または
/// 明示 Guid ref が「当該 DesignerItem を OwnerId に持つ AnchorViewModel」を指す。
/// </summary>
public static class NodeRelatedConnectorFinder
{
    public static IEnumerable<ConnectorBaseViewModel> FindRelated(
        IDiagramViewModel diagram, DesignerItemViewModelBase node)
    {
        if (diagram is null || node is null) return Array.Empty<ConnectorBaseViewModel>();
        var allItems = diagram.AllItems.Value;
        if (allItems is null) return Array.Empty<ConnectorBaseViewModel>();

        var nodeId = node.ID;
        // ZLinq の ValueEnumerator は yield 境界を越えられないので、HashSet/Array へ早めにマテリアライズする。
        var anchorIdsOwnedByNode = allItems
            .AsValueEnumerable()
            .OfType<AnchorViewModel>()
            .Where(a => a.OwnerId.Value == nodeId)
            .Select(a => a.ID)
            .ToArray()
            .ToHashSet();
        var connectors = allItems.AsValueEnumerable().OfType<ConnectorBaseViewModel>().ToArray();

        var result = new List<ConnectorBaseViewModel>();
        foreach (var c in connectors)
        {
            var (begin, end) = GetAnchorRefs(c);
            if (RefersToNode(begin, nodeId, anchorIdsOwnedByNode)
                || RefersToNode(end, nodeId, anchorIdsOwnedByNode))
            {
                result.Add(c);
            }
        }
        return result;
    }

    private static (string Begin, string End) GetAnchorRefs(ConnectorBaseViewModel c)
    {
        return c switch
        {
            OrthogonalConnectorViewModel ortho => (ortho.BeginAnchorRef.Value, ortho.EndAnchorRef.Value),
            AnchorBezierConnectorViewModel ab => (ab.BeginAnchorRef.Value, ab.EndAnchorRef.Value),
            _ => (null, null),
        };
    }

    /// <summary>AnchorRef が指定 node を直接/間接 (Anchor 経由) に指しているか。</summary>
    public static bool RefersToNode(string anchorRef, Guid nodeId, HashSet<Guid> anchorIdsOwnedByNode)
    {
        if (string.IsNullOrEmpty(anchorRef)) return false;
        var hashIndex = anchorRef.IndexOf('#');
        if (hashIndex >= 0)
        {
            var ownerText = anchorRef.Substring(0, hashIndex);
            return Guid.TryParse(ownerText, out var g) && g == nodeId;
        }
        return Guid.TryParse(anchorRef, out var anchorId) && anchorIdsOwnedByNode.Contains(anchorId);
    }
}
