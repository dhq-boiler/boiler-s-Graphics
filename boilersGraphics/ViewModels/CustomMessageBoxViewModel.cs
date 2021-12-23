using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;

namespace boilersGraphics.ViewModels
{
    internal class CustomMessageBoxViewModel : IDialogAware, IDisposable
    {
        private string _title;
        private CompositeDisposable disposables = new CompositeDisposable();
        private bool disposedValue;

        public string Title => _title;

        public event Action<IDialogResult> RequestClose;

        public ReactivePropertySlim<string> Text { get; } = new ReactivePropertySlim<string>();

        public ReactiveCommand OKCommand { get; } = new ReactiveCommand();

        public CustomMessageBoxViewModel()
        {
            OKCommand.Subscribe(_ =>
            {
                RequestClose.Invoke(new DialogResult(ButtonResult.OK));
            })
            .AddTo(disposables);
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
            _title = parameters.GetValue<string>("Title");
            Text.Value = parameters.GetValue<string>("Text");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    disposables.Dispose();
                }
                disposables = null;
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
