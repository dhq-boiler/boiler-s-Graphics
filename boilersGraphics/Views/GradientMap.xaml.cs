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
    /// GradientMap.xaml の相互作用ロジック
    /// </summary>
    public partial class GradientMap : UserControl
    {
        public GradientMap()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty FillProperty = DependencyProperty.Register("Fill", typeof(Brush), typeof(GradientMap));

        public Brush Fill
        {
            get { return (Brush)GetValue(FillProperty);}
            set { SetValue(FillProperty, value); }
        }
    }
}
