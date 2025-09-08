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

    public R3.ReactiveProperty<LayerTreeViewItemBase> Parent { get; } = new();

    public R3.ReactiveProperty<string> Name { get; } = new();

    public R3.ReactiveProperty<Brush> Background { get; } = new();

    public R3.ReactiveProperty<bool> IsExpanded { get; } = new(true);

    public R3.ReactiveProperty<bool> IsSelected { get; protected set; } = new();

    public R3.ReactiveProperty<bool> IsVisible { get; } = new();

    public R3.ReactiveProperty<Color> Color { get; } = new();

    public R3.ReactiveProperty<ImageSource> Appearance { get; } = new();

    public R3.ReactiveProperty<Visibility> BeforeSeparatorVisibility { get; } = new(Visibility.Hidden);

    public R3.ReactiveProperty<Visibility> AfterSeparatorVisibility { get; } = new(Visibility.Hidden);

    public NotifyCollectionChangedSynchronizedViewList<LayerTreeViewItemBase> Children { get; set; } = new ObservableList<LayerTreeViewItemBase>().ToWritableNotifyCollectionChanged();

    public ObservableList<Control> LayerTreeViewItemContextMenu { get; } = new();

    public ReactiveCommand ChangeNameCommand { get; } = new();

    public abstract void UpdateAppearance(IEnumerable<SelectableDesignerItemViewModelBase> items, bool backgroundIncluded = false);

    internal void UpdateAppearanceBothParentAndChild()
    {
        LogManager.GetCurrentClassLogger().Trace("detected Layer changes. run Layer.UpdateAppearance().");
        UpdateAppearance(Children
            .SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(xx => xx.Children)
            .AsValueEnumerable()
            .Select(x => (x as LayerItem)?.Item?.Value)
                .Where(x => x is not null).ToArray(), true);
        Children.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
            .AsValueEnumerable()
            .ToList()
            .ForEach(x =>
            {
                if (x is Layer l)
                {
                    l.UpdateAppearance(l.Children.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(xx => xx.Children)
                                               .AsValueEnumerable()
                                               .Select(x => (x as LayerItem).Item.Value).ToArray(), true);
                    l.Children.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
                              .AsValueEnumerable()
                              .ToList()
                              .ForEach(x =>
                              {
                                  if (x is LayerItem li)
                                  {
                                      li.UpdateAppearance(IfGroupBringChildren(li.Item.Value));
                                  }
                              });
                }
                else if (x is LayerItem li)
                {
                    li.UpdateAppearance(IfGroupBringChildren(li.Item.Value));
                }
            });
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
