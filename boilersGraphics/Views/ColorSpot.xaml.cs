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
    /// ColorSpot.xaml の相互作用ロジック
    /// </summary>
    public partial class ColorSpot : UserControl
    {
        public ColorSpot()
        {
            InitializeComponent();
        }

        public ReactivePropertySlim<Color> Color { get; } = new ReactivePropertySlim<Color>(Colors.White);

        public ReactivePropertySlim<bool> IsSelected { get; } = new ReactivePropertySlim<bool>();

        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(ColorSpot));

        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register("CommandParameter", typeof(object), typeof(ColorSpot));

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); } 
            set { SetValue(CommandProperty, value); }
        }

        public object CommandParameter
        {
            get { return (object)GetValue(CommandParameterProperty);}
            set { SetValue(CommandParameterProperty, value); }
        }
    }
}
