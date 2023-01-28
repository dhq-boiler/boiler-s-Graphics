using System.Windows.Controls;
using Prism.Regions;

namespace boilersGraphics.Views;

/// <summary>
///     DetailPathGeometry.xaml の相互作用ロジック
/// </summary>
public partial class DetailPicture : UserControl
{
    public DetailPicture(IRegionManager regionManager)
    {
        InitializeComponent();
        RegionManager.SetRegionManager(_DetailRegion, regionManager);
    }
}