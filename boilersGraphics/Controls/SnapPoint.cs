using boilersGraphics.Helpers;
using DependencyPropertyGenerator;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace boilersGraphics.Controls;

[DependencyProperty<SnapPointPosition>("SnapPointPosition")]
public partial class SnapPoint : Thumb
{
    public SnapPoint()
    {
    }

    public SnapPoint(double x, double y)
    {
        SetValue(Canvas.LeftProperty, x);
        SetValue(Canvas.TopProperty, y);
    }
}
