using boilersGraphics.Dao;
using boilersGraphics.Dao.Migration.Plan;
using boilersGraphics.Helpers;
using boilersGraphics.Models;
using boilersGraphics.Views;
using Homura.Core;
using Homura.ORM;
using Homura.ORM.Setup;
using NLog;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Windows;
using TsOperationHistory;
using TsOperationHistory.Extensions;

namespace boilersGraphics.ViewModels
{
    public class MainWindowViewModel : BindableBase, IDisposable
    {
        private DiagramViewModel _DiagramViewModel;
        private ToolBarViewModel _ToolBarViewModel;
        private CompositeDisposable _CompositeDisposable = new CompositeDisposable();
        private IDialogService dlgService = null;
        private DateTime _StartUpTime;

        public static MainWindowViewModel Instance { get; set; }

        public MainWindowViewModel()
        {
            Instance = this;
        }

        public MainWindowViewModel(IDialogService dialogService)
        {
            Instance = this;
            this.dlgService = dialogService;

            if (App.IsTest)
            {
                ConnectionManager.SetDefaultConnection($"DataSource=bg.db", typeof(SQLiteConnection));
            }
            else
            {
                var dbDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"dhq_boiler\boilersGraphics");
                var dbFilePath = Path.Combine(dbDirectory, "bg.db");
                Directory.CreateDirectory(dbDirectory);
                ConnectionManager.SetDefaultConnection($"DataSource={dbFilePath}", typeof(SQLiteConnection));
            }

            ManagebGDB();

            _StartUpTime = DateTime.Now;

            Recorder = new OperationRecorder(Controller);

            DiagramViewModel = new DiagramViewModel(this, this.dlgService, 1000, 1000);
            _CompositeDisposable.Add(DiagramViewModel);
            ToolBarViewModel = new ToolBarViewModel(dialogService);

            DiagramViewModel.EnableMiniMap.Value = true;
            DiagramViewModel.EnableBrushThickness.Value = true;

            Title.Value = $"{App.GetAppNameAndVersion()}";
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
                        Recorder.BeginRecode();
                        DiagramViewModel.EdgeColors.ToClearOperation().ExecuteTo(Recorder.Current);
                        DiagramViewModel.EdgeColors.ToAddOperation(exchange.New.Value).ExecuteTo(Recorder.Current);
                        foreach (var item in DiagramViewModel.SelectedItems.Value.OfType<SelectableDesignerItemViewModelBase>())
                        {
                            if (item is SnapPointViewModel snapPoint)
                                Recorder.Current.ExecuteSetProperty(snapPoint.Parent.Value, "EdgeColor.Value", exchange.New.Value);
                            else
                                Recorder.Current.ExecuteSetProperty(item, "EdgeColor.Value", exchange.New.Value);
                        }
                        Recorder.EndRecode();
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
                        Recorder.BeginRecode();
                        DiagramViewModel.FillColors.ToClearOperation().ExecuteTo(Recorder.Current);
                        DiagramViewModel.FillColors.ToAddOperation(exchange.New.Value).ExecuteTo(Recorder.Current);
                        foreach (var item in DiagramViewModel.SelectedItems.Value.OfType<DesignerItemViewModelBase>())
                        {
                            Recorder.Current.ExecuteSetProperty(item, "FillColor.Value", exchange.New.Value);
                        }
                        Recorder.EndRecode();
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
            SwitchBrushThicknessCommand = new DelegateCommand(() =>
            {
                DiagramViewModel.EnableBrushThickness.Value = !DiagramViewModel.EnableBrushThickness.Value;
                if (!DiagramViewModel.EnableBrushThickness.Value)
                {
                    BrushViewModel.CloseAllThicknessDialog();
                }
            });
            ShowLogCommand = new DelegateCommand(() =>
            {
                var p = new Process();
                p.StartInfo = new ProcessStartInfo(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "dhq_boiler\\boilersGraphics\\Logs\\boilersGraphics.log"))
                {
                    UseShellExecute = true
                };
                p.Start();
                UpdateStatisticsCountOpenApplicationLog();
            });
            ShowVersionCommand = new DelegateCommand(() =>
            {
                IDialogResult result = null;
                this.dlgService.ShowDialog(nameof(Views.Version), ret => result = ret);
            });
            SetLogLevelCommand = new DelegateCommand<LogLevel>(parameter =>
            {
                LogLevel.Value = parameter;
            });
            ShowStatisticsCommand = new DelegateCommand(() =>
            {
                IDialogResult result = null;
                this.dlgService.ShowDialog(nameof(Views.Statistics), new DialogParameters() { { "MainWindowViewModel", this } }, ret => result = ret);
            });
            DiagramViewModel.EdgeThickness.Subscribe(x =>
            {
                if (x.HasValue && !double.IsNaN(x.Value) && DiagramViewModel.SelectedItems.Value != null)
                {
                    Recorder.BeginRecode();
                    foreach (var item in DiagramViewModel.SelectedItems.Value?.OfType<SelectableDesignerItemViewModelBase>())
                    {
                        if (item is SnapPointViewModel snapPoint)
                            Recorder.Current.ExecuteSetProperty(snapPoint.Parent.Value, "EdgeThickness.Value", x.Value);
                        else
                            Recorder.Current.ExecuteSetProperty(item, "EdgeThickness.Value", x.Value);
                    }
                    Recorder.EndRecode();
                }
            })
            .AddTo(_CompositeDisposable);

