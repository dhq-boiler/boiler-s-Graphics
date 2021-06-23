using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace boilersGraphics.Views.Behaviors
{
    internal class EllipseDragAdorner : AbstractDragAdorner
    {
        public EllipseDragAdorner(UIElement owner, UIElement adornElement, double opacity, Point dragPos)
            : base(owner, adornElement, opacity, dragPos)
        { }

        protected override UIElement CreateVisualChild(UIElement adornElement, double opacity, Point dragPos)
        {
            var _brush = new VisualBrush(adornElement) { Opacity = opacity };
            var b = VisualTreeHelper.GetDescendantBounds(adornElement);
            var r = new System.Windows.Shapes.Ellipse() { Width = b.Width, Height = b.Height };

            this.XCenter = dragPos.X;
            this.YCenter = dragPos.Y;

            r.Fill = _brush;
            return r;
        }
    }
}
