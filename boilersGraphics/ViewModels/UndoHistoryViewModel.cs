using boilersGraphics.Views;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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
        public ReactiveCommand<IOperation> UndoCommand { get; } = new ReactiveCommand<IOperation>();
        public ReactiveCommand<IOperation> RedoCommand { get; } = new ReactiveCommand<IOperation>();
        public ReactiveCollection<MenuItem> ContextMenuItems { get; } = new ReactiveCollection<MenuItem>();
        public ReactiveCommand<RoutedEventArgs> ContextMenuOpeningCommand { get; } = new ReactiveCommand<RoutedEventArgs>();
        public ReadOnlyReactivePropertySlim<UndoStack<IOperation>> Operations { get; }
        public ReactivePropertySlim<int> CurrentPosition { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<object> SelectedOperation { get; } = new ReactivePropertySlim<object>();

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
            UndoCommand.Subscribe(operation =>
            {
                var mainWindowViewModel = (App.Current.MainWindow.DataContext as MainWindowViewModel);
                List<IOperation> operations = mainWindowViewModel.Controller.UndoStack.Undos.Value.ToList();
                var index = operations.IndexOf(operation);
                var undoList = operations.Skip(index).Take(operations.Count() - index);
                undoList = undoList.Reverse().ToList();
                foreach (var undo in undoList)
                {
                    var poped = mainWindowViewModel.Controller.UndoStack.Undos.Value.Pop();
                    Debug.Assert(poped == undo);
                    poped.Rollback();
                    mainWindowViewModel.Controller.UndoStack.Redos.Value.Push(undo);
                }
                CurrentPosition.Value = index;
            })
            .AddTo(compositeDisposable);
            RedoCommand.Subscribe(operation =>
            {
                var mainWindowViewModel = (App.Current.MainWindow.DataContext as MainWindowViewModel);
                List<IOperation> operations = mainWindowViewModel.Controller.UndoStack.Redos.Value.ToList();
                var index = operations.IndexOf(operation);
                var redoList = operations.Take(operations.Count() - index);
                redoList = redoList.Reverse().ToList();
                foreach (var redo in redoList)
                {
                    var poped = mainWindowViewModel.Controller.UndoStack.Redos.Value.Pop();
                    Debug.Assert(poped == redo);
                    poped.RollForward();
                    mainWindowViewModel.Controller.UndoStack.Undos.Value.Push(redo);
                }
            })
            .AddTo(compositeDisposable);
            ContextMenuOpeningCommand.Subscribe(args =>
            {
                ContextMenuItems.Clear();
                if (this.Operations.Value.Undos.Value.Contains(SelectedOperation.Value))
                {
                    var menuItem = new MenuItem() { Header = "元に戻す", Command = this.UndoCommand };
                    menuItem.SetBinding(MenuItem.CommandParameterProperty, new Binding());
                    ContextMenuItems.Add(menuItem);
                }
                if (this.Operations.Value.Redos.Value.Contains(SelectedOperation.Value))
                {
                    var menuItem = new MenuItem() { Header = "やり直す", Command = this.RedoCommand };
                    menuItem.SetBinding(MenuItem.CommandParameterProperty, new Binding());
                    ContextMenuItems.Add(menuItem);
                }
            })
            .AddTo(compositeDisposable);
            Operations = Observable.Return((App.Current.MainWindow.DataContext as MainWindowViewModel).Controller.UndoStack)
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
