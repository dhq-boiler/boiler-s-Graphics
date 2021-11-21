using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace boilersGraphics.Adorners
{
    internal class AuxiliaryLine : Adorner
    {
        public AuxiliaryLine(UIElement element, Point centerPoint, Point intersection)
            : base(element)
        {
            IsHitTestVisible = false;
            CenterPoint = centerPoint;
            Intersection = intersection;
        }

        public Point CenterPoint { get; }
        public Point Intersection { get; }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            //drawingContext.DrawEllipse(Brushes.Blue, new Pen(Brushes.Blue, 1), _point, 2, 2);
            drawingContext.DrawLine(new Pen(Brushes.Blue, 1), CenterPoint, Intersection);
        }
    }
}
