using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.Helpers.Anchors;
using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.Connectors;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using ZLinq;

namespace boilersGraphics.Adorners;

/// <summary>
/// Phase 3-d: Anchor 接続対応の新規ベジエコネクタのドラッグプレビュー + 配置確定。
/// プレビュー中の制御点は Begin/End の中点からそれぞれ垂直方向に Bounds/3 ずらした位置を初期値にする。
/// </summary>
public class AnchorBezierConnectorAdorner : Adorner
{
    private readonly DesignerCanvas _designerCanvas;
    private readonly AnchorBezierConnectorViewModel _item;
    private Point? _startPoint;
    private Point? _endPoint;
    private readonly Pen _previewPen;

    public AnchorBezierConnectorAdorner(DesignerCanvas designerCanvas, Point? dragStartPoint,
        AnchorBezierConnectorViewModel item)
        : base(designerCanvas)
    {
        _designerCanvas = designerCanvas;
        _startPoint = dragStartPoint;
        _item = item;
        var parent = designerCanvas.DataContext as IDiagramViewModel;
        var brush = parent?.EdgeBrush.Value?.Clone() ?? new SolidColorBrush(Colors.Black);
        brush.Opacity = 0.5;
        var thickness = parent?.EdgeThickness.Value ?? 1.0;
        _previewPen = new Pen(brush, thickness);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            if (!IsMouseCaptured) CaptureMouse();
            _endPoint = e.GetPosition(this);
            InvalidateVisual();
        }
        else if (IsMouseCaptured) ReleaseMouseCapture();

        e.Handled = true;
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        if (IsMouseCaptured) ReleaseMouseCapture();

        var layer = AdornerLayer.GetAdornerLayer(_designerCanvas);
        layer?.Remove(this);

        if (_startPoint.HasValue && _endPoint.HasValue)
        {
            var diagram = _designerCanvas.DataContext as IDiagramViewModel;
            if (diagram is not null)
            {
                _item.Owner = diagram;
                _item.EdgeBrush.Value = diagram.EdgeBrush.Value.Clone();
                _item.EdgeThickness.Value = diagram.EdgeThickness.Value ?? 1.0;
                _item.ZIndex.Value = diagram.Layers
                    .SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
                    .AsValueEnumerable().Count();
                _item.IsVisible.Value = true;

                // 制御点初期値: 各端点から「向き合う側」へ 1/3 ずらして自然なカーブに
                var (bc, ec) = ComputeInitialControlPoints(_startPoint.Value, _endPoint.Value);
                _item.BeginControlPoint.Value = bc;
                _item.EndControlPoint.Value = ec;

                // Phase 3-e: Begin/End それぞれで近くのアンカーを探して AnchorRef を設定
                var beginRef = AnchorSnap.FindNearestAnchorRef(diagram, _startPoint.Value);
                var endRef = AnchorSnap.FindNearestAnchorRef(diagram, _endPoint.Value);
                if (beginRef is not null) _item.BeginAnchorRef.Value = beginRef;
                if (endRef is not null) _item.EndAnchorRef.Value = endRef;

                _item.AddPointP2(diagram, _endPoint.Value);
                _item.StartAnchorFollowers();
                _item.RefreshPath();
                _item.SnapPoint0VM.Value.IsHitTestVisible.Value = true;
                _item.SnapPoint1VM.Value.IsHitTestVisible.Value = true;
                diagram.DeselectAll();
                _item.IsSelected.Value = true;
                diagram.AddItemCommand.Execute(_item);
            }

            _startPoint = null;
            _endPoint = null;
        }

        if (Application.Current?.MainWindow?.DataContext is MainWindowViewModel mvm)
        {
            mvm.CurrentOperation.Value = string.Empty;
            mvm.Details.Value = string.Empty;
        }

        e.Handled = true;
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));

        if (_startPoint.HasValue && _endPoint.HasValue)
        {
            var (bc, ec) = ComputeInitialControlPoints(_startPoint.Value, _endPoint.Value);
            var geom = GeometryCreator.CreateAnchorBezier(_startPoint.Value, bc, ec, _endPoint.Value);
            dc.DrawGeometry(null, _previewPen, geom);
        }
    }

    private static (Point Begin, Point End) ComputeInitialControlPoints(Point begin, Point end)
    {
        // 始点から終点ベクトルの 1/3 進んだ点を BeginControl、2/3 進んだ点を EndControl にする。
        // Phase 3-d 最小実装。後でユーザがハンドル編集できる UI を追加する想定。
        var bc = new Point(begin.X + (end.X - begin.X) / 3.0, begin.Y + (end.Y - begin.Y) / 3.0);
        var ec = new Point(begin.X + (end.X - begin.X) * 2.0 / 3.0, begin.Y + (end.Y - begin.Y) * 2.0 / 3.0);
        return (bc, ec);
    }
}
