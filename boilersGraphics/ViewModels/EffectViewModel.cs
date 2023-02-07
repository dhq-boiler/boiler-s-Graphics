using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.Models;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace boilersGraphics.ViewModels
{
    public abstract class EffectViewModel : DesignerItemViewModelBase
    {
        private bool isMonitored = false;
        private Dictionary<Guid, IDisposable> monitoringItems = new Dictionary<Guid, IDisposable>();
        public ReactivePropertySlim<WriteableBitmap> Bitmap { get; }

        public virtual void Initialize()
        {
            this.UpdatingStrategy.Value = SelectableDesignerItemViewModelBase.PathGeometryUpdatingStrategy.Initial;
            monitoringItems.ToList().ForEach(x => x.Value.Dispose());
            monitoringItems.Clear();

            BeginMonitoring(DiagramViewModel.Instance.AllItems.Value);

            DiagramViewModel.Instance.AllItems.Subscribe(items =>
            {
                BeginMonitoring(items);
            }).AddTo(_CompositeDisposable);
            DiagramViewModel.Instance.AllItems.Value
                .ToObservable().ToReadOnlyReactiveCollection().ObserveElementProperty(x => x.ZIndex.Value).Subscribe(
                    x =>
                    {
                        if (x.Value < this.ZIndex.Value)
                        {
                            BeginMonitoring(x.Instance);
                        }
                        else
                        {
                            DisposeMonitoringItem(x.Instance);
                        }
                    }).AddTo(_CompositeDisposable);
            DiagramViewModel.Instance.Layers.SelectRecursive((Func<LayerTreeViewItemBase, IEnumerable<LayerTreeViewItemBase>>)(x => x.Children)).ToObservable().ToReadOnlyReactiveCollection().ObserveElementProperty(x => x.IsVisible.Value).Subscribe(pp =>
            {
                if (pp.Instance is Layer)
                {
                    return;
                }
                var x = pp.Instance as LayerItem;
                if (pp.Value) //IsVisible == true
                {
                    BeginMonitoring(x.Item.Value);
                }

                if (x is not null)
                {
                    Render();
                    DisposeMonitoringItem(x.Item.Value);
                }
            }).AddTo(_CompositeDisposable);
        }

        public void BeginMonitoring(params SelectableDesignerItemViewModelBase[] items)
        {
            foreach (var item in items.Where(x => !monitoringItems.ContainsKey(x.ID) && x.ZIndex.Value < this.ZIndex.Value))
            {
                monitoringItems.Add(item.ID, item.BeginMonitor(() => Render()));
            }
        }

        public void DisposeMonitoringItem(SelectableDesignerItemViewModelBase x)
        {
            if (monitoringItems.ContainsKey(x.ID) && x.ZIndex.Value > this.ZIndex.Value)
            {
                var disposing = monitoringItems[x.ID];
                disposing?.Dispose();
                monitoringItems.Remove(x.ID);
            }
        }

        public abstract void Render();

        internal void UpdateLayout()
        {
            var view = App.Current.MainWindow.GetChildOfType<DesignerCanvas>().GetVisualChild<FrameworkElement>(this);
            if (view is null)
            {
                return;
            }
            view.InvalidateMeasure();
            view.InvalidateArrange();
            view.UpdateLayout();
        }

        public override IDisposable BeginMonitor(Action action)
        {
            var compositeDisposable = new CompositeDisposable();
            base.BeginMonitor(action).AddTo(compositeDisposable);
            if (!isMonitored)
            {
                this.ObserveProperty(x => x.Bitmap).Subscribe(_ => action()).AddTo(compositeDisposable);
                isMonitored = true;
            }

            return compositeDisposable;
        }

        public override PathGeometry CreateGeometry(bool flag = false)
        {
            switch (UpdatingStrategy.Value)
            {
                case PathGeometryUpdatingStrategy.Initial:
                    return GeometryCreator.CreateRectangle(this, 0, 0, flag);
                case PathGeometryUpdatingStrategy.ResizeWhilePreservingOriginalShape:
                    return GeometryCreator.Scale(this.PathGeometryNoRotate.Value, this.Width.Value / this.PathGeometryNoRotate.Value.Bounds.Width, this.Height.Value / this.PathGeometryNoRotate.Value.Bounds.Height);
                case PathGeometryUpdatingStrategy.Fixed:
                    return this.PathGeometryNoRotate.Value;
                default:
                    throw new NotSupportedException();
            }
        }

        public override void Dispose()
        {
            foreach (var keyValuePair in monitoringItems)
            {
                keyValuePair.Value.Dispose();
            }

            monitoringItems = null;
            if (Bitmap is not null)
            {
                Bitmap.Dispose();
            }

            base.Dispose();
        }
    }
}
