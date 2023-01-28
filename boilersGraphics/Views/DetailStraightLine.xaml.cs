using System.Windows.Controls;
using Prism.Regions;

namespace boilersGraphics.Views;

/// <summary>
///     DetailStraightLine.xaml の相互作用ロジック
/// </summary>
public partial class DetailStraightLine : UserControl
{
    public DetailStraightLine(IRegionManager regionManager)
    {
        InitializeComponent();
        RegionManager.SetRegionManager(_DetailRegion, regionManager);
    }
}