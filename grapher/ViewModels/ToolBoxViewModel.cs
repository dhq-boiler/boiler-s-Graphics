using grapher.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace grapher.ViewModels
{
    public class ToolBoxViewModel
    {
        private List<ToolBoxData> toolBoxItems = new List<ToolBoxData>();

        public ToolBoxViewModel()
        {
            toolBoxItems.Add(new ToolBoxData("../Assets/img/Setting.png", typeof(SettingsDesignerItemViewModel)));
            toolBoxItems.Add(new ToolBoxData("../Assets/img/Persist.png", typeof(PersistDesignerItemViewModel)));
        }

        public List<ToolBoxData> ToolBoxItems
        {
            get { return toolBoxItems; }
        }
    }
}
