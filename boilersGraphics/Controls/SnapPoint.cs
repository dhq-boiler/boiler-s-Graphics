using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using boilersGraphics.Helpers;

namespace boilersGraphics.Controls;

public class SnapPoint : Thumb
{
    public static readonly DependencyProperty SnapPointPositionProperty =
        DependencyProperty.Register("SnapPointPosition", typeof(SnapPointPosition), typeof(SnapPoint));

    public SnapPoint()
    {
    }

    public SnapPoint(double x, double y)
    {
        SetValue(Canvas.LeftProperty, x);
        SetValue(Canvas.TopProperty, y);
    }

    public SnapPointPosition SnapPointPosition
    {
        get => (SnapPointPosition)GetValue(SnapPointPositionProperty);
        set => SetValue(SnapPointPositionProperty, value);
    }
}