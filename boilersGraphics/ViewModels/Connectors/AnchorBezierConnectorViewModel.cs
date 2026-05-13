using boilersGraphics.Helpers;
using boilersGraphics.Helpers.Anchors;
using R3;
using System;
using System.Windows;
using System.Windows.Media;
using Path = System.Windows.Shapes.Path;

namespace boilersGraphics.ViewModels.Connectors;

/// <summary>
/// Phase 3-a §3.2 / Q-1 案 B (完全新規): Anchor 接続対応の新規ベジエコネクタ。
/// 既存 <see cref="BezierCurveViewModel"/> は触らない (後方互換維持)。
/// Points[0] = Begin、Points[1] = End。制御点は <see cref="BeginControlPoint"/> /
/// <see cref="EndControlPoint"/> でハンドル絶対座標を保持する。
/// </summary>
public class AnchorBezierConnectorViewModel : ConnectorBaseViewModel
{
    public BindableReactiveProperty<Point> BeginControlPoint { get; } = new();
    public BindableReactiveProperty<Point> EndControlPoint { get; } = new();

    public BindableReactiveProperty<string> BeginAnchorRef { get; } = new(string.Empty);
    public BindableReactiveProperty<string> EndAnchorRef { get; } = new(string.Empty);

    private AnchorFollower _beginFollower;
    private AnchorFollower _endFollower;

    public AnchorBezierConnectorViewModel()
    {
        WireRefreshTriggers();
    }

    public AnchorBezierConnectorViewModel(int id, IDiagramViewModel parent) : base(id, parent)
    {
        WireRefreshTriggers();
    }

    public AnchorBezierConnectorViewModel(IDiagramViewModel diagramViewModel, Point begin)
    {
        WireRefreshTriggers();
        AddPointP1(diagramViewModel, begin);
    }

    private void WireRefreshTriggers()
    {
        BeginControlPoint.Skip(1).Subscribe(_ => RefreshPath()).AddTo(_CompositeDisposable);
        EndControlPoint.Skip(1).Subscribe(_ => RefreshPath()).AddTo(_CompositeDisposable);
        Points.CollectionChanged += (_, _) => RefreshPath();
    }

    /// <summary>
    /// Phase 3-e: Owner 設定後に呼ぶことで、BeginAnchorRef / EndAnchorRef の追従を起動する。
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

    /// <summary>現在の Points と制御点から PathGeometryNoRotate を再構築する。</summary>
    public void RefreshPath()
    {
        if (Points is null || Points.Count < 2)
        {
            PathGeometryNoRotate.Value = new PathGeometry();
            return;
        }
        PathGeometryNoRotate.Value = GeometryCreator.CreateAnchorBezier(
            Points[0], BeginControlPoint.Value, EndControlPoint.Value, Points[1]);
    }

    public override bool SupportsPropertyDialog => false;

    public override Type GetViewType() => typeof(Path);

    public override object Clone()
    {
        var clone = new AnchorBezierConnectorViewModel(Owner, Points[0])
        {
            Owner = Owner,
        };
        clone.EdgeBrush.Value = EdgeBrush.Value;
        clone.FillBrush.Value = FillBrush.Value;
        clone.EdgeThickness.Value = EdgeThickness.Value;
        clone.AddPointP2(Owner, Points[1]);
        clone.BeginControlPoint.Value = BeginControlPoint.Value;
        clone.EndControlPoint.Value = EndControlPoint.Value;
        clone.BeginAnchorRef.Value = BeginAnchorRef.Value;
        clone.EndAnchorRef.Value = EndAnchorRef.Value;
        clone.StrokeStartLineCap.Value = StrokeStartLineCap.Value;
        clone.StrokeEndLineCap.Value = StrokeEndLineCap.Value;
        clone.StrokeLineJoin.Value = StrokeLineJoin.Value;
        clone.StrokeDashArray.Value = StrokeDashArray.Value;
        clone.StrokeMiterLimit.Value = StrokeMiterLimit.Value;
        return clone;
    }

    public override void OpenPropertyDialog()
    {
    }
}
