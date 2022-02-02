using boilersGraphics.Helpers;
using boilersGraphics.Models;
using System.Windows;
using System.Windows.Controls;

namespace boilersGraphics.Views
{
    /// <summary>
    /// SolidColorPicker.xaml の相互作用ロジック
    /// </summary>
    public partial class SolidColorPicker : UserControl
    {
        public SolidColorPicker()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty EditTargetProperty = DependencyProperty.Register("EditTarget", typeof(ColorExchange), typeof(SolidColorPicker));

        public static readonly DependencyProperty ColorSpotsProperty = DependencyProperty.Register("ColorSpots", typeof(ColorSpots), typeof(SolidColorPicker));

        public ColorExchange EditTarget
        {
            get { return (ColorExchange)GetValue(EditTargetProperty); }
            set { SetValue(EditTargetProperty, value); }
        }

        public ColorSpots ColorSpots
        {
            get { return (ColorSpots)GetValue(ColorSpotsProperty); }
            set { SetValue(ColorSpotsProperty, value); }
        }
    }
}
