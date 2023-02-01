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

            (Application.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel.AllItems.Subscribe(items =>
            {
                foreach (var item in items.Where(x => !monitoringItems.ContainsKey(x.ID) && x.ZIndex.Value < this.ZIndex.Value))
                {
                    monitoringItems.Add(item.ID, item.BeginMonitor(() => Render()));
                }
            }).AddTo(_CompositeDisposable);
            (Application.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel.AllItems.Value
                .ToObservable().ToReadOnlyReactiveCollection().ObserveElementProperty(x => x.ZIndex.Value).Subscribe(
                    x =>
                    {
                        if (monitoringItems.ContainsKey(x.Instance.ID) && x.Instance.ZIndex.Value > this.ZIndex.Value)
                        {
                            var disposing = monitoringItems[x.Instance.ID];
                            disposing?.Dispose();
                            monitoringItems.Remove(x.Instance.ID);
                        }
                    }).AddTo(_CompositeDisposable);
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
            Bitmap.Dispose();
            base.Dispose();
        }
    }
}
