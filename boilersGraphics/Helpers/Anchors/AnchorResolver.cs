using boilersGraphics.Models.Anchors;
using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.Anchors;
using System;
using System.Windows;
using ZLinq;

namespace boilersGraphics.Helpers.Anchors;

/// <summary>
/// Phase 3-b §4.4: AnchorRef 文字列 ("&lt;Guid&gt;" または "&lt;OwnerGuid&gt;#&lt;position&gt;") を
/// IDiagramViewModel.AllItems から解決して絶対座標 (Point) を返す静的ヘルパ。
/// '#' を含めば暗黙 9 点アンカー、含まなければユーザ追加 AnchorViewModel を Guid で逆引き。
/// </summary>
public static class AnchorResolver
{
    /// <summary>
    /// AnchorRef を解決して絶対座標を返す。解決失敗時は null。
    /// </summary>
    public static Point? Resolve(IDiagramViewModel diagram, string anchorRef)
    {
        if (diagram is null || string.IsNullOrEmpty(anchorRef)) return null;

        var hashIndex = anchorRef.IndexOf('#');
        if (hashIndex >= 0)
        {
            // 暗黙アンカー: "{OwnerGuid}#{position}"
            var ownerGuidText = anchorRef.Substring(0, hashIndex);
            var positionText = anchorRef.Substring(hashIndex + 1);

            if (!Guid.TryParse(ownerGuidText, out var ownerId)) return null;
            var position = AnchorMath.ParseReserved(positionText);
            if (position is null) return null;

            var owner = diagram.AllItems.Value
                .AsValueEnumerable()
                .OfType<DesignerItemViewModelBase>()
                .FirstOrDefault(d => d.ID == ownerId);
            if (owner is null) return null;

            var (relX, relY) = AnchorMath.RelativeOf(position.Value);
            return AnchorMath.ToWorld(
                owner.Left.Value, owner.Top.Value, owner.Width.Value, owner.Height.Value,
                owner.RotationAngle.Value, relX, relY);
        }

        // 明示 Anchor: "&lt;Guid&gt;" 形式
        if (!Guid.TryParse(anchorRef, out var anchorId)) return null;
        var anchor = diagram.AllItems.Value
            .AsValueEnumerable()
            .OfType<AnchorViewModel>()
            .FirstOrDefault(a => a.ID == anchorId);
        if (anchor is null) return null;

        return new Point(anchor.Left.Value, anchor.Top.Value);
    }

    /// <summary>暗黙アンカーの AnchorRef 文字列を組み立てる。</summary>
    public static string BuildImplicitRef(Guid ownerId, AnchorPosition position)
        => $"{ownerId}#{AnchorMath.ToReserved(position)}";

    /// <summary>明示 Anchor の AnchorRef 文字列 (Guid 文字列) を組み立てる。</summary>
    public static string BuildExplicitRef(Guid anchorId) => anchorId.ToString();
}
