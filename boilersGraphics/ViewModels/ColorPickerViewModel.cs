using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.Models;
using boilersGraphics.Views;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Windows;

namespace boilersGraphics.ViewModels
{
    public class ColorPickerViewModel : BindableBase, IDialogAware, IDisposable
    {
        private bool disposedValue;
        private CompositeDisposable disposables = new CompositeDisposable();
        private ColorPicker _colorPicker;
        private readonly IRegionManager regionManager;

        public string Title => "カラーピッカー";

        public event Action<IDialogResult> RequestClose;

        public ReactiveCommand OKCommand { get; } = new ReactiveCommand();
        public ReactiveCommand SelectSolidColorCommand { get; } = new ReactiveCommand();
        public ReactiveCommand SelectLinearGradientCommand { get; } = new ReactiveCommand();
        public ReactiveCommand SelectRadialGradientCommand { get; } = new ReactiveCommand();
        public ReactivePropertySlim<ColorExchange> EditTarget { get; } = new ReactivePropertySlim<ColorExchange>();
        public ReactivePropertySlim<ColorSpots> ColorSpots { get; } = new ReactivePropertySlim<ColorSpots>();

        public ReactiveCommand<RoutedEventArgs> UnloadedCommand { get; } = new ReactiveCommand<RoutedEventArgs>();
        public ReactiveCommand<RoutedEventArgs> LoadedCommand { get; } = new ReactiveCommand<RoutedEventArgs>();

        public ColorPickerViewModel(IRegionManager regionManager)
        {
            LoadedCommand.Subscribe(x =>
            {
                var source = x.Source;
                _colorPicker = source as ColorPicker;
            })
            .AddTo(disposables);
            UnloadedCommand.Subscribe(x =>
            {
                this.regionManager.Regions.Remove("ColorPickerRegion");
            })
            .AddTo(disposables);
            OKCommand.Subscribe(_ =>
            {
                this.regionManager.Regions.Remove("ColorPickerRegion");
                var solidColorPicker = _colorPicker.FindVisualChildren<SolidColorPicker>().FirstOrDefault();
                var solidColorPickerViewModel = solidColorPicker.DataContext as SolidColorPickerViewModel;
                var parameters = new DialogParameters()
                    {
                        { "ColorExchange", solidColorPickerViewModel.EditTarget.Value },
                        { "ColorSpots", solidColorPickerViewModel.ColorSpots.Value }
                    };
                var ret = new DialogResult(ButtonResult.OK, parameters);
                RequestClose.Invoke(ret);
            })
            .AddTo(disposables);
            this.regionManager = regionManager;
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
            EditTarget.Value = parameters.GetValue<ColorExchange>("ColorExchange");
            ColorSpots.Value = parameters.GetValue<ColorSpots>("ColorSpots");

            SelectSolidColorCommand.Subscribe(_ =>
            {
                regionManager.RequestNavigate("ColorPickerRegion", nameof(SolidColorPicker), new NavigationParameters()
                {
                    { "ColorExchange", EditTarget.Value },
                    { "ColorSpots", ColorSpots.Value }
                });
            })
            .AddTo(disposables);
            SelectLinearGradientCommand.Subscribe(_ =>
            {
                regionManager.RequestNavigate("ColorPickerRegion", nameof(LinearGradientBrushPicker));
            })
            .AddTo(disposables);
            SelectRadialGradientCommand.Subscribe(_ =>
            {
                regionManager.RequestNavigate("ColorPickerRegion", nameof(RadialGradientBrushPicker));
            })
            .AddTo(disposables);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    SelectSolidColorCommand.Dispose();
                    SelectLinearGradientCommand.Dispose();
                    SelectRadialGradientCommand.Dispose();
                    disposables.Dispose();
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
