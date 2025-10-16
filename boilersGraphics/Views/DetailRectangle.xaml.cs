using Prism.Regions;
using System.Windows.Controls;

namespace boilersGraphics.Views;

/// <summary>
///     Rectangle.xaml の相互作用ロジック
/// </summary>
public partial class DetailRectangle : UserControl
{
    public DetailRectangle(IRegionManager regionManager)
    {
        InitializeComponent();
        RegionManager.SetRegionManager(_DetailRegion, regionManager);
    }
}