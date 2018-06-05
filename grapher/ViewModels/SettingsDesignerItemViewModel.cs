using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace grapher.ViewModels
{
    public class SettingsDesignerItemViewModel : DesignerItemViewModelBase
    {

        public SettingsDesignerItemViewModel(int id, DiagramViewModel parent, double left, double top)
            : base(id, parent, left, top)
        {
            Init();
        }

        public SettingsDesignerItemViewModel()
        {
            Init();
        }

        private void Init()
        {
            this.ShowConnectors = false;
        }
    }
}
