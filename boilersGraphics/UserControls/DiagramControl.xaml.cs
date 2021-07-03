using boilersGraphics.Controls;
using boilersGraphics.Extensions;
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

namespace boilersGraphics.UserControls
{
    /// <summary>
    /// DiagramControl.xaml の相互作用ロジック
    /// </summary>
    public partial class DiagramControl : UserControl
    {
        public DiagramControl()
        {
            InitializeComponent();
        }

        private void DesignerCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            DesignerCanvas myDesignerCanvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();
            zoomBox.DesignerCanvas = myDesignerCanvas;
        }
    }
}
