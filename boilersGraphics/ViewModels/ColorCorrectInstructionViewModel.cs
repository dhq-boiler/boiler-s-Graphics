using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.Properties;
using boilersGraphics.Views;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;

namespace boilersGraphics.ViewModels
{
    public class ColorCorrectInstructionViewModel : BindableBase, IDialogAware, IDisposable
    {
        private CompositeDisposable disposable = new CompositeDisposable();
        private bool disposedValue;
        private readonly IRegionManager _regionManager;

        public string Title => Resources.Title_ColorCorrection;

        public event Action<IDialogResult> RequestClose;


        public ReactiveCommand<RoutedEventArgs> UnloadedCommand { get; } = new();
        public ReactivePropertySlim<int> OKTabIndex { get; } = new();
        public ReactivePropertySlim<ColorCorrectViewModel> ViewModel { get; } = new();

        public ReactiveCommand OKCommand { get; } = new();
        public ReactiveCommand<SelectionChangedEventArgs> CCTypeChangedCommand { get; } = new();

        public ColorCorrectInstructionViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            UnloadedCommand.Subscribe(x => { _regionManager.Regions.Remove("ColorCorrectInstructionRegion"); })
                .AddTo(disposable);
            CCTypeChangedCommand.Subscribe(x =>
            {
                ViewModel.Value.CCType.Value = (ColorCorrectType)x.AddedItems[0];
                if (ViewModel.Value.CCType.Value == ColorCorrectType.ToneCurve)
                {
                    foreach (var curve in ViewModel.Value.Curves)
                    {
                        var landmarkControls = System.Windows.Window.GetWindow(x.Source as Grid).EnumerateChildOfType<LandmarkControl>();
                        foreach (var landmark in landmarkControls)
                        {
                            curve.InOutPairs = Curve.CalcInOutPairs(landmark).ToObservable().ToReactiveCollection();
                        }
                    }
                }
                ViewModel.Value.Render();
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
                    case Hsv hsv:
                        _regionManager.RequestNavigate("ColorCorrectInstructionRegion", nameof(Views.ColorCorrect.Hsv),
                            new NavigationParameters()
                            {
                                { "ViewModel", ViewModel.Value },
                            });
                        break;
                    case ToneCurve toneCurve:
                        _regionManager.RequestNavigate("ColorCorrectInstructionRegion", nameof(Views.ColorCorrect.ToneCurve),
                            new NavigationParameters()
                            {
                                { "ViewModel", ViewModel.Value },
                            });
                        break;
                    case NegativePositiveConversion npc:
                        _regionManager.RequestNavigate("ColorCorrectInstructionRegion", nameof(Views.ColorCorrect.NegativePositiveConversion),
                            new NavigationParameters()
                            {
                                { "ViewModel", ViewModel.Value },
                            });
                        break;
                    case Binarization binarize:
                        _regionManager.RequestNavigate("ColorCorrectInstructionRegion", nameof(Views.ColorCorrect.Binarization),
                            new NavigationParameters()
                            {
                                { "ViewModel", ViewModel.Value },
                            });
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
