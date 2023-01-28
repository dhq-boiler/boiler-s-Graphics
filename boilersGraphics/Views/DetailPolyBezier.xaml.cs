using System.Windows.Controls;
using Prism.Regions;

namespace boilersGraphics.Views;

/// <summary>
///     DetailPolyBezier.xaml の相互作用ロジック
/// </summary>
public partial class DetailPolyBezier : UserControl
{
    public DetailPolyBezier(IRegionManager regionManager)
    {
        InitializeComponent();
        RegionManager.SetRegionManager(_DetailRegion, regionManager);
    }
}