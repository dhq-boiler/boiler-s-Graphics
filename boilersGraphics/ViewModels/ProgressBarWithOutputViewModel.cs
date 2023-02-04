using Prism.Mvvm;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using System;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Reactive.Bindings.Extensions;

namespace boilersGraphics.ViewModels
{
    public class ProgressBarWithOutputViewModel : BindableBase, IDialogAware, IDisposable
    {
        private CompositeDisposable compositeDisposable = new CompositeDisposable();
        public ProgressBarWithOutputViewModel()
        {
            LoadedCommand.Subscribe(_ =>
            {
                while (Action == null)
                {
                    Task.Delay(100);
                }

                Action();
                RequestClose.Invoke(new DialogResult(ButtonResult.OK));
            }).AddTo(compositeDisposable);
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
        }

        public async void OnDialogOpened(IDialogParameters parameters)
        {
            Maximum.Value = parameters.GetValue<double>("Maximum");
            Action = parameters.GetValue<Action>("LoadAction");
        }

        public string Title => "プログレスバー";
        public event Action<IDialogResult>? RequestClose;

        public ReactivePropertySlim<double> Maximum { get; } = new();
        public ReactivePropertySlim<double> Current { get; } = new(0);
        public ReactivePropertySlim<string> Output { get; } = new();
        public ReactiveCommand LoadedCommand { get; } = new();
        public Action Action { get; set; }

        public void Dispose()
        {
            compositeDisposable.Dispose();
            Maximum.Dispose();
            Current.Dispose();
            Output.Dispose();
            //CloseCommand.Dispose();
        }
    }
}
