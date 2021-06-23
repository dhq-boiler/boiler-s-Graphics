using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace boiler_sGraphics.Views.Behaviors
{
    internal class EllipseAdorner : AbstractAdorner
    {
        public EllipseAdorner(Visual visual, UIElement adornedElement)
            : base(visual, adornedElement)
        { }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var begin = BeginPoint;
            var end = EndPoint;

            var rect = new Rect(Math.Min(begin.X, end.X), Math.Min(begin.Y, end.Y),
                Math.Max(end.X - begin.X, begin.X - end.X), Math.Max(end.Y - begin.Y, begin.Y - end.Y));

            var ellipse = new Ellipse();
            ellipse.Stroke = new SolidColorBrush(Colors.Black);
            ellipse.Width = rect.Width;
            ellipse.Height = rect.Height;

            var brush = new VisualBrush(ellipse);
            brush.Opacity = 0.5;

            drawingContext.DrawRectangle(brush, null, rect);
        }
    }
}