            SnapPower.Value = 10;

            IncrementNumberOfBoots();

            DiagramViewModel.Initialize();

            var updateTicks = 0L;

            var id = Guid.Parse("00000000-0000-0000-0000-000000000000");
            var dao = new StatisticsDao();
            var statistics = dao.FindBy(new Dictionary<string, object>() { { "ID", id } });
            var statisticsObj = statistics.First();
            Statistics.Value = statisticsObj;
            updateTicks = statisticsObj.UptimeTicks;

            LogLevel.Subscribe(x =>
            {
                foreach (var rule in LogManager.Configuration.LoggingRules)
                {
                    rule.EnableLoggingForLevel(x);
                }
                LogManager.ReconfigExistingLoggers();

                var dao = new LogSettingDao();
                var id = Guid.Parse("00000000-0000-0000-0000-000000000000");
                var logSettings = dao.FindBy(new Dictionary<string, object>() { { "ID", id } });
                if (logSettings.Count() == 1)
                {
                    var finished = false;
                    do
                    {
                        try
                        {
                            var logSetting = logSettings.First();
                            logSetting.LogLevel = LogLevel.Value.ToString();
                            dao.Update(logSetting);
                            finished = true;
                        }
                        catch (SQLiteException ex)
                        {
                            LogManager.GetCurrentClassLogger().Warn(ex);
                            LogManager.GetCurrentClassLogger().Warn("The app will continue to update logSettings after sleep 10 seconds of sleep.");
                            Thread.Sleep(10000);
                        }
                    }
                    while (!finished);
                }

                LogManager.GetCurrentClassLogger().Info($"ログレベルが変更されました。変更後：{x}");
                UpdateStatisticsCountLogLevelChanged();
            })
            .AddTo(_CompositeDisposable);

            Observable.Interval(TimeSpan.FromSeconds(1))
                      .Subscribe(_ =>
                      {
                          Statistics.Value.UptimeTicks = ((DateTime.Now - _StartUpTime) + TimeSpan.FromTicks(updateTicks)).Ticks;
                      })
                      .AddTo(_CompositeDisposable);

