using boilersGraphics.Extensions;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace boilersGraphics.ViewModels
{
    public class ProgressBarWithOutputViewModel : BindableBase, IDialogAware, IDisposable
    {
        private Guid _id = Guid.NewGuid();
        private CompositeDisposable compositeDisposable = new();
        private ScrollViewer _scrollViewer;
        public ProgressBarWithOutputViewModel()
        {
            ScrollCommand.Subscribe(args =>
            {
                if (_scrollViewer is null)
                {
                    var windows = new Window[App.Current.Windows.Count];
                    App.Current.Windows.CopyTo(windows, 0);
                    _scrollViewer = windows.OfType<Prism.Services.Dialogs.DialogWindow>()
                    .Select(x =>
                    {
                        return x.GetVisualChild<ScrollViewer>();
                    }).FirstOrDefault(x => x.Name == "ProgressBarWithOutput_ScrollViewer");
                }
                _scrollViewer?.ScrollToBottom();
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
        public ReactiveCommand<RoutedEventArgs> ScrollCommand { get; } = new();

        public void Dispose()
        {
            compositeDisposable.Dispose();
            Maximum.Dispose();
            Current.Dispose();
            Output.Dispose();
        }
    }
}
