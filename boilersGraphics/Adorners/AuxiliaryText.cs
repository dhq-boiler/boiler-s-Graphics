using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace boilersGraphics.Adorners
{
    internal class AuxiliaryText : Adorner
    {
        public AuxiliaryText(UIElement element, string text, Point drawPoint)
            : base(element)
        {
            IsHitTestVisible = false;
            Text = text;
            DrawPoint = drawPoint;
        }

        public string Text { get; }
        public Point DrawPoint { get; }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            drawingContext.DrawText(new FormattedText(Text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Consolas"), 10, Brushes.Blue, VisualTreeHelper.GetDpi(this).PixelsPerDip), DrawPoint);
        }
    }
}
