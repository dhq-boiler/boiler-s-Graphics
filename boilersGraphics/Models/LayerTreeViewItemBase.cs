using boilersGraphics.Exceptions;
using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.UserControls;
using boilersGraphics.ViewModels;
using boilersGraphics.Views;
using NLog;
using ObservableCollections;
using Prism.Mvvm;
using R3;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using TsOperationHistory;
using TsOperationHistory.Extensions;
using ZLinq;
using Layers = boilersGraphics.Views.Layers;
using ReactiveCommand = R3.ReactiveCommand;

namespace boilersGraphics.Models;

public abstract class LayerTreeViewItemBase : BindableBase, IDisposable, IObservable<LayerTreeViewItemBaseObservable>
{
    protected R3.CompositeDisposable _disposable = new();

    private readonly List<IObserver<LayerTreeViewItemBaseObservable>> _observers = new();
    private bool disposedValue;

    // キャッシュ用フィールド
    private List<LayerTreeViewItemBase> _cachedAllChildren;
    private Dictionary<LayerTreeViewItemBase, List<LayerItem>> _cachedLayerItems = new();
    private bool _cacheInvalidated = true;

    public LayerTreeViewItemBase()
    {
        Parent.Subscribe(x =>
            {
                if (this is Layer && x is Layer)
                    throw new UnexpectedException("Layers cannot have layers in their children");
                if (x != null)
                    LogManager.GetCurrentClassLogger()
                        .Trace($"Set Parent Parent={{{x.Name.Value}}} Child={{{Name.Value}}}");
            })
            .AddTo(_disposable);

        // Childrenの変更を監視してキャッシュを無効化
        Children.CollectionChangedAsObservable()
            .Subscribe(_ => InvalidateCache())
            .AddTo(_disposable);

        ChangeNameCommand.Subscribe(_ =>
        {
            var diagramControl = Application.Current.MainWindow.FindName("DiagramControl") as DiagramControl;
            var layers = diagramControl.FindName("layers") as Layers; 
            var labelTextBox = layers.EnumVisualChildren<LabelTextBox>(this).AsValueEnumerable().FirstOrDefault(x => x.DataContext == this);
            labelTextBox?.FocusTextBox();
        })
        .AddTo(_disposable);
        if (!App.IsTest)
        {
            var menuItem = new MenuItem
            {
                Command = ChangeNameCommand
            };
            menuItem.SetBinding(MenuItem.HeaderProperty, new Binding()
            {
                Source = ResourceService.Current,
                Path = new PropertyPath("Resources.Command_Rename"),
            });
            LayerTreeViewItemContextMenu.Add(menuItem);

            if (this is LayerItem)
            {
                menuItem = new MenuItem
                {
                    Command = DiagramViewModel.Instance.PropertyCommand,
                };
                menuItem.SetBinding(MenuItem.HeaderProperty, new Binding()
                {
                    Source = ResourceService.Current,
                    Path = new PropertyPath("Resources.MenuItem_Property"),
                });
                LayerTreeViewItemContextMenu.Add(menuItem);
            }
        }
    }

    public R3.BindableReactiveProperty<LayerTreeViewItemBase> Parent { get; } = new();

    public R3.BindableReactiveProperty<string> Name { get; } = new();

    public R3.BindableReactiveProperty<Brush> Background { get; } = new();

    public R3.BindableReactiveProperty<bool> IsExpanded { get; } = new(true);

    public R3.BindableReactiveProperty<bool> IsSelected { get; protected set; } = new();

    public R3.BindableReactiveProperty<bool> IsVisible { get; } = new();

    public R3.BindableReactiveProperty<Color> Color { get; } = new();

    public R3.BindableReactiveProperty<ImageSource> Appearance { get; } = new();

    public R3.BindableReactiveProperty<Visibility> BeforeSeparatorVisibility { get; } = new(Visibility.Hidden);

    public R3.BindableReactiveProperty<Visibility> AfterSeparatorVisibility { get; } = new(Visibility.Hidden);

    public NotifyCollectionChangedSynchronizedViewList<LayerTreeViewItemBase> Children { get; set; } = new ObservableList<LayerTreeViewItemBase>().ToWritableNotifyCollectionChanged();

    public NotifyCollectionChangedSynchronizedViewList<Control> LayerTreeViewItemContextMenu { get; } = new ObservableList<Control>().ToWritableNotifyCollectionChanged();

    public ReactiveCommand ChangeNameCommand { get; } = new();

    public abstract void UpdateAppearance(IEnumerable<SelectableDesignerItemViewModelBase> items, bool backgroundIncluded = false);

