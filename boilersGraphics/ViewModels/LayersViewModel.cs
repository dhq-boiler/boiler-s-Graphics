using boilersGraphics.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace boilersGraphics.ViewModels
{
    class LayersViewModel : BindableBase, IDialogAware
    {
        private CompositeDisposable _disposables = new CompositeDisposable();

        public string Title => "レイヤー";

        public event Action<IDialogResult> RequestClose;

        public ReactiveCommand<RoutedPropertyChangedEventArgs<Object>> SelectedItemChangedCommand { get; } = new ReactiveCommand<RoutedPropertyChangedEventArgs<object>>();

        public DelegateCommand AddLayerCommand { get; }


        public ReadOnlyReactiveCollection<Layer> Layers { get; }

        public LayersViewModel()
        {
            Layers = (App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel.Layers.ToReadOnlyReactiveCollection();
            AddLayerCommand = new DelegateCommand(() =>
            {
                var layer = new Layer();
                layer.IsVisible.Value = true;
                layer.Name.Value = $"レイヤー{Layer.LayerCount++}";
                (App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel.Layers.Add(layer);
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
                        x.Items.ToList().ForEach(y =>
                        {
                            y.IsSelected.Value = false;
                        });
                    });
                    var selectedLayer = newItem as Layer;
                    selectedLayer.IsSelected.Value = true;
                }
                else if (newItem.GetType() == typeof(LayerItem))
                {
                    var selectedItem = newItem as LayerItem;
                    layers.ToList().ForEach(x =>
                    {
                        if (x == selectedItem.Owner.Value)
                        {
                            x.IsSelected.Value = true;
                        }
                        else
                        {
                            x.IsSelected.Value = false;
                        }
                        x.Items.ToList().ForEach(y =>
                        {
                            y.IsSelected.Value = false;
                        });
                    });
                    selectedItem.IsSelected.Value = true;
                }
            })
            .AddTo(_disposables);
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
