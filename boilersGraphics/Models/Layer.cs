using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.ViewModels;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace boilersGraphics.Models
{
    public class Layer : LayerTreeViewItemBase, IObservable<LayerObservable>
    {
        public static int LayerCount { get; set; } = 1;
       
        public ReactivePropertySlim<ImageSource> Appearance { get; } = new ReactivePropertySlim<ImageSource>();

        public ReactiveCommand SwitchVisibilityCommand { get; } = new ReactiveCommand();
        public ReactiveCommand SelectLayerCommand { get; } = new ReactiveCommand();

        public Layer()
        {
            SwitchVisibilityCommand.Subscribe(_ =>
            {
                IsVisible.Value = !IsVisible.Value;
            })
            .AddTo(_disposable);
            SelectLayerCommand.Subscribe(args =>
            {
                MouseEventArgs ea = args as MouseEventArgs;
                if (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
                {
                    var diagramVM = (App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel;
                    diagramVM.Layers.Where(x => x.IsSelected.Value == true)
                                    .ToList()
                                    .ForEach(x => x.IsSelected.Value = false);
                }

                IsSelected.Value = true;
            })
            .AddTo(_disposable);
            IsVisible.Subscribe(isVisible =>
            {
                if (!isVisible)
                {
                    Children.ToList().ForEach(x => x.IsVisible.Value = isVisible);
                }
            })
            .AddTo(_disposable);

            Children.ObserveElementProperty(x => (x as LayerItem).Item.Value)
                 .ToUnit()
                 .Merge(Children.ObserveElementObservableProperty(x => (x as LayerItem).Item.Value.EdgeColor).ToUnit())
                 .ToUnit()
                 .Merge(Children.ObserveElementObservableProperty(x => (x as LayerItem).Item.Value.EdgeThickness).ToUnit())
                 .ToUnit()
                 .Merge(Children.ObserveElementObservableProperty(x => (x as LayerItem).Item.Value.FillColor).ToUnit())
                 .Delay(TimeSpan.FromMilliseconds(500))
                 .ObserveOnDispatcher()
                 .Subscribe(x =>
                 {
                     Trace.WriteLine("detected Layer changes. run Layer.UpdateAppearance().");
                     UpdateAppearance(Children.Select(xx => (xx as LayerItem).Item.Value));
                     Children.ToList().ForEach(x => (x as LayerItem).UpdateAppearance((x as LayerItem).Item.Value));
                 })
                 .AddTo(_disposable);
            IsVisible.Value = true;
        }

        public IObservable<Unit> LayerItemsChangedAsObservable()
        {
            return Children.ObserveElementObservableProperty(x => (x as LayerItem).Item)
                        .ToUnit()
                        .Merge(Children.CollectionChangedAsObservable().Where(x => x.Action == NotifyCollectionChangedAction.Remove || x.Action == NotifyCollectionChangedAction.Reset).ToUnit());
        }

        public IObservable<Unit> SelectedLayerItemsChangedAsObservable()
        {
            return Children.ObserveElementObservableProperty(x => (x as LayerItem).Item.Value.IsSelected)
                        .ToUnit()
                        .Merge(Children.CollectionChangedAsObservable().Where(x => x.Action == NotifyCollectionChangedAction.Remove || x.Action == NotifyCollectionChangedAction.Reset).ToUnit());
        }

        private void UpdateAppearance(IEnumerable<SelectableDesignerItemViewModelBase> items)
        {
            var designerCanvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();
            double minX, maxX, minY, maxY;
            var width = GetWidth(items, out minX, out maxX);
            var height = GetHeight(items, out minY, out maxY);

            if (width < 0 || height < 0)
                return;

            var rtb = new RenderTargetBitmap((int)width, (int)height, 96, 96, PixelFormats.Pbgra32);
            
            DrawingVisual visual = InitializeBitmap(width, height);
            rtb.Render(visual);

            foreach (var item in items)
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

            Appearance.Value = rtb;
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

        private int GetWidth(IEnumerable<SelectableDesignerItemViewModelBase> items, out double minX, out double maxX)
        {
            minX = 0d;
            maxX = 0d;
            foreach (var item in items)
            {
                var desingerItem = item as DesignerItemViewModelBase;
                var connectorItem = item as ConnectorBaseViewModel;
                if (desingerItem != null)
                {
                    minX = Math.Min(Math.Min(minX, desingerItem.Left.Value), desingerItem.Right.Value);
                    maxX = Math.Max(Math.Max(maxX, desingerItem.Left.Value), desingerItem.Right.Value);
                }
                if (connectorItem != null)
                {
                    minX = Math.Min(Math.Min(minX, connectorItem.Points[0].X), connectorItem.Points[1].X);
                    maxX = Math.Max(Math.Max(maxX, connectorItem.Points[0].X), connectorItem.Points[1].X);
                }
            }
            return (int)(maxX - minX);
        }

        private int GetHeight(IEnumerable<SelectableDesignerItemViewModelBase> items, out double minY, out double maxY)
        {
            minY = 0d;
            maxY = 0d;
            foreach (var item in items)
            {
                var desingerItem = item as DesignerItemViewModelBase;
                var connectorItem = item as ConnectorBaseViewModel;
                if (desingerItem != null)
                {
                    minY = Math.Min(Math.Min(minY, desingerItem.Top.Value), desingerItem.Bottom.Value);
                    maxY = Math.Max(Math.Max(maxY, desingerItem.Top.Value), desingerItem.Bottom.Value);
                }
                if (connectorItem != null)
                {
                    minY = Math.Min(Math.Min(minY, connectorItem.Points[0].X), connectorItem.Points[1].X);
                    maxY = Math.Max(Math.Max(maxY, connectorItem.Points[0].X), connectorItem.Points[1].X);
                }
            }
            return (int)(maxY - minY);
        }

        public void RemoveItem(SelectableDesignerItemViewModelBase item)
        {
            var layerItems = Children.Where(x => (x as LayerItem).Item.Value == item);
            layerItems.ToList().ForEach(x =>
            {
                var removed = Children.Remove(x);
                Trace.WriteLine($"{x} removed from {Children} {removed}");
            });
        }

        public void AddItem(SelectableDesignerItemViewModelBase item)
        {
            var layerItem = new LayerItem(item, this, $"アイテム{LayerItem.LayerItemCount++}");
            layerItem.IsVisible.Value = true;
            layerItem.Parent.Value = this;
            Random rand = new Random();
            layerItem.Color.Value = Randomizer.RandomColor(rand);
            Children.Add(layerItem);
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
