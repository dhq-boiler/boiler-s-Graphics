using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using boilersGraphics.Exceptions;

namespace boilersGraphics.Views;

/// <summary>
///     DetailPathGeometry.xaml の相互作用ロジック
/// </summary>
public partial class DetailPathGeometry : UserControl
{
    public enum Placement
    {
        Left,
        Top,
        Right,
        Bottom
    }

    public static readonly DependencyProperty StretchProperty = DependencyProperty.Register("Stretch", typeof(Stretch),
        typeof(DetailPathGeometry), new FrameworkPropertyMetadata(Stretch.None, null));

    public static readonly DependencyProperty CenterVisibilityProperty = DependencyProperty.Register("CenterVisibility",
        typeof(Visibility), typeof(DetailPathGeometry), new FrameworkPropertyMetadata(Visibility.Visible, null));

    public static readonly DependencyProperty WidthPlacementProperty = DependencyProperty.Register("WidthPlacement",
        typeof(Placement), typeof(DetailPathGeometry), new FrameworkPropertyMetadata(Placement.Bottom, null));

    public DetailPathGeometry()
    {
        InitializeComponent();
    }

    public Stretch Stretch
    {
        get => (Stretch)GetValue(StretchProperty);
        set => SetValue(StretchProperty, value);
    }

    public Visibility CenterVisibility
    {
        get => (Visibility)GetValue(CenterVisibilityProperty);
        set => SetValue(CenterVisibilityProperty, value);
    }

    public Placement WidthPlacement
    {
        get => (Placement)GetValue(WidthPlacementProperty);
        set => SetValue(WidthPlacementProperty, value);
    }

    public int WidthRow
    {
        get
        {
            switch (WidthPlacement)
            {
                case Placement.Top:
                    return 1;
                case Placement.Bottom:
                    return 4;
                default:
                    throw new UnexpectedException();
            }
        }
    }
}