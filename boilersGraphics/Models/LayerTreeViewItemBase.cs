using boilersGraphics.Exceptions;
using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.UserControls;
using boilersGraphics.ViewModels;
using boilersGraphics.Views;
using NLog;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using TsOperationHistory;
using TsOperationHistory.Extensions;
using Layers = boilersGraphics.Views.Layers;

namespace boilersGraphics.Models;

public abstract class LayerTreeViewItemBase : BindableBase, IDisposable, IObservable<LayerTreeViewItemBaseObservable>
{
    protected CompositeDisposable _disposable = new();


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
            var labelTextBox = layers.EnumVisualChildren<LabelTextBox>(this).FirstOrDefault(x => x.DataContext == this);
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

    public ReactivePropertySlim<LayerTreeViewItemBase> Parent { get; } = new();

    public ReactivePropertySlim<string> Name { get; } = new();

    public ReactivePropertySlim<Brush> Background { get; } = new();

    public ReactivePropertySlim<bool> IsExpanded { get; } = new();

    public ReactiveProperty<bool> IsSelected { get; set; } = new();

    public ReactivePropertySlim<bool> IsVisible { get; } = new();

    public ReactivePropertySlim<Color> Color { get; } = new();

    public ReactivePropertySlim<ImageSource> Appearance { get; } = new();

    public ReactivePropertySlim<Visibility> BeforeSeparatorVisibility { get; } = new(Visibility.Hidden);

    public ReactivePropertySlim<Visibility> AfterSeparatorVisibility { get; } = new(Visibility.Hidden);

    public ReactiveCollection<LayerTreeViewItemBase> Children { get; set; } = new();

    public ReactiveCollection<Control> LayerTreeViewItemContextMenu { get; } = new();

    public ReactiveCommand ChangeNameCommand { get; } = new();

    public abstract void UpdateAppearance(IEnumerable<SelectableDesignerItemViewModelBase> items, bool backgroundIncluded = false);

    internal void UpdateAppearanceBothParentAndChild()
    {
        LogManager.GetCurrentClassLogger().Trace("detected Layer changes. run Layer.UpdateAppearance().");
        UpdateAppearance(Children
            .SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(xx => xx.Children)
            .Select(x => (x as LayerItem).Item.Value), true);
        Children.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
            .ToList()
            .ForEach(x =>
            {
                if (x is Layer l)
                {
                    l.UpdateAppearance(l.Children.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(xx => xx.Children)
                                               .Select(x => (x as LayerItem).Item.Value), true);
                    l.Children.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
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
                .Select(x => (x as LayerItem).Item.Value)
                .Where(x => x.ParentID == groupItemVM.ID);
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

    public IObservable<Unit> LayerChangedAsObservable()
    {
        return Children.CollectionChangedAsObservable()
            .Where(x => x.Action == NotifyCollectionChangedAction.Remove ||
                        x.Action == NotifyCollectionChangedAction.Reset)
            .ToUnit()
            .Merge(Children.Select(x => x.LayerChangedAsObservable()).Merge());
    }

    public IObservable<Unit> LayerItemsChangedAsObservable()
    {
        return Children.ObserveElementObservableProperty(x => (x as LayerItem).Item)
            .ToUnit()
            .Merge(Children.CollectionChangedAsObservable().Where(x =>
                    x.Action == NotifyCollectionChangedAction.Remove || x.Action == NotifyCollectionChangedAction.Reset)
                .ToUnit());
    }

    public IObservable<Unit> SelectedLayerItemsChangedAsObservable()
    {
        var ox1 = Children
            .ObserveElementObservableProperty(x => (x as LayerItem).Item.Value.IsSelected)
            .ToUnit()
            .Merge(Children.CollectionChangedAsObservable().Where(x =>
                    x.Action == NotifyCollectionChangedAction.Remove || x.Action == NotifyCollectionChangedAction.Reset)
                .ToUnit());

        var ox2 = Children.CollectionChangedAsObservable()
            .Select(_ => Children.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
                .OfType<LayerItem>()
                .Select(x => x.Item.Value)
                .OfType<ConnectorBaseViewModel>()
                .SelectMany(x => new[] { x.SnapPoint0VM.Value, x.SnapPoint1VM.Value })
                .Select(x => x.IsSelected.ToUnit())
                .Merge())
            .Switch();
        var ox3 = Children.CollectionChangedAsObservable()
            .Select(_ => Children.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
                .OfType<LayerItem>()
                .Select(x => x.Item.Value)
                .Select(x => x.IsSelected.ToUnit())
                .Merge())
            .Switch();
        return ox1.Merge(ox2).Merge(ox3);
    }

    public void AddItem(MainWindowViewModel mainWindowViewModel, DiagramViewModel diagramViewModel,
        SelectableDesignerItemViewModelBase item, string layerItemName = null)
    {
        if (layerItemName == null) layerItemName = Helpers.Name.GetNewLayerItemName(diagramViewModel);
        var layerItem = new LayerItem(item, this, layerItemName);
        layerItem.IsVisible.Value = true;
        layerItem.Parent.Value = this;
        var rand = new Random();
        layerItem.Color.Value = Randomizer.RandomColor(rand);
        mainWindowViewModel.Recorder.Current.ExecuteAdd(Children, layerItem);
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
        var layerItems = Children.Where(x => (x as LayerItem).Item.Value == item);
        layerItems.ToList().ForEach(x =>
        {
            mainWindowViewModel.Recorder.Current.ExecuteRemove(Children, x);
            LogManager.GetCurrentClassLogger().Trace($"{x} removed from {Children}");
        });
    }

    public void ChildrenSwitchIsHitTestVisible(bool isVisible)
    {
        Children.ToList().ForEach(x =>
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
        Children.ToList().ForEach(x =>
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
                Children.Dispose();
                LayerTreeViewItemContextMenu.Dispose();
                ChangeNameCommand.Dispose();
            }

            disposedValue = true;
        }
    }

    public int GetNewZIndex(IEnumerable<LayerTreeViewItemBase> layers)
    {
        var layerItems = layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
            .Where(x => x is LayerItem);
        var zindexes = Children.Union(layerItems).Cast<LayerItem>().Select(x => x.Item.Value.ZIndex.Value);
        if (zindexes.Count() == 0)
            return 0;
        return zindexes.Max() + 1;
    }

    public void PushZIndex(OperationRecorder recorder, int newZIndex)
    {
        var targetChildren = Children.Cast<LayerItem>().Where(x => x.Item.Value.ZIndex.Value >= newZIndex);
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