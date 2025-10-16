using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.Models;
using R3;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ZLinq;
using Disposable = R3.Disposable;

namespace boilersGraphics.ViewModels
{
    public abstract class EffectViewModel : DesignerItemViewModelBase
    {
        private bool isMonitored = false;
        private Dictionary<Guid, IDisposable> monitoringItems = new Dictionary<Guid, IDisposable>();
        public R3.ReactiveProperty<WriteableBitmap> Bitmap { get; }

        public virtual void Initialize()
        {
            monitoringItems.AsValueEnumerable().ToList().ForEach(x => x.Value.Dispose());
            monitoringItems.Clear();

            BeginMonitoring(DiagramViewModel.Instance.AllItems.Value);

            Disposable.AddTo(DiagramViewModel.Instance.AllItems.AsObservable().Subscribe(new Action<SelectableDesignerItemViewModelBase[]>(items =>
            {
                BeginMonitoring(items);
            })), _CompositeDisposable);
            DiagramViewModel.Instance.AllItems.Value
                .ToObservable().ToObservableList().ObserveElementObservableProperty(x => x).Subscribe(
                    x =>
                    {
                        if (x.ZIndex.Value < this.ZIndex.Value)
                        {
                            BeginMonitoring(x);
                        }
                        else
                        {
                            DisposeMonitoringItem(x);
                        }
                    }).AddTo(_CompositeDisposable);
            DiagramViewModel.Instance.Layers.SelectRecursive((Func<LayerTreeViewItemBase, IEnumerable<LayerTreeViewItemBase>>)(x => x.Children))
                .ToObservable().ToObservableList().ObserveElementObservableProperty(x => x).Subscribe(pp =>
            {
                if (pp is Layer)
                {
                    return;
                }
                var x = pp as LayerItem;
                if (pp.IsVisible.Value) //IsVisible == true
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
            foreach (var item in items.AsValueEnumerable().Where(x => !monitoringItems.ContainsKey(x.ID) && x.ZIndex.Value < this.ZIndex.Value))
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
            Disposable.AddTo(base.BeginMonitor(action), compositeDisposable);
            if (!isMonitored)
            {
                this.ObservePropertyChanged(x => x.Bitmap).Subscribe(_ => action()).AddTo(compositeDisposable);
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
