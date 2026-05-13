using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.Anchors;
using R3;
using System;
using System.Windows;
using ZLinq;

namespace boilersGraphics.Helpers.Anchors;

/// <summary>
/// Phase 3-e §5.4: コネクタの BeginAnchorRef / EndAnchorRef を監視し、
/// 紐づくオーナー (DesignerItem の暗黙 9 点 or 明示 <see cref="AnchorViewModel"/>) の
/// Bounds / Rotation 変化に追従して、現在の Anchor 絶対座標を Action&lt;Point&gt; で通知する。
/// 解決失敗時 (AnchorRef 不正 / オーナー未ロード) は通知しない。
/// </summary>
public sealed class AnchorFollower : IDisposable
{
    private readonly IDiagramViewModel _diagram;
    private readonly Action<Point> _onUpdate;
    private readonly CompositeDisposable _refSub = new();
    private CompositeDisposable _ownerSub = new();
    private string _currentRef;

    public AnchorFollower(IDiagramViewModel diagram, BindableReactiveProperty<string> anchorRefProp,
        Action<Point> onUpdate)
    {
        _diagram = diagram ?? throw new ArgumentNullException(nameof(diagram));
        _onUpdate = onUpdate ?? throw new ArgumentNullException(nameof(onUpdate));
        anchorRefProp.Subscribe(OnRefChanged).AddTo(_refSub);
    }

    private void OnRefChanged(string anchorRef)
    {
        _ownerSub.Dispose();
        _ownerSub = new CompositeDisposable();
        _currentRef = anchorRef;
        if (string.IsNullOrEmpty(anchorRef)) return;

        // 初回 push
        var initial = AnchorResolver.Resolve(_diagram, anchorRef);
        if (initial.HasValue) _onUpdate(initial.Value);

        var hashIndex = anchorRef.IndexOf('#');
        if (hashIndex >= 0)
        {
            // 暗黙 9 点アンカー: OwnerId#pos
            var ownerGuidText = anchorRef.Substring(0, hashIndex);
            if (Guid.TryParse(ownerGuidText, out var ownerId))
                SubscribeToOwnerBounds(ownerId);
            return;
        }

        // 明示 AnchorViewModel: Anchor 自身の Left/Top を Subscribe
        if (Guid.TryParse(anchorRef, out var anchorId))
        {
            var anchor = _diagram.AllItems.Value
                .AsValueEnumerable()
                .OfType<AnchorViewModel>()
                .FirstOrDefault(a => a.ID == anchorId);
            if (anchor is null) return;
            anchor.Left.Subscribe(_ => Refresh()).AddTo(_ownerSub);
            anchor.Top.Subscribe(_ => Refresh()).AddTo(_ownerSub);
        }
    }

    private void SubscribeToOwnerBounds(Guid ownerId)
    {
        var owner = _diagram.AllItems.Value
            .AsValueEnumerable()
            .OfType<DesignerItemViewModelBase>()
            .FirstOrDefault(d => d.ID == ownerId);
        if (owner is null) return;
        owner.Left.Subscribe(_ => Refresh()).AddTo(_ownerSub);
        owner.Top.Subscribe(_ => Refresh()).AddTo(_ownerSub);
        owner.Width.Subscribe(_ => Refresh()).AddTo(_ownerSub);
        owner.Height.Subscribe(_ => Refresh()).AddTo(_ownerSub);
        owner.RotationAngle.Subscribe(_ => Refresh()).AddTo(_ownerSub);
    }

    private void Refresh()
    {
        if (string.IsNullOrEmpty(_currentRef)) return;
        var p = AnchorResolver.Resolve(_diagram, _currentRef);
        if (p.HasValue) _onUpdate(p.Value);
    }

    public void Dispose()
    {
        _refSub.Dispose();
        _ownerSub.Dispose();
    }
}
