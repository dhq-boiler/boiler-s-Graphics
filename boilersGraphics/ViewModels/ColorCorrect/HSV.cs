using Prism.Regions;
using Reactive.Bindings;
using System;
using System.Reactive.Disposables;
using System.Windows.Media.Imaging;
using Prism.Mvvm;
using Reactive.Bindings.Extensions;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;

namespace boilersGraphics.ViewModels.ColorCorrect
{
    public class HSV : BindableBase, INavigationAware, IDisposable
    {
        private CompositeDisposable disposable = new CompositeDisposable();

        private bool disposedValue;
        public ReactivePropertySlim<int> AddHue { get; } = new();
        public ReactivePropertySlim<int> AddSaturation { get; } = new();
        public ReactivePropertySlim<int> AddValue { get; } = new();
        public ReactivePropertySlim<ColorCorrectViewModel> ViewModel { get; } = new();


        public ReactivePropertySlim<WriteableBitmap> HueSelector { get; } = new();

        public HSV()
        {
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
            GenerateHueSelectorMat();
        }

        private void GenerateHueSelectorMat()
        {
            using (var hsvMat = new Mat(10, 180 * 2, MatType.CV_8UC3))
            {
                unsafe
                {
                    var p = (byte*)hsvMat.Data.ToPointer();

                    for (var y = 0; y < 10; ++y)
                    {
                        var py = p + y * hsvMat.Step();

                        for (var x = 0; x < 180 * 2; ++x)
                        {
                            *(py + x * 3) = CirculalyClamp(x - 180, 0, 180); //hue
                            *(py + x * 3 + 1) = 255;
                            *(py + x * 3 + 2) = 255;
                        }
                    }
                }

                using (var bgrMat = new Mat())
                {
                    Cv2.CvtColor(hsvMat, bgrMat, ColorConversionCodes.HSV2BGR);
                    HueSelector.Value = bgrMat.ToWriteableBitmap();
                }
            }
        }

        private static byte CirculalyClamp(int value, byte min, byte max)
        {
            return (byte)(value > max ? value - Math.Abs(min) : (value < min ? value + max : value));
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
        // ~HSVBindableBase()
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

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            ViewModel.Value = navigationContext.Parameters.GetValue<ColorCorrectViewModel>("ViewModel");
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return false;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
    }
}
