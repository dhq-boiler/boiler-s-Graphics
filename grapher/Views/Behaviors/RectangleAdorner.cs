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

        public RectangleAdorner(Visual visual, UIElement adornedElement, Point point)
            : base(adornedElement)
        {
            this._layer = AdornerLayer.GetAdornerLayer(visual);
            this.LeftTop = point;
            this.RightBottom = point;

            rectangle = new Rectangle();
            rectangle.Stroke = new SolidColorBrush(Colors.Black);

            Attach();
        }

        public static readonly DependencyProperty LeftTopProperty = DependencyProperty.Register("LeftTop", typeof(Point), typeof(RectangleAdorner), new FrameworkPropertyMetadata(default(Point), FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty RightBottomProperty = DependencyProperty.Register("RightBottom", typeof(Point), typeof(RectangleAdorner), new FrameworkPropertyMetadata(default(Point), FrameworkPropertyMetadataOptions.AffectsRender));

        public Point LeftTop
        {
            get { return (Point)GetValue(LeftTopProperty); }
            set { SetValue(LeftTopProperty, value); }
        }

        public Point RightBottom
        {
            get { return (Point)GetValue(RightBottomProperty); }
            set { SetValue(RightBottomProperty, value); }
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
            var lefttop = LeftTop;
            var rightbottom = RightBottom;

            var rect = new Rect(Math.Min(lefttop.X, rightbottom.X), Math.Min(lefttop.Y, rightbottom.Y),
                Math.Max(rightbottom.X - lefttop.X, lefttop.X - rightbottom.X), Math.Max(rightbottom.Y - lefttop.Y, lefttop.Y - rightbottom.Y));

            rectangle.Width = rect.Width;
            rectangle.Height = rect.Height;

            var brush = new VisualBrush(rectangle);
            brush.Opacity = 0.5;

            drawingContext.DrawRectangle(brush, null, rect);
        }
    }
}
