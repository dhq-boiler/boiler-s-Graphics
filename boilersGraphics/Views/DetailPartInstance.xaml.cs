using Prism.Regions;
using System.Windows.Controls;

namespace boilersGraphics.Views;

public partial class DetailPartInstance : UserControl
{
    public DetailPartInstance(IRegionManager regionManager)
    {
        InitializeComponent();
        RegionManager.SetRegionManager(_DetailRegion, regionManager);
    }
}
