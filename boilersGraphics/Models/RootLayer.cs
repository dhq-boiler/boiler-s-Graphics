using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using boilersGraphics.ViewModels;

namespace boilersGraphics.Models
{
    internal class RootLayer : LayerTreeViewItemBase
    {
        public override void UpdateAppearance(IEnumerable<SelectableDesignerItemViewModelBase> items)
        {
            throw new NotSupportedException("Do not run.");
        }
    }
}
