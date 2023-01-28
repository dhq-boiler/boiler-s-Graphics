using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using boilersGraphics.Views;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using Reactive.Bindings;

namespace boilersGraphics.ViewModels;

public class DetailViewModelBase<T> : BindableBase, IDialogAware, INavigationAware, IDisposable
    where T : SelectableDesignerItemViewModelBase
{
    private readonly IRegionManager regionManager;
    private CompositeDisposable disposables = new();
    private bool disposedValue;

    public DetailViewModelBase(IRegionManager regionManager)
    {
        this.regionManager = regionManager;
    }

    public ReactivePropertySlim<int> OKTabIndex { get; } = new();

    public ReactivePropertySlim<T> ViewModel { get; } = new();

    public ReactiveCollection<PropertyOptionsValueCombination> Properties { get; } = new();

    public string Title => "プロパティ";

    public event Action<IDialogResult> RequestClose;

    public bool CanCloseDialog()
    {
        return true;
    }

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

    public void Dispose()
    {
        // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
        Dispose(true);
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

    public virtual void SetProperties()
    {
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
}