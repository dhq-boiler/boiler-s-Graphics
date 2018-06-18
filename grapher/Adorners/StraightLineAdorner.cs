using grapher.Controls;
using grapher.ViewModels;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace grapher.Adorners
{
    class StraightLineAdorner : Adorner
    {
        private DesignerCanvas designerCanvas;
        private Point? startPoint;
        private Point? endPoint;
        private Pen straightLinePen;

        public StraightLineAdorner(DesignerCanvas designerCanvas, Point? dragStartPoint)
            : base(designerCanvas)
        {
            this.designerCanvas = designerCanvas;
            this.startPoint = dragStartPoint;
            var brush = new SolidColorBrush(Colors.Black);
            brush.Opacity = 0.5;
            this.straightLinePen = new Pen(brush, 1);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (!this.IsMouseCaptured)
                    this.CaptureMouse();

                endPoint = e.GetPosition(this);
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
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this.designerCanvas);
            if (adornerLayer != null)
                adornerLayer.Remove(this);

            if (startPoint.HasValue && endPoint.HasValue)
            {
                var item = new StraightConnectorViewModel(new PartCreatedConnectionInfo(startPoint.Value), new PartCreatedConnectionInfo(endPoint.Value));
                ((AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel).AddItemCommand.Execute(item);

                startPoint = null;
                endPoint = null;
            }

            e.Handled = true;
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));

            if (startPoint.HasValue && endPoint.HasValue)
                dc.DrawLine(straightLinePen, startPoint.Value, endPoint.Value);
        }
    }
}
