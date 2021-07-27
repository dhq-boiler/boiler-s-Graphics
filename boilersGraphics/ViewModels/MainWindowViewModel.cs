using boilersGraphics.Helpers;
using boilersGraphics.Views;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Windows;

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

            DiagramViewModel.EnableMiniMap.Value = true;

            DiagramViewModel.FileName.Subscribe(x =>
            {
                Title.Value = $"{x}\t{App.GetAppNameAndVersion()}";
            })
            .AddTo(_CompositeDisposable);
            DiagramViewModel.FileName.Value = "*";

            EdgeThicknessOptions.Add(0.0);
            EdgeThicknessOptions.Add(1.0);
            EdgeThicknessOptions.Add(2.0);
            EdgeThicknessOptions.Add(3.0);
            EdgeThicknessOptions.Add(4.0);
            EdgeThicknessOptions.Add(5.0);
            EdgeThicknessOptions.Add(10.0);
            EdgeThicknessOptions.Add(15.0);
            EdgeThicknessOptions.Add(20.0);
            EdgeThicknessOptions.Add(25.0);
            EdgeThicknessOptions.Add(30.0);
            EdgeThicknessOptions.Add(35.0);
            EdgeThicknessOptions.Add(40.0);
            EdgeThicknessOptions.Add(45.0);
            EdgeThicknessOptions.Add(50.0);
            EdgeThicknessOptions.Add(100.0);

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
                        foreach (var item in DiagramViewModel.SelectedItems.Value.OfType<DesignerItemViewModelBase>())
                        {
                            item.EdgeColor.Value = exchange.New.Value;
                        }
                        foreach (var item in DiagramViewModel.SelectedItems.Value.OfType<ConnectorBaseViewModel>())
                        {
                            item.EdgeColor.Value = exchange.New.Value;
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
                        foreach (var item in DiagramViewModel.SelectedItems.Value.OfType<DesignerItemViewModelBase>())
                        {
                            item.FillColor = exchange.New.Value;
                        }
                    }
                }
            });
            ExitApplicationCommand = new DelegateCommand(() =>
            {
                Application.Current.Shutdown();
            });
            SwitchMiniMapCommand = new DelegateCommand(() =>
            {
                DiagramViewModel.EnableMiniMap.Value = !DiagramViewModel.EnableMiniMap.Value;
            });
            DiagramViewModel.EdgeThickness.Subscribe(x =>
            {
                if (x.HasValue && !double.IsNaN(x.Value) && DiagramViewModel.SelectedItems.Value != null)
                {
                    foreach (var item in DiagramViewModel.SelectedItems.Value?.OfType<DesignerItemViewModelBase>())
                    {
                        item.EdgeThickness.Value = x.Value;
                    }

                    foreach (var item in DiagramViewModel.SelectedItems.Value?.OfType<ConnectorBaseViewModel>())
                    {
                        item.EdgeThickness.Value = x.Value;
                    }
                }
            })
            .AddTo(_CompositeDisposable);

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

        public ReactiveCollection<double> EdgeThicknessOptions { get; } = new ReactiveCollection<double>();

        public ReactiveProperty<string> Title { get; } = new ReactiveProperty<string>();

        public DelegateCommand<object> DeleteSelectedItemsCommand { get; private set; }

        public DelegateCommand<DiagramViewModel> SelectColorCommand { get; }

        public DelegateCommand<DiagramViewModel> SelectFillColorCommand { get; }

        public DelegateCommand ExitApplicationCommand { get; }

        public DelegateCommand SwitchMiniMapCommand { get; }

        private void ExecuteDeleteSelectedItemsCommand(object parameter)
        {
            var itemsToRemove = DiagramViewModel.SelectedItems.Value.ToList();
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