    internal void UpdateAppearanceBothParentAndChild()
    {
        LogManager.GetCurrentClassLogger().Trace("detected Layer changes. run Layer.UpdateAppearance().");
        
        // キャッシュされた全子要素を取得
        var allChildren = GetCachedAllChildren();

        // 自分自身の更新: キャッシュされた結果を使用
        var allItems = allChildren
            .Select(x => (x as LayerItem)?.Item?.Value)
            .Where(x => x is not null)
            .ToArray();
        UpdateAppearance(allItems, true);

        // Layerとその子要素を分離して処理
        var layers = allChildren.OfType<Layer>().ToList();
        var directLayerItems = allChildren.OfType<LayerItem>().ToList();

        // 各Layerの更新処理（並列処理可能な場合は並列化）
        foreach (var layer in layers)
        {
            // キャッシュされたLayerItemsを取得
            var layerItems = GetCachedLayerItems(layer)
                .Select(x => x.Item.Value)
                .ToArray();
            
            layer.UpdateAppearance(layerItems, true);

            // そのLayerの直接の子LayerItemsを更新
            var directChildren = GetCachedLayerItems(layer);
            foreach (var layerItem in directChildren)
            {
                layerItem.UpdateAppearance(IfGroupBringChildren(layerItem.Item.Value));
            }
        }

        // 直接のLayerItemsを更新
        foreach (var layerItem in directLayerItems)
        {
            layerItem.UpdateAppearance(IfGroupBringChildren(layerItem.Item.Value));
        }
    }

    private IEnumerable<SelectableDesignerItemViewModelBase> IfGroupBringChildren(
        SelectableDesignerItemViewModelBase value)
    {
        if (value is GroupItemViewModel groupItemVM)
        {
            var children = Children.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
                .AsValueEnumerable()
                .Select(x => (x as LayerItem).Item.Value)
                .Where(x => x.ParentID == groupItemVM.ID)
                .ToArray();
            return children;
        }

        return new List<SelectableDesignerItemViewModelBase> { value };
    }

