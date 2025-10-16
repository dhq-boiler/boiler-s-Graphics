using Prism.Regions;
using System.Windows.Controls;

namespace boilersGraphics.Views;

/// <summary>
///     DetailLetter.xaml の相互作用ロジック
/// </summary>
public partial class DetailLetter : UserControl
{
    public DetailLetter(IRegionManager regionManager)
    {
        InitializeComponent();
        RegionManager.SetRegionManager(_DetailRegion, regionManager);
    }
}