using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.ViewModels;
using boilersGraphics.Views;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Prism.Unity;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace boilersGraphics.Models
{
    public class LayerItem : LayerTreeViewItemBase, IDisposable, IComparable<LayerTreeViewItemBase>, IComparable
    {
        private bool disposedValue;
        public ReactivePropertySlim<ImageSource> Appearance { get; } = new ReactivePropertySlim<ImageSource>();
        public ReactiveCommand SwitchVisibilityCommand { get; } = new ReactiveCommand();
        public ReactivePropertySlim<SelectableDesignerItemViewModelBase> Item { get; } = new ReactivePropertySlim<SelectableDesignerItemViewModelBase>();
        public ReactiveCommand MoveSnapPointCommand { get; } = new ReactiveCommand();


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
                Item.Value.IsVisible.Value = isVisible;
                ChildrenSwitchVisibility(isVisible);
            })
            .AddTo(_disposable);
            IsSelected = this.ObserveProperty(x => x.Item.Value.IsSelected)
                             .Select(x => x.Value)
                             .ToReactiveProperty()
                             .AddTo(_disposable);
            IsSelected.Subscribe(x =>
            {
                Item.Value.IsSelected.Value = x;
                if (x)
                {
                    Item.Value.SelectedOrder.Value = SelectableDesignerItemViewModelBase.SelectedOrderCount++ + 1;
                }
                else
                {
                    Item.Value.SelectedOrder.Value = -1;
                    SelectableDesignerItemViewModelBase.SelectedOrderCount--;
                }
            })
            .AddTo(_disposable);
            IsVisible.Value = true;
            if (!App.IsTest)
            {
                Item.Subscribe(x =>
                {
                    if (x is SnapPointViewModel)
                    {
                        LayerTreeViewItemContextMenu.Add(new MenuItem()
                        {
                            Header = "スナップポイントを移動",
                            Command = MoveSnapPointCommand
                        });
                    }
                })
                .AddTo(_disposable);
            }
            MoveSnapPointCommand.Subscribe(x =>
            {
                var dialogService = new DialogService((App.Current as PrismApplication).Container as IContainerExtension);
                IDialogResult result = null;
                var snapPointVM = Item.Value as SnapPointViewModel;
                Point point = new Point(snapPointVM.Left.Value, snapPointVM.Top.Value);
                dialogService.ShowDialog(nameof(SetSnapPoint), new DialogParameters() { { "Point", point }, { "LayerItem", this } }, ret => result = ret);
                if (result != null 
                 && result.Parameters != null 
                 && result.Parameters.ContainsKey("Point"))
                {
                    var newPoint = result.Parameters.GetValue<Point>("Point");
                    snapPointVM.Left.Value = newPoint.X;
                    snapPointVM.Top.Value = newPoint.Y;
                }
            })
            .AddTo(_disposable);
        }

        public void UpdateAppearance(IEnumerable<SelectableDesignerItemViewModelBase> items)
        {
            if (items.Count() == 0) return;

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

        public override string ToString()
        {
            return $"Name={Name.Value}, IsSelected={IsSelected.Value}, ZIndex={Item.Value.ZIndex.Value}, Parent={{{Parent.Value}}}";
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                base.Dispose(disposing);

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

        public int CompareTo(LayerTreeViewItemBase other)
        {
            if (other == null)
                return 1;
            else if (!(other is LayerItem otherLayerItem))
                return 1;
            else
                return Item.Value.ZIndex.Value.CompareTo(otherLayerItem.Item.Value.ZIndex.Value);
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;
            else if (!(obj is LayerItem otherLayerItem))
                return 1;
            else
                return Item.Value.ZIndex.Value.CompareTo(otherLayerItem.Item.Value.ZIndex.Value);
        }
    }
}
