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

        public ReactiveProperty<bool> IsVisible { get; } = new ReactiveProperty<bool>();

        public ReactivePropertySlim<Bitmap> Appearance { get; } = new ReactivePropertySlim<Bitmap>();

        public ReactivePropertySlim<string> Name { get; } = new ReactivePropertySlim<string>();

        public ReactiveCommand SwitchVisibilityCommand { get; } = new ReactiveCommand();

        public ObservableCollection<SelectableDesignerItemViewModelBase> Items { get; } = new ObservableCollection<SelectableDesignerItemViewModelBase>();

        public IObservable<PropertyPack<SelectableDesignerItemViewModelBase, bool>> Observable
        {
            get { return Items.ObserveElementProperty(x => x.IsSelected); }
        }

        public Layer()
        {
            SwitchVisibilityCommand.Subscribe(_ =>
            {
                IsVisible.Value = !IsVisible.Value;
            })
            .AddTo(_disposable);
        }
    }
}
