using boilersGraphics.Helpers;
using boilersGraphics.Properties;
using boilersGraphics.Views;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Media;

namespace boilersGraphics.ViewModels
{
    class PreferenceViewModel : BindableBase, IDialogAware, IDisposable
    {
        private IDialogService dlgService;
        private bool disposedValue;
        private CompositeDisposable _disposables = new CompositeDisposable();

        public ReactivePropertySlim<ReactiveCommand> OkCommand { get; set; } = new ReactivePropertySlim<ReactiveCommand>();
        public ReactiveCommand CancelCommand { get; set; }
        public ReactiveCommand ChangeCanvasBackgroundCommand { get; set; } = new ReactiveCommand();

        public ReactivePropertySlim<Models.Preference> EditTarget { get; set; } = new ReactivePropertySlim<Models.Preference>();

        public PreferenceViewModel(IDialogService dialogService)
        {
            this.dlgService = dialogService;
            EditTarget.Value = new Models.Preference();
            CancelCommand = new ReactiveCommand();
            CancelCommand.Subscribe(_ =>
            {
                var ret = new DialogResult(ButtonResult.Cancel, null);
                RequestClose.Invoke(ret);
            })
            .AddTo(_disposables);
            ChangeCanvasBackgroundCommand.Subscribe(_ =>
            {
                IDialogResult result = null;
                this.dlgService.ShowDialog(nameof(ColorPicker),
                                           new DialogParameters()
                                           {
                                               {
                                                   "ColorExchange",
                                                   new ColorExchange()
                                                   {
                                                       Old = EditTarget.Value.CanvasBackground.Value
                                                   }
                                               },
                                               {
                                                   "ColorSpots",
                                                   (App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel.ColorSpots.Value
                                               }
                                           },
                                           ret => result = ret);
                if (result != null)
                {
                    var exchange = result.Parameters.GetValue<ColorExchange>("ColorExchange");
                    if (exchange != null)
                    {
                        EditTarget.Value.CanvasBackground.Value = exchange.New;
                    }
                }
            })
            .AddTo(_disposables);
            EditTarget.Subscribe(_ =>
            {
                OkCommand.Value = EditTarget.Value
                             .Width
                             .CombineLatest(EditTarget.Value.Height, (x, y) => x * y)
                             .Select(x => x > 0)
                             .ToReactiveCommand();
                OkCommand.Value.Subscribe(__ =>
                {
                    var parameters = new DialogParameters() { { "Preferences", EditTarget.Value } };
                    var ret = new DialogResult(ButtonResult.OK, parameters);
                    RequestClose.Invoke(ret);
                })
                .AddTo(_disposables);
            })
            .AddTo(_disposables);
        }

        public string Title => Resources.Title_Preferences;

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
            EditTarget.Value = parameters.GetValue<Models.Preference>("Preferences");
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
