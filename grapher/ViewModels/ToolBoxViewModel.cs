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
        private List<ToolBoxData> _toolBoxItems = new List<ToolBoxData>();

        public ToolBoxViewModel()
        {
            _toolBoxItems.Add(new ToolBoxData("../Assets/img/Setting.png", typeof(SettingsDesignerItemViewModel)));
            _toolBoxItems.Add(new ToolBoxData("../Assets/img/Persist.png", typeof(PersistDesignerItemViewModel)));
        }

        public List<ToolBoxData> ToolBoxItems
        {
            get { return _toolBoxItems; }
        }
    }
}
