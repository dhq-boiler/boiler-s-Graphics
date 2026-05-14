using Prism.Regions;
using System.Windows.Controls;

namespace boilersGraphics.Views;

/// <summary>
/// プロパティダイアログ拡充: OrthogonalConnectorViewModel 用の Detail ダイアログ。
/// </summary>
public partial class DetailOrthogonalConnector : UserControl
{
    public DetailOrthogonalConnector(IRegionManager regionManager)
    {
        InitializeComponent();
        RegionManager.SetRegionManager(_DetailRegion, regionManager);
    }
}
