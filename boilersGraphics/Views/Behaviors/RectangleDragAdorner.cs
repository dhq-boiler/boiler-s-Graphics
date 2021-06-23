using System.Windows;
using System.Windows.Media;

namespace boilersGraphics.Views.Behaviors
{
    internal class RectangleDragAdorner : AbstractDragAdorner
    {
        public RectangleDragAdorner(UIElement owner, UIElement adornElement, double opacity, Point dragPos)
            : base(owner, adornElement, opacity, dragPos)
        { }

        protected override UIElement CreateVisualChild(UIElement adornElement, double opacity, Point dragPos)
        {
            var _brush = new VisualBrush(adornElement) { Opacity = opacity };
            var b = VisualTreeHelper.GetDescendantBounds(adornElement);
            var r = new System.Windows.Shapes.Rectangle() { Width = b.Width, Height = b.Height };

            this.XCenter = dragPos.X;
            this.YCenter = dragPos.Y;

            r.Fill = _brush;
            return r;
        }
    }
}
