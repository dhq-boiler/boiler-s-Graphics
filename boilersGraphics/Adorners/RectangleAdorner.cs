using boilersGraphics.Controls;
using boilersGraphics.Dao;
using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace boilersGraphics.Adorners;

internal class RectangleAdorner : Adorner
{
    private DesignerCanvas _designerCanvas;
    private Point? _endPoint;
    private readonly Pen _rectanglePen;
    private readonly SnapAction _snapAction;
    private Point? _startPoint;
    private readonly NRectangleViewModel item;

    public RectangleAdorner(UIElement element, Rect rect)
        : base(element)
    {
        _startPoint = rect.TopLeft;
        _endPoint= rect.BottomRight;
        _snapAction = new SnapAction();
        var parent = DiagramViewModel.Instance;
        var brush = parent.EdgeBrush.Value.Clone();
        brush.Opacity = 0.5;
        _rectanglePen = new Pen(brush, parent.EdgeThickness.Value.Value);
        this.IsHitTestVisible = false;
    }

    public RectangleAdorner(DesignerCanvas designerCanvas, Point? dragStartPoint, NRectangleViewModel item)
        : base(designerCanvas)
    {
        _designerCanvas = designerCanvas;
        _startPoint = dragStartPoint;
        this.item = item;
        var parent = DiagramViewModel.Instance;
        var brush = parent.EdgeBrush.Value.Clone();
        brush.Opacity = 0.5;
        _rectanglePen = new Pen(brush, parent.EdgeThickness.Value.Value);
        _snapAction = new SnapAction();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            if (!IsMouseCaptured)
                CaptureMouse();

            //ドラッグ終了座標を更新
            _endPoint = e.GetPosition(this);
            var currentPosition = _endPoint.Value;
            _snapAction.OnMouseMove(ref currentPosition);
            _endPoint = currentPosition;

            MainWindowViewModel.Instance.DiagramViewModel.CurrentPoint =
                currentPosition;
            MainWindowViewModel.Instance.Details.Value =
                $"({_startPoint.Value.X}, {_startPoint.Value.Y}) - ({_endPoint.Value.X}, {_endPoint.Value.Y}) (w, h) = ({_endPoint.Value.X - _startPoint.Value.X}, {_endPoint.Value.Y - _startPoint.Value.Y})";

            InvalidateVisual();
        }
        else
        {
            if (IsMouseCaptured) ReleaseMouseCapture();
        }

        e.Handled = true;
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        // release mouse capture
        if (IsMouseCaptured) ReleaseMouseCapture();

        _snapAction.OnMouseUp(this);

        if (_startPoint.HasValue && _endPoint.HasValue && item is not null)
        {
            var rand = new Random();
            item.Owner = DiagramViewModel.Instance;
            item.Left.Value = Math.Min(_startPoint.Value.X, _endPoint.Value.X);
            item.Top.Value = Math.Min(_startPoint.Value.Y, _endPoint.Value.Y);
            item.Width.Value = Math.Max(_startPoint.Value.X - _endPoint.Value.X,
                _endPoint.Value.X - _startPoint.Value.X);
            item.PathGeometryNoRotate.Value = null;
            item.Height.Value = Math.Max(_startPoint.Value.Y - _endPoint.Value.Y,
                _endPoint.Value.Y - _startPoint.Value.Y);
            item.EdgeBrush.Value = item.Owner.EdgeBrush.Value.Clone();
            item.FillBrush.Value = item.Owner.FillBrush.Value.Clone();
            item.EdgeThickness.Value = item.Owner.EdgeThickness.Value.Value;
            item.ZIndex.Value = item.Owner.Layers
                .SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children).Count();
            item.PathGeometryNoRotate.Value =
                GeometryCreator.CreateRectangle(item, item.RadiusX.Value, item.RadiusY.Value);
            item.IsSelected.Value = true;
            item.IsVisible.Value = true;
            item.Owner.DeselectAll();
            DiagramViewModel.Instance.AddItemCommand.Execute(item);

            if (_startPoint.Value.X < _endPoint.Value.X && _startPoint.Value.Y <= _endPoint.Value.Y)
                //右下
                _snapAction.PostProcess(SnapPointPosition.RightBottom, item);
            else if (_startPoint.Value.X < _endPoint.Value.X && _endPoint.Value.Y < _startPoint.Value.Y)
                //右上
                _snapAction.PostProcess(SnapPointPosition.RightTop, item);
            else if (_endPoint.Value.X <= _startPoint.Value.X && _startPoint.Value.Y <= _endPoint.Value.Y)
                //左下
                _snapAction.PostProcess(SnapPointPosition.LeftBottom, item);
            else if (_endPoint.Value.X <= _startPoint.Value.X && _endPoint.Value.Y < _startPoint.Value.Y)
                //左上
                _snapAction.PostProcess(SnapPointPosition.LeftTop, item);

            UpdateStatisticsCount();

            _startPoint = null;
            _endPoint = null;
        }

        MainWindowViewModel.Instance.CurrentOperation.Value = "";
        MainWindowViewModel.Instance.Details.Value = "";

        e.Handled = true;
    }

    private static void UpdateStatisticsCount()
    {
        var statistics = MainWindowViewModel.Instance.Statistics.Value;
        statistics.NumberOfDrawsOfTheRectangleTool++;
        var dao = new StatisticsDao();
        dao.Update(statistics);
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));

        if (_startPoint.HasValue && _endPoint.HasValue)
            dc.DrawRectangle(
                DiagramViewModel.Instance.FillBrush.Value.Clone(),
                _rectanglePen, ShiftEdgeThickness());
    }

    private Rect ShiftEdgeThickness()
    {
        var parent = DiagramViewModel.Instance;
        var point1 = _startPoint.Value;
        point1.X += parent.EdgeThickness.Value.Value / 2;
        point1.Y += parent.EdgeThickness.Value.Value / 2;
        var point2 = _endPoint.Value;
        point2.X -= parent.EdgeThickness.Value.Value / 2;
        point2.Y -= parent.EdgeThickness.Value.Value / 2;
        return new Rect(point1, point2);
    }
}