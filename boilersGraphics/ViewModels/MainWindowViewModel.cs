using boilersGraphics.Helpers;
using boilersGraphics.Views;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;

namespace boilersGraphics.ViewModels
{
    public class MainWindowViewModel : BindableBase, IDisposable
    {
        private DiagramViewModel _DiagramViewModel;
        private ToolBarViewModel _ToolBarViewModel;
        private CompositeDisposable _CompositeDisposable = new CompositeDisposable();
        private IDialogService dlgService = null;

        public MainWindowViewModel(IDialogService dialogService)
        {
            this.dlgService = dialogService;
            DiagramViewModel = new DiagramViewModel(this.dlgService, 1000, 1000);
            _CompositeDisposable.Add(DiagramViewModel);
            ToolBarViewModel = new ToolBarViewModel(dialogService);

            DeleteSelectedItemsCommand = new DelegateCommand<object>(p =>
            {
                ExecuteDeleteSelectedItemsCommand(p);
            });
            SelectColorCommand = new DelegateCommand<DiagramViewModel>(p =>
            {
                IDialogResult result = null;
                this.dlgService.ShowDialog(nameof(ColorPicker), 
                                           new DialogParameters()
                                           {
                                               {
                                                   "ColorExchange",
                                                   new ColorExchange()
                                                   {
                                                       Old = DiagramViewModel.EdgeColors.FirstOrDefault()
                                                   }
                                               }
                                           },
                                           ret => result = ret);
                if (result != null)
                {
                    var exchange = result.Parameters.GetValue<ColorExchange>("ColorExchange");
                    if (exchange != null)
                    {
                        DiagramViewModel.EdgeColors.Clear();
                        DiagramViewModel.EdgeColors.Add(exchange.New.Value);
                        foreach (var item in DiagramViewModel.SelectedItems.OfType<DesignerItemViewModelBase>())
                        {
                            item.EdgeColor = exchange.New.Value;
                        }
                        foreach (var item in DiagramViewModel.SelectedItems.OfType<ConnectorBaseViewModel>())
                        {
                            item.EdgeColor = exchange.New.Value;
                        }
                    }
                }
            });
            SelectFillColorCommand = new DelegateCommand<DiagramViewModel>(p =>
            {
                IDialogResult result = null;
                this.dlgService.ShowDialog(nameof(ColorPicker),
                                           new DialogParameters()
                                           {
                                               {
                                                   "ColorExchange",
                                                   new ColorExchange()
                                                   {
                                                       Old = DiagramViewModel.FillColors.FirstOrDefault()
                                                   }
                                               }
                                           },
                                           ret => result = ret);
                if (result != null)
                {
                    var exchange = result.Parameters.GetValue<ColorExchange>("ColorExchange");
                    if (exchange != null)
                    {
                        DiagramViewModel.FillColors.Clear();
                        DiagramViewModel.FillColors.Add(exchange.New.Value);
                        foreach (var item in DiagramViewModel.SelectedItems.OfType<DesignerItemViewModelBase>())
                        {
                            item.FillColor = exchange.New.Value;
                        }
                    }
                }
            });

            SnapPower.Value = 10;
        }

        public DiagramViewModel DiagramViewModel
        {
            get { return _DiagramViewModel; }
            set { SetProperty(ref _DiagramViewModel, value); }
        }

        public ToolBarViewModel ToolBarViewModel
        {
            get { return _ToolBarViewModel; }
            set { SetProperty(ref _ToolBarViewModel, value); }
        }

        public ReactiveProperty<string> CurrentOperation { get; } = new ReactiveProperty<string>();

        public ReactiveProperty<string> Details { get; } = new ReactiveProperty<string>();

        public ReactiveProperty<double> SnapPower { get; } = new ReactiveProperty<double>();

        public DelegateCommand<object> DeleteSelectedItemsCommand { get; private set; }

        public DelegateCommand<DiagramViewModel> SelectColorCommand { get; }

        public DelegateCommand<DiagramViewModel> SelectFillColorCommand { get; }

        private void ExecuteDeleteSelectedItemsCommand(object parameter)
        {
            var itemsToRemove = DiagramViewModel.SelectedItems.ToList();
            foreach (var selectedItem in itemsToRemove)
            {
                DiagramViewModel.RemoveItemCommand.Execute(selectedItem);
            }
        }

        #region IDisposable

        public void Dispose()
        {
            _CompositeDisposable.Dispose();
        }

        #endregion //IDisposable
    }
}
