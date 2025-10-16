using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.ViewModels;
using OpenCvSharp;
using Prism.Mvvm;
using R3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ZLinq;
using Point = System.Windows.Point;
using Rect = System.Windows.Rect;
using Size = System.Windows.Size;

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
            var temp = Children.ObserveElementObservableProperty(x => (x as LayerItem).Item.Value)
                .ToUnit()
                .Merge(Children.ObserveElementObservableProperty(x => x.IsSelected).ToUnit())
                .ToUnit()
                .Merge(Children.ObserveElementObservableProperty(x => x.IsVisible).ToUnit())
                .ToUnit()
                .Merge(Children.ObserveElementObservableProperty(x => (x as LayerItem).Item.Value.ChangeFormTriggerObject).ToUnit())
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

            temp.ObserveOnCurrentSynchronizationContext()
                .Subscribe(x =>
                {
                    DiagramViewModel.Instance.Renderer.InvalidateViewCache();
                    InvalidateCache();
                    UpdateAppearanceBothParentAndChildBatched();
                })
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

        var otherInt = other.Children.AsValueEnumerable().OfType<LayerItem>().Max(x => x.Item.Value.ZIndex.Value);
        return Children.AsValueEnumerable().OfType<LayerItem>().Max(x => x.Item.Value.ZIndex.Value).CompareTo(otherInt);
    }

    public int CompareTo(LayerTreeViewItemBase other)
    {
        if (other == null)
            return 1;
        return Children.AsValueEnumerable().OfType<LayerItem>().Max(x => x.Item.Value.ZIndex.Value)
            .CompareTo(other.Children.OfType<LayerItem>().Max(x => x.Item.Value.ZIndex.Value));
    }

    public IDisposable Subscribe(IObserver<LayerObservable> observer)
    {
        _observers.Add(observer);
        observer.OnNext(new LayerObservable());
        return new LayerDisposable(this, observer);
    }

    // レンダリング呼び出し制限用
    private static readonly Dictionary<object, DateTime> _lastRenderTimes = new();
    private static readonly TimeSpan _minRenderInterval = TimeSpan.FromMilliseconds(16); // 60FPS相当

    public static bool ShouldRender(object key)
    {
        var now = DateTime.UtcNow;
        if (_lastRenderTimes.TryGetValue(key, out var lastTime))
        {
            if (now - lastTime < _minRenderInterval)
            {
                return false; // まだ間隔が短いのでスキップ
            }
        }

        _lastRenderTimes[key] = now;
        return true;
    }

    public override void UpdateAppearance(IEnumerable<SelectableDesignerItemViewModelBase> items, bool backgroundIncluded = false)
    {
        if (!backgroundIncluded && items.AsValueEnumerable().Count() == 0)
            return;

        if (!ShouldRender(this))
            return; // 頻繁な更新をスキップ

        double minX, maxX, minY, maxY;
        var _items = items;
        if (backgroundIncluded)
        {
            _items = _items.AsValueEnumerable().Union(new SelectableDesignerItemViewModelBase[] { DiagramViewModel.Instance.BackgroundItem.Value }).ToArray();
        }
        var width = Measure.GetWidth(_items, out minX, out maxX);
        var height = Measure.GetHeight(_items, out minY, out maxY);

        if (width <= 0 || height <= 0)
            return;

        Rect? sliceRect = null;
        _items.AsValueEnumerable().Cast<IRect>().ToList().ForEach(x => sliceRect = (!sliceRect.HasValue ? x.Rect.Value : Rect.Union(sliceRect.Value, x.Rect.Value)));
        var renderer = DiagramViewModel.Instance.Renderer;
        var cache = renderer.GetCache();

        // Dirtyなアイテムのみをフィルタリング
        var dirtyItems = items.AsValueEnumerable()
            .Where(item => cache.IsDirty(item))
            .ToList();

        //背景オブジェクトを除く、アイテムがない場合
        if (_items.AsValueEnumerable().Except(new SelectableDesignerItemViewModelBase[]{ DiagramViewModel.Instance.BackgroundItem.Value }).Count() == 0)
        {
            //アピアランスには背景オブジェクトのみをレンダリングする
            Appearance.Value = renderer.Render(sliceRect, DesignerCanvas.GetInstance(),
                DiagramViewModel.Instance,
                DiagramViewModel.Instance.BackgroundItem.Value, null, -1,
                -1);
        }
        else //背景オブジェクトを除く、アイテムがある場合
        {
            //アピアランスには背景オブジェクトを除いて、ZIndexが範囲内のオブジェクトのみをレンダリングする
            Appearance.Value = renderer.Render(sliceRect, DesignerCanvas.GetInstance(),
                DiagramViewModel.Instance,
                DiagramViewModel.Instance.BackgroundItem.Value, null, _items.AsValueEnumerable().Except(new SelectableDesignerItemViewModelBase[] { DiagramViewModel.Instance.BackgroundItem.Value }).Min(x => x.ZIndex.Value),
                _items.AsValueEnumerable().Max(x => x.ZIndex.Value));
        }

        // レンダリング完了後、Dirtyフラグをクリア
        foreach (var item in dirtyItems)
        {
            cache.ClearDirtyFlag(item);
        }
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