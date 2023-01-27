using System.Windows.Controls;
using Prism.Regions;

namespace boilersGraphics.Views;

public partial class ColorPicker : UserControl
{
    public ColorPicker(IRegionManager regionManager)
    {
        InitializeComponent();
        RegionManager.SetRegionManager(_ColorPickerResion, regionManager);
    }
}