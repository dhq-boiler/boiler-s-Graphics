﻿using boilersGraphics.Extensions;
using boilersGraphics.Models;
using boilersGraphics.Views.Behaviors;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;

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


        public ReadOnlyReactiveCollection<Layer> Layers { get; }

        public LayersViewModel()
        {
            var mainWindowVM = App.Current.MainWindow.DataContext as MainWindowViewModel;
            Layers = mainWindowVM.DiagramViewModel.Layers.ToReadOnlyReactiveCollection();
            AddLayerCommand = new DelegateCommand(() =>
            {
                var layer = new Layer();
                layer.IsVisible.Value = true;
                layer.Name.Value = $"レイヤー{Layer.LayerCount++}";
                (App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel.Layers.Add(layer);
            });
            RemoveLayerCommand = new DelegateCommand(() =>
            {
                var diagramViewModel = (App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel;
                var layers = diagramViewModel.Layers;
                var selectedLayers = diagramViewModel.SelectedLayers;
                foreach (var remove in selectedLayers.Value.ToList())
                {
                    layers.Remove(remove);
                }
            });
            SelectedItemChangedCommand.Subscribe(args =>
            {
                var newItem = args.NewValue;
                if (newItem == null) return;
                var layers = (App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel.Layers;
                if (newItem.GetType() == typeof(Layer))
                {
                    layers.ToList().ForEach(x => x.IsSelected.Value = false);

                    var layerItems = layers.SelectRecursive<Layer, LayerTreeViewItemBase>(x => x.Children)
                                           .Where(x => x is LayerItem);

                    layerItems.ToList().ForEach(x =>
                    {
                        if (x.HasAsAncestor(newItem as LayerTreeViewItemBase))
                        {
                            x.IsSelected.Value = true;
                        }
                        else
                        {
                            x.IsSelected.Value = false;
                        }
                    });

                    var selectedLayer = newItem as Layer;
                    selectedLayer.IsSelected.Value = true;
                }
                else if (newItem.GetType() == typeof(LayerItem))
                {
                    var selectedItem = newItem as LayerItem;

                    Layers.ToList().ForEach(x => x.IsSelected.Value = false);

                    var layerItems = layers.SelectRecursive<Layer, LayerTreeViewItemBase>(x => x.Children)
                                           .Where(x => x is LayerItem);
                    layerItems.ToList().ForEach(x =>
                    {
                        x.IsSelected.Value = false;
                    });
                    selectedItem.IsSelected.Value = true;

                    LayerTreeViewItemBase temp = selectedItem;
                    while (temp.Parent.Value != null)
                    {
                        if (temp is Layer)
                            break;
                        temp = temp.Parent.Value;
                    }
                    temp.IsSelected.Value = true;
                }
            })
            .AddTo(_disposables);
            DropCommand = new DelegateCommand<DropArguments>(Drop);
        }

        private void Drop(DropArguments args)
        {
            switch (args.Type)
            {
                case MoveableTreeViewBehavior.InsertType.Before:
                    Debug.WriteLine("Before");
                    break;
                case MoveableTreeViewBehavior.InsertType.After:
                    Debug.WriteLine("After");
                    break;
                case MoveableTreeViewBehavior.InsertType.Children:
                    Debug.WriteLine("Children");
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
