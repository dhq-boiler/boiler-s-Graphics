using Prism.Commands;
using Reactive.Bindings;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace boiler_sGraphics.ViewModels
{
    public interface IDiagramViewModel
    {
        DelegateCommand<object> AddItemCommand { get; }
        DelegateCommand<object> RemoveItemCommand { get; }
        DelegateCommand<object> ClearSelectedItemsCommand { get; }
        ReactiveCollection<SelectableDesignerItemViewModelBase> SelectedItems { get; }
        ObservableCollection<Color> EdgeColors { get; }
        ObservableCollection<Color> FillColors { get; }
        ObservableCollection<SelectableDesignerItemViewModelBase> Items { get; }

        void DeselectAll();

        int Width { get; }

        int Height { get; }
    }
}