            Observable.Interval(TimeSpan.FromSeconds(10))
                      .Subscribe(_ =>
                      {
                          try
                          {
                              var dao = new StatisticsDao();
                              dao.Update(Statistics.Value);
                          }
                          catch (SQLiteException ex)
                          {
                              LogManager.GetCurrentClassLogger().Warn(ex);
                          }
                      })
                      .AddTo(_CompositeDisposable);
        }

        private void UpdateStatisticsCountLogLevelChanged()
        {
            var statistics = Statistics.Value;
            statistics.NumberOfLogLevelChanges++;
            var dao = new StatisticsDao();
            dao.Update(statistics);
        }

        private static void UpdateStatisticsCountOpenApplicationLog()
        {
            var statistics = (App.Current.MainWindow.DataContext as MainWindowViewModel).Statistics.Value;
            statistics.NumberOfTimesTheApplicationLogWasDisplayed++;
            var dao = new StatisticsDao();
            dao.Update(statistics);
        }

        private void ManagebGDB()
        {
            var dvManager = new DataVersionManager();
            dvManager.CurrentConnection = ConnectionManager.DefaultConnection;
            dvManager.Mode = VersioningStrategy.ByTick;
            dvManager.RegisterChangePlan(new ChangePlan_bG_VersionOrigin());
            dvManager.RegisterChangePlan(new ChangePlan_bG_Version1());
            dvManager.FinishedToUpgradeTo += DvManager_FinishedToUpgradeTo;

            dvManager.UpgradeToTargetVersion();
        }

        private void DvManager_FinishedToUpgradeTo(object sender, ModifiedEventArgs e)
        {
            LogManager.GetCurrentClassLogger().Info($"Heavy Modifying AppDB Count : {e.ModifiedCount}");

            if (e.ModifiedCount > 0)
            {
                SQLiteBaseDao<Dummy>.Vacuum(ConnectionManager.DefaultConnection);
            }
        }

        private void IncrementNumberOfBoots()
        {
            var id = Guid.Parse("00000000-0000-0000-0000-000000000000");
            var dao = new StatisticsDao();
            var statistics = dao.FindBy(new Dictionary<string, object>() { { "ID", id } });
            if (statistics.Count() == 0)
            {
                var newStatistics = new Models.Statistics();
                newStatistics.ID = id;
                newStatistics.NumberOfBoots = 1;
                dao.Insert(newStatistics);
            }
            else
            {
                var existStatistics = statistics.First();
                existStatistics.NumberOfBoots += 1;
                dao.Update(existStatistics);
            }
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

        public ReactivePropertySlim<string> CurrentOperation { get; } = new ReactivePropertySlim<string>();

        public ReactivePropertySlim<string> Details { get; } = new ReactivePropertySlim<string>();
        public ReactivePropertySlim<string> Message { get; } = new ReactivePropertySlim<string>();

        public ReactivePropertySlim<double> SnapPower { get; } = new ReactivePropertySlim<double>();

        public ReactiveCollection<double> EdgeThicknessOptions { get; } = new ReactiveCollection<double>();

        public ReactivePropertySlim<string> Title { get; } = new ReactivePropertySlim<string>();

        public ReactivePropertySlim<Models.Statistics> Statistics { get; } = new ReactivePropertySlim<Models.Statistics>();

        public IOperationController Controller { get; } = new OperationController();

        public OperationRecorder Recorder { get; }

        public ReactivePropertySlim<LogLevel> LogLevel { get; } = new ReactivePropertySlim<LogLevel>();

        public DelegateCommand<object> DeleteSelectedItemsCommand { get; private set; }

        public DelegateCommand<DiagramViewModel> SelectColorCommand { get; }

        public DelegateCommand<DiagramViewModel> SelectFillColorCommand { get; }

        public DelegateCommand ExitApplicationCommand { get; }

        public DelegateCommand SwitchMiniMapCommand { get; }

        public DelegateCommand SwitchBrushThicknessCommand { get; }

        public DelegateCommand ShowLogCommand { get; }

        public DelegateCommand ShowVersionCommand { get; }

        public DelegateCommand<LogLevel> SetLogLevelCommand { get; }

        public DelegateCommand ShowStatisticsCommand { get; }

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
            Instance = null;
        }

        #endregion //IDisposable
    }
}
