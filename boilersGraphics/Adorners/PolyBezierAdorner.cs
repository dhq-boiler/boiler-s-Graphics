using boilersGraphics.Controls;
using boilersGraphics.Dao;
using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace boilersGraphics.Adorners;

internal class PolyBezierAdorner : Adorner
{
    private readonly Pen _bezierCurvePen;
    private DesignerCanvas _designerCanvas;
    private Point? _endPoint;
    private readonly PolyBezierViewModel _item;
    private readonly SnapAction _snapAction;
    private Point? _startPoint;

    public PolyBezierAdorner(DesignerCanvas designerCanvas, Point? dragStartPoint, PolyBezierViewModel item)
        : base(designerCanvas)
    {
        _designerCanvas = designerCanvas;
        _startPoint = dragStartPoint;
        _item = item;
        var parent = (AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel;
        var brush = parent.EdgeBrush.Value.Clone();
        brush.Opacity = 0.5;
        _bezierCurvePen = new Pen(brush, parent.EdgeThickness.Value.Value);
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
            _item.Points.Add(_endPoint.Value);


            //(App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel.CurrentPoint = currentPosition;
            //(App.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = $"({_startPoint.Value.X}, {_startPoint.Value.Y}) - ({_endPoint.Value.X}, {_endPoint.Value.Y}) (w, h) = ({_endPoint.Value.X - _startPoint.Value.X}, {_endPoint.Value.Y - _startPoint.Value.Y})";

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

        if (_startPoint.HasValue && _endPoint.HasValue)
        {
            var points = new List<Point>();
            points.Add(_startPoint.Value);
            points.Add(_endPoint.Value);
            _item.Owner = (AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel;
            _item.LeftTop.Value =
                new Point(_item.Points.Select(x => x.X).Min() - _item.Owner.EdgeThickness.Value.Value / 2,
                    _item.Points.Select(x => x.Y).Min() - _item.Owner.EdgeThickness.Value.Value / 2);
            _item.EdgeBrush.Value = _item.Owner.EdgeBrush.Value.Clone();
            _item.EdgeThickness.Value = _item.Owner.EdgeThickness.Value.Value;
            _item.ZIndex.Value = _item.Owner.Layers
                .SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children).Count();
            _item.IsSelected.Value = true;
            _item.PathGeometryNoRotate.Value = GeometryCreator.CreatePolyBezier(_item);
            _item.IsVisible.Value = true;
            _item.SnapPoint0VM.Value.IsSelected.Value = true;
            _item.SnapPoint1VM.Value.IsSelected.Value = true;
            _item.SnapPoint0VM.Value.IsHitTestVisible.Value = true;
            _item.SnapPoint1VM.Value.IsHitTestVisible.Value = true;
            _item.Owner.DeselectAll();
            ((AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel).AddItemCommand.Execute(_item);

            UpdateStatisticsCount();

            _startPoint = null;
            _endPoint = null;
        }

        (Application.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = "";
        (Application.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = "";

        e.Handled = true;
    }

    private static void UpdateStatisticsCount()
    {
        var statistics = (Application.Current.MainWindow.DataContext as MainWindowViewModel).Statistics.Value;
        statistics.NumberOfDrawsOfFreeHandTool++;
        var dao = new StatisticsDao();
        dao.Update(statistics);
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));

        if (_startPoint.HasValue && _endPoint.HasValue)
        {
            var diff = _endPoint.Value - _startPoint.Value;
            var points = new List<Point>();
            points.Add(_startPoint.Value);
            points.Add(_endPoint.Value);
            var width = points.Select(x => x.X).Max() - points.Select(x => x.X).Min();
            var height = points.Select(x => x.Y).Max() - points.Select(x => x.Y).Min();
            var geometry = new StreamGeometry();
            geometry.FillRule = FillRule.EvenOdd;
            using (var ctx = geometry.Open())
            {
                ctx.BeginFigure(_startPoint.Value, true, false);
                ctx.PolyBezierTo(_item.Points.Skip(1).ToList(), true, false);
            }

            dc.DrawGeometry(Brushes.Transparent, _bezierCurvePen, geometry);
        }
    }
}