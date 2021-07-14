using boilersGraphics.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace boilersGraphics.Controls
{
    public class ControlPoint : SnapPoint
    {
        public static readonly DependencyProperty PointProperty = DependencyProperty.Register(nameof(Point), typeof(Point), typeof(ControlPoint));

        public Point Point
        {
            get => (Point)GetValue(PointProperty);
            set => SetValue(PointProperty, value);
        }

        public ControlPoint()
        {
            base.DragDelta += ControlPoint_DragDelta;
        }

        private void ControlPoint_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            var bezierCurveViewModel = this.DataContext as BezierCurveViewModel;
            double minLeft, minTop, minDeltaHorizontal, minDeltaVertical;
            double dragDeltaVertical, dragDeltaHorizontal;
            CalculateDragLimits(bezierCurveViewModel, out minLeft, out minTop,
                                out minDeltaHorizontal, out minDeltaVertical);
            dragDeltaVertical = Math.Min(Math.Max(-minTop, e.VerticalChange), minDeltaVertical);
            dragDeltaHorizontal = Math.Min(Math.Max(-minLeft, e.HorizontalChange), minDeltaHorizontal);
            Point point = Point;
            point.X += dragDeltaHorizontal;
            point.Y += dragDeltaVertical;
            Point = point;
        }

        private static void CalculateDragLimits(SelectableDesignerItemViewModelBase selectedDesignerItem, out double minLeft, out double minTop, out double minDeltaHorizontal, out double minDeltaVertical)
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
                double left = Math.Min(viewModel.Points[0].X, viewModel.Points[1].X);
                double top = Math.Min(viewModel.Points[0].Y, viewModel.Points[1].Y);

                double width = Math.Max(viewModel.Points[0].X, viewModel.Points[1].X) - Math.Min(viewModel.Points[0].X, viewModel.Points[1].X);
                double height = Math.Max(viewModel.Points[0].Y, viewModel.Points[1].Y) - Math.Min(viewModel.Points[0].Y, viewModel.Points[1].Y);

                minDeltaVertical = Math.Min(minDeltaVertical, height);
                minDeltaHorizontal = Math.Min(minDeltaHorizontal, width);
            }
        }
    }
}
