using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.Helpers.Anchors;
using boilersGraphics.Models;
using boilersGraphics.Models.Connectors;
using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.Connectors;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using ZLinq;

namespace boilersGraphics.Adorners;

/// <summary>
/// Phase 3-c: L 字コネクタのドラッグプレビュー + 配置確定。
/// プレビューは Auto モードで決まる経路を白パスで描画する。
/// 確定時に OrthogonalConnectorViewModel を生成して AddItemCommand。
/// </summary>
public class OrthogonalConnectorAdorner : Adorner
{
    private readonly DesignerCanvas _designerCanvas;
    private readonly OrthogonalConnectorViewModel _item;
    private Point? _startPoint;
    private Point? _endPoint;
    private readonly Pen _previewPen;

    public OrthogonalConnectorAdorner(DesignerCanvas designerCanvas, Point? dragStartPoint,
        OrthogonalConnectorViewModel item)
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
            // プレビューは Auto モード相当 (1 中間点) で描画
            var mids = OrthogonalRouter.ComputeMidPoints(_startPoint.Value, _endPoint.Value,
                OrthogonalRoutingMode.Auto, null);
            var geom = GeometryCreator.CreateOrthogonal(_startPoint.Value, mids, _endPoint.Value, 0);
            dc.DrawGeometry(null, _previewPen, geom);
        }
    }
}
