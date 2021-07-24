using boilersGraphics.ViewModels;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace boilersGraphics.Models
{
    public class LayerItem : BindableBase, IDisposable
    {
        private CompositeDisposable _disposable = new CompositeDisposable();
        private bool disposedValue;
        public static int LayerItemCount { get; set; } = 1;

        public ReactivePropertySlim<bool> IsVisible { get; } = new ReactivePropertySlim<bool>();
        public ReactivePropertySlim<Bitmap> Appearance { get; } = new ReactivePropertySlim<Bitmap>();
        public ReactivePropertySlim<string> Name { get; } = new ReactivePropertySlim<string>();
        public ReactivePropertySlim<Layer> Owner { get; } = new ReactivePropertySlim<Layer>();
        public ReactiveCommand SwitchVisibilityCommand { get; } = new ReactiveCommand();
        public ReactivePropertySlim<SelectableDesignerItemViewModelBase> Item { get; } = new ReactivePropertySlim<SelectableDesignerItemViewModelBase>();

        public IObservable<bool> Observable
        {
            get { return Item.ObserveProperty(x => x.Value.IsSelected); }
        }

        public IObservable<SelectableDesignerItemViewModelBase> AllItemsObservable
        {
            get { return System.Reactive.Linq.Observable.Return(Item.Value); }
        }

        public LayerItem()
        {
            Init();
        }

        private void Init()
        {
            SwitchVisibilityCommand.Subscribe(_ =>
            {
                IsVisible.Value = !IsVisible.Value;
            })
            .AddTo(_disposable);
            IsVisible.Subscribe(isVisible =>
            {
                if (Item.Value != null)
                {
                    Item.Value.IsVisible.Value = isVisible;
                    if (isVisible && !Owner.Value.IsVisible.Value)
                    {
                        Owner.Value.IsVisible.Value = true;
                    }
                }
            })
            .AddTo(_disposable);
            IsVisible.Value = true;
        }

        public LayerItem(SelectableDesignerItemViewModelBase item, Layer owner)
        {
            Init();
            Item.Value = item;
            Owner.Value = owner;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    IsVisible.Dispose();
                    Appearance.Dispose();
                    Name.Dispose();
                    SwitchVisibilityCommand.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
