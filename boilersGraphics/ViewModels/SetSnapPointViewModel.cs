using boilersGraphics.Models;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace boilersGraphics.ViewModels
{
    class SetSnapPointViewModel : BindableBase, IDialogAware, IDisposable
    {
        private CompositeDisposable _disposables = new CompositeDisposable();
        private bool disposedValue;

        public LayerItem LayerItem { get; set; }

        public ReactivePropertySlim<double> X { get; } = new ReactivePropertySlim<double>();

        public ReactivePropertySlim<double> Y { get; } = new ReactivePropertySlim<double>();

        public ReactiveCommand OKCommand { get; }
        public ReactiveCommand CancelCommand { get; }

        public string Title => $"{LayerItem.Name.Value}の移動";

        public event Action<IDialogResult> RequestClose;

        public SetSnapPointViewModel()
        {
            OKCommand = X.CombineLatest(Y, (x, y) => x > 0 && y > 0)
                         .ToReactiveCommand();
            OKCommand.Subscribe(x =>
            {
                DialogResult result = new DialogResult(ButtonResult.OK, new DialogParameters() { { "Point", new Point(X.Value, Y.Value) } });
                RequestClose.Invoke(result);
            })
            .AddTo(_disposables);
            CancelCommand = new ReactiveCommand();
            CancelCommand.Subscribe(x =>
            {
                DialogResult result = new DialogResult(ButtonResult.Cancel, null);
                RequestClose.Invoke(result);
            })
            .AddTo(_disposables);
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            var point = parameters.GetValue<Point>("Point");
            LayerItem = parameters.GetValue<LayerItem>("LayerItem");
            X.Value = point.X;
            Y.Value = point.Y;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _disposables.Dispose();
                }

                _disposables = null;
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
