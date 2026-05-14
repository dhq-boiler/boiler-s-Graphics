using boilersGraphics.Helpers;
using boilersGraphics.Helpers.Anchors;
using boilersGraphics.Models.Connectors;
using ObservableCollections;
using R3;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Media;
using Path = System.Windows.Shapes.Path;

namespace boilersGraphics.ViewModels.Connectors;

/// <summary>
/// Phase 3-a §3.1 / Q-1 案 B (完全新規) / Q-2 案 A (Auto) / Q-3 案 B (MidPoints 任意数) / Q-4 案 A (CornerRadius):
/// L 字 (直角) コネクタ。Points[0] = Begin、Points[1] = End として既存 ConnectorBase のパターンに乗る。
/// MidPoints は Manual モードでのみ実際に使われ、Auto/HFirst/VFirst では <see cref="OrthogonalRouter"/> で都度計算。
/// </summary>
public class OrthogonalConnectorViewModel : ConnectorBaseViewModel
{
    public BindableReactiveProperty<OrthogonalRoutingMode> RoutingMode { get; } = new(OrthogonalRoutingMode.Auto);
    public ObservableCollection<Point> MidPoints { get; } = new();
    public BindableReactiveProperty<double> CornerRadius { get; } = new(0);

    public BindableReactiveProperty<string> BeginAnchorRef { get; } = new(string.Empty);
    public BindableReactiveProperty<string> EndAnchorRef { get; } = new(string.Empty);

    /// <summary>
    /// Phase 3-h §5.7 / Q-9: ExposedProperty Binding ターゲット用の Points[0] 双方向プロキシ。
    /// 内部の Points (ObservableCollection&lt;Point&gt;) は Reflection ベースの Binding から
    /// 名前指定できないので、BindableReactiveProperty&lt;Point&gt; として公開する。
    /// </summary>
    public BindableReactiveProperty<Point> BeginPoint { get; } = new();
    /// <summary>Phase 3-h: Points[1] 双方向プロキシ。詳細は <see cref="BeginPoint"/> 参照。</summary>
    public BindableReactiveProperty<Point> EndPoint { get; } = new();

    private AnchorFollower _beginFollower;
    private AnchorFollower _endFollower;

    public OrthogonalConnectorViewModel()
    {
        WireRefreshTriggers();
    }

    public OrthogonalConnectorViewModel(int id, IDiagramViewModel parent) : base(id, parent)
    {
        WireRefreshTriggers();
    }

    public OrthogonalConnectorViewModel(IDiagramViewModel diagramViewModel, Point begin)
    {
        WireRefreshTriggers();
        AddPointP1(diagramViewModel, begin);
    }

    private void WireRefreshTriggers()
    {
        RoutingMode.Skip(1).Subscribe(_ => RefreshPath()).AddTo(_CompositeDisposable);
        CornerRadius.Skip(1).Subscribe(_ => RefreshPath()).AddTo(_CompositeDisposable);
        MidPoints.CollectionChanged += (_, _) => RefreshPath();
        Points.CollectionChanged += (_, _) =>
        {
            RefreshPath();
            // Phase 3-h: BeginPoint / EndPoint プロキシを Points 側変化に追従させる。
            if (Points.Count > 0 && BeginPoint.Value != Points[0]) BeginPoint.Value = Points[0];
            if (Points.Count > 1 && EndPoint.Value != Points[1]) EndPoint.Value = Points[1];
        };
        // Phase 3-h: BeginPoint / EndPoint プロキシからの逆方向書き戻し。Skip(1) で初期値ループを防止。
        BeginPoint.Skip(1).Subscribe(p =>
        {
            if (Points.Count > 0 && Points[0] != p) Points[0] = p;
        }).AddTo(_CompositeDisposable);
        EndPoint.Skip(1).Subscribe(p =>
        {
            if (Points.Count > 1 && Points[1] != p) Points[1] = p;
        }).AddTo(_CompositeDisposable);
    }

    /// <summary>
    /// Phase 3-e: Owner 設定後に呼ぶことで、BeginAnchorRef / EndAnchorRef の追従を起動する。
    /// 多重呼び出しは前回の Follower を Dispose してから再起動する。
    /// </summary>
    public void StartAnchorFollowers()
    {
        if (Owner is null) return;
        _beginFollower?.Dispose();
        _endFollower?.Dispose();
        _beginFollower = new AnchorFollower(Owner, BeginAnchorRef, p =>
        {
            if (Points is not null && Points.Count > 0) Points[0] = p;
        });
        _endFollower = new AnchorFollower(Owner, EndAnchorRef, p =>
        {
            if (Points is not null && Points.Count > 1) Points[1] = p;
        });
    }

    /// <summary>現在の Points / RoutingMode / MidPoints / CornerRadius から PathGeometryNoRotate を再構築する。</summary>
    public void RefreshPath()
    {
        if (Points is null || Points.Count < 2)
        {
            PathGeometryNoRotate.Value = new PathGeometry();
            return;
        }
        var mids = ComputeEffectiveMidPoints();
        PathGeometryNoRotate.Value = GeometryCreator.CreateOrthogonal(Points[0], mids, Points[1], CornerRadius.Value);
    }

    public override bool SupportsPropertyDialog => true;

    public override Type GetViewType() => typeof(Path);

    public override object Clone()
    {
        var clone = new OrthogonalConnectorViewModel(Owner, Points[0])
        {
            Owner = Owner,
        };
        clone.EdgeBrush.Value = EdgeBrush.Value;
        clone.FillBrush.Value = FillBrush.Value;
        clone.EdgeThickness.Value = EdgeThickness.Value;
        clone.AddPointP2(Owner, Points[1]);
        clone.RoutingMode.Value = RoutingMode.Value;
        clone.CornerRadius.Value = CornerRadius.Value;
        clone.BeginAnchorRef.Value = BeginAnchorRef.Value;
        clone.EndAnchorRef.Value = EndAnchorRef.Value;
        foreach (var p in MidPoints) clone.MidPoints.Add(p);
        clone.StrokeStartLineCap.Value = StrokeStartLineCap.Value;
        clone.StrokeEndLineCap.Value = StrokeEndLineCap.Value;
        clone.StrokeLineJoin.Value = StrokeLineJoin.Value;
        clone.StrokeDashArray.Value = StrokeDashArray.Value;
        clone.StrokeMiterLimit.Value = StrokeMiterLimit.Value;
        return clone;
    }

    public override void OpenPropertyDialog()
    {
        // プロパティダイアログ拡充: DetailOrthogonalConnector を Prism Dialog として起動。
        if (System.Windows.Application.Current is not Prism.Unity.PrismApplication app) return;
        if (app.Container is not Prism.Ioc.IContainerExtension container) return;
        var dialogService = new Prism.Services.Dialogs.DialogService(container);
        Prism.Services.Dialogs.IDialogResult result = null;
        dialogService.Show(
            nameof(boilersGraphics.Views.DetailOrthogonalConnector),
            new Prism.Services.Dialogs.DialogParameters { { "ViewModel", this } },
            ret => result = ret);
    }

    /// <summary>Manual モードでなくとも参照可能な「実効 MidPoints」を <see cref="OrthogonalRouter"/> で求める。</summary>
    public IReadOnlyList<Point> ComputeEffectiveMidPoints()
    {
        if (Points is null || Points.Count < 2) return Array.Empty<Point>();
        return OrthogonalRouter.ComputeMidPoints(Points[0], Points[1], RoutingMode.Value, MidPoints);
    }
}
