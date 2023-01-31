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

namespace boilersGraphics.ViewModels
{
    public abstract class EffectViewModel : DesignerItemViewModelBase
    {
        private bool isMonitored = false;
        public ReactivePropertySlim<WriteableBitmap> Bitmap { get; }

        public virtual void Initialize()
        {
            (Application.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel.AllItems.Subscribe(items =>
            {
                foreach (var item in items.Where(x => x.ZIndex.Value < this.ZIndex.Value))
                    item.BeginMonitor(() => { Render(); }).AddTo(_CompositeDisposable);
            }).AddTo(_CompositeDisposable);
        }

        public abstract void Render();

        internal void UpdateLayout()
        {
            var view = App.Current.MainWindow.GetChildOfType<DesignerCanvas>().EnumVisualChildren<FrameworkElement>().FirstOrDefault(x => x.DataContext == this);
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
                //Rect.Subscribe(_ => action()).AddTo(compositeDisposable);
                this.ObserveProperty(x => x.Bitmap).Subscribe(_ => action()).AddTo(compositeDisposable);
                isMonitored = true;
            }

            return compositeDisposable;
        }
    }
}
