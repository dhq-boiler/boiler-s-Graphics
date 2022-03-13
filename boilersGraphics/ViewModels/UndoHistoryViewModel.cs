using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.Properties;
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

        public static UndoHistoryViewModel Instance { get; private set; }

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
            Instance = this;
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
                var undoList = operations.Skip(index + 1).Take(operations.Count() - index);
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
                var redoList = A(operations, index);
                foreach (var redo in redoList)
                {
                    var poped = mainWindowViewModel.Controller.UndoStack.Redos.Value.Pop();
                    Debug.Assert(poped == redo);
                    poped.RollForward();
                    mainWindowViewModel.Controller.UndoStack.Undos.Value.Push(redo);
                }
                CurrentPosition.Value = mainWindowViewModel.Controller.UndoStack.Undos.Value.Count() - 1;
            })
            .AddTo(compositeDisposable);
            ContextMenuOpeningCommand.Subscribe(args =>
            {
                ContextMenuItems.Clear();
                if (this.Operations.Value.Undos.Value.Contains(SelectedOperation.Value))
                {
                    var menuItem = new MenuItem() { Header = Resources.MenuItem_Undo_NoShortcut, Command = this.UndoCommand };
                    menuItem.SetBinding(MenuItem.CommandParameterProperty, new Binding());
                    ContextMenuItems.Add(menuItem);
                }
                if (this.Operations.Value.Redos.Value.Contains(SelectedOperation.Value))
                {
                    var menuItem = new MenuItem() { Header = Resources.MenuItem_Redo_NoShortcut, Command = this.RedoCommand };
                    menuItem.SetBinding(MenuItem.CommandParameterProperty, new Binding());
                    ContextMenuItems.Add(menuItem);
                }
            })
            .AddTo(compositeDisposable);
            Operations = Observable.Return((App.Current.MainWindow.DataContext as MainWindowViewModel).Controller.UndoStack)
                                   .ToReadOnlyReactivePropertySlim();
            CurrentPosition.Subscribe(cp =>
            {
                var mainWindowViewModel = (App.Current.MainWindow.DataContext as MainWindowViewModel);
                var history = mainWindowViewModel.Controller.UndoStack.History.Value;
                for (int i = 0; i < history.Count(); i++)
                {
                    var record = history.ElementAt(i);
                    if (i == cp)
                        record.ArrowVisibility.Value = Visibility.Visible;
                    else
                        record.ArrowVisibility.Value = Visibility.Hidden;
                }
            })
            .AddTo(compositeDisposable);
            Operations.Value.History.Subscribe(_ =>
            {
                if (Operations.Value.Count() == Operations.Value.Undos.Value.Count())
                {
                    CurrentPosition.Value = Operations.Value.Count() - 1;
                }
            })
            .AddTo(compositeDisposable);
        }

        private List<IOperation> A(List<IOperation> operations, int index)
        {
            var ret = new List<IOperation>();
            for (int i = 0; i < operations.Count(); i++)
            {
                if (i >= index)
                    ret.Add(operations[i]);
            }
            ret.Reverse();
            return ret;
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
