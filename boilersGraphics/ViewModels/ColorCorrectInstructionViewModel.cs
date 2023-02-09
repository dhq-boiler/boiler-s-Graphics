using Prism.Mvvm;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Reactive.Disposables;

namespace boilersGraphics.ViewModels
{
    public class ColorCorrectInstructionViewModel : BindableBase, IDialogAware, IDisposable
    {
        private CompositeDisposable disposable = new CompositeDisposable();
        private bool disposedValue;

        public string Title => "からーこれくといんすとらくしょん";

        public event Action<IDialogResult> RequestClose;


        public ReactivePropertySlim<int> OKTabIndex { get; } = new();
        public ReactivePropertySlim<ColorCorrectViewModel> ViewModel { get; } = new();

        public ReactiveCommand OKCommand { get; } = new ();
        public ReactivePropertySlim<int> AddHue { get; } = new();
        public ReactivePropertySlim<int> AddSaturation { get; } = new();
        public ReactivePropertySlim<int> AddValue { get; } = new();

        public ColorCorrectInstructionViewModel()
        {
            OKCommand.Subscribe(_ =>
            {
                RequestClose.Invoke(new DialogResult(ButtonResult.OK, new DialogParameters() { { "ViewModel", ViewModel.Value } }));
            }).AddTo(disposable);
            AddHue.Subscribe(hue =>
            {
                if (ViewModel.Value is not null)
                {
                    ViewModel.Value.AddHue.Value = hue;
                    ViewModel.Value.Render();
                }
            }).AddTo(disposable);
            AddSaturation.Subscribe(saturation =>
            {
                if (ViewModel.Value is not null)
                {
                    ViewModel.Value.AddSaturation.Value = saturation;
                    ViewModel.Value.Render();
                }
            }).AddTo(disposable);
            AddValue.Subscribe(value =>
            {
                if (ViewModel.Value is not null)
                {
                    ViewModel.Value.AddValue.Value = value;
                    ViewModel.Value.Render();
                }
            }).AddTo(disposable);
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
            ViewModel.Value = parameters.GetValue<ColorCorrectViewModel>("ViewModel");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    disposable.Dispose();
                }

                disposable = null;
                disposedValue = true;
            }
        }

        // // TODO: 'Dispose(bool disposing)' にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします
        // ~ColorCorrectInstructionViewModel()
        // {
        //     // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
