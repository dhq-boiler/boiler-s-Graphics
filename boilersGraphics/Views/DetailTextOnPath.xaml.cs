using Prism.Regions;
using System.Windows.Controls;

namespace boilersGraphics.Views;

/// <summary>
/// プロパティダイアログ拡充: TextOnPathBlockViewModel 用の Detail ダイアログ。
/// </summary>
public partial class DetailTextOnPath : UserControl
{
    public DetailTextOnPath(IRegionManager regionManager)
    {
        InitializeComponent();
        RegionManager.SetRegionManager(_DetailRegion, regionManager);
    }
}
