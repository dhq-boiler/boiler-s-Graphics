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
                regionManager.RequestNavigate("ColorPickerRegion", nameof(SolidColorPicker), new NavigationParameters()
                {
                    { "ColorExchange", EditTarget.Value },
                    { "ColorSpots", ColorSpots.Value }
                });
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
                var colorSpots = new ColorSpots();
                colorSpots.ColorSpot0 = solidColorPickerViewModel.ColorSpot0.Value;
                colorSpots.ColorSpot1 = solidColorPickerViewModel.ColorSpot1.Value;
                colorSpots.ColorSpot2 = solidColorPickerViewModel.ColorSpot2.Value;
                colorSpots.ColorSpot3 = solidColorPickerViewModel.ColorSpot3.Value;
                colorSpots.ColorSpot4 = solidColorPickerViewModel.ColorSpot4.Value;
                colorSpots.ColorSpot5 = solidColorPickerViewModel.ColorSpot5.Value;
                colorSpots.ColorSpot6 = solidColorPickerViewModel.ColorSpot6.Value;
                colorSpots.ColorSpot7 = solidColorPickerViewModel.ColorSpot7.Value;
                colorSpots.ColorSpot8 = solidColorPickerViewModel.ColorSpot8.Value;
                colorSpots.ColorSpot9 = solidColorPickerViewModel.ColorSpot9.Value;
                colorSpots.ColorSpot10 = solidColorPickerViewModel.ColorSpot10.Value;
                colorSpots.ColorSpot11 = solidColorPickerViewModel.ColorSpot11.Value;
                colorSpots.ColorSpot12 = solidColorPickerViewModel.ColorSpot12.Value;
                colorSpots.ColorSpot13 = solidColorPickerViewModel.ColorSpot13.Value;
                colorSpots.ColorSpot14 = solidColorPickerViewModel.ColorSpot14.Value;
                colorSpots.ColorSpot15 = solidColorPickerViewModel.ColorSpot15.Value;
                colorSpots.ColorSpot16 = solidColorPickerViewModel.ColorSpot16.Value;
                colorSpots.ColorSpot17 = solidColorPickerViewModel.ColorSpot17.Value;
                colorSpots.ColorSpot18 = solidColorPickerViewModel.ColorSpot18.Value;
                colorSpots.ColorSpot19 = solidColorPickerViewModel.ColorSpot19.Value;
                colorSpots.ColorSpot20 = solidColorPickerViewModel.ColorSpot20.Value;
                colorSpots.ColorSpot21 = solidColorPickerViewModel.ColorSpot21.Value;
                colorSpots.ColorSpot22 = solidColorPickerViewModel.ColorSpot22.Value;
                colorSpots.ColorSpot23 = solidColorPickerViewModel.ColorSpot23.Value;
                colorSpots.ColorSpot24 = solidColorPickerViewModel.ColorSpot24.Value;
                colorSpots.ColorSpot25 = solidColorPickerViewModel.ColorSpot25.Value;
                colorSpots.ColorSpot26 = solidColorPickerViewModel.ColorSpot26.Value;
                colorSpots.ColorSpot27 = solidColorPickerViewModel.ColorSpot27.Value;
                colorSpots.ColorSpot28 = solidColorPickerViewModel.ColorSpot28.Value;
                colorSpots.ColorSpot29 = solidColorPickerViewModel.ColorSpot29.Value;
                colorSpots.ColorSpot30 = solidColorPickerViewModel.ColorSpot30.Value;
                colorSpots.ColorSpot31 = solidColorPickerViewModel.ColorSpot31.Value;
                colorSpots.ColorSpot32 = solidColorPickerViewModel.ColorSpot32.Value;
                colorSpots.ColorSpot33 = solidColorPickerViewModel.ColorSpot33.Value;
                colorSpots.ColorSpot34 = solidColorPickerViewModel.ColorSpot34.Value;
                colorSpots.ColorSpot35 = solidColorPickerViewModel.ColorSpot35.Value;
                colorSpots.ColorSpot36 = solidColorPickerViewModel.ColorSpot36.Value;
                colorSpots.ColorSpot37 = solidColorPickerViewModel.ColorSpot37.Value;
                colorSpots.ColorSpot38 = solidColorPickerViewModel.ColorSpot38.Value;
                colorSpots.ColorSpot39 = solidColorPickerViewModel.ColorSpot39.Value;
                colorSpots.ColorSpot40 = solidColorPickerViewModel.ColorSpot40.Value;
                colorSpots.ColorSpot41 = solidColorPickerViewModel.ColorSpot41.Value;
                colorSpots.ColorSpot42 = solidColorPickerViewModel.ColorSpot42.Value;
                colorSpots.ColorSpot43 = solidColorPickerViewModel.ColorSpot43.Value;
                colorSpots.ColorSpot44 = solidColorPickerViewModel.ColorSpot44.Value;
                colorSpots.ColorSpot45 = solidColorPickerViewModel.ColorSpot45.Value;
                colorSpots.ColorSpot46 = solidColorPickerViewModel.ColorSpot46.Value;
                colorSpots.ColorSpot47 = solidColorPickerViewModel.ColorSpot47.Value;
                colorSpots.ColorSpot48 = solidColorPickerViewModel.ColorSpot48.Value;
                colorSpots.ColorSpot49 = solidColorPickerViewModel.ColorSpot49.Value;
                colorSpots.ColorSpot50 = solidColorPickerViewModel.ColorSpot50.Value;
                colorSpots.ColorSpot51 = solidColorPickerViewModel.ColorSpot51.Value;
                colorSpots.ColorSpot52 = solidColorPickerViewModel.ColorSpot52.Value;
                colorSpots.ColorSpot53 = solidColorPickerViewModel.ColorSpot53.Value;
                colorSpots.ColorSpot54 = solidColorPickerViewModel.ColorSpot54.Value;
                colorSpots.ColorSpot55 = solidColorPickerViewModel.ColorSpot55.Value;
                colorSpots.ColorSpot56 = solidColorPickerViewModel.ColorSpot56.Value;
                colorSpots.ColorSpot57 = solidColorPickerViewModel.ColorSpot57.Value;
                colorSpots.ColorSpot58 = solidColorPickerViewModel.ColorSpot58.Value;
                colorSpots.ColorSpot59 = solidColorPickerViewModel.ColorSpot59.Value;
                colorSpots.ColorSpot60 = solidColorPickerViewModel.ColorSpot60.Value;
                colorSpots.ColorSpot61 = solidColorPickerViewModel.ColorSpot61.Value;
                colorSpots.ColorSpot62 = solidColorPickerViewModel.ColorSpot62.Value;
                colorSpots.ColorSpot63 = solidColorPickerViewModel.ColorSpot63.Value;
                colorSpots.ColorSpot64 = solidColorPickerViewModel.ColorSpot64.Value;
                colorSpots.ColorSpot65 = solidColorPickerViewModel.ColorSpot65.Value;
                colorSpots.ColorSpot66 = solidColorPickerViewModel.ColorSpot66.Value;
                colorSpots.ColorSpot67 = solidColorPickerViewModel.ColorSpot67.Value;
                colorSpots.ColorSpot68 = solidColorPickerViewModel.ColorSpot68.Value;
                colorSpots.ColorSpot69 = solidColorPickerViewModel.ColorSpot69.Value;
                colorSpots.ColorSpot70 = solidColorPickerViewModel.ColorSpot70.Value;
                colorSpots.ColorSpot71 = solidColorPickerViewModel.ColorSpot71.Value;
                colorSpots.ColorSpot72 = solidColorPickerViewModel.ColorSpot72.Value;
                colorSpots.ColorSpot73 = solidColorPickerViewModel.ColorSpot73.Value;
                colorSpots.ColorSpot74 = solidColorPickerViewModel.ColorSpot74.Value;
                colorSpots.ColorSpot75 = solidColorPickerViewModel.ColorSpot75.Value;
                colorSpots.ColorSpot76 = solidColorPickerViewModel.ColorSpot76.Value;
                colorSpots.ColorSpot77 = solidColorPickerViewModel.ColorSpot77.Value;
                colorSpots.ColorSpot78 = solidColorPickerViewModel.ColorSpot78.Value;
                colorSpots.ColorSpot79 = solidColorPickerViewModel.ColorSpot79.Value;
                colorSpots.ColorSpot80 = solidColorPickerViewModel.ColorSpot80.Value;
                colorSpots.ColorSpot81 = solidColorPickerViewModel.ColorSpot81.Value;
                colorSpots.ColorSpot82 = solidColorPickerViewModel.ColorSpot82.Value;
                colorSpots.ColorSpot83 = solidColorPickerViewModel.ColorSpot83.Value;
                colorSpots.ColorSpot84 = solidColorPickerViewModel.ColorSpot84.Value;
                colorSpots.ColorSpot85 = solidColorPickerViewModel.ColorSpot85.Value;
                colorSpots.ColorSpot86 = solidColorPickerViewModel.ColorSpot86.Value;
                colorSpots.ColorSpot87 = solidColorPickerViewModel.ColorSpot87.Value;
                colorSpots.ColorSpot88 = solidColorPickerViewModel.ColorSpot88.Value;
                colorSpots.ColorSpot89 = solidColorPickerViewModel.ColorSpot89.Value;
                colorSpots.ColorSpot90 = solidColorPickerViewModel.ColorSpot90.Value;
                colorSpots.ColorSpot91 = solidColorPickerViewModel.ColorSpot91.Value;
                colorSpots.ColorSpot92 = solidColorPickerViewModel.ColorSpot92.Value;
                colorSpots.ColorSpot93 = solidColorPickerViewModel.ColorSpot93.Value;
                colorSpots.ColorSpot94 = solidColorPickerViewModel.ColorSpot94.Value;
                colorSpots.ColorSpot95 = solidColorPickerViewModel.ColorSpot95.Value;
                colorSpots.ColorSpot96 = solidColorPickerViewModel.ColorSpot96.Value;
                colorSpots.ColorSpot97 = solidColorPickerViewModel.ColorSpot97.Value;
                colorSpots.ColorSpot98 = solidColorPickerViewModel.ColorSpot98.Value;
                colorSpots.ColorSpot99 = solidColorPickerViewModel.ColorSpot99.Value;
                var parameters = new DialogParameters()
                    {
                        { "ColorExchange", solidColorPickerViewModel.EditTarget.Value },
                        { "ColorSpots", colorSpots }
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
                regionManager.RequestNavigate("ColorPickerRegion", nameof(LinearGradientBrushPicker), new NavigationParameters()
                {
                    { "ColorSpots", ColorSpots.Value }
                });
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
