using boilersGraphics.Views;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using TsOperationHistory;
using TsOperationHistory.Internal;

namespace boilersGraphics.ViewModels
{
    public class UndoHistoryViewModel : BindableBase, IDialogAware
    {
        private CompositeDisposable compositeDisposable = new CompositeDisposable();

        public event Action<IDialogResult> RequestClose;

        public ReactiveCommand<AutoScrollingLabel> MouseEnterCommand { get; } = new ReactiveCommand<AutoScrollingLabel>();
        public ReactiveCommand<AutoScrollingLabel> MouseLeaveCommand { get; } = new ReactiveCommand<AutoScrollingLabel>();

        public ReadOnlyReactivePropertySlim<UndoStack<IOperation>> Operations { get; }

        public string Title => "";

        public UndoHistoryViewModel()
        {
            MouseEnterCommand.Subscribe(label =>
            {
                label.EnableAutoScroll();
            })
            .AddTo(compositeDisposable);
            MouseLeaveCommand.Subscribe(label =>
            {
                label.DisableAutoScroll();
            })
            .AddTo(compositeDisposable);
            Operations = Observable.Return((App.Current.MainWindow.DataContext as MainWindowViewModel).Controller.Operations)
                                   .ToReadOnlyReactivePropertySlim();
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
        }
    }
}
