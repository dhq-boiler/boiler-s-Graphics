using boilersGraphics.ViewModels;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Windows.Input;

namespace boilersGraphics.Models
{
    public class Layer : BindableBase
    {
        private CompositeDisposable _disposable = new CompositeDisposable();
        public static int LayerCount { get; set; } = 1;
        public static ObservableCollection<Layer> SelectedLayers { get; } = new ObservableCollection<Layer>();

        public ReactivePropertySlim<bool> IsVisible { get; } = new ReactivePropertySlim<bool>();

        public ReactivePropertySlim<bool> IsSelected { get; } = new ReactivePropertySlim<bool>();

        public ReactivePropertySlim<bool> IsExpanded { get; } = new ReactivePropertySlim<bool>();

        public ReactivePropertySlim<string> Name { get; } = new ReactivePropertySlim<string>();

        public ReactiveCommand SwitchVisibilityCommand { get; } = new ReactiveCommand();
        public ReactiveCommand SelectLayerCommand { get; } = new ReactiveCommand();

        public ObservableCollection<LayerItem> Items { get; } = new ObservableCollection<LayerItem>();

        public IObservable<PropertyPack<LayerItem, bool>> Observable
        {
            get { return Items.ObserveElementObservableProperty(x => x.Observable); }
        }
        public IObservable<PropertyPack<LayerItem, SelectableDesignerItemViewModelBase>> AllItemsObservable
        {
            get { return Items.ObserveElementObservableProperty(x => x.AllItemsObservable); }
        }

        public Layer()
        {
            SwitchVisibilityCommand.Subscribe(_ =>
            {
                IsVisible.Value = !IsVisible.Value;
            })
            .AddTo(_disposable);
            SelectLayerCommand.Subscribe(args =>
            {
                MouseEventArgs ea = args as MouseEventArgs;
                if (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
                {
                    var diagramVM = (App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel;
                    diagramVM.Layers.Where(x => x.IsSelected.Value == true)
                                    .ToList()
                                    .ForEach(x => x.IsSelected.Value = false);
                }

                IsSelected.Value = true;
            })
            .AddTo(_disposable);
            IsVisible.Subscribe(isVisible =>
            {
                if (!isVisible)
                {
                    Items.ToList().ForEach(x => x.IsVisible.Value = isVisible);
                }
            })
            .AddTo(_disposable);
            IsVisible.Value = true;
        }

        public void RemoveItem(SelectableDesignerItemViewModelBase item)
        {
            var layerItems = Items.Where(x => x.Item.Value == item);
            layerItems.ToList().ForEach(x => Items.Remove(x));
        }

        public void AddItem(SelectableDesignerItemViewModelBase item)
        {
            var layerItem = new LayerItem(item, this);
            layerItem.IsVisible.Value = true;
            layerItem.Name.Value = $"アイテム{LayerItem.LayerItemCount++}";
            Items.Add(layerItem);
        }
    }
}
