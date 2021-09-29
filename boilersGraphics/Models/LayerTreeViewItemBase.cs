using boilersGraphics.Exceptions;
using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.ViewModels;
using boilersGraphics.Views;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using TsOperationHistory;
using TsOperationHistory.Extensions;

namespace boilersGraphics.Models
{
    public class LayerTreeViewItemBase : BindableBase, IDisposable, IObservable<LayerTreeViewItemBaseObservable>
    {
        protected CompositeDisposable _disposable = new CompositeDisposable();
        private bool disposedValue;

        public ReactivePropertySlim<LayerTreeViewItemBase> Parent { get; } = new ReactivePropertySlim<LayerTreeViewItemBase>();

        public ReactivePropertySlim<string> Name { get; } = new ReactivePropertySlim<string>();

        public ReactivePropertySlim<Brush> Background { get; } = new ReactivePropertySlim<Brush>();

        public ReactivePropertySlim<bool> IsExpanded { get; } = new ReactivePropertySlim<bool>();

        public ReactiveProperty<bool> IsSelected { get; set; } = new ReactiveProperty<bool>();

        public ReactivePropertySlim<bool> IsVisible { get; } = new ReactivePropertySlim<bool>();

        public ReactivePropertySlim<Color> Color { get; } = new ReactivePropertySlim<Color>();

        public ReactivePropertySlim<Visibility> BeforeSeparatorVisibility { get; } = new ReactivePropertySlim<Visibility>(Visibility.Hidden);

        public ReactivePropertySlim<Visibility> AfterSeparatorVisibility { get; } = new ReactivePropertySlim<Visibility>(Visibility.Hidden);

        public ReactiveCollection<LayerTreeViewItemBase> Children { get; set; } = new ReactiveCollection<LayerTreeViewItemBase>();

        public ReactiveCollection<Control> LayerTreeViewItemContextMenu { get; } = new ReactiveCollection<Control>();

        public ReactiveCommand ChangeNameCommand { get; } = new ReactiveCommand();

        public LayerTreeViewItemBase()
        {
            Parent.Subscribe(x =>
            {
                if (this is Layer && x is Layer)
                    throw new UnexpectedException("Layers cannot have layers in their children");
                if (x != null)
                    Trace.WriteLine($"Set Parent Parent={{{x.Name.Value}}} Child={{{Name.Value}}}");
            })
            .AddTo(_disposable);
            ChangeNameCommand.Subscribe(_ =>
            {
                var labelTextBox = App.Current.MainWindow.GetCorrespondingViews<LabelTextBox>(this).First();
                labelTextBox.FocusTextBox();
            })
            .AddTo(_disposable);
            if (!App.IsTest)
            {
                LayerTreeViewItemContextMenu.Add(new MenuItem()
                {
                    Header = "名前の変更",
                    Command = ChangeNameCommand
                });
            }
        }

        public IObservable<Unit> LayerChangedAsObservable()
        {
            return this.Children.CollectionChangedAsObservable()
                                .Where(x => x.Action == NotifyCollectionChangedAction.Remove || x.Action == NotifyCollectionChangedAction.Reset)
                                .ToUnit()
                                .Merge(this.Children.Select(x => x.LayerChangedAsObservable()).Merge());
        }

        public IObservable<Unit> LayerItemsChangedAsObservable()
        {
            return Children.ObserveElementObservableProperty(x => (x as LayerItem).Item)
                        .ToUnit()
                        .Merge(Children.CollectionChangedAsObservable().Where(x => x.Action == NotifyCollectionChangedAction.Remove || x.Action == NotifyCollectionChangedAction.Reset).ToUnit());
        }

