using System.Windows;
using System.Windows.Media;

namespace boilersGraphics.Views.Behaviors
{
    internal class StraightLineAdorner : AbstractAdorner
    {
        public StraightLineAdorner(Visual visual, UIElement adornedElement)
            : base(visual, adornedElement)
        { }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var begin = BeginPoint;
            var end = EndPoint;

            var brush = new SolidColorBrush(Colors.Black)
            {
                Opacity = 0.5
            };
            var pen = new Pen(brush, 1);

            drawingContext.DrawLine(pen, begin, end);
        }
    }
}
