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

namespace boilersGraphics.ViewModels
{
    class SettingViewModel : BindableBase, IDialogAware, IDisposable
    {
        private bool disposedValue;
        private CompositeDisposable _disposables = new CompositeDisposable();

        public ReactiveCommand OkCommand { get; set; }
        public ReactiveCommand CancelCommand { get; set; }

        public ReactiveProperty<Models.Setting> EditTarget { get; set; } = new ReactiveProperty<Setting>();

        public SettingViewModel()
        {
            EditTarget.Value = new Setting();
            CancelCommand = new ReactiveCommand();
            OkCommand = new ReactiveCommand();
            OkCommand.Subscribe(_ =>
            {
                var parameters = new DialogParameters() { { "Setting", EditTarget.Value } };
                var ret = new DialogResult(ButtonResult.OK, parameters);
                RequestClose.Invoke(ret);
            })
            .AddTo(_disposables);
            CancelCommand.Subscribe(_ =>
            {
                var ret = new DialogResult(ButtonResult.Cancel, null);
                RequestClose.Invoke(ret);
            })
            .AddTo(_disposables);
        }

        public string Title => "設定";

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            EditTarget.Value = parameters.GetValue<Models.Setting>("Setting");
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
