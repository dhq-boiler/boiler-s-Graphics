using Prism.Mvvm;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using System;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.Windows.Threading;
using Reactive.Bindings.Extensions;

namespace boilersGraphics.ViewModels
{
    public class ProgressBarWithOutputViewModel : BindableBase, IDialogAware, IDisposable
    {
        private Guid _id = Guid.NewGuid();
        private CompositeDisposable compositeDisposable = new();
        public ProgressBarWithOutputViewModel()
        {
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
            Action = parameters.GetValue<Func<ProgressBarWithOutputViewModel, Task>>("LoadAction");
            Task.Factory.StartNew(() =>
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    Action(this);
                }, DispatcherPriority.ApplicationIdle);
                App.Current.Dispatcher.Invoke(() =>
                {
                    RequestClose.Invoke((new DialogResult(ButtonResult.OK)));
                }, DispatcherPriority.ApplicationIdle);
            });
        }

        public string Title => "プログレスバー";
        public event Action<IDialogResult>? RequestClose;

        public ReactivePropertySlim<double> Maximum { get; } = new ReactivePropertySlim<double>(mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe);
        public ReactivePropertySlim<double> Current { get; } = new ReactivePropertySlim<double>(mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe);
        public ReactivePropertySlim<string> Output { get; } = new ReactivePropertySlim<string>(mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe);
        public Func<ProgressBarWithOutputViewModel, Task> Action { get; set; }

        public void Dispose()
        {
            compositeDisposable.Dispose();
            Maximum.Dispose();
            Current.Dispose();
            Output.Dispose();
        }
    }
}
