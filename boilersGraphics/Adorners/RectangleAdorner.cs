using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.ViewModels;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace boilersGraphics.Adorners
{
    internal class RectangleAdorner : Adorner
    {
        private DesignerCanvas _designerCanvas;
        private Point? _startPoint;
        private Point? _endPoint;
        private Pen _rectanglePen;
        private SnapAction _snapAction;

        public RectangleAdorner(DesignerCanvas designerCanvas, Point? dragStartPoint)
            : base(designerCanvas)
        {
            _designerCanvas = designerCanvas;
            _startPoint = dragStartPoint;
            var parent = (AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel;
            var brush = new SolidColorBrush(parent.EdgeColors.First());
            brush.Opacity = 0.5;
            _rectanglePen = new Pen(brush, parent.EdgeThickness.Value.Value);
            _snapAction = new SnapAction();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (!this.IsMouseCaptured)
                    this.CaptureMouse();

                //ドラッグ終了座標を更新
                _endPoint = e.GetPosition(this);
                var currentPosition = _endPoint.Value;
                _snapAction.OnMouseMove(ref currentPosition);
                _endPoint = currentPosition;

                (App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel.CurrentPoint = currentPosition;
                (App.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = $"({_startPoint.Value.X}, {_startPoint.Value.Y}) - ({_endPoint.Value.X}, {_endPoint.Value.Y}) (w, h) = ({_endPoint.Value.X - _startPoint.Value.X}, {_endPoint.Value.Y - _startPoint.Value.Y})";

                this.InvalidateVisual();
            }
            else
            {
                if (this.IsMouseCaptured) this.ReleaseMouseCapture();
            }

            e.Handled = true;
        }

        protected override void OnMouseUp(System.Windows.Input.MouseButtonEventArgs e)
        {
            // release mouse capture
            if (this.IsMouseCaptured) this.ReleaseMouseCapture();

            _snapAction.OnMouseUp(this);

            if (_startPoint.HasValue && _endPoint.HasValue)
            {
                var rand = new Random();
                var item = new NRectangleViewModel();
                item.Owner = (AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel;
                item.Left.Value = Math.Min(_startPoint.Value.X, _endPoint.Value.X);
                item.Top.Value = Math.Min(_startPoint.Value.Y, _endPoint.Value.Y);
                item.Width.Value = Math.Max(_startPoint.Value.X - _endPoint.Value.X, _endPoint.Value.X - _startPoint.Value.X);
                item.Height.Value = Math.Max(_startPoint.Value.Y - _endPoint.Value.Y, _endPoint.Value.Y - _startPoint.Value.Y);
                item.EdgeColor.Value = item.Owner.EdgeColors.First();
                item.FillColor = item.Owner.FillColors.First();
                item.EdgeThickness.Value = item.Owner.EdgeThickness.Value.Value;
                item.ZIndex.Value = item.Owner.Layers.Items().Count();
                item.PathGeometry.Value = GeometryCreator.CreateRectangle(item);
                item.IsSelected.Value = true;
                item.IsVisible.Value = true;
                item.Owner.DeselectAll();
                ((AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel).AddItemCommand.Execute(item);

                _startPoint = null;
                _endPoint = null;
            }

            (App.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = "";
            (App.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = "";

            e.Handled = true;
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));

            if (_startPoint.HasValue && _endPoint.HasValue)
                dc.DrawRectangle(Brushes.Transparent, _rectanglePen, ShiftEdgeThickness());
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
}
