using boilersGraphics.Extensions;
using boilersGraphics.Properties;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using R3;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ZLinq;

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
                    _scrollViewer = windows.AsValueEnumerable().OfType<Prism.Services.Dialogs.DialogWindow>()
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

        public string Title => Resources.Title_NowLoading;
        public event Action<IDialogResult>? RequestClose;

        public BindableReactiveProperty<double> Maximum { get; } = new BindableReactiveProperty<double>();
        public BindableReactiveProperty<double> Current { get; } = new BindableReactiveProperty<double>();
        public BindableReactiveProperty<string> Output { get; } = new BindableReactiveProperty<string>();
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
