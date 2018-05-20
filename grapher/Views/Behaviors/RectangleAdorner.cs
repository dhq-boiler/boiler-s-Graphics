using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace grapher.Views.Behaviors
{
    internal class RectangleAdorner : Adorner
    {
        private AdornerLayer _layer;
        private bool _isAttached;
        private Rectangle rectangle;

        public RectangleAdorner(Visual visual, UIElement adornedElement, Point beginPoint)
            : base(adornedElement)
        {
            this._layer = AdornerLayer.GetAdornerLayer(visual);
            this.BeginPoint = beginPoint;
            this.EndPoint = beginPoint;

            rectangle = new Rectangle();
            rectangle.Stroke = new SolidColorBrush(Colors.Black);

            Attach();
        }

        public static readonly DependencyProperty BeginPointProperty = DependencyProperty.Register("BeginPoint", typeof(Point), typeof(RectangleAdorner), new FrameworkPropertyMetadata(default(Point), FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty EndPointProperty = DependencyProperty.Register("EndPoint", typeof(Point), typeof(RectangleAdorner), new FrameworkPropertyMetadata(default(Point), FrameworkPropertyMetadataOptions.AffectsRender));

        public Point BeginPoint
        {
            get { return (Point)GetValue(BeginPointProperty); }
            set { SetValue(BeginPointProperty, value); }
        }

        public Point EndPoint
        {
            get { return (Point)GetValue(EndPointProperty); }
            set { SetValue(EndPointProperty, value); }
        }

        public void Attach()
        {
            if (this._layer != null)
            {
                if (!this._isAttached)
                {
                    this._layer.Add(this);
                    this._isAttached = true;
                }
            }
        }

        public void Detach()
        {
            if (this._layer != null)
            {
                if (this._isAttached)
                {
                    this._layer.Remove(this);
                    this._isAttached = false;
                }
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var begin = BeginPoint;
            var end = EndPoint;

            var rect = new Rect(Math.Min(begin.X, end.X), Math.Min(begin.Y, end.Y),
                Math.Max(end.X - begin.X, begin.X - end.X), Math.Max(end.Y - begin.Y, begin.Y - end.Y));

            rectangle.Width = rect.Width;
            rectangle.Height = rect.Height;

            var brush = new VisualBrush(rectangle);
            brush.Opacity = 0.5;

            drawingContext.DrawRectangle(brush, null, rect);
        }
    }
}
