using boilersGraphics.Helpers;
using boilersGraphics.Models.Text;
using R3;
using System;
using System.Collections.ObjectModel;
using System.Windows.Media;
using ZLinq;

namespace boilersGraphics.ViewModels.Text;

/// <summary>
/// Phase 2.5-b: TextOnPathBlock の VM。
/// PathReferenceId に紐づく PolyBezierViewModel を Owner.AllItems から解決し、
/// PathGeometry を <see cref="TextOnPathGenerator"/> に渡して <see cref="Placements"/> を再生成する。
/// </summary>
public class TextOnPathBlockViewModel : TextElementBaseViewModel
{
    public new TextOnPathBlock Model => (TextOnPathBlock)base.Model;

    public BindableReactiveProperty<Guid?> PathReferenceId { get; }
    public BindableReactiveProperty<double> StartOffset { get; }
    public BindableReactiveProperty<double> Spacing { get; }
    public BindableReactiveProperty<TextOnPathSide> Side { get; }
    public BindableReactiveProperty<TextOnPathRotation> Rotation { get; }

    /// <summary>DataTemplate からバインドする 1 文字単位の配置情報。</summary>
    public ObservableCollection<TextOnPathCharPlacement> Placements { get; } = new();

    public TextOnPathBlockViewModel() : this(new TextOnPathBlock())
    {
    }

    public TextOnPathBlockViewModel(TextOnPathBlock model) : base(model)
    {
        PathReferenceId = new BindableReactiveProperty<Guid?>(model.PathReferenceId);
        StartOffset = new BindableReactiveProperty<double>(model.StartOffset);
        Spacing = new BindableReactiveProperty<double>(model.Spacing);
        Side = new BindableReactiveProperty<TextOnPathSide>(model.Side);
        Rotation = new BindableReactiveProperty<TextOnPathRotation>(model.Rotation);

        PathReferenceId.Subscribe(v => model.PathReferenceId = v).AddTo(_CompositeDisposable);
        StartOffset.Subscribe(v => model.StartOffset = v).AddTo(_CompositeDisposable);
        Spacing.Subscribe(v => model.Spacing = v).AddTo(_CompositeDisposable);
        Side.Subscribe(v => model.Side = v).AddTo(_CompositeDisposable);
        Rotation.Subscribe(v => model.Rotation = v).AddTo(_CompositeDisposable);

        // 再生成トリガ: 共通テキスト関連 + 自要素固有プロパティ
        Text.Skip(1).Subscribe(_ => Regenerate()).AddTo(_CompositeDisposable);
        FontSize.Skip(1).Subscribe(_ => Regenerate()).AddTo(_CompositeDisposable);
        PathReferenceId.Skip(1).Subscribe(_ => Regenerate()).AddTo(_CompositeDisposable);
        StartOffset.Skip(1).Subscribe(_ => Regenerate()).AddTo(_CompositeDisposable);
        Spacing.Skip(1).Subscribe(_ => Regenerate()).AddTo(_CompositeDisposable);
        Side.Skip(1).Subscribe(_ => Regenerate()).AddTo(_CompositeDisposable);
        Rotation.Skip(1).Subscribe(_ => Regenerate()).AddTo(_CompositeDisposable);

        // 初期化時にも 1 回試みる (Owner が後付けの場合は空のままになるが、後で再生成されれば OK)
        Regenerate();
    }

    public override bool IsResizable => true;

    public override bool SupportsPropertyDialog => true;

    public override void OpenPropertyDialog()
    {
        // プロパティダイアログ拡充: DetailTextOnPath を Prism Dialog として起動。
        if (System.Windows.Application.Current is not Prism.Unity.PrismApplication app) return;
        if (app.Container is not Prism.Ioc.IContainerExtension container) return;
        var dialogService = new Prism.Services.Dialogs.DialogService(container);
        Prism.Services.Dialogs.IDialogResult result = null;
        dialogService.Show(
            nameof(boilersGraphics.Views.DetailTextOnPath),
            new Prism.Services.Dialogs.DialogParameters { { "ViewModel", this } },
            ret => result = ret);
    }

    public override object Clone()
    {
        var cloneModel = new TextOnPathBlock
        {
            PathReferenceId = Model.PathReferenceId,
            StartOffset = Model.StartOffset,
            Spacing = Model.Spacing,
            Side = Model.Side,
            Rotation = Model.Rotation,
        };
        var clone = new TextOnPathBlockViewModel(cloneModel);
        CopyCommonPropertiesTo(clone);
        return clone;
    }

    /// <summary>
    /// PolyBezier 実体を参照して PathGeometry を取り、Placements を再構築する。
    /// PolyBezier が解決できない場合は Placements は空のまま。
    /// </summary>
    public void Regenerate()
    {
        Placements.Clear();
        var path = ResolvePathGeometry();
        if (path is null) return;

        var placements = TextOnPathGenerator.Generate(
            Text.Value ?? string.Empty, path,
            StartOffset.Value, Spacing.Value, Side.Value, Rotation.Value, FontSize.Value);
        foreach (var p in placements)
            Placements.Add(p);
    }

    private PathGeometry ResolvePathGeometry()
    {
        if (PathReferenceId.Value is null) return null;
        if (Owner is null) return null;

        var target = Owner.AllItems.Value
            .AsValueEnumerable()
            .OfType<PolyBezierViewModel>()
            .FirstOrDefault(p => p.ID == PathReferenceId.Value.Value);
        if (target is null) return null;

        return GeometryCreator.CreatePolyBezier(target);
    }
}
