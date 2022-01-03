using boilersGraphics.Exceptions;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace boilersGraphics.Views
{
    /// <summary>
    /// DetailPathGeometry.xaml の相互作用ロジック
    /// </summary>
    public partial class DetailPathGeometry : UserControl
    {
        public DetailPathGeometry()
        {
            InitializeComponent();
        }

        public enum Placement
        {
            Left,
            Top,
            Right,
            Bottom
        }

        public static readonly DependencyProperty StretchProperty = DependencyProperty.Register("Stretch", typeof(Stretch), typeof(DetailPathGeometry), new FrameworkPropertyMetadata(Stretch.None, null));

        public static readonly DependencyProperty CenterVisibilityProperty = DependencyProperty.Register("CenterVisibility", typeof(Visibility), typeof(DetailPathGeometry), new FrameworkPropertyMetadata(Visibility.Visible, null));

        public static readonly DependencyProperty WidthPlacementProperty = DependencyProperty.Register("WidthPlacement", typeof(Placement), typeof(DetailPathGeometry), new FrameworkPropertyMetadata(Placement.Bottom, null));
        public Stretch Stretch
        {
            get { return (Stretch)GetValue(StretchProperty); }
            set { SetValue(StretchProperty, value); }
        }

        public Visibility CenterVisibility
        {
            get { return (Visibility)GetValue(CenterVisibilityProperty); }
            set { SetValue(CenterVisibilityProperty, value); }
        }

        public Placement WidthPlacement
        {
            get { return (Placement)GetValue(WidthPlacementProperty); }
            set { SetValue(WidthPlacementProperty, value); }
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
}
