using boilersGraphics.Controls;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace boilersGraphics.ViewModels
{
    public class MiniMapViewModel : BindableBase, IDisposable
    {
        private CompositeDisposable disposables = new CompositeDisposable();
        public ReactiveProperty<double> CanvasLeft { get; set; } = new ReactiveProperty<double>();
        public ReactiveProperty<double> CanvasTop { get; set; } = new ReactiveProperty<double>();
        public ReactiveProperty<double> CanvasWidth { get; set; } = new ReactiveProperty<double>();
        public ReactiveProperty<double> CanvasHeight { get; set; } = new ReactiveProperty<double>();
        public ReactiveProperty<double> MiniMapWidth { get; set; } = new ReactiveProperty<double>();
        public ReactiveProperty<double> MiniMapHeight { get; set; } = new ReactiveProperty<double>();
        public ReactiveProperty<double> ViewportLeft { get; set; } = new ReactiveProperty<double>();
        public ReactiveProperty<double> ViewportTop { get; set; } = new ReactiveProperty<double>();
        public ReactiveProperty<double> ViewportWidth { get; set; } = new ReactiveProperty<double>();
        public ReactiveProperty<double> ViewportHeight { get; set; } = new ReactiveProperty<double>();
        public ReactiveProperty<double> Scale { get; set; } = new ReactiveProperty<double>();
        public ReactiveProperty<double> Ratio { get; set; } = new ReactiveProperty<double>();

        private MiniMap parent;
        private bool disposedValue;

        public MiniMapViewModel(MiniMap parent)
        {
            this.parent = parent;
            Scale.Value = 1.0;

            this.ViewportLeft
                .Subscribe(x =>
                {
                    if (x < 0)
                        ViewportLeft.Value = 0;
                    else if (x + ViewportWidth.Value > MiniMapWidth.Value)
                        ViewportLeft.Value = MiniMapWidth.Value - ViewportWidth.Value;
                    parent.ScrollViewer?.ScrollToHorizontalOffset(ViewportLeft.Value / Ratio.Value);
                })
                .AddTo(disposables);
            this.ViewportTop
                .Subscribe(x =>
                {
                    if (x < 0)
                        ViewportTop.Value = 0;
                    else if (x + ViewportHeight.Value > MiniMapHeight.Value)
                        ViewportTop.Value = MiniMapHeight.Value - ViewportHeight.Value;
                    parent.ScrollViewer?.ScrollToVerticalOffset(ViewportTop.Value / Ratio.Value);
                })
                .AddTo(disposables);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    CanvasLeft.Dispose();
                    CanvasTop.Dispose();
                    CanvasWidth.Dispose();
                    CanvasHeight.Dispose();
                    MiniMapWidth.Dispose();
                    MiniMapHeight.Dispose();
                    ViewportLeft.Dispose();
                    ViewportTop.Dispose();
                    ViewportWidth.Dispose();
                    ViewportHeight.Dispose();
                    Scale.Dispose();
                    Ratio.Dispose();
                }

                CanvasLeft = null;
                CanvasTop = null;
                CanvasWidth = null;
                CanvasHeight = null;
                MiniMapWidth = null;
                MiniMapHeight = null;
                ViewportLeft = null;
                ViewportTop = null;
                ViewportWidth = null;
                ViewportHeight = null;
                Scale = null;
                Ratio = null;
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // このコードを変更しないでください。
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
