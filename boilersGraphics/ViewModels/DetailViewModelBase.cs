using boilersGraphics.Views;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;

namespace boilersGraphics.ViewModels
{
    public class DetailViewModelBase<T> : BindableBase, IDialogAware, INavigationAware, IDisposable where T : SelectableDesignerItemViewModelBase
    {
        private CompositeDisposable disposables = new CompositeDisposable();
        private bool disposedValue;
        private readonly IRegionManager regionManager;

        public string Title => "プロパティ";

        public ReactivePropertySlim<int> OKTabIndex { get; } = new ReactivePropertySlim<int>();

        public event Action<IDialogResult> RequestClose;

        public DetailViewModelBase(IRegionManager regionManager)
        {
            this.regionManager = regionManager;
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public ReactivePropertySlim<T> ViewModel { get; } = new ReactivePropertySlim<T>();

        public ReactiveCollection<PropertyOptionsValueCombination> Properties { get; } = new ReactiveCollection<PropertyOptionsValueCombination>();

        public virtual void SetProperties()
        { }

        public void OnDialogClosed()
        {
            regionManager.Regions.Remove("DetailRegion");
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            ViewModel.Value = parameters.GetValue<T>("ViewModel");
            regionManager.RequestNavigate("DetailRegion", nameof(Detail));
            SetProperties();
            var properties = Properties.OrderBy(x => x.PropertyName.Value).ToList();
            Properties.Clear();
            Properties.AddRange(properties);
            var i = 0;
            properties.ForEach(x => { x.TabIndex.Value = i++; });
            OKTabIndex.Value = i++;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    ViewModel.Dispose();
                    Properties.Dispose();
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

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return false;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
    }
}
