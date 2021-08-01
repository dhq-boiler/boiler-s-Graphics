using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.ViewModels;
using boilersGraphics.Views;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        public void UpdateAppearance(SelectableDesignerItemViewModelBase item)
        {
            var designerCanvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();
            var views = designerCanvas.GetCorrespondingViews<FrameworkElement>(item).Where(x => x.GetType() == item.GetViewType());
            foreach (var view in views)
            {
                if (view != null)
                {
                    if (view.ActualWidth >= 1 && view.ActualHeight >= 1)
                    {
                        var rtb = new RenderTargetBitmap((int)view.ActualWidth, (int)view.ActualHeight, 96, 96, PixelFormats.Pbgra32);

                        DrawingVisual visual = new DrawingVisual();
                        using (DrawingContext context = visual.RenderOpen())
                        {
                            context.DrawRectangle(Brushes.White, null, new Rect(new Point(), new Size(view.ActualWidth, view.ActualHeight)));

                            VisualBrush brush = new VisualBrush(view);
                            context.DrawRectangle(brush, null, new Rect(new Point(), new Size(view.ActualWidth, view.ActualHeight)));
                        }

                        rtb.Render(visual);

                        Appearance.Value = rtb;
                    }
                }
                else
                {
                    throw new Exception("view not found");
                }
            }
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
