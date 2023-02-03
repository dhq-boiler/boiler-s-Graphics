using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.ViewModels;
using NLog;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace boilersGraphics.Models;

public class Layer : LayerTreeViewItemBase, IObservable<LayerObservable>, IComparable<LayerTreeViewItemBase>,
    IComparable
{
    private readonly List<IObserver<LayerObservable>> _observers = new();

    public Layer(bool isPreview = false)
    {
        SwitchVisibilityCommand.Subscribe(_ => { IsVisible.Value = !IsVisible.Value; })
            .AddTo(_disposable);
        IsVisible.Subscribe(isVisible =>
            {
                ChildrenSwitchIsHitTestVisible(isVisible);
                ChildrenSwitchVisibility(isVisible);
            })
            .AddTo(_disposable);

        if (!isPreview)
        {
            var temp = Children.ObserveElementProperty(x => (x as LayerItem).Item.Value)
                .ToUnit()
                .Merge(Children.ObserveElementObservableProperty(x => x.IsSelected).ToUnit())
                .ToUnit()
                .Merge(Children.ObserveElementObservableProperty(x => x.IsVisible).ToUnit())
                .ToUnit()
                .Merge(Children.ObserveElementObservableProperty(x => (x as LayerItem).Item.Value.EdgeBrush).ToUnit())
                .ToUnit()
                .Merge(Children.ObserveElementObservableProperty(x => (x as LayerItem).Item.Value.EdgeThickness)
                    .ToUnit())
                .ToUnit()
                .Merge(Children.ObserveElementObservableProperty(x => (x as LayerItem).Item.Value.FillBrush).ToUnit());

            if (!App.IsTest)
                temp = temp.Where(_ =>
                    !MainWindowViewModel.Instance.ToolBarViewModel.Behaviors.Contains(MainWindowViewModel.Instance
                        .ToolBarViewModel.BrushBehavior));

            temp.Delay(TimeSpan.FromMilliseconds(100))
                .ObserveOn(new DispatcherScheduler(Dispatcher.CurrentDispatcher, DispatcherPriority.ApplicationIdle))
                .Subscribe(x => { UpdateAppearanceBothParentAndChild(); })
                .AddTo(_disposable);
        }

        IsVisible.Value = true;
    }

    public ReactiveCommand SwitchVisibilityCommand { get; } = new();

    public int CompareTo(object obj)
    {
        if (obj == null) return 1;

        if (!(obj is LayerTreeViewItemBase other))
        {
            return 1;
        }

        var otherInt = other.Children.OfType<LayerItem>().Max(x => x.Item.Value.ZIndex.Value);
        return Children.OfType<LayerItem>().Max(x => x.Item.Value.ZIndex.Value).CompareTo(otherInt);
    }

    public int CompareTo(LayerTreeViewItemBase other)
    {
        if (other == null)
            return 1;
        return Children.OfType<LayerItem>().Max(x => x.Item.Value.ZIndex.Value)
            .CompareTo(other.OfType<LayerItem>().Max(x => x.Item.Value.ZIndex.Value));
    }

    public IDisposable Subscribe(IObserver<LayerObservable> observer)
    {
        _observers.Add(observer);
        observer.OnNext(new LayerObservable());
        return new LayerDisposable(this, observer);
    }
    public override void UpdateAppearance(IEnumerable<SelectableDesignerItemViewModelBase> items)
    {
        if (items.Count() == 0)
            return;
        double minX, maxX, minY, maxY;
        var width = Measure.GetWidth(items, out minX, out maxX);
        var height = Measure.GetHeight(items, out minY, out maxY);

        if (width <= 0 || height <= 0)
            return;

        Rect? sliceRect = DiagramViewModel.Instance.BackgroundItem.Value.Rect.Value;
        items.Cast<IRect>().ToList().ForEach(x => sliceRect = (!sliceRect.HasValue ? x.Rect.Value : Rect.Union(sliceRect.Value, x.Rect.Value)));
        var renderer = new Renderer(new WpfVisualTreeHelper());
        Appearance.Value = renderer.Render(sliceRect, DesignerCanvas.GetInstance(),
            DiagramViewModel.Instance,
            DiagramViewModel.Instance.BackgroundItem.Value, items.Max(x => x.ZIndex.Value));
    }

    private static void UpdateAppearanceByItem(DesignerCanvas designerCanvas, double minX, double minY,
        RenderTargetBitmap rtb, DrawingVisual visual, SelectableDesignerItemViewModelBase item)
    {
        var views = designerCanvas.EnumVisualChildren<FrameworkElement>(item);
        foreach (var view in views)
            if (view != null)
            {
                if (view.ActualWidth >= 1 && view.ActualHeight >= 1)
                {
                    using (var context = visual.RenderOpen())
                    {
                        double diffX = 0;
                        double diffY = 0;
                        var designerItem = item as DesignerItemViewModelBase;
                        var connectorItem = item as ConnectorBaseViewModel;
                        if (designerItem != null)
                        {
                            diffX = designerItem.Left.Value - minX;
                            diffY = designerItem.Top.Value - minY;
                        }

                        if (connectorItem != null)
                        {
                            diffX = Math.Min(connectorItem.Points[0].X, connectorItem.Points[1].X);
                            diffY = Math.Min(connectorItem.Points[0].Y, connectorItem.Points[1].Y);
                        }

                        view.InvalidateMeasure();
                        view.InvalidateArrange();
                        view.UpdateLayout();
                        var brush = new VisualBrush(view);
                        brush.Stretch = Stretch.None;
                        context.DrawRectangle(brush, null,
                            new Rect(new Point(diffX, diffY), new Size(view.ActualWidth, view.ActualHeight)));
                    }

                    rtb.Render(visual);
                }
            }
            else
            {
                throw new Exception("view not found");
            }
    }

    private static DrawingVisual InitializeBitmap(double minX, double minY, double width, double height)
    {
        var visual = new DrawingVisual();
        using (var context = visual.RenderOpen())
        {
            //白背景でビットマップを初期化
            context.DrawRectangle(DiagramViewModel.Instance.BackgroundItem.Value.FillBrush.Value, null, new Rect(new Point(), new Size(width, height)));
        }

        return visual;
    }

    public override string ToString()
    {
        return $"Name={Name.Value}, IsSelected={IsSelected.Value}, Parent={{{Parent.Value}}}";
    }

    public class LayerDisposable : IDisposable
    {
        private readonly Layer layer;
        private readonly IObserver<LayerObservable> observer;

        public LayerDisposable(Layer layer, IObserver<LayerObservable> observer)
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

public class LayerObservable : BindableBase
{
}