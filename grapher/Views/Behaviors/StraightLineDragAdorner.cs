using System.Windows;
using System.Windows.Shapes;

namespace grapher.Views.Behaviors
{
    internal class StraightLineDragAdorner : AbstractDragAdorner
    {
        public StraightLineDragAdorner(UIElement owner, UIElement adornElement, double opacity, Point dragPos)
            : base(owner, adornElement, opacity, dragPos)
        { }

        protected override UIElement CreateVisualChild(UIElement adornElement, double opacity, Point dragPos)
        {
            var l = new Line();

            this.XCenter = dragPos.X;
            this.YCenter = dragPos.Y;

            var adornedLine = adornElement as Line;
            l.X1 = adornedLine.X1;
            l.Y1 = adornedLine.Y1;
            l.X2 = adornedLine.X2;
            l.Y2 = adornedLine.Y2;
            l.Stroke = adornedLine.Stroke;
            l.Stroke.Opacity = opacity;
            return l;
        }
    }
}
