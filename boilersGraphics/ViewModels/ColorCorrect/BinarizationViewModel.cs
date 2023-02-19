using boilersGraphics.Controls;
using boilersGraphics.Helpers;
using OpenCvSharp.WpfExtensions;
using Prism.Mvvm;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Reactive.Disposables;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace boilersGraphics.ViewModels.ColorCorrect
{
    internal class BinarizationViewModel : BindableBase, INavigationAware, IDisposable
    {
        private CompositeDisposable _disposable = new CompositeDisposable();

        private bool _disposedValue;

        public ReactivePropertySlim<ColorCorrectViewModel> ViewModel { get; } = new();
        public ReactivePropertySlim<ThresholdTypes> ThresholdTypes { get; } = new();
        public ReactivePropertySlim<double> Threshold { get; } = new();
        public ReactivePropertySlim<double> MaxValue { get; } = new();
        public ReactivePropertySlim<bool> OtsuEnabled { get; } = new();

        public ReactiveCommand<SelectionChangedEventArgs> ThresholdTypesChangedCommand { get; } = new();

        public BinarizationViewModel()
        {
            ThresholdTypes.Subscribe(types =>
            {
                if (ViewModel.Value is not null)
                {
                    ViewModel.Value.ThresholdTypes.Value = types;
                    ViewModel.Value.Render();
                }
            }).AddTo(_disposable);
            OtsuEnabled.Subscribe(enabled =>
            {
                if (ViewModel.Value is not null)
                {
                    ViewModel.Value.OtsuEnabled.Value = enabled;
                    if (enabled)
                    {
                        Threshold.Value = ViewModel.Value.Threshold.Value = GetThresholdByOtsu();
                    }
                    ViewModel.Value.Render();
                }
            }).AddTo(_disposable);
            Threshold.Subscribe(threshold =>
            {
                if (ViewModel.Value is not null)
                {
                    ViewModel.Value.Threshold.Value = threshold;
                    ViewModel.Value.Render();
                }
            }).AddTo(_disposable);
            MaxValue.Subscribe(maxvalue =>
            {
                if (ViewModel.Value is not null)
                {
                    ViewModel.Value.MaxValue.Value = maxvalue;
                    ViewModel.Value.Render();
                }
            }).AddTo(_disposable); 
            ThresholdTypesChangedCommand.Subscribe(x =>
            {
                ViewModel.Value.ThresholdTypes.Value = (ThresholdTypes)x.AddedItems[0];
                ViewModel.Value.Render();
            }).AddTo(_disposable);
        }

        private double GetThresholdByOtsu()
        {
            var renderer = new EffectRenderer(new WpfVisualTreeHelper());
            var rtb = renderer.Render(ViewModel.Value.Rect.Value, DesignerCanvas.GetInstance(),
                MainWindowViewModel.Instance.DiagramViewModel, MainWindowViewModel.Instance.DiagramViewModel.BackgroundItem.Value, ViewModel.Value, ViewModel.Value.ZIndex.Value - 1);
            var newFormattedBitmapSource = new FormatConvertedBitmap();
            newFormattedBitmapSource.BeginInit();
            newFormattedBitmapSource.Source = rtb;
            newFormattedBitmapSource.DestinationFormat = PixelFormats.Bgr24;
            newFormattedBitmapSource.EndInit();

            using (var grayscale = newFormattedBitmapSource.ToMat())
            {
                // 入力画像のヒストグラムを計算する
                int[] hist = new int[256];
                for (int i = 0; i < grayscale.Rows; i++)
                {
                    for (int j = 0; j < grayscale.Cols; j++)
                    {
                        hist[(int)grayscale.At<byte>(i, j)]++;
                    }
                }

                // ヒストグラムの総和を計算する
                int sum = 0;
                for (int i = 0; i < 256; i++)
                {
                    sum += i * hist[i];
                }

                // クラス間分散を最大化するしきい値を計算する
                double max_variance = 0;
                int threshold = 0;
                long w1 = 0;
                long w2 = 0;
                int sum1 = 0;
                int sum2 = 0;
                double variance = 0;
                for (int i = 0; i < 256; i++)
                {
                    w1 += hist[i];
                    if (w1 == 0)
                    {
                        continue;
                    }
                    w2 = grayscale.Total() - w1;
                    if (w2 == 0)
                    {
                        break;
                    }
                    sum1 += i * hist[i];
                    sum2 = sum - sum1;
                    double mean1 = sum1 / (double)w1;
                    double mean2 = sum2 / (double)w2;
                    variance = w1 * w2 * Math.Pow(mean1 - mean2, 2);
                    if (variance > max_variance)
                    {
                        max_variance = variance;
                        threshold = i;
                    }
                }

                return threshold;
            }
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            ViewModel.Value = navigationContext.Parameters.GetValue<ColorCorrectViewModel>("ViewModel");
            ThresholdTypes.Value = ViewModel.Value.ThresholdTypes.Value;
            Threshold.Value = ViewModel.Value.Threshold.Value;
            MaxValue.Value = ViewModel.Value.MaxValue.Value;
            OtsuEnabled.Value = ViewModel.Value.OtsuEnabled.Value;
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return false;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _disposable.Dispose();
                }

                _disposable = null;
                _disposedValue = true;
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
    }
}
