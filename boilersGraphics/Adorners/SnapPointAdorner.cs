using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace boilersGraphics.Adorners;

internal class SnapPointAdorner : Adorner
{
    private readonly double size;
    private readonly double thickness;
    private readonly Point _point;

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