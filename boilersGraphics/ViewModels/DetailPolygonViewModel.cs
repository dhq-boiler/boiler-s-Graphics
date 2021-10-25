using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Reactive.Disposables;

namespace boilersGraphics.ViewModels
{
    class DetailPolygonViewModel : IDialogAware, IDisposable
    {
        private CompositeDisposable disposables = new CompositeDisposable();
        private bool disposedValue;

        public string Title => "プロパティ";

        public ReactiveCommand OKCommand { get; } = new ReactiveCommand();

        public event Action<IDialogResult> RequestClose;

        public DetailPolygonViewModel()
        {
            OKCommand.Subscribe(_ =>
            {
                RequestClose.Invoke(new DialogResult(ButtonResult.OK, new DialogParameters() { { "ViewModel", ViewModel.Value } }));
            })
            .AddTo(disposables);
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public ReactivePropertySlim<NPolygonViewModel> ViewModel { get; } = new ReactivePropertySlim<NPolygonViewModel>();

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            ViewModel.Value = parameters.GetValue<NPolygonViewModel>("ViewModel");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    OKCommand.Dispose();
                    ViewModel.Dispose();
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
