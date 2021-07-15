using System;
using System.Windows;

namespace boilersGraphics.Views.Behaviors
{
    [Obsolete]
    internal class RectangleDragStartBehavior : AbstractDragStartBehavior
    {
        protected override AbstractDragAdorner CreateDragAdorner(UIElement owner, UIElement adornedElement, double opacity, Point dragPos)
        {
            return new RectangleDragAdorner(owner, adornedElement, opacity, dragPos);
        }
    }
}
