using Prism.Regions;
using System.Windows.Controls;

namespace boilersGraphics.Views;

/// <summary>
/// プロパティダイアログ拡充: AnchorViewModel 用の Detail ダイアログ。
/// </summary>
public partial class DetailAnchor : UserControl
{
    public DetailAnchor(IRegionManager regionManager)
    {
        InitializeComponent();
        RegionManager.SetRegionManager(_DetailRegion, regionManager);
    }
}
