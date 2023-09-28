using Prism.Regions;
using System.Windows.Controls;

namespace boilersGraphics.Views
{
    /// <summary>
    /// ColorCorrectInstruction.xaml の相互作用ロジック
    /// </summary>
    public partial class ColorCorrectInstruction : UserControl
    {
        public ColorCorrectInstruction(IRegionManager regionManager)
        {
            InitializeComponent();
            RegionManager.SetRegionManager(_ColorCorrectInstructionResion, regionManager);
        }
    }
}
