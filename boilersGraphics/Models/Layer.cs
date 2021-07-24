using boilersGraphics.ViewModels;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;

namespace boilersGraphics.Models
{
    public class Layer : BindableBase
    {
        private CompositeDisposable _disposable = new CompositeDisposable();
        public static ObservableCollection<Layer> SelectedLayers { get; } = new ObservableCollection<Layer>();

        public ReactivePropertySlim<bool> IsVisible { get; } = new ReactivePropertySlim<bool>();

        public ReactivePropertySlim<string> Name { get; } = new ReactivePropertySlim<string>();

        public ReactiveCommand SwitchVisibilityCommand { get; } = new ReactiveCommand();

        public ObservableCollection<LayerItem> Items { get; } = new ObservableCollection<LayerItem>();

        public IObservable<PropertyPack<LayerItem, bool>> Observable
        {
            get { return Items.ObserveElementObservableProperty(x => x.Observable); }
        }

        public Layer()
        {
            SwitchVisibilityCommand.Subscribe(_ =>
            {
                IsVisible.Value = !IsVisible.Value;
            })
            .AddTo(_disposable);
            IsVisible.Value = true;
        }

        public void RemoveItem(SelectableDesignerItemViewModelBase item)
        {
            var layerItems = Items.Where(x => x.Item.Value == item);
            layerItems.ToList().ForEach(x => Items.Remove(x));
        }
    }
}
