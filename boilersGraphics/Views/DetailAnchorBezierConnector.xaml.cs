using Prism.Regions;
using System.Windows.Controls;

namespace boilersGraphics.Views;

/// <summary>
/// プロパティダイアログ拡充: AnchorBezierConnectorViewModel 用の Detail ダイアログ。
/// </summary>
public partial class DetailAnchorBezierConnector : UserControl
{
    public DetailAnchorBezierConnector(IRegionManager regionManager)
    {
        InitializeComponent();
        RegionManager.SetRegionManager(_DetailRegion, regionManager);
    }
}
