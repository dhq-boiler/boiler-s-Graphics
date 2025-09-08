using boilersGraphics.Dao;
using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.Models;
using boilersGraphics.Properties;
using boilersGraphics.Views.Behaviors;
using NLog;
using ObservableCollections;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using R3;
using TsOperationHistory.Extensions;
using ZLinq;

namespace boilersGraphics.ViewModels;

internal class LayersViewModel : BindableBase, IDialogAware
{
    private readonly CompositeDisposable _disposables = new();

    public LayersViewModel()
    {
        var mainWindowVM = App.Instance.MainWindow.DataContext as MainWindowViewModel;
        Layers = mainWindowVM.DiagramViewModel.Layers;
        InitializeHitTestVisible(mainWindowVM);
        AddLayerCommand = new DelegateCommand(() =>
        {
            var layer = new Layer();
            layer.IsVisible.Value = true;
            layer.Name.Value = Name.GetNewLayerName(mainWindowVM.DiagramViewModel);
            var rand = new Random();
            layer.Color.Value = Randomizer.RandomColor(rand);
            mainWindowVM.Controller.ExecuteAdd(mainWindowVM.DiagramViewModel.Layers, layer);
            UpdateStatisticsCountNewLayer(mainWindowVM);
        });
        RemoveLayerCommand = new DelegateCommand(() =>
        {
            var diagramViewModel = mainWindowVM.DiagramViewModel;
            var layers = diagramViewModel.Layers;
            var selectedLayers = diagramViewModel.SelectedLayers;
            foreach (var remove in selectedLayers.Value.AsValueEnumerable().ToList()) mainWindowVM.Controller.ExecuteRemove(layers, remove);
            UpdateStatisticsCountRemoveLayer(mainWindowVM);
        });
        SelectedItemChangedCommand.Subscribe(args =>
            {
                var newItem = args.NewValue;
                if (newItem == null) return;
                var layers = mainWindowVM.DiagramViewModel.Layers;
                if (newItem.GetType() == typeof(Layer))
                {
                    layers.AsValueEnumerable().ToList().ForEach(x =>
                    {
                        x.IsSelected.Value = false;
                        x.ChildrenSwitchIsHitTestVisible(false);
                    });

                    var layerItems = layers
                        .SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
                        .AsValueEnumerable()
                        .Where(x => x is LayerItem);

                    layerItems.ToList().ForEach(x =>
                    {
                        if (x.HasAsAncestor(newItem as LayerTreeViewItemBase))
                        {
                            x.IsSelected.Value = true;
                            if ((x as LayerItem).Item.Value is ConnectorBaseViewModel connector)
                            {
                                connector.SnapPoint0VM.Value.IsSelected.Value = true;
                                connector.SnapPoint1VM.Value.IsSelected.Value = true;
                            }

                            if ((x as LayerItem).Item.Value is SnapPointViewModel snapPointVM)
                                snapPointVM.Opacity.Value = 1.0;
                        }
                        else
                        {
                            x.IsSelected.Value = false;
                            if ((x as LayerItem).Item.Value is ConnectorBaseViewModel connector)
                            {
                                connector.SnapPoint0VM.Value.IsSelected.Value = false;
                                connector.SnapPoint1VM.Value.IsSelected.Value = false;
                            }

                            if ((x as LayerItem).Item.Value is SnapPointViewModel snapPointVM)
                                snapPointVM.Opacity.Value = 0.5;
                        }
                    });

                    var selectedLayer = newItem as Layer;
                    selectedLayer.IsSelected.Value = true;
                    selectedLayer.ChildrenSwitchIsHitTestVisible(true);

                    selectedLayer.UpdateAppearanceBothParentAndChild();
                }
                else if (newItem.GetType() == typeof(LayerItem))
                {
                    var selectedItem = newItem as LayerItem;

                    if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
                        Layers.AsValueEnumerable().ToList().ForEach(x => x.IsSelected.Value = false);

                    var layerItems = layers
                        .SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
                        .AsValueEnumerable()
                        .Where(x => x is LayerItem);
                    layerItems.ToList().ForEach(x =>
                    {
                        if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
                        {
                            x.IsSelected.Value = false;
                            if ((x as LayerItem).Item.Value is ConnectorBaseViewModel connector)
                            {
                                connector.SnapPoint0VM.Value.IsSelected.Value = false;
                                connector.SnapPoint1VM.Value.IsSelected.Value = false;
                            }
                        }

                        if ((x as LayerItem).Item.Value is SnapPointViewModel snapPointVM)
                            snapPointVM.Opacity.Value = 0.5;
                    });
                    selectedItem.IsSelected.Value = true;
                    if (selectedItem.Item.Value is ConnectorBaseViewModel connector)
                    {
                        connector.SnapPoint0VM.Value.IsSelected.Value = true;
                        connector.SnapPoint1VM.Value.IsSelected.Value = true;
                    }

                    if (selectedItem.Item.Value is SnapPointViewModel snapPointVM) snapPointVM.Opacity.Value = 1.0;
                    if (selectedItem.Item.Value is DesignerItemViewModelBase designerItem)
                    {
                        if (layerItems.Where(x => x.IsSelected.Value).Count() > 1)
                        {
                        }
                        else
                        {
                            mainWindowVM.DiagramViewModel.EdgeBrush.Value = designerItem.EdgeBrush.Value.Clone();
                            mainWindowVM.DiagramViewModel.FillBrush.Value = designerItem.FillBrush.Value.Clone();
                        }
                    }

                    LayerTreeViewItemBase temp = selectedItem;
                    while (temp.Parent.Value != null)
                    {
                        if (temp is Layer)
                            break;
                        temp = temp.Parent.Value;
                    }

                    temp.IsSelected.Value = true;

                    selectedItem.UpdateAppearance(IfGroupBringChildren(selectedItem.Children, selectedItem.Item.Value));
                }
            })
            .AddTo(_disposables);
        DropCommand = new DelegateCommand<DropArguments>(Drop);
    }

