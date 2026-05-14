using Prism.Regions;
using System.Windows.Controls;

namespace boilersGraphics.Views;

/// <summary>
/// プロパティダイアログ拡充: TextMatrixBlockViewModel 用の Detail ダイアログ。
/// </summary>
public partial class DetailTextMatrix : UserControl
{
    public DetailTextMatrix(IRegionManager regionManager)
    {
        InitializeComponent();
        RegionManager.SetRegionManager(_DetailRegion, regionManager);
    }
}
