using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace boilersGraphics.Views.Behaviors
{
    internal class EllipseDragStartBehavior : AbstractDragStartBehavior
    {
        protected override AbstractDragAdorner CreateDragAdorner(UIElement owner, UIElement adornedElement, double opacity, Point dragPos)
        {
            return new EllipseDragAdorner(owner, adornedElement, opacity, dragPos);
        }
    }
}
