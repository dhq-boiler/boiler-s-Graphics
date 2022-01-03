using Reactive.Bindings;
using System.Windows.Controls;
using static boilersGraphics.Views.DetailPathGeometry;

namespace boilersGraphics.Views
{
    /// <summary>
    /// Rectangle.xaml の相互作用ロジック
    /// </summary>
    public partial class DetailPie : UserControl
    {
        public DetailPie()
        {
            InitializeComponent();
        }

        public Placement WidthPlacement
        {
            get {
                //var detailPathGeometry = this.DetailPathGeometry;
                //var dockPanel = detailPathGeometry.ContentTemplate.FindName("WidthCell", detailPathGeometry);
                return Placement.Top;
            }
        }
    }
}