    public ReactiveCommand<RoutedPropertyChangedEventArgs<object>> SelectedItemChangedCommand { get; } = new();

    public DelegateCommand AddLayerCommand { get; }

    public DelegateCommand RemoveLayerCommand { get; }

    public ICommand DropCommand { get; }


    public NotifyCollectionChangedSynchronizedViewList<LayerTreeViewItemBase> Layers { get; }

    public string Title => Resources.ResourceManager.GetString("Title_Layers", Resources.Culture);

#pragma warning disable CS0067
    public event Action<IDialogResult> RequestClose;
#pragma warning restore CS0067

    public bool CanCloseDialog()
    {
        return true;
    }

    public void OnDialogClosed()
    {
    }

    public void OnDialogOpened(IDialogParameters parameters)
    {
    }

    private static void UpdateStatisticsCountNewLayer(MainWindowViewModel mainWindowVM)
    {
        var statistics = mainWindowVM.Statistics.Value;
        statistics.NumberOfNewlyCreatedLayers++;
        var dao = new StatisticsDao();
        dao.Update(statistics);
    }

    private static void UpdateStatisticsCountRemoveLayer(MainWindowViewModel mainWindowVM)
    {
        var statistics = mainWindowVM.Statistics.Value;
        statistics.NumberOfDeletedLayers++;
        var dao = new StatisticsDao();
        dao.Update(statistics);
    }

    private IEnumerable<SelectableDesignerItemViewModelBase> IfGroupBringChildren(
        NotifyCollectionChangedSynchronizedViewList<LayerTreeViewItemBase> Children, SelectableDesignerItemViewModelBase value)
    {
        if (value is GroupItemViewModel groupItemVM)
        {
            var children = Children.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
                .AsValueEnumerable()
                .Select(x => (x as LayerItem).Item.Value)
                .Where(x => x.ParentID == groupItemVM.ID).ToArray();
            return children;
        }

        return new List<SelectableDesignerItemViewModelBase> { value };
    }

    public void InitializeHitTestVisible(MainWindowViewModel mainWindowVM)
    {
        Layers.AsValueEnumerable().ToList().ForEach(x => { x.ChildrenSwitchIsHitTestVisible(false); });
        mainWindowVM.DiagramViewModel.SelectedLayers.Value.AsValueEnumerable().ToList().ForEach(x =>
        {
            x.ChildrenSwitchIsHitTestVisible(true);
        });
    }

    private void Drop(DropArguments args)
    {
        switch (args.Type)
        {
            case MoveableTreeViewBehavior.InsertType.Before:
                LogManager.GetCurrentClassLogger().Trace("Before");
                LogManager.GetCurrentClassLogger().Trace(string.Join(", ", Layers));
                break;
            case MoveableTreeViewBehavior.InsertType.After:
                LogManager.GetCurrentClassLogger().Trace("After");
                LogManager.GetCurrentClassLogger().Trace(string.Join(", ", Layers));
                break;
            case MoveableTreeViewBehavior.InsertType.Children:
                LogManager.GetCurrentClassLogger().Trace("Children");
                LogManager.GetCurrentClassLogger().Trace(string.Join(", ", Layers));
                break;
        }
    }
}