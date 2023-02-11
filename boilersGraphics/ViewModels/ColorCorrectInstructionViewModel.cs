using boilersGraphics.Properties;
using boilersGraphics.Views.ColorCorrect;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Reactive.Disposables;
using System.Windows.Media.Imaging;

namespace boilersGraphics.ViewModels
{
    public class ColorCorrectInstructionViewModel : BindableBase, IDialogAware, IDisposable
    {
        private CompositeDisposable disposable = new CompositeDisposable();
        private bool disposedValue;
        private readonly IRegionManager _regionManager;

        public string Title => Resources.Title_ColorCorrection;

        public event Action<IDialogResult> RequestClose;


        public ReactivePropertySlim<int> OKTabIndex { get; } = new();
        public ReactivePropertySlim<ColorCorrectViewModel> ViewModel { get; } = new();

        public ReactiveCommand OKCommand { get; } = new ();

        public ColorCorrectInstructionViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            OKCommand.Subscribe(_ =>
            {
                RequestClose.Invoke(new DialogResult(ButtonResult.OK, new DialogParameters() { { "ViewModel", ViewModel.Value } }));
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
            ViewModel.Value.CCType.Subscribe(cctype =>
            {
                switch (cctype)
                {
                    case ColorCorrectType.HSV:
                        _regionManager.RequestNavigate("ColorCorrectInstructionRegion", nameof(HSV),
                            new NavigationParameters()
                            {
                                { "ViewModel", ViewModel.Value },
                            });
                        break;
                    case ColorCorrectType.ToneCurve:
                        break;
                }
            }).AddTo(disposable);
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
