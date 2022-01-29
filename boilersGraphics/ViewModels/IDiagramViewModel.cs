using boilersGraphics.Models;
using Prism.Commands;
using Reactive.Bindings;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace boilersGraphics.ViewModels
{
    public interface IDiagramViewModel
    {
        MainWindowViewModel MainWindowVM { get; }
        DelegateCommand<object> AddItemCommand { get; }
        DelegateCommand<object> RemoveItemCommand { get; }
        DelegateCommand<object> ClearSelectedItemsCommand { get; }
        ReadOnlyReactivePropertySlim<SelectableDesignerItemViewModelBase[]> SelectedItems { get; }
        ReactivePropertySlim<Brush> EdgeBrush { get; }
        ReactivePropertySlim<Brush> FillBrush { get; }
        ReactivePropertySlim<double?> EdgeThickness { get; }
        ReactiveCollection<LayerTreeViewItemBase> Layers { get; }
        int Width { get; }
        int Height { get; }

        void DeselectAll();
    }
}
