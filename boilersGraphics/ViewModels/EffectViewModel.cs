using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using Reactive.Bindings.Extensions;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace boilersGraphics.ViewModels
{
    public abstract class EffectViewModel : DesignerItemViewModelBase
    {
        private bool isMonitored = false;
        private Dictionary<Guid, IDisposable> monitoringItems = new Dictionary<Guid, IDisposable>();
        public ReactivePropertySlim<WriteableBitmap> Bitmap { get; }

        public virtual void Initialize()
        {
            monitoringItems.ToList().ForEach(x => x.Value.Dispose());
            monitoringItems.Clear();

            DiagramViewModel.Instance.AllItems.Subscribe(items =>
            {
                BeginMonitoring(items);
            }).AddTo(_CompositeDisposable);
            DiagramViewModel.Instance.AllItems.Value
                .ToObservable().ToReadOnlyReactiveCollection().ObserveElementProperty(x => x.ZIndex.Value).Subscribe(
                    x =>
                    {
                        DisposeMonitoringItem(x.Instance);
                    }).AddTo(_CompositeDisposable);
        }

        private void BeginMonitoring(params SelectableDesignerItemViewModelBase[] items)
        {
            foreach (var item in items.Where(x => !monitoringItems.ContainsKey(x.ID) && x.ZIndex.Value < this.ZIndex.Value))
            {
                monitoringItems.Add(item.ID, item.BeginMonitor(() => Render()));
            }
        }

        private void DisposeMonitoringItem(SelectableDesignerItemViewModelBase x)
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
