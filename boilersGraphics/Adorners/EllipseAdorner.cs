using boilersGraphics.Controls;
using boilersGraphics.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace boilersGraphics.Adorners
{
    internal class EllipseAdorner : Adorner
    {
        private DesignerCanvas _designerCanvas;
        private Point? _startPoint;
        private Point? _endPoint;
        private Pen _ellipsePen;

        public EllipseAdorner(DesignerCanvas designerCanvas, Point? dragStartPoint)
            : base(designerCanvas)
        {
            _designerCanvas = designerCanvas;
            _startPoint = dragStartPoint;
            var parent = (AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel;
            var brush = new SolidColorBrush(parent.EdgeColors.First());
            brush.Opacity = 0.5;
            _ellipsePen = new Pen(brush, 1);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (!this.IsMouseCaptured)
                    this.CaptureMouse();

                _endPoint = e.GetPosition(this);

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

            // remove this adorner from adorner layer
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(_designerCanvas);
            if (adornerLayer != null)
                adornerLayer.Remove(this);

            if (_startPoint.HasValue && _endPoint.HasValue)
            {
                var rand = new Random();
                var item = new NEllipseViewModel();
                item.Owner = (AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel;
                item.Left.Value = Math.Min(_startPoint.Value.X, _endPoint.Value.X);
                item.Top.Value = Math.Min(_startPoint.Value.Y, _endPoint.Value.Y);
                item.Width.Value = Math.Max(_startPoint.Value.X - _endPoint.Value.X, _endPoint.Value.X - _startPoint.Value.X);
                item.Height.Value = Math.Max(_startPoint.Value.Y - _endPoint.Value.Y, _endPoint.Value.Y - _startPoint.Value.Y);
                item.EdgeColor = item.Owner.EdgeColors.First();
                item.FillColor = item.Owner.FillColors.First();
                item.ZIndex.Value = item.Owner.Items.Count;
                item.IsSelected = true;
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
            {
                var center = new Point((_endPoint.Value.X + _startPoint.Value.X) / 2, (_endPoint.Value.Y + _startPoint.Value.Y) / 2);
                var radiusX = (_endPoint.Value.X - _startPoint.Value.X) / 2;
                var radiusY = (_endPoint.Value.Y - _startPoint.Value.Y) / 2;
                dc.DrawEllipse(Brushes.Transparent, _ellipsePen, center, radiusX, radiusY);
            }
        }
    }
}
