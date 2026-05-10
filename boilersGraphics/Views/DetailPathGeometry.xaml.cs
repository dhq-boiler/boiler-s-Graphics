using boilersGraphics.Exceptions;
using DependencyPropertyGenerator;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace boilersGraphics.Views;

/// <summary>
///     DetailPathGeometry.xaml の相互作用ロジック
/// </summary>
[DependencyProperty<Stretch>("Stretch", DefaultValue = Stretch.None)]
[DependencyProperty<Visibility>("CenterVisibility", DefaultValue = Visibility.Visible)]
[DependencyProperty<DetailPathGeometry.Placement>("WidthPlacement", DefaultValue = DetailPathGeometry.Placement.Bottom)]
public partial class DetailPathGeometry : UserControl
{
    public enum Placement
    {
        Left,
        Top,
        Right,
        Bottom
    }

    public DetailPathGeometry()
    {
        InitializeComponent();
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
