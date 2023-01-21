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
        private readonly double size;
        private readonly double thickness;

        public SnapPointAdorner(UIElement element, Point point, double size, double thickness)
            : base(element)
        {
            _point = point;
            this.size = size;
            this.thickness = thickness;
            IsHitTestVisible = false;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            drawingContext.DrawEllipse(Brushes.Blue, new Pen(Brushes.Blue, thickness), _point, size, size);
        }
    }
}
