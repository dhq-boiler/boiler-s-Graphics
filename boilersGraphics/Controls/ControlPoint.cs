using boilersGraphics.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace boilersGraphics.Controls;

public class ControlPoint : SnapPoint
{
    public static readonly DependencyProperty PointProperty =
        DependencyProperty.Register(nameof(Point), typeof(Point), typeof(ControlPoint));

    public ControlPoint()
    {
        DragDelta += ControlPoint_DragDelta;
    }

    public Point Point
    {
        get => (Point)GetValue(PointProperty);
        set => SetValue(PointProperty, value);
    }

    private void ControlPoint_DragDelta(object sender, DragDeltaEventArgs e)
    {
        var bezierCurveViewModel = DataContext as BezierCurveViewModel;
        double minLeft, minTop, minDeltaHorizontal, minDeltaVertical;
        double dragDeltaVertical, dragDeltaHorizontal;
        CalculateDragLimits(bezierCurveViewModel, out minLeft, out minTop,
            out minDeltaHorizontal, out minDeltaVertical);
        dragDeltaVertical = Math.Min(Math.Max(-minTop, e.VerticalChange), minDeltaVertical);
        dragDeltaHorizontal = Math.Min(Math.Max(-minLeft, e.HorizontalChange), minDeltaHorizontal);
        var point = Point;
        point.X += dragDeltaHorizontal;
        point.Y += dragDeltaVertical;
        Point = point;
    }

    private static void CalculateDragLimits(SelectableDesignerItemViewModelBase selectedDesignerItem,
        out double minLeft, out double minTop, out double minDeltaHorizontal, out double minDeltaVertical)
    {
        minLeft = double.MaxValue;
        minTop = double.MaxValue;
        minDeltaHorizontal = double.MaxValue;
        minDeltaVertical = double.MaxValue;

        // drag limits are set by these parameters: canvas top, canvas left, minHeight, minWidth
        // calculate min value for each parameter for each item

        if (selectedDesignerItem is BezierCurveViewModel)
        {
            var viewModel = selectedDesignerItem as BezierCurveViewModel;
            var left = Math.Min(viewModel.Points[0].X, viewModel.Points[1].X);
            var top = Math.Min(viewModel.Points[0].Y, viewModel.Points[1].Y);

            var width = Math.Max(viewModel.Points[0].X, viewModel.Points[1].X) -
                        Math.Min(viewModel.Points[0].X, viewModel.Points[1].X);
            var height = Math.Max(viewModel.Points[0].Y, viewModel.Points[1].Y) -
                         Math.Min(viewModel.Points[0].Y, viewModel.Points[1].Y);

            minDeltaVertical = Math.Min(minDeltaVertical, height);
            minDeltaHorizontal = Math.Min(minDeltaHorizontal, width);
        }
    }
}