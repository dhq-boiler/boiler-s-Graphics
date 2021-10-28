using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.Models;
using boilersGraphics.Views.Behaviors;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using TsOperationHistory.Extensions;

namespace boilersGraphics.ViewModels
{
    class LayersViewModel : BindableBase, IDialogAware
    {
        private CompositeDisposable _disposables = new CompositeDisposable();

        public string Title => "レイヤー";

#pragma warning disable CS0067
        public event Action<IDialogResult> RequestClose;
#pragma warning restore CS0067

        public ReactiveCommand<RoutedPropertyChangedEventArgs<Object>> SelectedItemChangedCommand { get; } = new ReactiveCommand<RoutedPropertyChangedEventArgs<object>>();

        public DelegateCommand AddLayerCommand { get; }

        public DelegateCommand RemoveLayerCommand { get; }

        public ICommand DropCommand { get; }


        public ReactiveCollection<LayerTreeViewItemBase> Layers { get; }

        public LayersViewModel()
        {
            var mainWindowVM = App.Current.MainWindow.DataContext as MainWindowViewModel;
            Layers = mainWindowVM.DiagramViewModel.Layers;
            InitializeHitTestVisible();
            AddLayerCommand = new DelegateCommand(() =>
            {
                var layer = new Layer();
                layer.IsVisible.Value = true;
                layer.Name.Value = Name.GetNewLayerName(mainWindowVM.DiagramViewModel);
                Random rand = new Random();
                layer.Color.Value = Randomizer.RandomColor(rand);
                (App.Current.MainWindow.DataContext as MainWindowViewModel).Controller.ExecuteAdd((App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel.Layers, layer);
            });
            RemoveLayerCommand = new DelegateCommand(() =>
            {
                var diagramViewModel = (App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel;
                var layers = diagramViewModel.Layers;
                var selectedLayers = diagramViewModel.SelectedLayers;
                foreach (var remove in selectedLayers.Value.ToList())
                {
                    (App.Current.MainWindow.DataContext as MainWindowViewModel).Controller.ExecuteRemove(layers, remove);
                }
            });
            SelectedItemChangedCommand.Subscribe(args =>
            {
                var newItem = args.NewValue;
                if (newItem == null) return;
                var layers = (App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel.Layers;
                if (newItem.GetType() == typeof(Layer))
                {
                    layers.ToList().ForEach(x =>
                    {
                        x.IsSelected.Value = false;
                        x.ChildrenSwitchIsHitTestVisible(false);
                    });

                    var layerItems = layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
                                           .Where(x => x is LayerItem);

                    layerItems.ToList().ForEach(x =>
                    {
                        if (x.HasAsAncestor(newItem as LayerTreeViewItemBase))
                        {
                            x.IsSelected.Value = true;
                            if ((x as LayerItem).Item.Value is SnapPointViewModel snapPointVM)
                            {
                                snapPointVM.Opacity.Value = 1.0;
                            }
                        }
                        else
                        {
                            x.IsSelected.Value = false;
                            if ((x as LayerItem).Item.Value is SnapPointViewModel snapPointVM)
                            {
                                snapPointVM.Opacity.Value = 0.5;
                            }
                        }
                    });

                    var selectedLayer = newItem as Layer;
                    selectedLayer.IsSelected.Value = true;
                    selectedLayer.ChildrenSwitchIsHitTestVisible(true);

                    selectedLayer.UpdateAppearance(selectedLayer.Children.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(xx => xx.Children).Select(x => (x as LayerItem).Item.Value));
                    selectedLayer.Children.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
                                          .ToList()
                                          .ForEach(x => (x as LayerItem).UpdateAppearance(IfGroupBringChildren(selectedLayer.Children, (x as LayerItem).Item.Value)));
                }
                else if (newItem.GetType() == typeof(LayerItem))
                {
                    var selectedItem = newItem as LayerItem;

                    if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
                    {
                        Layers.ToList().ForEach(x => x.IsSelected.Value = false);
                    }

                    var layerItems = layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
                                           .Where(x => x is LayerItem);
                    layerItems.ToList().ForEach(x =>
                    {
                        if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
                        {
                            x.IsSelected.Value = false;
                        }
                        if ((x as LayerItem).Item.Value is SnapPointViewModel snapPointVM)
                        {
                            snapPointVM.Opacity.Value = 0.5;
                        }
                    });
                    selectedItem.IsSelected.Value = true;
                    if (selectedItem.Item.Value is SnapPointViewModel snapPointVM)
                    {
                        snapPointVM.Opacity.Value = 1.0;
                    }
                    if (selectedItem.Item.Value is DesignerItemViewModelBase designerItem)
                    {
                        if (layerItems.Where(x => x.IsSelected.Value == true).Count() > 1)
                        {
                            (App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel.EdgeColors.Clear();
                            (App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel.FillColors.Clear();
                        }
                        else
                        {
                            (App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel.EdgeColors.Clear();
                            (App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel.FillColors.Clear();
                            (App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel.EdgeColors.Add(designerItem.EdgeColor.Value);
                            (App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel.FillColors.Add(designerItem.FillColor.Value);
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

        private IEnumerable<SelectableDesignerItemViewModelBase> IfGroupBringChildren(ReactiveCollection<LayerTreeViewItemBase> Children, SelectableDesignerItemViewModelBase value)
        {
            if (value is GroupItemViewModel groupItemVM)
            {
                var children = Children.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
                                       .Select(x => (x as LayerItem).Item.Value)
                                       .Where(x => x.ParentID == groupItemVM.ID);
                return children;
            }
            else
            {
                return new List<SelectableDesignerItemViewModelBase>() { value };
            }
        }

        public void InitializeHitTestVisible()
        {
            var mainWindowVM = App.Current.MainWindow.DataContext as MainWindowViewModel;
            Layers.ToList().ForEach(x =>
            {
                x.ChildrenSwitchIsHitTestVisible(false);
            });
            mainWindowVM.DiagramViewModel.SelectedLayers.Value.ToList().ForEach(x =>
            {
                x.ChildrenSwitchIsHitTestVisible(true);
            });
        }

        private void Drop(DropArguments args)
        {
            switch (args.Type)
            {
                case MoveableTreeViewBehavior.InsertType.Before:
                    LoggerHelper.GetLogger().Trace("Before");
                    LoggerHelper.GetLogger().Trace(string.Join(", ", Layers));
                    break;
                case MoveableTreeViewBehavior.InsertType.After:
                    LoggerHelper.GetLogger().Trace("After");
                    LoggerHelper.GetLogger().Trace(string.Join(", ", Layers));
                    break;
                case MoveableTreeViewBehavior.InsertType.Children:
                    LoggerHelper.GetLogger().Trace("Children");
                    LoggerHelper.GetLogger().Trace(string.Join(", ", Layers));
                    break;
            }
        }

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
    }
}
