using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace boilersGraphics.Views;

/// <summary>
///     GradientMap.xaml の相互作用ロジック
/// </summary>
public partial class GradientMap : UserControl
{
    public static readonly DependencyProperty FillProperty =
        DependencyProperty.Register("Fill", typeof(Brush), typeof(GradientMap));

    public GradientMap()
    {
        InitializeComponent();
    }

    public Brush Fill
    {
        get => (Brush)GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }
}