using boilersGraphics.Models;
using ObservableCollections;
using Prism.Commands;
using R3;
using System.Windows.Media;

namespace boilersGraphics.ViewModels;

public interface IDiagramViewModel
{
    MainWindowViewModel MainWindowVM { get; }
    DelegateCommand<object> AddItemCommand { get; }
    DelegateCommand<object> RemoveItemCommand { get; }
    DelegateCommand<object> ClearSelectedItemsCommand { get; }
    IReadOnlyBindableReactiveProperty<SelectableDesignerItemViewModelBase[]> SelectedItems { get; }
    BindableReactiveProperty<Brush> EdgeBrush { get; }
    BindableReactiveProperty<Brush> FillBrush { get; }
    BindableReactiveProperty<double?> EdgeThickness { get; }
    NotifyCollectionChangedSynchronizedViewList<LayerTreeViewItemBase> Layers { get; }
    BindableReactiveProperty<BackgroundViewModel> BackgroundItem { get; }

    void DeselectAll();
}