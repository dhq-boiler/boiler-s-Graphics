using System.Windows;
using System.Windows.Controls;
using boilersGraphics.Helpers;
using boilersGraphics.Models;

namespace boilersGraphics.Views;

/// <summary>
///     SolidColorPicker.xaml の相互作用ロジック
/// </summary>
public partial class SolidColorPicker : UserControl
{
    public static readonly DependencyProperty EditTargetProperty =
        DependencyProperty.Register("EditTarget", typeof(ColorExchange), typeof(SolidColorPicker));

    public static readonly DependencyProperty ColorSpotsProperty =
        DependencyProperty.Register("ColorSpots", typeof(ColorSpots), typeof(SolidColorPicker));

    public SolidColorPicker()
    {
        InitializeComponent();
    }

    public ColorExchange EditTarget
    {
        get => (ColorExchange)GetValue(EditTargetProperty);
        set => SetValue(EditTargetProperty, value);
    }

    public ColorSpots ColorSpots
    {
        get => (ColorSpots)GetValue(ColorSpotsProperty);
        set => SetValue(ColorSpotsProperty, value);
    }
}