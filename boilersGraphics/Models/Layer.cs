using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.ViewModels;
using NLog;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace boilersGraphics.Models
{
    public class Layer : LayerTreeViewItemBase, IObservable<LayerObservable>, IComparable<LayerTreeViewItemBase>, IComparable
    {       
        public ReactivePropertySlim<ImageSource> Appearance { get; } = new ReactivePropertySlim<ImageSource>();

        public ReactiveCommand SwitchVisibilityCommand { get; } = new ReactiveCommand();

        public Layer(bool isPreview = false)
        {
            SwitchVisibilityCommand.Subscribe(_ =>
            {
                IsVisible.Value = !IsVisible.Value;
            })
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
                     .Merge(Children.ObserveElementObservableProperty(x => (x as LayerItem).Item.Value.EdgeThickness).ToUnit())
                     .ToUnit()
                     .Merge(Children.ObserveElementObservableProperty(x => (x as LayerItem).Item.Value.FillBrush).ToUnit());

                if (!App.IsTest)
                {
                    temp = temp.Where(_ => !MainWindowViewModel.Instance.ToolBarViewModel.Behaviors.Contains(MainWindowViewModel.Instance.ToolBarViewModel.BrushBehavior));
                }

                temp.Delay(TimeSpan.FromMilliseconds(100))
                .ObserveOn(new DispatcherScheduler(Dispatcher.CurrentDispatcher, DispatcherPriority.ApplicationIdle))
                .Subscribe(x =>
                {
                    LogManager.GetCurrentClassLogger().Trace("detected Layer changes. run Layer.UpdateAppearance().");
                    UpdateAppearance(Children.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(xx => xx.Children).Select(x => (x as LayerItem).Item.Value));
                    Children.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
                            .ToList()
                            .ForEach(x => (x as LayerItem).UpdateAppearance(IfGroupBringChildren((x as LayerItem).Item.Value)));
                })
                .AddTo(_disposable);
            }

            IsVisible.Value = true;
        }

        private IEnumerable<SelectableDesignerItemViewModelBase> IfGroupBringChildren(SelectableDesignerItemViewModelBase value)
        {
            if (value is GroupItemViewModel groupItemVM)
            {
                var children = Children.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
                                       .Select(x => (x as LayerItem).Item.Value)
                                       .Where(x => x.ParentID == groupItemVM.ID);
                return children;
            }
            else
            {
                return new List<SelectableDesignerItemViewModelBase>() { value };
            }
        }

        public void UpdateAppearance(IEnumerable<SelectableDesignerItemViewModelBase> items)
        {
            if (items.Count() == 0)
                return;
            var designerCanvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();
            double minX, maxX, minY, maxY;
            var width = Measure.GetWidth(items, out minX, out maxX);
            var height = Measure.GetHeight(items, out minY, out maxY);

            if (width <= 0 || height <= 0)
                return;

            var rtb = new RenderTargetBitmap((int)width, (int)height, 96, 96, PixelFormats.Pbgra32);
            
            DrawingVisual visual = InitializeBitmap(width, height);
            rtb.Render(visual);

            foreach (var item in items)
            {
                UpdateAppearanceByItem(designerCanvas, minX, minY, rtb, visual, item);
            }

            Appearance.Value = rtb;
        }

        private static void UpdateAppearanceByItem(DesignerCanvas designerCanvas, double minX, double minY, RenderTargetBitmap rtb, DrawingVisual visual, SelectableDesignerItemViewModelBase item)
        {
            var views = designerCanvas.GetCorrespondingViews<FrameworkElement>(item).Where(x => x.GetType() == item.GetViewType());
            foreach (var view in views)
            {
                if (view != null)
                {
                    if (view.ActualWidth >= 1 && view.ActualHeight >= 1)
                    {
                        using (DrawingContext context = visual.RenderOpen())
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
                            VisualBrush brush = new VisualBrush(view);
                            brush.Stretch = Stretch.None;
                            context.DrawRectangle(brush, null, new Rect(new Point(diffX, diffY), new Size(view.ActualWidth, view.ActualHeight)));
                        }

                        rtb.Render(visual);
                    }
                }
                else
                {
                    throw new Exception("view not found");
                }
            }
        }

        private static DrawingVisual InitializeBitmap(int width, int height)
        {
            DrawingVisual visual = new DrawingVisual();
            using (DrawingContext context = visual.RenderOpen())
            {
                //白背景でビットマップを初期化
                context.DrawRectangle(Brushes.White, null, new Rect(new Point(), new Size(width, height)));
            }
            return visual;
        }        

        private List<IObserver<LayerObservable>> _observers = new List<IObserver<LayerObservable>>();

        public IDisposable Subscribe(IObserver<LayerObservable> observer)
        {
            _observers.Add(observer);
            observer.OnNext(new LayerObservable());
            return new LayerDisposable(this, observer);
        }

        public override string ToString()
        {
            return $"Name={Name.Value}, IsSelected={IsSelected.Value}, Parent={{{Parent.Value}}}";
        }

        public int CompareTo(LayerTreeViewItemBase other)
        {
            if (other == null)
                return 1;
            else
                return Children.OfType<LayerItem>().Max(x => x.Item.Value.ZIndex.Value).CompareTo(other.OfType<LayerItem>().Max(x => x.Item.Value.ZIndex.Value));
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;
            else if (!(obj is LayerTreeViewItemBase other))
                return 1;
            else
            {
                int otherInt = other.Children.OfType<LayerItem>().Max(x => x.Item.Value.ZIndex.Value);
                return Children.OfType<LayerItem>().Max(x => x.Item.Value.ZIndex.Value).CompareTo(otherInt);
            }
        }

        public class LayerDisposable : IDisposable
        {
            private Layer layer;
            private IObserver<LayerObservable> observer;

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
}
