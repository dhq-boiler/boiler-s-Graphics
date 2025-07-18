using boilersGraphics.Controls;
using boilersGraphics.Dao;
using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using NLog;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using ZLinq;

namespace boilersGraphics.Adorners;

internal class StraightLineAdorner : Adorner
{
    private readonly DesignerCanvas _designerCanvas;
    private Point? _endPoint;
    private readonly SnapAction _snapAction;
    private Point? _startPoint;
    private readonly Pen _straightLinePen;
    private readonly StraightConnectorViewModel item;

    public StraightLineAdorner(DesignerCanvas designerCanvas, Point? dragStartPoint, StraightConnectorViewModel item)
        : base(designerCanvas)
    {
        this.item = item;
        _designerCanvas = designerCanvas;
        _startPoint = dragStartPoint;
        var parent = (AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel;
        var brush = parent.EdgeBrush.Value.Clone();
        brush.Opacity = 0.5;
        _straightLinePen = new Pen(brush, parent.EdgeThickness.Value.Value);
        _snapAction = new SnapAction();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            if (!IsMouseCaptured)
                CaptureMouse();

            var ellipses = (_designerCanvas.DataContext as DiagramViewModel).AllItems.Value.AsValueEnumerable().OfType<NEllipseViewModel>();
            var pies = (_designerCanvas.DataContext as DiagramViewModel).AllItems.Value.AsValueEnumerable().OfType<NPieViewModel>();

            //ドラッグ終了座標を更新
            _endPoint = e.GetPosition(this);

            var appendIntersectionPoints = new List<Tuple<Point, object>>();
            _snapAction.SnapIntersectionOfEllipseAndTangent(ellipses, _startPoint.Value, _endPoint.Value,
                appendIntersectionPoints);
            _snapAction.SnapIntersectionOfPieAndTangent(pies, _startPoint.Value, _endPoint.Value,
                appendIntersectionPoints);

            var currentPosition = _endPoint.Value;
            var vec = new Vector();
            vec = Point.Subtract(_endPoint.Value, _startPoint.Value);
            _snapAction.OnMouseMove(ref currentPosition, vec, appendIntersectionPoints);
            _endPoint = currentPosition;

            (Application.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel.CurrentPoint =
                currentPosition;
            (Application.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value =
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

        if (_startPoint.HasValue && _endPoint.HasValue)
        {
            var viewModel = (AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel;
            item.Owner = viewModel;
            item.EdgeBrush.Value = item.Owner.EdgeBrush.Value.Clone();
            item.EdgeThickness.Value = item.Owner.EdgeThickness.Value.Value;
            item.ZIndex.Value = item.Owner.Layers
                .SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children).AsValueEnumerable().Count();
            item.IsSelected.Value = true;
            item.IsVisible.Value = true;
            item.AddPointP2(viewModel, _endPoint.Value);
            item.PathGeometryNoRotate.Value = GeometryCreator.CreateLine(item);
            item.SnapPoint0VM.Value.IsSelected.Value = true;
            item.SnapPoint1VM.Value.IsSelected.Value = true;
            item.SnapPoint0VM.Value.IsHitTestVisible.Value = true;
            item.SnapPoint1VM.Value.IsHitTestVisible.Value = true;
            item.Owner.DeselectAll();
            LogManager.GetCurrentClassLogger().Debug($"Confirm straight line P1:{item.Points[0]} P2:{item.Points[1]}");
            ((AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel).AddItemCommand.Execute(item);

            _snapAction.PostProcess(SnapPointPosition.EndEdge, item);

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
        statistics.NumberOfDrawsOfTheStraightLineTool++;
        var dao = new StatisticsDao();
        dao.Update(statistics);
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));

        if (_startPoint.HasValue && _endPoint.HasValue)
            dc.DrawLine(_straightLinePen, _startPoint.Value, _endPoint.Value);
    }
}