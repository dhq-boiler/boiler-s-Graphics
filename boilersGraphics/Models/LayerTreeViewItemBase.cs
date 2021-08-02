using boilersGraphics.Exceptions;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Media;

namespace boilersGraphics.Models
{
    public class LayerTreeViewItemBase : BindableBase
    {
        protected CompositeDisposable _disposable = new CompositeDisposable();

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

        public void AddChildren(LayerTreeViewItemBase infoBase)
        {
            Children.Add(infoBase);
        }

        public void RemoveChildren(LayerTreeViewItemBase infoBase)
        {
            Children.Remove(infoBase);
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
    }
}
