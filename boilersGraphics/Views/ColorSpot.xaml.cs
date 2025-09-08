using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using R3;

namespace boilersGraphics.Views;

/// <summary>
///     ColorSpot.xaml の相互作用ロジック
/// </summary>
public partial class ColorSpot : UserControl
{
    public static readonly DependencyProperty BrushProperty =
        DependencyProperty.Register("Brush", typeof(Brush), typeof(ColorSpot));

    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register("Command", typeof(ICommand), typeof(ColorSpot));

    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.Register("CommandParameter", typeof(object), typeof(ColorSpot));

    public ColorSpot()
    {
        InitializeComponent();
        Brush = Brushes.White;
    }

    public BindableReactiveProperty<bool> IsSelected { get; } = new();

    public Brush Brush
    {
        get => (Brush)GetValue(BrushProperty);
        set => SetValue(BrushProperty, value);
    }

    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }
}