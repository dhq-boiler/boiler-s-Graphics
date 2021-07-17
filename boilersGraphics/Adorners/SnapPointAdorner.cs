using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace boilersGraphics.Adorners
{
    internal class SnapPointAdorner : Adorner
    {
        private Point _point;
        public SnapPointAdorner(UIElement element, Point point)
            : base(element)
        {
            _point = point;
            IsHitTestVisible = false;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            Trace.WriteLine($"SnapPointAdorner = {_point}");
            drawingContext.DrawEllipse(Brushes.Blue, new Pen(Brushes.Blue, 1), _point, 2, 2);
        }
    }
}
