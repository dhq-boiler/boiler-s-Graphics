using System;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using boilersGraphics.Controls;
using boilersGraphics.Dao;
using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.Models;
using boilersGraphics.ViewModels;

namespace boilersGraphics.Adorners;

internal class MosaicAdorner : Adorner
{
    private DesignerCanvas _designerCanvas;
    private Point? _endPoint;
    private readonly Pen _rectanglePen;
    private readonly SnapAction _snapAction;
    private Point? _startPoint;
    private readonly MosaicViewModel item;

    public MosaicAdorner(DesignerCanvas designerCanvas, Point? dragStartPoint, MosaicViewModel item)
        : base(designerCanvas)
    {
        _designerCanvas = designerCanvas;
        _startPoint = dragStartPoint;
        this.item = item;
        var parent = (AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel;
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

    protected override async void OnMouseUp(MouseButtonEventArgs e)
    {
        // release mouse capture
        if (IsMouseCaptured) ReleaseMouseCapture();

        _snapAction.OnMouseUp(this);

        if (_startPoint.HasValue && _endPoint.HasValue)
        {
            var rand = new Random();
            item.Owner = (AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel;
            item.UpdatingStrategy.Value = SelectableDesignerItemViewModelBase.PathGeometryUpdatingStrategy.Unknown;
            item.Left.Value = Math.Min(_startPoint.Value.X, _endPoint.Value.X);
            item.Top.Value = Math.Min(_startPoint.Value.Y, _endPoint.Value.Y);
            item.Width.Value = Math.Max(_startPoint.Value.X - _endPoint.Value.X,
                _endPoint.Value.X - _startPoint.Value.X);
            item.Height.Value = Math.Max(_startPoint.Value.Y - _endPoint.Value.Y,
                _endPoint.Value.Y - _startPoint.Value.Y);
            item.EdgeBrush.Value = item.Owner.EdgeBrush.Value.Clone();
            item.FillBrush.Value = item.Owner.FillBrush.Value.Clone();
            item.EdgeThickness.Value = item.Owner.EdgeThickness.Value.Value;
            item.ZIndex.Value = item.Owner.Layers
                .SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children).Count();
            item.UpdatingStrategy.Value = SelectableDesignerItemViewModelBase.PathGeometryUpdatingStrategy.Initial;
            item.PathGeometryNoRotate.Value = item.CreateGeometry();
            item.IsSelected.Value = true;
            item.IsVisible.Value = true;
            item.Owner.DeselectAll();
            ((AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel).AddItemCommand.Execute(item);

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

            //UpdateStatisticsCount();

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
        statistics.NumberOfDrawsOfTheRectangleTool++;
        var dao = new StatisticsDao();
        dao.Update(statistics);
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));

        if (_startPoint.HasValue && _endPoint.HasValue)
            dc.DrawRectangle(new SolidColorBrush(new Color
            {
                A = byte.MaxValue / 2,
                B = byte.MaxValue,
                G = 0,
                R = 0
            }), _rectanglePen, ShiftEdgeThickness());
    }

    private Rect ShiftEdgeThickness()
    {
        var parent = (AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel;
        var point1 = _startPoint.Value;
        point1.X += parent.EdgeThickness.Value.Value / 2;
        point1.Y += parent.EdgeThickness.Value.Value / 2;
        var point2 = _endPoint.Value;
        point2.X -= parent.EdgeThickness.Value.Value / 2;
        point2.Y -= parent.EdgeThickness.Value.Value / 2;
        return new Rect(point1, point2);
    }
}