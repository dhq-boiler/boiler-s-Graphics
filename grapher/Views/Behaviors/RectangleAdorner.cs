using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace grapher.Views.Behaviors
{
    internal class RectangleAdorner : AbstractAdorner
    {
        public RectangleAdorner(Visual visual, UIElement adornedElement)
            : base(visual, adornedElement)
        { }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var begin = BeginPoint;
            var end = EndPoint;

            var rect = new Rect(Math.Min(begin.X, end.X), Math.Min(begin.Y, end.Y),
                Math.Max(end.X - begin.X, begin.X - end.X), Math.Max(end.Y - begin.Y, begin.Y - end.Y));

            var rectangle = new Rectangle();
            rectangle.Stroke = new SolidColorBrush(Colors.Black);
            rectangle.Width = rect.Width;
            rectangle.Height = rect.Height;

            var brush = new VisualBrush(rectangle);
            brush.Opacity = 0.5;

            drawingContext.DrawRectangle(brush, null, rect);
        }
    }
}
