using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DependencyPropertyGenerator;
using R3;

namespace boilersGraphics.Views;

/// <summary>
///     ColorSpot.xaml の相互作用ロジック
/// </summary>
[DependencyProperty<Brush>("Brush")]
[DependencyProperty<ICommand>("Command")]
[DependencyProperty<object>("CommandParameter")]
public partial class ColorSpot : UserControl
{
    public ColorSpot()
    {
        InitializeComponent();
        Brush = Brushes.White;
    }

    public BindableReactiveProperty<bool> IsSelected { get; } = new();
}
