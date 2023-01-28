using System.Windows.Controls;
using Prism.Regions;

namespace boilersGraphics.Views;

/// <summary>
///     DetailEllipse.xaml の相互作用ロジック
/// </summary>
public partial class DetailEllipse : UserControl
{
    public DetailEllipse(IRegionManager regionManager)
    {
        InitializeComponent();
        RegionManager.SetRegionManager(_DetailRegion, regionManager);
    }
}