using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.ViewModels;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace boilersGraphics.Models
{
    public class LayerItem : LayerTreeViewItemBase, IDisposable
    {
        private bool disposedValue;
        public static int LayerItemCount { get; set; } = 1;
        public ReactivePropertySlim<ImageSource> Appearance { get; } = new ReactivePropertySlim<ImageSource>();
        public ReactiveCommand SwitchVisibilityCommand { get; } = new ReactiveCommand();
        public ReactivePropertySlim<SelectableDesignerItemViewModelBase> Item { get; } = new ReactivePropertySlim<SelectableDesignerItemViewModelBase>();

        [Obsolete]
        public LayerItem(SelectableDesignerItemViewModelBase item)
        {
            Item.Value = item;
            Init();
        }

        public LayerItem(SelectableDesignerItemViewModelBase item, LayerTreeViewItemBase owner, string name)
        {
            Name.Value = name;
            Item.Value = item;
            Parent.Value = owner;
            Init();
        }

        private void Init()
        {
            SwitchVisibilityCommand.Subscribe(_ =>
            {
                IsVisible.Value = !IsVisible.Value;
            })
            .AddTo(_disposable);
            IsVisible.Subscribe(isVisible =>
            {
                if (Item.Value != null)
                {
                    Item.Value.IsVisible.Value = isVisible;
                    if (isVisible && !Parent.Value.IsVisible.Value)
                    {
                        Parent.Value.IsVisible.Value = true;
                    }
                }
            })
            .AddTo(_disposable);
            IsSelected = this.ObserveProperty(x => x.Item.Value.IsSelected)
                             .Select(x => x.Value)
                             .ToReactiveProperty()
                             .AddTo(_disposable);
            IsVisible.Value = true;
        }

        public void UpdateAppearance(IEnumerable<SelectableDesignerItemViewModelBase> items)
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
                    IsVisible.Dispose();
                    Appearance.Dispose();
                    Name.Dispose();
                    SwitchVisibilityCommand.Dispose();
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
    }
}
