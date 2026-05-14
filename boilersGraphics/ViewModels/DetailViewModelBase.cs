using boilersGraphics.Helpers.Animation;
using boilersGraphics.Properties;
using boilersGraphics.Views;
using NLog;
using ObservableCollections;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using R3;
using System;
using System.Reflection;
using boilersGraphics.Extensions;
using ZLinq;

namespace boilersGraphics.ViewModels;

public class DetailViewModelBase<T> : BindableBase, IDialogAware, INavigationAware, IDisposable
    where T : SelectableDesignerItemViewModelBase
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private readonly IRegionManager regionManager;
    private CompositeDisposable disposables = new();
    private bool disposedValue;

    public DetailViewModelBase(IRegionManager regionManager)
    {
        this.regionManager = regionManager;

        ToggleKeyframeAtNowCommand = new ReactiveCommand<PropertyOptionsValueCombination>();
        ToggleKeyframeAtNowCommand
            .Subscribe(ToggleKeyframeAtNow)
            .AddTo(disposables);
    }

    public BindableReactiveProperty<int> OKTabIndex { get; } = new();

    public BindableReactiveProperty<T> ViewModel { get; } = new();

    public NotifyCollectionChangedSynchronizedViewList<PropertyOptionsValueCombination> Properties { get; } = new ObservableList<PropertyOptionsValueCombination>().ToWritableNotifyCollectionChanged();

    /// <summary>
    /// Phase 5-d-3: プロパティ行の ◇ ボタンから飛んでくる「現時刻にキーフレームをトグル」コマンド。
    /// CommandParameter は対象の <see cref="PropertyOptionsValueCombination"/> (= 行 VM)。
    /// 対象図形は row.Object.Value、プロパティパスは row.PropertyName.Value、Timeline は item.Owner 経由で解決する。
    /// </summary>
    public ReactiveCommand<PropertyOptionsValueCombination> ToggleKeyframeAtNowCommand { get; }

    private void ToggleKeyframeAtNow(PropertyOptionsValueCombination row)
    {
        if (row is null) return;

        var item = TryGetReactiveValue(row, "Object") as SelectableDesignerItemViewModelBase;
        if (item is null) return;

        var diagram = item.Owner as DiagramViewModel;
        if (diagram is null) return;

        var propertyPath = row.PropertyName.Value;
        if (string.IsNullOrEmpty(propertyPath)) return;

        var currentValue = TryGetReactiveValue(row, "PropertyValue");

        try
        {
            KeyframeToggleHelper.ToggleKeyframeAtNow(item.ID, propertyPath, currentValue, diagram.Timeline);
        }
        catch (Exception ex)
        {
            _logger.Warn(ex, $"ToggleKeyframeAtNow failed for {item.ID} / {propertyPath}");
        }
    }

    /// <summary>
    /// row の指定名プロパティを取得し、それが ReactiveProperty 系であれば .Value を返す。
    /// PropertyOptionsValueCombination の派生は (Object, PropertyValue) を型パラメータごとに別シグネチャで
    /// 公開するため、ジェネリック解決を避けてリフレクションで吸収する。
    /// internal なのはテストから直接呼べるようにするため。
    /// </summary>
    internal static object TryGetReactiveValue(object row, string propertyName)
    {
        if (row is null) return null;
        var prop = row.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        if (prop is null) return null;
        var holder = prop.GetValue(row);
        if (holder is null) return null;
        var valueProp = holder.GetType().GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);
        return valueProp is null ? holder : valueProp.GetValue(holder);
    }

    public string Title => Resources.ResourceManager.GetString("Title_Property", Resources.Culture);

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
        var properties = Properties.AsValueEnumerable().OrderBy(x => x.PropertyName.Value).ToList();
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