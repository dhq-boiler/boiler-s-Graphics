using grapher.Controls;
using grapher.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace grapher.Adorners
{
    public class SettingAdorner : Adorner
    {
        private Point? startPoint;
        private Point? endPoint;

        private DesignerCanvas designerCanvas;

        public SettingAdorner(DesignerCanvas designerCanvas, Point? dragStartPoint)
            : base(designerCanvas)
        {
            this.designerCanvas = designerCanvas;
            this.startPoint = dragStartPoint;
        }

        protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
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
                DesignerItemViewModelBase itemBase = new SettingsDesignerItemViewModel();
                itemBase.Left = Math.Max(0, startPoint.Value.X);
                itemBase.Top = Math.Max(0, startPoint.Value.Y);
                itemBase.Width = Math.Abs(endPoint.Value.X - startPoint.Value.X);
                itemBase.Height = Math.Abs(endPoint.Value.Y - startPoint.Value.Y);
                itemBase.IsSelected = true;
                ((AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel).AddItemCommand.Execute(itemBase);

                startPoint = null;
                endPoint = null;
            }

            e.Handled = true;
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            var brush = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/img/Setting.png")));
            brush.Opacity = 0.5;

            dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));

            if (this.startPoint.HasValue && this.endPoint.HasValue)
                dc.DrawRectangle(brush, null, new Rect(this.startPoint.Value, this.endPoint.Value));
        }
    }
}
