using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace grapher.ViewModels
{
    public interface ISelectItems
    {
        DelegateCommand<object> SelectItemCommand { get; }
    }


    public class SelectableDesignerItemViewModelBase : BindableBase, ISelectItems
    {
        private bool _IsSelected;

        public SelectableDesignerItemViewModelBase(int id, IDiagramViewModel parent)
        {
            this.Id = id;
            this.Parent = parent;
            Init();
        }

        public SelectableDesignerItemViewModelBase()
        {
            Init();
        }

        public List<SelectableDesignerItemViewModelBase> SelectedItems
        {
            get { return Parent.SelectedItems; }
        }

        public IDiagramViewModel Parent { get; set; }
        public DelegateCommand<object> SelectItemCommand { get; private set; }
        public int Id { get; set; }

        public bool IsSelected
        {
            get { return _IsSelected; }
            set { SetProperty(ref _IsSelected, value); }
        }

        private void ExecuteSelectItemCommand(object param)
        {
            SelectItem((bool)param, !IsSelected);
        }

        private void SelectItem(bool newselect, bool select)
        {
            if (newselect)
            {
                foreach (var designerItemViewModelBase in Parent.SelectedItems.ToList())
                {
                    designerItemViewModelBase.IsSelected = false;
                }
            }

            IsSelected = select;
        }

        private void Init()
        {
            SelectItemCommand = new DelegateCommand<object>(p => SelectItem((bool)p, !IsSelected));
        }
    }
}
