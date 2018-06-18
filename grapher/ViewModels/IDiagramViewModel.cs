using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace grapher.ViewModels
{
    public interface IDiagramViewModel
    {
        DelegateCommand<object> AddItemCommand { get; }
        DelegateCommand<object> RemoveItemCommand { get; }
        DelegateCommand<object> ClearSelectedItemsCommand { get; }
        List<SelectableDesignerItemViewModelBase> SelectedItems { get; }
        ObservableCollection<SelectableDesignerItemViewModelBase> Items { get; }
    }
}
