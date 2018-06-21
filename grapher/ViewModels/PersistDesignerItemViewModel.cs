using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace grapher.ViewModels
{
    public class PersistDesignerItemViewModel : DesignerItemViewModelBase
    {
        public PersistDesignerItemViewModel(int id, DiagramViewModel parent, double left, double top)
            : base(id, parent, left, top)
        {
            Init();
        }

        public PersistDesignerItemViewModel() : base()
        {
            Init();
        }

        private void Init()
        {
            this.ShowConnectors = false;
        }
    }
}
