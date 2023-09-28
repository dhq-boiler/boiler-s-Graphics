using Prism.Regions;
using System.Windows.Controls;

namespace boilersGraphics.Views;

/// <summary>
///     DetailPolygon.xaml の相互作用ロジック
/// </summary>
public partial class DetailPolygon : UserControl
{
    public DetailPolygon(IRegionManager regionManager)
    {
        InitializeComponent();
        RegionManager.SetRegionManager(_DetailRegion, regionManager);
    }
}