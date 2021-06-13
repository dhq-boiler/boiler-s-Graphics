using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace grapher.ViewModels
{
    public class PictureDesignerItemViewModel : DesignerItemViewModelBase
    {
        private string _FileName;

        public string FileName
        {
            get { return _FileName; }
            set { SetProperty(ref _FileName, value); }
        }

        public PictureDesignerItemViewModel(int id, DiagramViewModel parent, double left, double top)
            : base(id, parent, left, top)
        {
            Init();
        }

        public PictureDesignerItemViewModel()
        {
            Init();
        }

        private void Init()
        {
            this.ShowConnectors = false;
        }

        #region IClonable

        public override object Clone()
        {
            var clone = new PictureDesignerItemViewModel();
            clone.Owner = Owner;
            clone.Left.Value = Left.Value;
            clone.Top.Value = Top.Value;
            clone.Width.Value = Width.Value;
            clone.Height.Value = Height.Value;
            clone.EdgeColor = EdgeColor;
            clone.FillColor = FillColor;
            clone.Matrix.Value = Matrix.Value;
            clone.RotationAngle.Value = RotationAngle.Value;
            return clone;
        }

        #endregion //IClonable
    }
}