    public void Dispose()
    {
        // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public IDisposable Subscribe(IObserver<LayerTreeViewItemBaseObservable> observer)
    {
        _observers.Add(observer);
        observer.OnNext(new LayerTreeViewItemBaseObservable());
        return new LayerTreeViewItemBaseDisposable(this, observer);
    }

    public R3.Observable<Unit> LayerChangedAsObservable()
    {
        return Children.CollectionChangedAsObservable()
            .Where(x => x.Action == NotifyCollectionChangedAction.Remove ||
                        x.Action == NotifyCollectionChangedAction.Reset)
            .ToUnit()
            .Merge(R3.Observable.Merge(Children.Select(x => x.LayerChangedAsObservable())));
    }

    public R3.Observable<Unit> LayerItemsChangedAsObservable()
    {
        Debug.WriteLine("LayerItemsChangedAsObservable called"); // ここにブレークポイント

        return Children.CollectionChangedAsObservable()
            .Do(x => Debug.WriteLine($"LayerItemsChanged - Children.CollectionChanged: {x.Action} in {Name.Value}"))
            .ToUnit()
            .Merge(
                Children.ObserveElementObservableProperty(x => (x as LayerItem)?.Item)
                    .Where(x => x != null)
                    .Do(x => Debug.WriteLine($"LayerItemsChanged - Item property changed in {Name.Value}"))
                    .ToUnit()
            );
    }

    public R3.Observable<Unit> SelectedLayerItemsChangedAsObservable()
    {
        var ox1 = Children
            .ObserveElementObservableProperty(x => (x as LayerItem)?.Item?.Value?.IsSelected)
            .Where(x => x != null)
            .ToUnit()
            .Merge(Children.CollectionChangedAsObservable().Where(x =>
                    x.Action == NotifyCollectionChangedAction.Remove || x.Action == NotifyCollectionChangedAction.Reset)
                .ToUnit());

        var ox2 = Children.CollectionChangedAsObservable()
            .Select(_ => 
            {
                var snapPoints = Children.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
                    .OfType<LayerItem>()
                    .Select(x => x.Item.Value)
                    .OfType<ConnectorBaseViewModel>()
                    .SelectMany(x => new[] { x.SnapPoint0VM.Value, x.SnapPoint1VM.Value })
                    .ToArray();
                    
                if (snapPoints.Length > 0)
                {
                    var observables = snapPoints.Select(snapPoint => snapPoint.IsSelected.ToUnit()).ToArray();
                    return observables.Length > 0 ? R3.Observable.Merge(observables) : R3.Observable.Return(Unit.Default);
                }
                return R3.Observable.Return(Unit.Default);
            })
            .Switch();
            
        var ox3 = Children.CollectionChangedAsObservable()
            .Select(_ => 
            {
                var layerItems = Children.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
                    .OfType<LayerItem>()
                    .ToArray();
                    
                if (layerItems.Length > 0)
                {
                    var observables = layerItems.Select(item => item.Item.Value.IsSelected.ToUnit()).ToArray();
                    return observables.Length > 0 ? R3.Observable.Merge(observables) : R3.Observable.Return(Unit.Default);
                }
                return R3.Observable.Return(Unit.Default);
            })
            .Switch();
            
        return R3.Observable.Merge<Unit>(ox1, ox2, ox3);
    }

    public void AddItem(MainWindowViewModel mainWindowViewModel, DiagramViewModel diagramViewModel,
        SelectableDesignerItemViewModelBase item, string layerItemName = null)
    {
        Debug.WriteLine($"=== LayerTreeViewItemBase.AddItem START ===");
        Debug.WriteLine($"Layer: {Name.Value}");
        Debug.WriteLine($"Item: {item}");
        Debug.WriteLine($"LayerItemName: {layerItemName ?? "null"}");

        if (layerItemName == null) layerItemName = Helpers.Name.GetNewLayerItemName(diagramViewModel);
        Debug.WriteLine($"Final layerItemName: {layerItemName}");

        var layerItem = new LayerItem(item, this, layerItemName);
        Debug.WriteLine($"Created LayerItem: {layerItem}");

        layerItem.IsVisible.Value = true;
        layerItem.Parent.Value = this;
        var rand = new Random();
        layerItem.Color.Value = Randomizer.RandomColor(rand);

        Debug.WriteLine($"Children count before ExecuteAdd: {Children.Count}");
        Debug.WriteLine($"About to call ExecuteAdd...");

        mainWindowViewModel.Recorder.Current.ExecuteAdd(Children, layerItem);

        Debug.WriteLine($"Children count after ExecuteAdd: {Children.Count}");
        Debug.WriteLine("=== LayerTreeViewItemBase.AddItem END ===");
    }

    public void AddItem(MainWindowViewModel mainWindowVM, DiagramViewModel diagramViewModel, LayerItem item)
    {
        item.IsVisible.Value = true;
        item.Parent.Value = this;
        var rand = new Random();
        item.Color.Value = Randomizer.RandomColor(rand);
        mainWindowVM.Recorder.Current.ExecuteAdd(Children, item);
    }

    public void RemoveItem(MainWindowViewModel mainWindowViewModel, SelectableDesignerItemViewModelBase item)
    {
        var layerItems = Children.AsValueEnumerable().Where(x => (x as LayerItem).Item.Value == item);
        layerItems.ToList().ForEach(x =>
        {
            mainWindowViewModel.Recorder.Current.ExecuteRemove(Children, x);
            LogManager.GetCurrentClassLogger().Trace($"{x} removed from {Children}");
        });
    }

    public void ChildrenSwitchIsHitTestVisible(bool isVisible)
    {
        Children.AsValueEnumerable().ToList().ForEach(x =>
        {
            if (x is LayerItem layerItem)
            {
                layerItem.Item.Value.IsHitTestVisible.Value = isVisible;
                LogManager.GetCurrentClassLogger()
                    .Trace($"{layerItem.Name.Value}.IsHitTestVisible={layerItem.Item.Value.IsHitTestVisible.Value}");
            }

            x.ChildrenSwitchIsHitTestVisible(isVisible);
        });
    }

    public void ChildrenSwitchVisibility(bool isVisible)
    {
        Children.AsValueEnumerable().ToList().ForEach(x =>
        {
            x.IsVisible.Value = isVisible;
            x.ChildrenSwitchVisibility(isVisible);
        });
    }

    public void SetParentToChildren(LayerTreeViewItemBase parent = null)
    {
        Parent.Value = parent;

        if (Children == null)
            return;
        foreach (var child in Children) child.SetParentToChildren(this);
    }

    public void InsertBeforeChildren(LayerTreeViewItemBase from, LayerTreeViewItemBase to)
    {
        var index = Children.IndexOf(to);
        if (index < 0)
            return;

        Children.Insert(index, from);
    }

    public void InsertAfterChildren(LayerTreeViewItemBase from, LayerTreeViewItemBase to)
    {
        var index = Children.IndexOf(to);
        if (index < 0)
            return;

        Children.Insert(index + 1, from);
    }

    public void AddChildren(OperationRecorder recorder, LayerTreeViewItemBase infoBase)
    {
        recorder.Current.ExecuteAdd(Children, infoBase);
    }

    public void RemoveChildren(OperationRecorder recorder, LayerTreeViewItemBase infoBase)
    {
        recorder.Current.ExecuteRemove(Children, infoBase);
    }

    public bool ContainsParent(LayerTreeViewItemBase infoBase)
    {
        if (Parent.Value == null)
            return false;
        return Parent.Value == infoBase || Parent.Value.ContainsParent(infoBase);
    }

    public override string ToString()
    {
        return $"Name={Name.Value}, IsSelected={IsSelected.Value}, Parent={{{Parent.Value}}}";
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                foreach (var child in Children) child.Dispose();

                Parent.Dispose();
                Name.Dispose();
                Background.Dispose();
                IsExpanded.Dispose();
                IsSelected.Dispose();
                IsVisible.Dispose();
                Color.Dispose();
                BeforeSeparatorVisibility.Dispose();
                AfterSeparatorVisibility.Dispose();
                Children.Dispose(); // This now uses the extension method
                LayerTreeViewItemContextMenu.Dispose(); // This now uses the extension method
                ChangeNameCommand.Dispose();
                _disposable.Dispose();

                // キャッシュをクリア
                _cachedAllChildren?.Clear();
                _cachedLayerItems?.Clear();
                _cachedAllChildren = null;
                _cachedLayerItems = null;
            }

            disposedValue = true;
        }
    }

    public int GetNewZIndex(IEnumerable<LayerTreeViewItemBase> layers)
    {
        var layerItems = layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
            .AsValueEnumerable()
            .Where(x => x is LayerItem);
        var zindexes = Children.AsValueEnumerable().Union(layerItems).Cast<LayerItem>().Select(x => x.Item.Value.ZIndex.Value);
        if (zindexes.Count() == 0)
            return 0;
        return zindexes.Max() + 1;
    }

    public void PushZIndex(OperationRecorder recorder, int newZIndex)
    {
        var targetChildren = Children.AsValueEnumerable().Cast<LayerItem>().Where(x => x.Item.Value.ZIndex.Value >= newZIndex);
        foreach (var target in targetChildren)
            recorder.Current.ExecuteSetProperty(target.Item.Value, "ZIndex.Value", target.Item.Value.ZIndex.Value + 1);
    }

    public int SetZIndex(OperationRecorder recorder, int foregroundZIndex)
    {
        if (this is LayerItem layerItem)
            recorder.Current.ExecuteSetProperty(layerItem.Item.Value, "ZIndex.Value", foregroundZIndex++);

        var zindex = foregroundZIndex;
        foreach (var child in Children) zindex = child.SetZIndex(recorder, zindex);
        return zindex;
    }

    // キャッシュを無効化するメソッド
    private void InvalidateCache()
    {
        _cacheInvalidated = true;
        _cachedAllChildren = null;
        _cachedLayerItems.Clear();
        
        // 親にも通知してキャッシュを無効化
        Parent.Value?.InvalidateCache();
    }

    // キャッシュされた全子要素を取得
    private List<LayerTreeViewItemBase> GetCachedAllChildren()
    {
        if (_cacheInvalidated || _cachedAllChildren == null)
        {
            _cachedAllChildren = Children
                .SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(xx => xx.Children)
                .AsValueEnumerable()
                .ToList();
            _cacheInvalidated = false;
        }
        return _cachedAllChildren;
    }

    // 特定のLayerの子LayerItemsをキャッシュ付きで取得
    private List<LayerItem> GetCachedLayerItems(LayerTreeViewItemBase layer)
    {
        if (!_cachedLayerItems.ContainsKey(layer) || _cacheInvalidated)
        {
            _cachedLayerItems[layer] = layer.Children
                .SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
                .AsValueEnumerable()
                .OfType<LayerItem>()
                .ToList();
        }
        return _cachedLayerItems[layer];
    }

    public class LayerTreeViewItemBaseDisposable : IDisposable
    {
        private readonly LayerTreeViewItemBase layer;
        private readonly IObserver<LayerTreeViewItemBaseObservable> observer;

        public LayerTreeViewItemBaseDisposable(LayerTreeViewItemBase layer,
            IObserver<LayerTreeViewItemBaseObservable> observer)
        {
            this.layer = layer;
            this.observer = observer;
        }

        public void Dispose()
        {
            layer._observers.Remove(observer);
        }
    }
}

public class LayerTreeViewItemBaseObservable : BindableBase
{
}
