using Prism.Regions;
using System.Windows.Controls;

namespace boilersGraphics.Views;

public partial class ColorPicker : UserControl
{
    public ColorPicker(IRegionManager regionManager)
    {
        InitializeComponent();
        RegionManager.SetRegionManager(_ColorPickerResion, regionManager);
    }
}