using grapher.Helpers;
using grapher.Views;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;

namespace grapher.ViewModels
{
    public class MainWindowViewModel : BindableBase, IDisposable
    {
        private DiagramViewModel _DiagramViewModel;
        private List<SelectableDesignerItemViewModelBase> _itemsToRemove;
        private ToolBarViewModel _ToolBarViewModel;
        private CompositeDisposable _CompositeDisposable = new CompositeDisposable();
        private IDialogService dlgService = null;

        public MainWindowViewModel(IDialogService dialogService)
        {
            this.dlgService = dialogService;
            DiagramViewModel = new DiagramViewModel(1000, 1000);
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

        public DelegateCommand<object> DeleteSelectedItemsCommand { get; private set; }

        public DelegateCommand<DiagramViewModel> SelectColorCommand { get; }

        public DelegateCommand<DiagramViewModel> SelectFillColorCommand { get; }

        private void ExecuteDeleteSelectedItemsCommand(object parameter)
        {
            _itemsToRemove = DiagramViewModel.SelectedItems.ToList();
            List<SelectableDesignerItemViewModelBase> connectionsToAlsoRemove = new List<SelectableDesignerItemViewModelBase>();

            //TODO オプション化
            //foreach (var connector in DiagramViewModel.Items.OfType<ConnectorBaseViewModel>())
            //{
            //    if (connector.SourceConnectorInfo is FullyCreatedConnectorInfo
            //        && ItemsToDeleteHasConnector(_itemsToRemove, connector.SourceConnectorInfo as FullyCreatedConnectorInfo))
            //    {
            //        connectionsToAlsoRemove.Add(connector);
            //    }

            //    if (connector.SinkConnectorInfo is FullyCreatedConnectorInfo
            //        && ItemsToDeleteHasConnector(_itemsToRemove, connector.SinkConnectorInfo as FullyCreatedConnectorInfo))
            //    {
            //        connectionsToAlsoRemove.Add(connector);
            //    }
            //}
            //_itemsToRemove.AddRange(connectionsToAlsoRemove);
            foreach (var selectedItem in _itemsToRemove)
            {
                DiagramViewModel.RemoveItemCommand.Execute(selectedItem);
            }
        }

        private bool ItemsToDeleteHasConnector(List<SelectableDesignerItemViewModelBase> itemsToRemove, FullyCreatedConnectorInfo connector)
        {
            return itemsToRemove.Contains(connector.DataItem);
        }

        #region IDisposable

        public void Dispose()
        {
            _CompositeDisposable.Dispose();
        }

        #endregion //IDisposable
    }
}
