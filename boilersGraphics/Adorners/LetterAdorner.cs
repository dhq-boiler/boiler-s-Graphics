using boilersGraphics.Controls;
using boilersGraphics.Dao;
using boilersGraphics.Extensions;
using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using ZLinq;

namespace boilersGraphics.Adorners;

public class LetterAdorner : Adorner
{
    private readonly DesignerCanvas _designerCanvas;
    private Point? _endPoint;
    private readonly Pen _rectanglePen;
    private Point? _startPoint;

    public LetterAdorner(DesignerCanvas designerCanvas, Point? dragStartPoint)
        : base(designerCanvas)
    {
        _designerCanvas = designerCanvas;
        _startPoint = dragStartPoint;
        var brush = new SolidColorBrush(Colors.Black);
        brush.Opacity = 0.5;
        _rectanglePen = new Pen(brush, 1);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            if (!IsMouseCaptured)
                CaptureMouse();

            _endPoint = e.GetPosition(this);
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

        // remove this adorner from adorner layer
        var adornerLayer = AdornerLayer.GetAdornerLayer(_designerCanvas);
        if (adornerLayer != null)
            adornerLayer.Remove(this);

        if (_startPoint.HasValue && _endPoint.HasValue)
        {
            var itemBase = new LetterDesignerItemViewModel();
            itemBase.Owner = (AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel;
            itemBase.Left.Value = Math.Max(0, _startPoint.Value.X);
            itemBase.Top.Value = Math.Max(0, _startPoint.Value.Y);
            itemBase.Width.Value = Math.Abs(_endPoint.Value.X - _startPoint.Value.X);
            itemBase.Height.Value = Math.Abs(_endPoint.Value.Y - _startPoint.Value.Y);
            itemBase.EdgeBrush.Value = itemBase.Owner.EdgeBrush.Value.Clone();
            itemBase.EdgeThickness.Value = itemBase.Owner.EdgeThickness.Value.Value;
            itemBase.FillBrush.Value = itemBase.Owner.FillBrush.Value.Clone();
            itemBase.IsSelected.Value = true;
            itemBase.IsVisible.Value = true;
            itemBase.Owner.DeselectAll();
            itemBase.ZIndex.Value = itemBase.Owner.Layers
                .SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children).AsValueEnumerable().Count();
            ((AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel).AddItemCommand.Execute(itemBase);

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
        statistics.NumberOfDrawsOfTheLetterTool++;
        var dao = new StatisticsDao();
        dao.Update(statistics);
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));

        if (_startPoint.HasValue && _endPoint.HasValue)
            dc.DrawRectangle(Brushes.Transparent, _rectanglePen, new Rect(_startPoint.Value, _endPoint.Value));
    }
}