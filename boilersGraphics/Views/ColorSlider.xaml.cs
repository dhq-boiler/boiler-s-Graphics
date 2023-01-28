using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace boilersGraphics.Views;

/// <summary>
///     ColorSlider.xaml の相互作用ロジック
/// </summary>
public partial class ColorSlider : UserControl
{
    public static readonly DependencyProperty BackgroundBitmapSourceProperty =
        DependencyProperty.Register("BackgroundBitmapSource", typeof(ImageSource), typeof(ColorSlider));

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register("Maximum", typeof(int), typeof(ColorSlider));

    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register("Minimum", typeof(int), typeof(ColorSlider));

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register("Value", typeof(int), typeof(ColorSlider));

    public ColorSlider()
    {
        InitializeComponent();
    }

    public ImageSource BackgroundBitmapSource
    {
        get => (ImageSource)GetValue(BackgroundBitmapSourceProperty);
        set => SetValue(BackgroundBitmapSourceProperty, value);
    }

    public int Maximum
    {
        get => (int)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public int Minimum
    {
        get => (int)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public int Value
    {
        get => (int)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
}