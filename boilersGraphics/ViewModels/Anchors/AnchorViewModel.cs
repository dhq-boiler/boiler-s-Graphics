using boilersGraphics.Helpers;
using boilersGraphics.Models.Anchors;
using boilersGraphics.Views;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Prism.Unity;
using R3;
using System;
using System.Windows;
using System.Windows.Media;
using ZLinq;
using Path = System.Windows.Shapes.Path;

namespace boilersGraphics.ViewModels.Anchors;

/// <summary>
/// Phase 3-b §3.3.2 / Q-6 案 A: ユーザが「アンカー追加」ツールで明示的に追加する Anchor の VM。
/// Owner DesignerItem (OwnerId で一致するもの) の Left/Top/Width/Height/RotationAngle と
/// RelativeX/Y から絶対座標 (Left/Top) を R3 リアクティブに計算する。
/// 暗黙 9 点アンカー (<see cref="AnchorPosition"/>) はこの VM を生成せず、AnchorResolver 側で都度計算する。
/// </summary>
public class AnchorViewModel : SelectableDesignerItemViewModelBase
{
    public BindableReactiveProperty<Guid> OwnerId { get; } = new();
    public BindableReactiveProperty<double> RelativeX { get; } = new();
    public BindableReactiveProperty<double> RelativeY { get; } = new();
    public BindableReactiveProperty<string> AnchorName { get; } = new(string.Empty);

    /// <summary>絶対座標 (キャンバス座標) X。Owner の Bounds / Rotation と RelativeX/Y から派生。</summary>
    public BindableReactiveProperty<double> Left { get; } = new();

    /// <summary>絶対座標 (キャンバス座標) Y。Owner の Bounds / Rotation と RelativeY から派生。</summary>
    public BindableReactiveProperty<double> Top { get; } = new();

    /// <summary>描画用の半径。固定 4 px。</summary>
    public BindableReactiveProperty<double> Radius { get; } = new(4);

    private CompositeDisposable _ownerSubscriptions;

    public AnchorViewModel()
    {
    }

    public AnchorViewModel(Anchor model)
    {
        OwnerId.Value = model.OwnerId;
        RelativeX.Value = model.RelativeX;
        RelativeY.Value = model.RelativeY;
        AnchorName.Value = model.Name ?? string.Empty;

        OwnerId.Subscribe(_ => RebindOwner()).AddTo(_CompositeDisposable);
        RelativeX.Skip(1).Subscribe(_ => RefreshWorld()).AddTo(_CompositeDisposable);
        RelativeY.Skip(1).Subscribe(_ => RefreshWorld()).AddTo(_CompositeDisposable);
    }

    /// <summary>Owner DesignerItem (Id で一致するもの) を AllItems から探して、Bounds 変更を Subscribe し直す。</summary>
    public void RebindOwner()
    {
        _ownerSubscriptions?.Dispose();
        _ownerSubscriptions = new CompositeDisposable();

        var owner = ResolveOwner();
        if (owner is null)
        {
            // Owner 未解決時は Refresh だけ呼んでクリアしておく
            RefreshWorld();
            return;
        }

        owner.Left.Subscribe(_ => RefreshWorld()).AddTo(_ownerSubscriptions);
        owner.Top.Subscribe(_ => RefreshWorld()).AddTo(_ownerSubscriptions);
        owner.Width.Subscribe(_ => RefreshWorld()).AddTo(_ownerSubscriptions);
        owner.Height.Subscribe(_ => RefreshWorld()).AddTo(_ownerSubscriptions);
        owner.RotationAngle.Subscribe(_ => RefreshWorld()).AddTo(_ownerSubscriptions);

        RefreshWorld();
    }

    /// <summary>Owner DesignerItem を <see cref="Owner"/>.AllItems から OwnerId で逆引きする。</summary>
    private DesignerItemViewModelBase ResolveOwner()
    {
        if (Owner is null) return null;
        return Owner.AllItems.Value
            .AsValueEnumerable()
            .OfType<DesignerItemViewModelBase>()
            .FirstOrDefault(d => d.ID == OwnerId.Value);
    }

    private void RefreshWorld()
    {
        var owner = ResolveOwner();
        if (owner is null)
        {
            // Owner 未解決時は移動を見送る (初期化途中で AllItems 未確定のとき等)
            return;
        }

        var point = AnchorMath.ToWorld(
            owner.Left.Value, owner.Top.Value, owner.Width.Value, owner.Height.Value,
            owner.RotationAngle.Value, RelativeX.Value, RelativeY.Value);
        Left.Value = point.X;
        Top.Value = point.Y;
    }

    public override bool SupportsPropertyDialog => true;

    public override object Clone()
    {
        var clone = new AnchorViewModel
        {
            Owner = Owner,
        };
        clone.OwnerId.Value = OwnerId.Value;
        clone.RelativeX.Value = RelativeX.Value;
        clone.RelativeY.Value = RelativeY.Value;
        clone.AnchorName.Value = AnchorName.Value;
        clone.ID = ID;
        clone.ParentID = ParentID;
        clone.ZIndex.Value = ZIndex.Value;
        clone.IsVisible.Value = IsVisible.Value;
        clone.IsSelected.Value = IsSelected.Value;
        return clone;
    }

    public override Type GetViewType() => typeof(Path);

    public override void OnNext(GroupTransformNotification value)
    {
    }

    public override void OpenPropertyDialog()
    {
        // プロパティダイアログ拡充: AnchorViewModel の RelativeX/Y/AnchorName/OwnerId/派生 Left/Top を編集 / 閲覧可能に。
        if (Application.Current is not PrismApplication app) return;
        if (app.Container is not IContainerExtension container) return;
        var dialogService = new DialogService(container);
        IDialogResult result = null;
        dialogService.Show(nameof(DetailAnchor), new DialogParameters { { "ViewModel", this } }, ret => result = ret);
    }

    /// <summary>このアンカーから <see cref="Anchor"/> Model を生成して返す (シリアライズ用)。</summary>
    public Anchor ToModel()
    {
        return new Anchor
        {
            Id = ID,
            OwnerId = OwnerId.Value,
            RelativeX = RelativeX.Value,
            RelativeY = RelativeY.Value,
            Name = string.IsNullOrEmpty(AnchorName.Value) ? null : AnchorName.Value,
        };
    }
}
