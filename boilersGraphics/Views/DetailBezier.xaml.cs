using Prism.Regions;
using System.Windows.Controls;

namespace boilersGraphics.Views;

/// <summary>
///     DetailBezier.xaml の相互作用ロジック
/// </summary>
public partial class DetailBezier : UserControl
{
    public DetailBezier(IRegionManager regionManager)
    {
        InitializeComponent();
        RegionManager.SetRegionManager(_DetailRegion, regionManager);
    }
}