        public IObservable<Unit> SelectedLayerItemsChangedAsObservable()
        {
            var ox1 = Children
                .ObserveElementObservableProperty(x => (x as LayerItem).Item.Value.IsSelected)
                .ToUnit()
                .Merge(Children.CollectionChangedAsObservable().Where(x => x.Action == NotifyCollectionChangedAction.Remove || x.Action == NotifyCollectionChangedAction.Reset).ToUnit());

            var ox2 = Children.CollectionChangedAsObservable()
                    .Select(_ => Children.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
                        .OfType<LayerItem>()
                        .Select(x => x.Item.Value)
                        .OfType<ConnectorBaseViewModel>()
                        .SelectMany(x => new[] { x.SnapPoint0VM.Value, x.SnapPoint1VM.Value })
                        .Select(x => x.IsSelected.ToUnit())
                        .Merge())
                    .Switch();
            return ox1.Merge(ox2);
        }

        public void AddItem(MainWindowViewModel mainWindowViewModel, DiagramViewModel diagramViewModel, SelectableDesignerItemViewModelBase item)
        {
            var layerItem = new LayerItem(item, this, boilersGraphics.Helpers.Name.GetNewLayerItemName(diagramViewModel));
            layerItem.IsVisible.Value = true;
            layerItem.Parent.Value = this;
            Random rand = new Random();
            layerItem.Color.Value = Randomizer.RandomColor(rand);
            mainWindowViewModel.Recorder.Current.ExecuteAdd(Children, layerItem);
        }

        public void RemoveItem(MainWindowViewModel mainWindowViewModel, SelectableDesignerItemViewModelBase item)
        {
            var layerItems = Children.Where(x => (x as LayerItem).Item.Value == item);
            layerItems.ToList().ForEach(x =>
            {
                mainWindowViewModel.Recorder.Current.ExecuteRemove(Children, x);
                Trace.WriteLine($"{x} removed from {Children}");
            });
        }

        public void ChildrenSwitchIsHitTestVisible(bool isVisible)
        {
            Children.ToList().ForEach(x =>
            {
                if (x is LayerItem layerItem)
                {
                    layerItem.Item.Value.IsHitTestVisible.Value = isVisible;
                    Trace.WriteLine($"{layerItem.Name.Value}.IsHitTestVisible={layerItem.Item.Value.IsHitTestVisible.Value}");
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
            foreach (var child in Children)
            {
                child.SetParentToChildren(this);
            }
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
                    foreach (var child in Children)
                    {
                        child.Dispose();
                    }

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

        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }


        private List<IObserver<LayerTreeViewItemBaseObservable>> _observers = new List<IObserver<LayerTreeViewItemBaseObservable>>();

        public IDisposable Subscribe(IObserver<LayerTreeViewItemBaseObservable> observer)
        {
            _observers.Add(observer);
            observer.OnNext(new LayerTreeViewItemBaseObservable());
            return new LayerTreeViewItemBaseDisposable(this, observer);
        }

        public class LayerTreeViewItemBaseDisposable : IDisposable
        {
            private LayerTreeViewItemBase layer;
            private IObserver<LayerTreeViewItemBaseObservable> observer;

            public LayerTreeViewItemBaseDisposable(LayerTreeViewItemBase layer, IObserver<LayerTreeViewItemBaseObservable> observer)
            {
                this.layer = layer;
                this.observer = observer;
            }

            public void Dispose()
            {
                layer._observers.Remove(observer);
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
            {
                recorder.Current.ExecuteSetProperty(target.Item.Value, "ZIndex.Value", target.Item.Value.ZIndex.Value + 1);
            }
        }

        public int SetZIndex(OperationRecorder recorder, int foregroundZIndex)
        {
            if (this is LayerItem layerItem)
            {
                recorder.Current.ExecuteSetProperty(layerItem.Item.Value, "ZIndex.Value", foregroundZIndex++);
            }

            int zindex = foregroundZIndex;
            foreach (var child in Children)
            {
                zindex = child.SetZIndex(recorder, zindex);
            }
            return zindex;
        }
    }

    public class LayerTreeViewItemBaseObservable : BindableBase
    {
    }
}
