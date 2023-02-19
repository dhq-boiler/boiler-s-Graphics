using Prism.Mvvm;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Reactive.Disposables;
using System.Windows.Controls;

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

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            ViewModel.Value = navigationContext.Parameters.GetValue<ColorCorrectViewModel>("ViewModel");
            ThresholdTypes.Value = ViewModel.Value.ThresholdTypes.Value;
            Threshold.Value = ViewModel.Value.Threshold.Value;
            MaxValue.Value = ViewModel.Value.MaxValue.Value;
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
