using boilersGraphics.Controls;
using boilersGraphics.Dao;
using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.Messenger;
using boilersGraphics.Models;
using boilersGraphics.Properties;
using boilersGraphics.UserControls;
using boilersGraphics.Views;
using Microsoft.Win32;
using NLog;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Linq;
using TsOperationHistory.Extensions;

namespace boilersGraphics.ViewModels
{
    public class DiagramViewModel : BindableBase, IDiagramViewModel, IDisposable
    {
        public MainWindowViewModel MainWindowVM { get; private set; }
        private IDialogService dlgService;
        private Point _CurrentPoint;
        private ObservableCollection<Color> _FillColors = new ObservableCollection<Color>();
        private CompositeDisposable _CompositeDisposable = new CompositeDisposable();
        private int _Width;
        private int _Height;
        private double _CanvasBorderThickness;
        private bool _MiddleButtonIsPressed;
        private Point _MousePointerPosition;
        private bool disposedValue;

        public DelegateCommand<object> AddItemCommand { get; private set; }
        public DelegateCommand<object> RemoveItemCommand { get; private set; }
        public DelegateCommand<object> ClearSelectedItemsCommand { get; private set; }
        public DelegateCommand<object> CreateNewDiagramCommand { get; private set; }
        public DelegateCommand LoadCommand { get; private set; }
        public DelegateCommand<string> LoadFileCommand { get; private set; }
        public DelegateCommand SaveCommand { get; private set; }
        public DelegateCommand OverwriteCommand { get; private set; }
        public DelegateCommand ExportCommand { get; private set; }
        public DelegateCommand GroupCommand { get; private set; }
        public DelegateCommand UngroupCommand { get; private set; }
        public DelegateCommand BringForegroundCommand { get; private set; }
        public DelegateCommand BringForwardCommand { get; private set; }
        public DelegateCommand SendBackwardCommand { get; private set; }
        public DelegateCommand SendBackgroundCommand { get; private set; }
        public DelegateCommand AlignTopCommand { get; private set; }
        public DelegateCommand AlignVerticalCenterCommand { get; private set; }
        public DelegateCommand AlignBottomCommand { get; private set; }
        public DelegateCommand AlignLeftCommand { get; private set; }
        public DelegateCommand AlignHorizontalCenterCommand { get; private set; }
        public DelegateCommand AlignRightCommand { get; private set; }
        public DelegateCommand DistributeHorizontalCommand { get; private set; }
        public DelegateCommand DistributeVerticalCommand { get; private set; }
        public DelegateCommand SelectAllCommand { get; private set; }
        public DelegateCommand SettingCommand { get; private set; }
        public DelegateCommand UniformWidthCommand { get; private set; }
        public DelegateCommand UniformHeightCommand { get; private set; }
        public DelegateCommand DuplicateCommand { get; private set; }
        public DelegateCommand CutCommand { get; private set; }
        public DelegateCommand CopyCommand { get; private set; }
        public DelegateCommand PasteCommand { get; private set; }
        public DelegateCommand EditMenuOpenedCommand { get; private set; }
        public DelegateCommand UnionCommand { get; private set; }
        public DelegateCommand IntersectCommand { get; private set; }
        public DelegateCommand XorCommand { get; private set; }
        public DelegateCommand ExcludeCommand { get; private set; }
        public DelegateCommand ClipCommand { get; private set; }
        public DelegateCommand UndoCommand { get; private set; }
        public DelegateCommand RedoCommand { get; private set; }
        public DelegateCommand<MouseWheelEventArgs> MouseWheelCommand { get; private set; }
        public DelegateCommand<MouseEventArgs> PreviewMouseDownCommand { get; private set; }
        public DelegateCommand<MouseEventArgs> PreviewMouseUpCommand { get; private set; }
        public DelegateCommand<MouseEventArgs> MouseMoveCommand { get; private set; }
        public DelegateCommand<MouseEventArgs> MouseLeaveCommand { get; private set; }
        public DelegateCommand<MouseEventArgs> MouseEnterCommand { get; private set; }
        public DelegateCommand<KeyEventArgs> PreviewKeyDownCommand { get; private set; }
        public DelegateCommand PropertyCommand { get; private set; }
        public DelegateCommand<System.Windows.Shapes.Line> MouseDownStraightLineCommand { get; private set; }
        public DelegateCommand<System.Windows.Shapes.Path> MouseDownBezierCurveCommand { get; private set; }
        public DelegateCommand<System.Windows.Shapes.Path> MouseDownPolyBezierCommand { get; private set; }
        public DelegateCommand LoadedCommand { get; private set; }

        #region Property

        public ReactivePropertySlim<LayerTreeViewItemBase> RootLayer { get; set; } = new ReactivePropertySlim<LayerTreeViewItemBase>(new LayerTreeViewItemBase());

        public ReactiveCollection<LayerTreeViewItemBase> Layers { get; }

        public ReadOnlyReactivePropertySlim<LayerTreeViewItemBase[]> SelectedLayers { get; }

        public ReadOnlyReactivePropertySlim<SelectableDesignerItemViewModelBase[]> AllItems { get; }

        public ReadOnlyReactivePropertySlim<SelectableDesignerItemViewModelBase[]> SelectedItems { get; }

        public ReactivePropertySlim<BackgroundViewModel> BackgroundItem { get; } = new ReactivePropertySlim<BackgroundViewModel>();

        public ReactivePropertySlim<double?> EdgeThickness { get; } = new ReactivePropertySlim<double?>();

        public ReactivePropertySlim<bool> EnableMiniMap { get; } = new ReactivePropertySlim<bool>();

        public ReactivePropertySlim<bool> EnableBrushThickness { get; } = new ReactivePropertySlim<bool>();

        public ReactivePropertySlim<string> FileName { get; } = new ReactivePropertySlim<string>();

        public ReactivePropertySlim<Brush> CanvasBackground { get; } = new ReactivePropertySlim<Brush>();

        public ReactivePropertySlim<bool> EnablePointSnap { get; } = new ReactivePropertySlim<bool>();

        public ReactivePropertySlim<bool> EnableAutoSave { get; } = new ReactivePropertySlim<bool>();

        public ReactivePropertySlim<DateTime> AutoSavedDateTime { get; } = new ReactivePropertySlim<DateTime>();

        public ReactivePropertySlim<AutoSaveType> AutoSaveType { get; } = new ReactivePropertySlim<AutoSaveType>();

        public ReactivePropertySlim<TimeSpan> AutoSaveInterval { get; } = new ReactivePropertySlim<TimeSpan>(TimeSpan.FromMinutes(1));

        public ReactiveCollection<string> AutoSaveFiles { get; set; } = new ReactiveCollection<string>();

        public ReactivePropertySlim<AngleType> AngleType { get; set; } = new ReactivePropertySlim<AngleType>();

        public ReactivePropertySlim<bool> EnableImageEmbedding { get; set; } = new ReactivePropertySlim<bool>();

        public ReactivePropertySlim<Visibility> ContextMenuVisibility { get; } = new ReactivePropertySlim<Visibility>(Visibility.Visible);

        public ReactivePropertySlim<ColorSpots> ColorSpots { get; } = new ReactivePropertySlim<ColorSpots>();

        public ReactivePropertySlim<Brush> EdgeBrush { get; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<Brush> FillBrush { get; } = new ReactivePropertySlim<Brush>();

        public int Width
        {
            get { return _Width; }
            set { SetProperty(ref _Width, value); }
        }

        public int Height
        {
            get { return _Height; }
            set { SetProperty(ref _Height, value); }
        }

        /// <summary>
        /// 現在ポインティングしている座標
        /// ステータスバー上の座標インジケーターに使用される
        /// </summary>
        public Point CurrentPoint
        {
            get { return _CurrentPoint; }
            set { SetProperty(ref _CurrentPoint, value); }
        }
        public double CanvasBorderThickness
        {
            get { return _CanvasBorderThickness; }
            set { SetProperty(ref _CanvasBorderThickness, value); }
        }

        public double ScaleX { get; set; } = 1.0;
        public double ScaleY { get; set; } = 1.0;
        public System.Version BGSXFileVersion { get; } = new System.Version(2, 4);

        public int LayerCount { get; set; } = 1;

        public int LayerItemCount { get; set; } = 1;

        public IEnumerable<Tuple<SnapPoint, Point>> SnapPoints
        {
            get
            {
                var designerCanvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();
                var resizeThumbs = designerCanvas.EnumerateChildOfType<SnapPoint>();
                var sets = resizeThumbs
                                .Select(x => new Tuple<SnapPoint, Point>(x, GetCenter(x)))
                                .Distinct();
                return sets;
            }
        }

        public IEnumerable<Tuple<SnapPoint, Point>> GetSnapPoints(IEnumerable<SnapPoint> exceptSnapPoints)
        {
            var designerCanvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();
            var resizeThumbs = designerCanvas.EnumerateChildOfType<SnapPoint>();
            var sets = resizeThumbs
                            .Where(x => !exceptSnapPoints.Contains(x))
                            .Select(x => new Tuple<SnapPoint, Point>(x, GetCenter(x)))
                            .Distinct();
            return sets;
        }

        public IEnumerable<Tuple<SnapPoint, Point>> GetSnapPoints(Point exceptPoint)
        {
            var designerCanvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();
            var resizeThumbs = designerCanvas.EnumerateChildOfType<SnapPoint>();
            var sets = resizeThumbs
                            .Where(x => x.InputHitTest(exceptPoint) == null)
                            .Select(x => new Tuple<SnapPoint, Point>(x, GetCenter(x)))
                            .Distinct();
            return sets;
        }

        #endregion //Property

        public DiagramViewModel(MainWindowViewModel mainWindowViewModel, int width, int height)
        {
            MainWindowVM = mainWindowViewModel;

            AddItemCommand = new DelegateCommand<object>(p => ExecuteAddItemCommand(p));
            RemoveItemCommand = new DelegateCommand<object>(p => ExecuteRemoveItemCommand(p));
            ClearSelectedItemsCommand = new DelegateCommand<object>(p => ExecuteClearSelectedItemsCommand(p));
            CreateNewDiagramCommand = new DelegateCommand<object>(p => ExecuteCreateNewDiagramCommand(p));
            LoadCommand = new DelegateCommand(() => ExecuteLoadCommand());
            LoadFileCommand = new DelegateCommand<string>(file => ExecuteLoadCommand(file));
            SaveCommand = new DelegateCommand(() => ExecuteSaveCommand());
            OverwriteCommand = new DelegateCommand(() => ExecuteOverwriteCommand());
            ExportCommand = new DelegateCommand(() => ExecuteExportCommand());
            GroupCommand = new DelegateCommand(() => ExecuteGroupItemsCommand(), () => CanExecuteGroup());
            UngroupCommand = new DelegateCommand(() => ExecuteUngroupItemsCommand(), () => CanExecuteUngroup());
            BringForwardCommand = new DelegateCommand(() => ExecuteBringForwardCommand(), () => CanExecuteOrder());
            SendBackwardCommand = new DelegateCommand(() => ExecuteSendBackwardCommand(), () => CanExecuteOrder());
            BringForegroundCommand = new DelegateCommand(() => ExecuteBringForegroundCommand(), () => CanExecuteOrder());
            SendBackgroundCommand = new DelegateCommand(() => ExecuteSendBackgroundCommand(), () => CanExecuteOrder());
            AlignTopCommand = new DelegateCommand(() => ExecuteAlignTopCommand(), () => CanExecuteAlign());
            AlignVerticalCenterCommand = new DelegateCommand(() => ExecuteAlignVerticalCenterCommand(), () => CanExecuteAlign());
            AlignBottomCommand = new DelegateCommand(() => ExecuteAlignBottomCommand(), () => CanExecuteAlign());
            AlignLeftCommand = new DelegateCommand(() => ExecuteAlignLeftCommand(), () => CanExecuteAlign());
            AlignHorizontalCenterCommand = new DelegateCommand(() => ExecuteAlignHorizontalCenterCommand(), () => CanExecuteAlign());
            AlignRightCommand = new DelegateCommand(() => ExecuteAlignRightCommand(), () => CanExecuteAlign());
            DistributeHorizontalCommand = new DelegateCommand(() => ExecuteDistributeHorizontalCommand(), () => CanExecuteDistribute());
            DistributeVerticalCommand = new DelegateCommand(() => ExecuteDistributeVerticalCommand(), () => CanExecuteDistribute());
            SelectAllCommand = new DelegateCommand(() => ExecuteSelectAllCommand());
            SettingCommand = new DelegateCommand(() => ExecuteSettingCommand());
            UniformWidthCommand = new DelegateCommand(() => ExecuteUniformWidthCommand(), () => CanExecuteUniform());
            UniformHeightCommand = new DelegateCommand(() => ExecuteUniformHeightCommand(), () => CanExecuteUniform());
            DuplicateCommand = new DelegateCommand(() => ExecuteDuplicateCommand(), () => CanExecuteDuplicate());
            CutCommand = new DelegateCommand(() => ExecuteCutCommand(), () => CanExecuteCut());
            CopyCommand = new DelegateCommand(() => ExecuteCopyCommand(), () => CanExecuteCopy());
            PasteCommand = new DelegateCommand(() => ExecutePasteCommand(), () => CanExecutePaste());
            UnionCommand = new DelegateCommand(() => ExecuteUnionCommand(), () => CanExecuteUnion());
            IntersectCommand = new DelegateCommand(() => ExecuteIntersectCommand(), () => CanExecuteIntersect());
            XorCommand = new DelegateCommand(() => ExecuteXorCommand(), () => CanExecuteXor());
            ExcludeCommand = new DelegateCommand(() => ExecuteExcludeCommand(), () => CanExecuteExclude());
            ClipCommand = new DelegateCommand(() => ExecuteClipCommand(), () => CanExecuteClip());
            UndoCommand = new DelegateCommand(() => ExecuteUndoCommand(), () => CanExecuteUndo());
            RedoCommand = new DelegateCommand(() => ExecuteRedoCommand(), () => CanExecuteRedo());
            MouseWheelCommand = new DelegateCommand<MouseWheelEventArgs>(args =>
            {
                var diagramControl = App.Current.MainWindow.GetChildOfType<DiagramControl>();
                var zoomBox = diagramControl.GetChildOfType<ZoomBox>();
                if (args.Delta > 0)
                    zoomBox.ZoomSliderPlus();
                else if (args.Delta < 0)
                    zoomBox.ZoomSliderMinus();
                args.Handled = true;
            });
            PreviewMouseDownCommand = new DelegateCommand<MouseEventArgs>(args =>
            {
                if (args.MiddleButton == MouseButtonState.Pressed)
                {
                    _MiddleButtonIsPressed = true;
                    var diagramControl = App.Current.MainWindow.GetChildOfType<DiagramControl>();
                    _MousePointerPosition = args.GetPosition(diagramControl);
                    diagramControl.Cursor = Cursors.SizeAll;
                }
            });
            PreviewMouseUpCommand = new DelegateCommand<MouseEventArgs>(args =>
            {
                ReleaseMiddleButton(args);
            });
            MouseMoveCommand = new DelegateCommand<MouseEventArgs>(args =>
            {
                if (_MiddleButtonIsPressed)
                {
                    var diagramControl = App.Current.MainWindow.GetChildOfType<DiagramControl>();
                    var scrollViewer = diagramControl.GetChildOfType<ScrollViewer>();
                    var newMousePointerPosition = args.GetPosition(diagramControl);
                    var diff = newMousePointerPosition - _MousePointerPosition;
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - diff.Y);
                    scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - diff.X);
                    _MousePointerPosition = newMousePointerPosition;
                }
            });
            MouseLeaveCommand = new DelegateCommand<MouseEventArgs>(args =>
            {
                if (_MiddleButtonIsPressed)
                {
                    ReleaseMiddleButton(args);
                }
            });
            MouseEnterCommand = new DelegateCommand<MouseEventArgs>(args =>
            {
                if (_MiddleButtonIsPressed)
                {
                    ReleaseMiddleButton(args);
                }
            });
            PreviewKeyDownCommand = new DelegateCommand<KeyEventArgs>(args =>
            {
                switch (args.Key)
                {
                    case Key.Left:
                        MoveSelectedItems(-1, 0);
                        args.Handled = true;
                        break;
                    case Key.Up:
                        MoveSelectedItems(0, -1);
                        args.Handled = true;
                        break;
                    case Key.Right:
                        MoveSelectedItems(1, 0);
                        args.Handled = true;
                        break;
                    case Key.Down:
                        MoveSelectedItems(0, 1);
                        args.Handled = true;
                        break;
                }
            });
            EditMenuOpenedCommand = new DelegateCommand(() =>
            {
                CutCommand.RaiseCanExecuteChanged();
                CopyCommand.RaiseCanExecuteChanged();
                PasteCommand.RaiseCanExecuteChanged();
            });
            PropertyCommand = new DelegateCommand(() =>
            {
                var first = SelectedItems.Value.First();
                first.OpenPropertyDialog();
            },
            () => CanOpenPropertyDialog());
            MouseDownStraightLineCommand = new DelegateCommand<System.Windows.Shapes.Line>(line =>
            {
                var straightLineVM = line.DataContext as StraightConnectorViewModel;
                straightLineVM.IsSelected.Value = true;
                straightLineVM.SnapPoint0VM.Value.IsSelected.Value = true;
                straightLineVM.SnapPoint1VM.Value.IsSelected.Value = true;
            });
            MouseDownBezierCurveCommand = new DelegateCommand<System.Windows.Shapes.Path>(line =>
            {
                var bezierCurveVM = line.DataContext as BezierCurveViewModel;
                bezierCurveVM.IsSelected.Value = true;
                bezierCurveVM.SnapPoint0VM.Value.IsSelected.Value = true;
                bezierCurveVM.SnapPoint1VM.Value.IsSelected.Value = true;
            });
            MouseDownPolyBezierCommand = new DelegateCommand<System.Windows.Shapes.Path>(line =>
            {
                var polyBezierVM = line.DataContext as PolyBezierViewModel;
                polyBezierVM.IsSelected.Value = true;
                polyBezierVM.SnapPoint0VM.Value.IsSelected.Value = true;
                polyBezierVM.SnapPoint1VM.Value.IsSelected.Value = true;
            });
            LoadedCommand = new DelegateCommand(() =>
            {
                //var filename = @"Z:\Git\boilersGraphics\boilersGraphics.Test\bin\Debug\XmlFiles\checker_pattern.xml";
                //ExecuteLoadCommand(filename, false);
                //BackgroundItem.Value.FillColor.Value = Colors.Red;
            });

            //EdgeColors.CollectionChangedAsObservable()
            //    .Subscribe(_ => RaisePropertyChanged("EdgeColors"))
            //    .AddTo(_CompositeDisposable);
            //FillColors.CollectionChangedAsObservable()
            //    .Subscribe(_ => RaisePropertyChanged("FillColors"))
            //    .AddTo(_CompositeDisposable);

            Layers = RootLayer.Value.Children.CollectionChangedAsObservable()
                           .Select(_ => RootLayer.Value.LayerChangedAsObservable())
                           .Switch()
                           .SelectMany(_ => RootLayer.Value.Children)
                           .ToReactiveCollection();

            AllItems = Layers.CollectionChangedAsObservable()
                             .Select(_ => Layers.Select(x => x.LayerItemsChangedAsObservable()).Merge()
                                .Merge(this.ObserveProperty(y => y.BackgroundItem.Value).ToUnit()))
                             .Switch()
                             .Select(_ => Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
                                                .Where(x => x.GetType() == typeof(LayerItem))
                                                .Select(y => (y as LayerItem).Item.Value)
                                                .Union(new SelectableDesignerItemViewModelBase[] { BackgroundItem.Value })
                                                .ToArray())
                             .ToReadOnlyReactivePropertySlim(Array.Empty<SelectableDesignerItemViewModelBase>());

            AllItems.Subscribe(x =>
            {
                LogManager.GetCurrentClassLogger().Trace($"{x.Length} items in AllItems.");
                LogManager.GetCurrentClassLogger().Trace(string.Join(", ", x.Select(y => y?.ToString() ?? "null")));
            })
            .AddTo(_CompositeDisposable);

            SelectedItems = Layers
                .CollectionChangedAsObservable()
                .Select(_ =>
                    Layers
                        .Select(x => x.SelectedLayerItemsChangedAsObservable())
                        .Merge()
                )
                .Switch()
                .Do(x => LogManager.GetCurrentClassLogger().Debug("SelectedItems updated"))
                .Select(_ => Layers
                    .SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
                    .Where(x => x is LayerItem)
                    .Select(y => (y as LayerItem).Item.Value)
                    .Except(Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
                        .Where(x => x.GetType() == typeof(LayerItem))
                        .Select(y => (y as LayerItem).Item.Value)
                        .OfType<ConnectorBaseViewModel>())
                    .Union(Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
                        .Where(x => x.GetType() == typeof(LayerItem))
                        .Select(y => (y as LayerItem).Item.Value)
                        .OfType<ConnectorBaseViewModel>()
                        .SelectMany(x => new[] { x.SnapPoint0VM.Value, x.SnapPoint1VM.Value }.Where(y => y.IsSelected.Value))
                    )
                    .Where(z => z.IsSelected.Value == true)
                    .OrderBy(z => z.SelectedOrder.Value)
                    .ToArray()
                ).ToReadOnlyReactivePropertySlim(Array.Empty<SelectableDesignerItemViewModelBase>());

            SelectedItems.Subscribe(selectedItems =>
            {
                LogManager.GetCurrentClassLogger().Trace($"SelectedItems changed {string.Join(", ", selectedItems.Select(x => x?.ToString() ?? "null"))}");

                GroupCommand.RaiseCanExecuteChanged();
                UngroupCommand.RaiseCanExecuteChanged();
                BringForwardCommand.RaiseCanExecuteChanged();
                SendBackwardCommand.RaiseCanExecuteChanged();
                BringForegroundCommand.RaiseCanExecuteChanged();
                SendBackgroundCommand.RaiseCanExecuteChanged();

                AlignTopCommand.RaiseCanExecuteChanged();
                AlignVerticalCenterCommand.RaiseCanExecuteChanged();
                AlignBottomCommand.RaiseCanExecuteChanged();
                AlignLeftCommand.RaiseCanExecuteChanged();
                AlignHorizontalCenterCommand.RaiseCanExecuteChanged();
                AlignRightCommand.RaiseCanExecuteChanged();
                DistributeHorizontalCommand.RaiseCanExecuteChanged();
                DistributeVerticalCommand.RaiseCanExecuteChanged();

                UniformWidthCommand.RaiseCanExecuteChanged();
                UniformHeightCommand.RaiseCanExecuteChanged();

                UnionCommand.RaiseCanExecuteChanged();
                IntersectCommand.RaiseCanExecuteChanged();
                XorCommand.RaiseCanExecuteChanged();
                ExcludeCommand.RaiseCanExecuteChanged();

                PropertyCommand.RaiseCanExecuteChanged();
            })
            .AddTo(_CompositeDisposable);

            SelectedLayers = Layers.ObserveElementObservableProperty(x => x.IsSelected)
                                   .Select(_ => Layers.Where(x => x.IsSelected.Value == true).ToArray())
                                   .ToReadOnlyReactivePropertySlim(Array.Empty<LayerTreeViewItemBase>());

            SelectedLayers.Subscribe(x =>
            {
                LogManager.GetCurrentClassLogger().Trace($"SelectedLayers changed {string.Join(", ", x.Select(x => x.ToString()))}");
            })
            .AddTo(_CompositeDisposable);

            Layers.ObserveAddChanged()
                  .Subscribe(x =>
                  {
                      RootLayer.Value.Children = new ReactiveCollection<LayerTreeViewItemBase>(Layers.Cast<LayerTreeViewItemBase>().ToObservable());
                      x.SetParentToChildren(RootLayer.Value);
                  })
            .AddTo(_CompositeDisposable);

            Width = width;
            Height = height;

            EnableAutoSave.Subscribe(x =>
            {
                if (!x && _AutoSaveTimerDisposableObj != null)
                    _AutoSaveTimerDisposableObj.Dispose();
            })
            .AddTo(_CompositeDisposable);
            EnableAutoSave.Value = true;
            AutoSaveType.Value = Models.AutoSaveType.SetInterval;
            AutoSaveInterval.Value = TimeSpan.FromSeconds(30);

            var id = Guid.Parse("00000000-0000-0000-0000-000000000000");
            var dao = new LogSettingDao();
            var logSettings = dao.FindBy(new Dictionary<string, object>() { { "ID", id } });
            if (logSettings.Count() == 0)
            {
                var newLogSetting = new Models.LogSetting();
                newLogSetting.ID = id;
                newLogSetting.LogLevel = NLog.LogLevel.Info.ToString();
                dao.Insert(newLogSetting);
            }
            logSettings = dao.FindBy(new Dictionary<string, object>() { { "ID", id } });
            var logSetting = logSettings.First();
            MainWindowVM.LogLevel.Value = NLog.LogLevel.FromString(logSetting.LogLevel);
            PackAutoSaveFiles();

            AngleType.Value = Helpers.AngleType.Minus180To180;
            EnableImageEmbedding.Value = true;
            ColorSpots.Value = new ColorSpots();

            SettingIfDebug();
        }

        [Conditional("DEBUG")]
        private void SettingIfDebug()
        {
            EnableAutoSave.Value = false;
        }

        private void PackAutoSaveFiles()
        {
            if (AutoSaveFiles != null)
                AutoSaveFiles.ClearOnScheduler();
            try
            {
                var files = Directory.EnumerateFiles(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "dhq_boiler\\boilersGraphics\\AutoSave"), "AutoSave-*-*-*-*-*-*.xml");
                foreach (var file in files.OrderByDescending(x => new FileInfo(x).LastWriteTime))
                {
                    AutoSaveFiles.AddOnScheduler(file);
                }
            }
            catch (DirectoryNotFoundException)
            {
                //Ignore it as it only happens on Azure DevOps
            }
        }

        private bool CanOpenPropertyDialog()
        {
            return (SelectedItems.Value.Length == 1 && SelectedItems.Value.First().SupportsPropertyDialog)
                || (SelectedItems.Value.OfType<SnapPointViewModel>().Count() == 2 && SelectedItems.Value.OfType<SnapPointViewModel>().First().Parent.Value.SupportsPropertyDialog);
        }

        private void MoveSelectedItems(int horizontalDiff, int verticalDiff)
        {
            MainWindowVM.Recorder.BeginRecode();
            SelectedItems.Value.OfType<DesignerItemViewModelBase>().ToList().ForEach(x =>
            {
                MainWindowVM.Recorder.Current.ExecuteSetProperty(x, "Left.Value", x.Left.Value + horizontalDiff);
                MainWindowVM.Recorder.Current.ExecuteSetProperty(x, "Top.Value", x.Top.Value + verticalDiff);
            });
            SelectedItems.Value.OfType<SnapPointViewModel>().ToList().ForEach(x =>
            {
                MainWindowVM.Recorder.Current.ExecuteSetProperty(x, "Left.Value", x.Left.Value + horizontalDiff);
                MainWindowVM.Recorder.Current.ExecuteSetProperty(x, "Top.Value", x.Top.Value + verticalDiff);
            });
            MainWindowVM.Recorder.EndRecode();
        }

        private IDisposable _AutoSaveTimerDisposableObj;

        public void Initialize()
        {
            MainWindowVM.Recorder.BeginRecode();

            InitialSetting(MainWindowVM, true, true);

            MainWindowVM.Recorder.EndRecode();

            MainWindowVM.Controller.Flush();
            
            SetAutoSave();
        }

        private void SetAutoSave()
        {
            if (_AutoSaveTimerDisposableObj != null)
            {
                _AutoSaveTimerDisposableObj.Dispose();
            }

            MainWindowVM.Recorder.Current.StackChanged -= Current_StackChanged;

            if (EnableAutoSave.Value)
            {
                if (AutoSaveType.Value == Models.AutoSaveType.SetInterval)
                {
                    var source = Observable.Timer(AutoSaveInterval.Value, AutoSaveInterval.Value);

                    _AutoSaveTimerDisposableObj = source.Subscribe(_ =>
                    {
                        AutoSave();
                    });
                    _CompositeDisposable.Add(_AutoSaveTimerDisposableObj);
                }
                else if (AutoSaveType.Value == Models.AutoSaveType.EveryTimeCampusChanges)
                {
                    MainWindowVM.Recorder.Current.StackChanged += Current_StackChanged;
                }
            }
        }

        private void Current_StackChanged(object sender, TsOperationHistory.OperationStackChangedEventArgs e)
        {
            AutoSave();
        }

        private void AutoSave()
        {
            if (App.IsTest)
            {
                LogManager.GetCurrentClassLogger().Warn($"AutoSave()が呼び出されましたが、App.IsTest=trueのため、処理を実行しませんでした。");
                return;
            }

            AutoSavedDateTime.Value = DateTime.Now;
            var path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), $"dhq_boiler\\boilersGraphics\\AutoSave\\AutoSave-{AutoSavedDateTime.Value.Year}-{AutoSavedDateTime.Value.Month}-{AutoSavedDateTime.Value.Day}-{AutoSavedDateTime.Value.Hour}-{AutoSavedDateTime.Value.Minute}-{AutoSavedDateTime.Value.Second}.xml");
            var autoSaveDir = Path.GetDirectoryName(path);
            if (!Directory.Exists(autoSaveDir))
            {
                Directory.CreateDirectory(autoSaveDir);
            }

            App.GetCurrentApp().Dispatcher.Invoke(() =>
            {
                XElement versionXML = new XElement("Version", BGSXFileVersion.ToString());
                XElement layersXML = new XElement("Layers", ObjectSerializer.SerializeLayers(Layers));
                XElement configurationXML = new XElement("Configuration", ObjectSerializer.SerializeConfiguration(this));

                XElement root = new XElement("boilersGraphics");
                root.Add(versionXML);
                root.Add(layersXML);
                root.Add(configurationXML);
                root.Save(path);
            });

            MainWindowVM.Message.Value = $"{AutoSavedDateTime.Value} {Resources.Message_Autosaved}";

            LogManager.GetCurrentClassLogger().Info($"{AutoSavedDateTime.Value} {path} に自動保存しました。");

            Observable.Timer(TimeSpan.FromSeconds(5))
                        .Subscribe(_ => MainWindowVM.Message.Value = "")
                        .AddTo(_CompositeDisposable);

            PackAutoSaveFiles();
            UpdateStatisticsCountAutoSave();
        }


        private void UpdateStatisticsCountAutoSave()
        {
            var statistics = MainWindowVM.Statistics.Value;
            statistics.NumberOfTimesAutomaticallySaved++;
            var dao = new StatisticsDao();
            dao.Update(statistics);
        }

        public LayerTreeViewItemBase GetLayerTreeViewItemBase(SelectableDesignerItemViewModelBase item)
        {
            return Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
                         .Where(x => x is LayerItem)
                         .First(x => (x as LayerItem).Item.Value == item);
        }

        private Point GetCenter(SnapPoint snapPoint)
        {
            var designerCanvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();
            var leftTop = snapPoint.TransformToAncestor(designerCanvas).Transform(new Point(0, 0));
            switch (snapPoint.Tag)
            {
                case "左上":
                    return new Point(leftTop.X + snapPoint.Width - 1, leftTop.Y + snapPoint.Height - 1);
                case "右上":
                    return new Point(leftTop.X + 1, leftTop.Y + snapPoint.Height - 1);
                case "左下":
                    return new Point(leftTop.X + snapPoint.Width - 1, leftTop.Y + 1);
                case "右下":
                    return new Point(leftTop.X + 1, leftTop.Y + 1);
                case "左":
                case "上":
                case "右":
                case "下":
                    return new Point(leftTop.X, leftTop.Y);
                case "中央":
                    return new Point(leftTop.X + snapPoint.Width / 2, leftTop.Y + snapPoint.Height / 2);
                case "始点":
                case "終点":
                case "制御点":
                case "独立点":
                    return new Point(leftTop.X + snapPoint.Width / 2, leftTop.Y + snapPoint.Height / 2);
                case "頂点":
                    return new Point(leftTop.X + snapPoint.Width / 2, leftTop.Y + snapPoint.Height / 2);
                default:
                    throw new Exception("ResizeThumb.Tag doesn't set");
            }
        }

        [Conditional("DEBUG")]
        private void DebugPrint(int width, int height, IEnumerable<Tuple<SnapPoint, Point>> sets)
        {
            var designerCanvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();
            var rtb = new RenderTargetBitmap((int)designerCanvas.ActualWidth, (int)designerCanvas.ActualHeight, 96, 96, PixelFormats.Pbgra32);

            DrawingVisual visual = new DrawingVisual();
            using (DrawingContext context = visual.RenderOpen())
            {
                VisualBrush brush = new VisualBrush(designerCanvas);
                context.DrawRectangle(brush, null, new Rect(new Point(), new Size(designerCanvas.Width, designerCanvas.Height)));

                Random rand = new Random();
                foreach (var set in sets)
                {
                    context.DrawText(new FormattedText((string)set.Item1.Tag, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("メイリオ"), 12, Randomizer.RandomColorBrush(rand), VisualTreeHelper.GetDpi(designerCanvas).PixelsPerDip), set.Item2);
                    context.DrawEllipse(Brushes.Red, new Pen(Brushes.Red, 1), set.Item2, 2, 2);
                }
            }

            rtb.Render(visual);

            OpenCvSharpHelper.ImShow("DebugPrint", rtb);
        }

        private void InitialSetting(MainWindowViewModel mainwindowViewModel, bool addingLayer = false, bool initCanvasBackground = false)
        {
            mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "EdgeBrush.Value", new SolidColorBrush(Colors.Black));
            mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "FillBrush.Value", new SolidColorBrush(Colors.White));
            mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "EdgeThickness.Value", 1.0);
            mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "CanvasBorderThickness", 0.0);
            if (initCanvasBackground)
            {
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "CanvasBackground.Value", new SolidColorBrush(Colors.White));
            }
            mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "BackgroundItem.Value", new BackgroundViewModel());
            mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "BackgroundItem.Value.ZIndex.Value", -1);
            mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "BackgroundItem.Value.FillBrush.Value", CanvasBackground.Value);
            mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "BackgroundItem.Value.Left.Value", 0d);
            mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "BackgroundItem.Value.Top.Value", 0d);
            mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "BackgroundItem.Value.Width.Value", (double)Width);
            mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "BackgroundItem.Value.Height.Value", (double)Height);
            mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "BackgroundItem.Value.Owner", this);
            mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "BackgroundItem.Value.EdgeBrush.Value", new SolidColorBrush(Colors.Black));
            mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "BackgroundItem.Value.EdgeThickness.Value", 1d);
            mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "BackgroundItem.Value.EnableForSelection.Value", false);
            mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "BackgroundItem.Value.IsVisible.Value", true);
            mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "EnablePointSnap.Value", true);
            mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "LayerCount", 1);
            mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "LayerItemCount", 1);
            RootLayer.Dispose();
            RootLayer = new ReactivePropertySlim<LayerTreeViewItemBase>(new LayerTreeViewItemBase());
            Layers.ToClearOperation().ExecuteTo(mainwindowViewModel.Recorder.Current);
            if (addingLayer)
            {
                var layer = new Layer();
                layer.IsVisible.Value = true;
                layer.IsSelected.Value = true;
                layer.Name.Value = Name.GetNewLayerName(this);
                Random rand = new Random();
                layer.Color.Value = Randomizer.RandomColor(rand);
                mainwindowViewModel.Recorder.Current.ExecuteAdd(Layers, layer);
            }
        }

        private void ExecuteRedoCommand()
        {
            MainWindowVM.Controller.Redo();
            RedoCommand.RaiseCanExecuteChanged();
            UpdateStatisticsCountRedo();
        }

        private void UpdateStatisticsCountRedo()
        {
            var statistics = MainWindowVM.Statistics.Value;
            statistics.NumberOfRedoes++;
            var dao = new StatisticsDao();
            dao.Update(statistics);
        }

        public bool CanExecuteRedo()
        {
            return MainWindowVM.Controller.CanRedo;
        }

        private void ExecuteUndoCommand()
        {
            MainWindowVM.Controller.Undo();
            UndoCommand.RaiseCanExecuteChanged();
            RedoCommand.RaiseCanExecuteChanged();
            UpdateStatisticsCountUndo();
        }

        private void UpdateStatisticsCountUndo()
        {
            var statistics = MainWindowVM.Statistics.Value;
            statistics.NumberOfUndos++;
            var dao = new StatisticsDao();
            dao.Update(statistics);
        }

        public bool CanExecuteUndo()
        {
            return MainWindowVM.Controller.CanUndo;
        }


        private void ExecuteClipCommand()
        {
            var picture = SelectedItems.Value.OfType<PictureDesignerItemViewModel>().First();
            var other = SelectedItems.Value.OfType<DesignerItemViewModelBase>().Last();
            var pathGeometry = GeometryCreator.CreateRectangle(other as NRectangleViewModel, picture.Left.Value, picture.Top.Value);
            (picture.TransformNortification.Value.Sender as PictureDesignerItemViewModel).Clip.Value = pathGeometry;
            (picture.TransformNortification.Value.Sender as PictureDesignerItemViewModel).ClipObject.Value = other;
            picture.TransformNortification.Zip(picture.TransformNortification.Skip(1), (Old, New) => new { OldItem = Old, NewItem = New })
            .Where(x => x.NewItem.PropertyName == "Width" || x.NewItem.PropertyName == "Height")
            .Subscribe(x =>
            {
                var _other = picture.ClipObject.Value;
                var _pathGeometry = GeometryCreator.CreateRectangle(_other as NRectangleViewModel, picture.Left.Value, picture.Top.Value, x.NewItem.PropertyName, (double)x.NewItem.OldValue, (double)x.NewItem.NewValue);
                picture.Clip.Value = _pathGeometry;
            })
            .AddTo(_CompositeDisposable);
            Remove(other);
        }

        public bool CanExecuteClip()
        {
            return SelectedItems.Value.Count() == 2 &&
                   SelectedItems.Value.First().GetType() == typeof(PictureDesignerItemViewModel);
        }

        private void ExecuteExcludeCommand()
        {
            CombineAndAddItem(GeometryCombineMode.Exclude);
            UpdateStatisticsCountExclude();
        }

        private void UpdateStatisticsCountExclude()
        {
            var statistics = MainWindowVM.Statistics.Value;
            statistics.NumberOfExcludes++;
            var dao = new StatisticsDao();
            dao.Update(statistics);
        }

        public bool CanExecuteExclude()
        {
            var countIsCorrent = SelectedItems.Value.Count() == 2;
            if (countIsCorrent)
            {
                var firstElementTypeIsCorrect = SelectedItems.ElementAt(0).GetType() != typeof(PictureDesignerItemViewModel);
                var secondElementTypeIsCorrect = SelectedItems.ElementAt(1).GetType() != typeof(PictureDesignerItemViewModel);
                return countIsCorrent && firstElementTypeIsCorrect && secondElementTypeIsCorrect;
            }
            return false;
        }

        private void ExecuteXorCommand()
        {
            CombineAndAddItem(GeometryCombineMode.Xor);
            UpdateStatisticsCountXor();
        }

        private void UpdateStatisticsCountXor()
        {
            var statistics = MainWindowVM.Statistics.Value;
            statistics.NumberOfXors++;
            var dao = new StatisticsDao();
            dao.Update(statistics);
        }

        public bool CanExecuteXor()
        {
            var countIsCorrent = SelectedItems.Value.Count() == 2;
            if (countIsCorrent)
            {
                var firstElementTypeIsCorrect = SelectedItems.ElementAt(0).GetType() != typeof(PictureDesignerItemViewModel);
                var secondElementTypeIsCorrect = SelectedItems.ElementAt(1).GetType() != typeof(PictureDesignerItemViewModel);
                return countIsCorrent && firstElementTypeIsCorrect && secondElementTypeIsCorrect;
            }
            return false;
        }

        private void ExecuteIntersectCommand()
        {
            CombineAndAddItem(GeometryCombineMode.Intersect);
            UpdateStatisticsCountIntersect();
        }

        private void UpdateStatisticsCountIntersect()
        {
            var statistics = MainWindowVM.Statistics.Value;
            statistics.NumberOfIntersects++;
            var dao = new StatisticsDao();
            dao.Update(statistics);
        }

        public bool CanExecuteIntersect()
        {
            var countIsCorrent = SelectedItems.Value.Count() == 2;
            if (countIsCorrent)
            {
                var firstElementTypeIsCorrect = SelectedItems.ElementAt(0).GetType() != typeof(PictureDesignerItemViewModel);
                var secondElementTypeIsCorrect = SelectedItems.ElementAt(1).GetType() != typeof(PictureDesignerItemViewModel);
                return countIsCorrent && firstElementTypeIsCorrect && secondElementTypeIsCorrect;
            }
            return false;
        }

        private void ExecuteUnionCommand()
        {
            CombineAndAddItem(GeometryCombineMode.Union);
            UpdateStatisticsCountUnion();
        }

        private void UpdateStatisticsCountUnion()
        {
            var statistics = MainWindowVM.Statistics.Value;
            statistics.NumberOfUnions++;
            var dao = new StatisticsDao();
            dao.Update(statistics);
        }

        private void CombineAndAddItem(GeometryCombineMode mode)
        {
            MainWindowVM.Recorder.BeginRecode();
            var selectedItems = GetSelectedItemsForCombine();
            var item1 = GetSelectedItemFirst();
            if (selectedItems.Count() == 1 && item1 is PolyBezierViewModel pb)
            {
                Remove(pb);
                var combine = new CombineGeometryViewModel();
                MainWindowVM.Recorder.Current.ExecuteSetProperty(combine, "EdgeBrush.Value", pb.EdgeBrush.Value);
                MainWindowVM.Recorder.Current.ExecuteSetProperty(combine, "EdgeThickness.Value", pb.EdgeThickness.Value);
                MainWindowVM.Recorder.Current.ExecuteSetProperty(combine, "IsSelected.Value", true);
                MainWindowVM.Recorder.Current.ExecuteSetProperty(combine, "Owner", this);
                MainWindowVM.Recorder.Current.ExecuteSetProperty(combine, "ZIndex.Value", Layers.SelectMany(x => x.Children).Count());
                MainWindowVM.Recorder.Current.ExecuteSetProperty(combine, "IsHitTestVisible.Value", MainWindowVM.ToolBarViewModel.CurrentHitTestVisibleState.Value);
                MainWindowVM.Recorder.Current.ExecuteSetProperty(combine, "PathGeometry.Value", GeometryCreator.CreateCombineGeometry(pb));
                MainWindowVM.Recorder.Current.ExecuteSetProperty(combine, "Left.Value", combine.PathGeometry.Value.Bounds.Left);
                MainWindowVM.Recorder.Current.ExecuteSetProperty(combine, "Top.Value", combine.PathGeometry.Value.Bounds.Top);
                MainWindowVM.Recorder.Current.ExecuteSetProperty(combine, "Width.Value", combine.PathGeometry.Value.Bounds.Width);
                MainWindowVM.Recorder.Current.ExecuteSetProperty(combine, "Height.Value", combine.PathGeometry.Value.Bounds.Height);
                Add(combine);
            }
            else
            {
                var item2 = GetSelectedItemLast();
                var combine = new CombineGeometryViewModel();
                Remove(item1);
                Remove(item2);
                MainWindowVM.Recorder.Current.ExecuteSetProperty(combine, "EdgeBrush.Value", item1.EdgeBrush.Value);
                MainWindowVM.Recorder.Current.ExecuteSetProperty(combine, "EdgeThickness.Value", item1.EdgeThickness.Value);
                MainWindowVM.Recorder.Current.ExecuteSetProperty(combine, "IsSelected.Value", true);
                MainWindowVM.Recorder.Current.ExecuteSetProperty(combine, "Owner", this);
                MainWindowVM.Recorder.Current.ExecuteSetProperty(combine, "ZIndex.Value", Layers.SelectMany(x => x.Children).Count());
                MainWindowVM.Recorder.Current.ExecuteSetProperty(combine, "IsHitTestVisible.Value", MainWindowVM.ToolBarViewModel.CurrentHitTestVisibleState.Value);
                MainWindowVM.Recorder.Current.ExecuteSetProperty(combine, "PathGeometry.Value", GeometryCreator.CreateCombineGeometry(item1, item2));
                if (combine.PathGeometry.Value == null || combine.PathGeometry.Value.Figures.Count() == 0)
                {
                    var item1PathGeometry = item1.PathGeometry.Value;
                    var item2PathGeometry = item2.PathGeometry.Value;

                    if (item1 is DesignerItemViewModelBase designerItem1 && item1.RotationAngle.Value != 0)
                        item1PathGeometry = designerItem1.RotatePathGeometry.Value;
                    if (item2 is DesignerItemViewModelBase designerItem2 && item2.RotationAngle.Value != 0)
                        item2PathGeometry = designerItem2.RotatePathGeometry.Value;

                    CastToLetterAndSetTransform(item1, item2, item1PathGeometry, item2PathGeometry);

                    MainWindowVM.Recorder.Current.ExecuteSetProperty(combine, "PathGeometry.Value", Geometry.Combine(item1PathGeometry, item2PathGeometry, mode, null));
                }
                MainWindowVM.Recorder.Current.ExecuteSetProperty(combine, "Left.Value", combine.PathGeometry.Value.Bounds.Left);
                MainWindowVM.Recorder.Current.ExecuteSetProperty(combine, "Top.Value", combine.PathGeometry.Value.Bounds.Top);
                MainWindowVM.Recorder.Current.ExecuteSetProperty(combine, "Width.Value", combine.PathGeometry.Value.Bounds.Width);
                MainWindowVM.Recorder.Current.ExecuteSetProperty(combine, "Height.Value", combine.PathGeometry.Value.Bounds.Height);
                Add(combine);
            }
            MainWindowVM.Recorder.EndRecode();
        }

        private SelectableDesignerItemViewModelBase GetSelectedItemFirst()
        {
            return GetSelectedItemsForCombine().First();
        }

        private SelectableDesignerItemViewModelBase GetSelectedItemLast()
        {
            return GetSelectedItemsForCombine().Skip(1).Take(1).First();
        }

        private void CastToLetterAndSetTransform(SelectableDesignerItemViewModelBase item1, SelectableDesignerItemViewModelBase item2, PathGeometry item1PathGeometry, PathGeometry item2PathGeometry)
        {
            InternalCastToLetterAndSetTransform(item1, item1PathGeometry);
            InternalCastToLetterVerticalAndSetTransform(item1, item1PathGeometry);
            InternalCastToLetterAndSetTransform(item2, item2PathGeometry);
            InternalCastToLetterVerticalAndSetTransform(item2, item2PathGeometry);
            InternalCastToPolygonAndSetTransform(item1, item1PathGeometry);
            InternalCastToPolygonAndSetTransform(item2, item2PathGeometry);
        }

        private void InternalCastToPolygonAndSetTransform(SelectableDesignerItemViewModelBase item, PathGeometry itemPathGeometry)
        {
            if (item is NPolygonViewModel)
            {
                var item_ = item as NPolygonViewModel;
                var scaleX = item_.Width.Value / itemPathGeometry.Bounds.Width;
                var scaleY = item_.Height.Value / itemPathGeometry.Bounds.Height;
                var transformGroup = new TransformGroup();
                transformGroup.Children.Add(new ScaleTransform(scaleX, scaleY));
                transformGroup.Children.Add(new TranslateTransform(item_.Left.Value, item_.Top.Value));
                if (itemPathGeometry.Transform != null)
                    transformGroup.Children.Add(itemPathGeometry.Transform);
                MainWindowVM.Recorder.Current.ExecuteSetPropertyWithEnforcePropertyType<PathGeometry, Transform>(itemPathGeometry, "Transform", transformGroup);
            }
        }

        private void InternalCastToLetterVerticalAndSetTransform(SelectableDesignerItemViewModelBase item, PathGeometry itemPathGeometry)
        {
            if (item is LetterVerticalDesignerItemViewModel)
            {
                var item_ = item as LetterVerticalDesignerItemViewModel;
                var transformGroup = new TransformGroup();
                transformGroup.Children.Add(new TranslateTransform(item_.Left.Value, item_.Top.Value));
                if (itemPathGeometry.Transform != null)
                    transformGroup.Children.Add(itemPathGeometry.Transform);
                MainWindowVM.Recorder.Current.ExecuteSetPropertyWithEnforcePropertyType<PathGeometry, Transform>(itemPathGeometry, "Transform", transformGroup);
                item_.CloseLetterSettingDialog();
            }
        }

        private void InternalCastToLetterAndSetTransform(SelectableDesignerItemViewModelBase item, PathGeometry itemPathGeometry)
        {
            if (item is LetterDesignerItemViewModel)
            {
                var item_ = item as LetterDesignerItemViewModel;
                var transformGroup = new TransformGroup();
                transformGroup.Children.Add(new TranslateTransform(item_.Left.Value, item_.Top.Value));
                if (itemPathGeometry.Transform != null)
                    transformGroup.Children.Add(itemPathGeometry.Transform);
                MainWindowVM.Recorder.Current.ExecuteSetPropertyWithEnforcePropertyType<PathGeometry, Transform>(itemPathGeometry, "Transform", transformGroup);
                item_.CloseLetterSettingDialog();
            }
        }

        public bool CanExecuteUnion()
        {
            var countIsCorrent = GetCountIsCorrent();
            if (countIsCorrent)
            {
                var firstElementTypeIsCorrect = SelectedItems.ElementAt(0).GetType() != typeof(PictureDesignerItemViewModel);
                var secondElementTypeIsCorrect = SelectedItems.ElementAt(1).GetType() != typeof(PictureDesignerItemViewModel);
                return countIsCorrent && firstElementTypeIsCorrect && secondElementTypeIsCorrect;
            }
            var polyBezier = GetSelectedItemsForCombine().FirstOrDefault() as PolyBezierViewModel;
            if (polyBezier != null)
            {
                return true;
            }
            return false;
        }

        private bool GetCountIsCorrent()
        {
            List<SelectableDesignerItemViewModelBase> newlist = GetSelectedItemsForCombine();
            return newlist.Count() == 2;
        }

        private List<SelectableDesignerItemViewModelBase> GetSelectedItemsForCombine()
        {
            var list = SelectedItems.Value.ToList();
            var newlist = new List<SelectableDesignerItemViewModelBase>();
            foreach (var item in list)
            {
                if (item is DesignerItemViewModelBase)
                    newlist.Add(item);
                if (item is SnapPointViewModel snapPoint)
                    newlist.Add(snapPoint.Parent.Value);
            }
            newlist = newlist.Distinct().ToList();
            return newlist;
        }

        private void ExecuteCopyCommand()
        {
            CopyToClipboard();
            UpdateStatisticsCountCopy();
        }

        private void UpdateStatisticsCountCopy()
        {
            var statistics = MainWindowVM.Statistics.Value;
            statistics.NumberOfCopies++;
            var dao = new StatisticsDao();
            dao.Update(statistics);
        }

        public bool CanExecuteCopy()
        {
            return SelectedItems.Value.Count() > 0;
        }

        private void ExecutePasteCommand()
        {
            var obj = Clipboard.GetDataObject();
            if (obj.GetDataPresent(typeof(ClipboardDTO)))
            {
                var clipboardDTO = obj.GetData(typeof(ClipboardDTO)) as ClipboardDTO;
                var root = XElement.Parse(clipboardDTO.Root);
                ObjectDeserializer.ReadCopyObjectsFromXML(this, root);
            }
            else if (Clipboard.ContainsImage())
            {
                var bitmap = Clipboard.GetImage();
                var pic = new PictureDesignerItemViewModel();
                pic.Owner = this;
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                MemoryStream memoryStream = new MemoryStream();
                BitmapImage bImg = new BitmapImage();

                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                encoder.Save(memoryStream);

                memoryStream.Position = 0;
                bImg.BeginInit();
                bImg.CacheOption = BitmapCacheOption.OnLoad;
                bImg.StreamSource = memoryStream;
                bImg.EndInit();
                bImg.Freeze();

                memoryStream.Close();
                pic.EmbeddedImage.Value = bImg;
                pic.Left.Value = 0;
                pic.Top.Value = 0;
                pic.Width.Value = bImg.PixelWidth;
                pic.Height.Value = bImg.PixelHeight;
                pic.FileWidth = bImg.PixelWidth;
                pic.FileHeight = bImg.PixelHeight;
                pic.IsVisible.Value = true;
                pic.IsSelected.Value = true;
                pic.IsHitTestVisible.Value = true;
                pic.ZIndex.Value = pic.Owner.Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children).Count();
                Add(pic);
            }
            UpdateStatisticsCountPaste();
        }

        private void UpdateStatisticsCountPaste()
        {
            var statistics = MainWindowVM.Statistics.Value;
            statistics.NumberOfPasted++;
            var dao = new StatisticsDao();
            dao.Update(statistics);
        }

        public bool CanExecutePaste()
        {
            var obj = Clipboard.GetDataObject();
            if (obj.GetDataPresent(typeof(ClipboardDTO)))
            {
                var clipboardDTO = obj.GetData(typeof(ClipboardDTO)) as ClipboardDTO;
                var str = clipboardDTO.Root;
                try
                {
                    var root = XElement.Parse(str);
                    var rootNameIsCopyObjects = root.Name == "boilersGraphics";
                    var rootHasElements = root.HasElements;
                    if (rootNameIsCopyObjects && rootHasElements)
                    {
                        var copyObjsEnumerable = root.Elements().Where(x => x.Name == "CopyObjects");
                        var copyObjs = copyObjsEnumerable.FirstOrDefault();
                        var rootHasCopyObjects = copyObjs != null;
                        if (rootHasCopyObjects)
                        {
                            var copyObjsHasLayers = copyObjs.Elements().Where(x => x.Name == "Layers").Count() == 1;
                            var copyObjsHasItems = copyObjs.Elements().Where(x => x.Name == "LayerItems").Count() == 1;
                            if (copyObjsHasLayers)
                            {
                                var layers = copyObjs.Elements().Where(x => x.Name == "Layers").FirstOrDefault();
                                return layers.Elements().Count() >= 1;
                            }
                            else if (copyObjsHasItems)
                            {
                                var items = copyObjs.Elements().Where(x => x.Name == "LayerItems").FirstOrDefault();
                                return items.Elements().Count() >= 1;
                            }
                        }
                    }
                }
                catch (XmlException)
                {
                    return false;
                }
            }
            else if (Clipboard.ContainsImage())
            {
                return true;
            }
            return false;
        }

        private void ExecuteCutCommand()
        {
            CopyToClipboard();

            if (SelectedLayers.Value.Count() > 0 && SelectedItems.Value.Count() > 0)
            {
                SelectedLayers.Value.ToList().ForEach(x =>
                {
                    foreach (var selectedItem in SelectedItems.Value)
                    {
                        x.RemoveItem(MainWindowVM, selectedItem);
                        selectedItem.Dispose();
                    }
                });
            }
            else if (SelectedLayers.Value.Count() > 0)
            {
                //Copy Layer and LayerItem
                foreach (var selectedLayer in SelectedLayers.Value)
                {
                    Layers.Remove(selectedLayer);
                }
            }

            UpdateStatisticsCountCut();
        }

        private void UpdateStatisticsCountCut()
        {
            var statistics = MainWindowVM.Statistics.Value;
            statistics.NumberOfCuts++;
            var dao = new StatisticsDao();
            dao.Update(statistics);
        }

        private void CopyToClipboard()
        {
            if (SelectedLayers.Value.Count() > 0 && SelectedItems.Value.Count() > 0)
            {
                //Copy only LayerItem
                var root = new XElement("boilersGraphics");
                var copyObj = new XElement("CopyObjects");
                root.Add(copyObj);
                copyObj.Add(ObjectSerializer.ExtractItems(Layers.SelectMany(x => x.Children).Where(x => (x as LayerItem).IsSelected.Value).Cast<LayerItem>()));
                Clipboard.SetDataObject(new ClipboardDTO(root.ToString()), false);
            }
            else if (SelectedLayers.Value.Count() > 0)
            {
                //Copy Layer and LayerItem
                var root = new XElement("boilersGraphics");
                var copyObj = new XElement("CopyObjects");
                root.Add(copyObj);
                copyObj.Add(new XElement("Layers"));
                copyObj.Element("Layers").Add(ObjectSerializer.SerializeLayers(SelectedLayers.Value.ToObservableCollection()));
                Clipboard.SetDataObject(new ClipboardDTO(root.ToString()), false);
            }
        }

        public bool CanExecuteCut()
        {
            return (SelectedLayers.Value.Count() > 0 && SelectedItems.Value.Count() > 0)
                || (SelectedLayers.Value.Count() > 0);
        }

        private void ExecuteSettingCommand()
        {
            IDialogResult result = null;
            var preferences = new Models.Preference();
            preferences.Width.Value = this.Width;
            preferences.Height.Value = this.Height;
            Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
                                                     .Where(x => x.GetType() == typeof(LayerItem))
                                                     .Select(y => (y as LayerItem).Item.Value)
                                                     .Where(z => z is BrushViewModel)
                                                     .Cast<BrushViewModel>()
                                                     .ToList()
                                                     .ForEach(z =>
                                                     {
                                                         z.Width.Value = this.Width;
                                                         z.Height.Value = this.Height;
                                                     });
            preferences.CanvasBackground.Value = this.CanvasBackground.Value;
            preferences.EnablePointSnap.Value = this.EnablePointSnap.Value;
            preferences.SnapPower.Value = (App.Current.MainWindow.DataContext as MainWindowViewModel).SnapPower.Value;
            preferences.EnableAutoSave.Value = this.EnableAutoSave.Value;
            preferences.AutoSaveType.Value = this.AutoSaveType.Value;
            preferences.AutoSaveInterval.Value = this.AutoSaveInterval.Value;
            preferences.AngleType.Value = this.AngleType.Value;
            preferences.EnableImageEmbedding.Value = this.EnableImageEmbedding.Value;
            dlgService.ShowDialog(nameof(Views.Preference), new DialogParameters() { { "Preferences",  preferences} }, ret => result = ret);
            if (result != null && result.Result == ButtonResult.OK)
            {
                var s = result.Parameters.GetValue<Models.Preference>("Preferences");
                Width = s.Width.Value;
                Height = s.Height.Value;
                CanvasBackground.Value = s.CanvasBackground.Value;
                BackgroundItem.Value.FillBrush.Value = CanvasBackground.Value;
                EnablePointSnap.Value = s.EnablePointSnap.Value;
                (App.Current.MainWindow.DataContext as MainWindowViewModel).SnapPower.Value = s.SnapPower.Value;
                BackgroundItem.Value.Width.Value = Width;
                BackgroundItem.Value.Height.Value = Height;
                EnableAutoSave.Value = s.EnableAutoSave.Value;
                AutoSaveType.Value = s.AutoSaveType.Value;
                AutoSaveInterval.Value = s.AutoSaveInterval.Value;
                AngleType.Value = s.AngleType.Value;
                EnableImageEmbedding.Value = s.EnableImageEmbedding.Value;
                SetAutoSave();
            }
        }

        private void ReleaseMiddleButton(MouseEventArgs args)
        {
            if (args.MiddleButton == MouseButtonState.Released)
            {
                _MiddleButtonIsPressed = false;
                var diagramControl = App.Current.MainWindow.GetChildOfType<DiagramControl>();
                diagramControl.Cursor = Cursors.Arrow;
            }
        }

        public DiagramViewModel(MainWindowViewModel MainWindowVM, IDialogService dlgService, int width, int height)
            : this(MainWindowVM, width, height)
        {
            this.dlgService = dlgService;

            Mediator.Instance.Register(this);
        }

        public void DeselectAll()
        {
            foreach (var layerItem in Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
                                            .Where(x => x is LayerItem))
            {
                (layerItem as LayerItem).Item.Value.IsSelected.Value = false;
                (layerItem as LayerItem).IsSelected.Value = false;
            }
        }

        private void ExecuteAddItemCommand(object parameter)
        {
            if (parameter is SelectableDesignerItemViewModelBase item)
            {
                var targetLayer = SelectedLayers.Value.First();
                var newZIndex = targetLayer.GetNewZIndex(Layers.TakeWhile(x => x != targetLayer));
                Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
                      .Where(x => x != targetLayer)
                      .ToList()
                      .ForEach(x => x.PushZIndex(MainWindowVM.Recorder, newZIndex));
                item.ZIndex.Value = newZIndex;
                item.Owner = this;
                Add(item);
            }
        }

        private void ExecuteRemoveItemCommand(object parameter)
        {
            if (parameter is SelectableDesignerItemViewModelBase)
            {
                SelectableDesignerItemViewModelBase item = (SelectableDesignerItemViewModelBase)parameter;
                if (item is SnapPointViewModel snapPoint && !(snapPoint.Parent.Value is null))
                {
                    item = snapPoint.Parent.Value;
                }
                RemoveGroupMembers(item);
                Remove(item);
                if (item is LetterDesignerItemViewModel)
                {
                    (item as LetterDesignerItemViewModel).CloseLetterSettingDialog();
                }
                if (item is LetterVerticalDesignerItemViewModel)
                {
                    (item as LetterVerticalDesignerItemViewModel).CloseLetterSettingDialog();
                }
                item.Dispose();
                UpdateZIndex();
            }
        }

        private void UpdateZIndex()
        {
            var items = (from item in Layers.SelectMany(x => x.Children)
                         orderby (item as LayerItem).Item.Value.ZIndex.Value ascending
                         select item).ToList();

            for (int i = 0; i < items.Count; ++i)
            {
                (items.ElementAt(i) as LayerItem).Item.Value.ZIndex.Value = i;
            }
        }

        private void RemoveGroupMembers(SelectableDesignerItemViewModelBase item)
        {
            if (item is GroupItemViewModel groupItem)
            {
                var children = (from it in Layers.SelectMany(x => x.Children)
                                where (it as LayerItem).Item.Value.ParentID == groupItem.ID
                                select it).ToList();

                foreach (var child in children)
                {
                    RemoveGroupMembers((child as LayerItem).Item.Value);
                    Remove((child as LayerItem).Item.Value);
                    (child as LayerItem).Dispose();
                }
            }
        }

        private void ExecuteClearSelectedItemsCommand(object parameter)
        {
            foreach (LayerItem layerItem in Layers.SelectMany(x => x.Children))
            {
                layerItem.Item.Value.IsSelected.Value = false;
            }
        }

        private void ExecuteCreateNewDiagramCommand(object parameter)
        {
            Layers.Clear();
            Layers.Add(new Layer());
        }

        private void ExecuteExportCommand()
        {
            ExportCanvas();
        }

        private void ExportCanvas()
        {
            IDialogResult result = null;
            this.dlgService.ShowDialog(nameof(Export), ret => result = ret);
            if (result != null)
            {
                
            }
        }

        private void Add(SelectableDesignerItemViewModelBase item)
        {
            SelectedLayers.Value.First().AddItem(MainWindowVM, this, item);
            LogManager.GetCurrentClassLogger().Info($"Add item {item.ShowPropertiesAndFields()}");
            UpdateStatisticsCountAdd();
        }

        private void UpdateStatisticsCountAdd()
        {
            var statistics = MainWindowVM.Statistics.Value;
            statistics.NumberOfTimesTheItemWasDrawn++;
            var dao = new StatisticsDao();
            dao.Update(statistics);
        }

        private void Remove(SelectableDesignerItemViewModelBase item)
        {
            Layers.ToList().ForEach(x => x.RemoveItem(MainWindowVM, item));
            LogManager.GetCurrentClassLogger().Info($"Remove item {item.ShowPropertiesAndFields()}");
            UpdateStatisticsCountRemove();
        }

        private void UpdateStatisticsCountRemove()
        {
            var statistics = MainWindowVM.Statistics.Value;
            statistics.NumberOfTimesTheItemWasDeleted++;
            var dao = new StatisticsDao();
            dao.Update(statistics);
        }

        #region Save


        private void ExecuteSaveCommand()
        {
            XElement versionXML = new XElement("Version", BGSXFileVersion.ToString());
            XElement layersXML = new XElement("Layers", ObjectSerializer.SerializeLayers(Layers));
            XElement configurationXML = new XElement("Configuration", ObjectSerializer.SerializeConfiguration(this));

            XElement root = new XElement("boilersGraphics");
            root.Add(versionXML);
            root.Add(layersXML);
            root.Add(configurationXML);

            SaveFile(root);
        }

        private void SaveFile(XElement xElement)
        {
            var saveFile = new SaveFileDialog();
            saveFile.Filter = "Files (*.xml)|*.xml|All Files (*.*)|*.*";
            var oldFileName = FileName.Value;
            if (saveFile.ShowDialog() == true)
            {
                try
                {
                    FileName.Value = saveFile.FileName;
                    xElement.Save(saveFile.FileName);

                    UpdateStatisticsCountSaveAs();
                }
                catch (Exception ex)
                {
                    FileName.Value = oldFileName;
                    MessageBox.Show(ex.StackTrace, ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void UpdateStatisticsCountSaveAs()
        {
            var statistics = MainWindowVM.Statistics.Value;
            statistics.NumberOfTimesYouHaveNamedAndSaved++;
            var dao = new StatisticsDao();
            dao.Update(statistics);
        }

        #endregion //Save

        #region Overwrite

        private void ExecuteOverwriteCommand()
        {
            XElement versionXML = new XElement("Version", BGSXFileVersion.ToString());
            XElement layersXML = new XElement("Layers", ObjectSerializer.SerializeLayers(Layers));
            XElement configurationXML = new XElement("Configuration", ObjectSerializer.SerializeConfiguration(this));

            XElement root = new XElement("boilersGraphics");
            root.Add(versionXML);
            root.Add(layersXML);
            root.Add(configurationXML);
            Save(root);
        }

        private void Save(XElement root)
        {
            if (FileName.Value == "*")
            {
                var saveFile = new SaveFileDialog();
                saveFile.Filter = "Files (*.xml)|*.xml|All Files (*.*)|*.*";
                if (saveFile.ShowDialog() == true)
                {
                    try
                    {
                        FileName.Value = saveFile.FileName;
                        root.Save(saveFile.FileName);
                        UpdateStatisticsCountSaveAs();
                    }
                    catch (Exception ex)
                    {
                        FileName.Value = "*";
                        LogManager.GetCurrentClassLogger().Error(ex);
                        MessageBox.Show(ex.StackTrace, ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                try
                {
                    root.Save(FileName.Value);
                    UpdateStatisticsCountOverwrite();
                }
                catch (Exception ex)
                {
                    LogManager.GetCurrentClassLogger().Error(ex);
                    MessageBox.Show(ex.StackTrace, ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void UpdateStatisticsCountOverwrite()
        {
            var statistics = MainWindowVM.Statistics.Value;
            statistics.NumberOfTimesSaved++;
            var dao = new StatisticsDao();
            dao.Update(statistics);
        }

        #endregion

        #region Load

        private void ExecuteLoadCommand()
        {
            var result = MessageBox.Show(Resources.Message_CanvasWillDiscardedConfirm, Resources.DialogTitle_Confirm, MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.Cancel)
                return;
            var (root, filename) = LoadSerializedDataFromFile();
            LoadInternal(root, filename);
            var statistics = MainWindowVM.Statistics.Value;
            statistics.NumberOfTimesTheFileWasOpenedBySpecifyingIt++;
            var dao = new StatisticsDao();
            dao.Update(statistics);
        }

        private void LoadInternal(XElement root, string filename)
        {
            if (root == null)
            {
                return;
            }

            if (root.Element("Version") != null)
            {
                var version = new System.Version(root.Element("Version").Value);
                if (version > BGSXFileVersion)
                {
                    MessageBox.Show(Resources.Message_FileCannotOpenBecauseTooNew);
                    return;
                }
            }
            else
            {
                LogManager.GetCurrentClassLogger().Info(Resources.Log_ForceReadMode);
            }


            var mainwindowViewModel = MainWindowVM;
            try
            {
                mainwindowViewModel.Recorder.BeginRecode();

                var configuration = root.Element("Configuration");
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "Width", int.Parse(configuration.Element("Width").Value));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "Height", int.Parse(configuration.Element("Height").Value));
                var brushConverter = new BrushConverter();
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "CanvasBackground.Value", brushConverter.ConvertFromString(configuration.Element("CanvasBackground").Value));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "EnablePointSnap.Value", bool.Parse(configuration.Element("EnablePointSnap").Value));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(mainwindowViewModel, "SnapPower.Value", double.Parse(configuration.Element("SnapPower").Value));
                if (configuration.Element("ColorSpots") != null)
                {
                    var colorSpots = configuration.Element("ColorSpots");
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot0", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot0").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot1", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot1").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot2", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot2").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot3", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot3").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot4", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot4").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot5", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot5").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot6", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot6").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot7", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot7").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot8", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot8").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot9", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot9").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot10", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot10").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot11", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot11").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot12", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot12").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot13", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot13").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot14", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot14").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot15", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot15").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot16", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot16").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot17", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot17").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot18", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot18").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot19", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot19").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot20", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot20").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot21", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot21").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot22", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot22").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot23", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot23").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot24", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot24").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot25", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot25").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot26", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot26").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot27", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot27").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot28", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot28").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot29", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot29").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot30", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot30").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot31", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot31").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot32", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot32").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot33", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot33").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot34", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot34").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot35", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot35").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot36", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot36").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot37", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot37").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot38", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot38").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot39", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot39").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot40", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot40").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot41", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot41").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot42", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot42").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot43", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot43").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot44", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot44").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot45", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot45").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot46", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot46").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot47", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot47").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot48", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot48").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot49", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot49").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot50", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot50").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot51", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot51").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot52", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot52").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot53", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot53").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot54", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot54").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot55", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot55").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot56", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot56").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot57", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot57").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot58", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot58").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot59", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot59").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot60", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot60").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot61", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot61").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot62", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot62").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot63", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot63").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot64", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot64").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot65", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot65").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot66", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot66").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot67", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot67").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot68", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot68").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot69", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot69").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot70", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot70").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot71", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot71").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot72", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot72").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot73", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot73").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot74", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot74").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot75", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot75").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot76", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot76").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot77", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot77").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot78", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot78").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot79", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot79").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot80", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot80").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot81", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot81").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot82", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot82").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot83", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot83").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot84", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot84").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot85", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot85").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot86", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot86").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot87", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot87").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot88", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot88").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot89", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot89").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot90", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot90").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot91", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot91").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot92", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot92").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot93", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot93").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot94", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot94").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot95", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot95").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot96", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot96").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot97", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot97").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot98", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot98").Value));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this.ColorSpots.Value, "ColorSpot99", brushConverter.ConvertFromString(colorSpots.Element("ColorSpot99").Value));
                }

                InitialSetting(mainwindowViewModel, false, false);

                ObjectDeserializer.ReadObjectsFromXML(this, root);
            }
            catch (Exception)
            {
                MessageBox.Show(Resources.Message_FileCannotOpenBecauseTooOldOrCorrupted, Resources.DialogTitle_ReadError);
                LogManager.GetCurrentClassLogger().Error(Resources.Log_FileCannotOpenBecauseTooOldOrCorrupted);
                FileName.Value = "*";
                return;
            }
            finally
            {
                mainwindowViewModel.Recorder.EndRecode();
            }

            var layersViewModel = App.Current.MainWindow.GetChildOfType<Views.Layers>().DataContext as LayersViewModel;
            layersViewModel.InitializeHitTestVisible(mainwindowViewModel);
            Layers.First().IsSelected.Value = true;

            LogManager.GetCurrentClassLogger().Info(string.Format(Resources.Log_LoadedFile, filename));
        }

        private void ExecuteLoadCommand(string file, bool showConfirmDialog = true)
        {
            if (showConfirmDialog)
            {
                var result = MessageBox.Show(Resources.Message_CanvasWillDiscardedConfirm, Resources.DialogTitle_Confirm, MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.Cancel)
                    return;
            }
            FileName.Value = file;
            var root = XElement.Load(file);
            LoadInternal(root, file);
            var statistics = MainWindowVM.Statistics.Value;
            statistics.NumberOfTimesTheAutoSaveFileIsSpecifiedAndOpened++;
            var dao = new StatisticsDao();
            dao.Update(statistics);
        }

        public void Preview(string file)
        {
            var root = XElement.Load(file);
            LoadInternal(root, file);
        }

        private (XElement, string) LoadSerializedDataFromFile()
        {
            var openFile = new OpenFileDialog();
            openFile.Filter = "Designer Files (*.xml)|*.xml|All Files (*.*)|*.*";

            var oldFileName = FileName.Value;

            if (openFile.ShowDialog() == true)
            {
                try
                {
                    FileName.Value = openFile.FileName;
                    return (XElement.Load(openFile.FileName), openFile.FileName);
                }
                catch (Exception e)
                {
                    FileName.Value = oldFileName;
                    MessageBox.Show(e.StackTrace, e.Message, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            return (null, string.Empty);
        }

        #endregion //Load

        #region Grouping

        private void ExecuteGroupItemsCommand()
        {
            MainWindowVM.Recorder.BeginRecode();

            var items = SelectedItems.Value.Where(x => Guid.Equals(x.ParentID, Guid.Empty)).ToList();

            var rect = GetBoundingRectangle(items);

            var groupItem = new GroupItemViewModel();
            groupItem.Width.Value = rect.Width;
            groupItem.Height.Value = rect.Height;
            groupItem.Left.Value = rect.Left;
            groupItem.Top.Value = rect.Top;
            groupItem.IsHitTestVisible.Value = MainWindowVM.ToolBarViewModel.CurrentHitTestVisibleState.Value;

            AddItemCommand.Execute(groupItem);

            var groupItemLayerItem = Layers.SelectMany(x => x.Children).First(x => (x as LayerItem).Item.Value == groupItem);

            var list = new List<Tuple<LayerItem, LayerTreeViewItemBase>>();

            foreach (var item in items)
            {
                LayerItem layerItem = Layers.SelectMany(x => x.Children).First(x => (x as LayerItem).Item.Value == item) as LayerItem;
                list.Add(new Tuple<LayerItem, LayerTreeViewItemBase>(layerItem, layerItem.Parent.Value));
                MainWindowVM.Recorder.Current.ExecuteSetProperty(layerItem, "Parent.Value", groupItemLayerItem);
                MainWindowVM.Recorder.Current.ExecuteAdd(groupItemLayerItem.Children, layerItem);
                groupItem.AddGroup(MainWindowVM.Recorder, item);
                MainWindowVM.Recorder.Current.ExecuteSetProperty(item, "ParentID", groupItem.ID);
                MainWindowVM.Recorder.Current.ExecuteSetProperty(item, "EnableForSelection.Value", false);
            }

            foreach (var item in list)
            {
                LayerItem layerItem = item.Item1;
                LayerTreeViewItemBase parent = item.Item2;
                parent.RemoveChildren(MainWindowVM.Recorder, layerItem);
            }

            groupItem.SelectItemCommand.Execute(true);

            MainWindowVM.Recorder.EndRecode();

            UpdateStatisticsCountGroup();
        }

        private void UpdateStatisticsCountGroup()
        {
            var statistics = MainWindowVM.Statistics.Value;
            statistics.NumberOfTimesGrouped++;
            var dao = new StatisticsDao();
            dao.Update(statistics);
        }

        private void Remove(LayerItem layerItem)
        {
            var layer = Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
                              .Where(x => x is LayerItem)
                              .First(x => (x as LayerItem) == layerItem)
                              .Parent.Value;
            layer.RemoveChildren(MainWindowVM.Recorder, layerItem);
        }

        public bool CanExecuteGroup()
        {
            var items = from item in SelectedItems.Value
                        where item.ParentID == Guid.Empty
                        select item;
            return items.Count() > 1;
        }

        private void ExecuteUngroupItemsCommand()
        {
            MainWindowVM.Recorder.BeginRecode();

            var groups = from item in SelectedItems.Value
                         where item.ParentID == Guid.Empty
                         select item;

            foreach (var groupRoot in groups.ToList())
            {
                var children = (from child in Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
                               where child is LayerItem && (child as LayerItem).Item.Value.ParentID == groupRoot.ID
                               select child).ToList();

                var group = groupRoot as GroupItemViewModel;

                foreach (var child in children)
                {
                    var layerItem = child as LayerItem;
                    MainWindowVM.Recorder.Current.ExecuteDispose(layerItem.Item.Value.GroupDisposable, () => group.GroupDisposable = group.Subscribe(layerItem.Item.Value));
                    MainWindowVM.Recorder.Current.ExecuteSetProperty(layerItem.Item.Value, "ParentID", Guid.Empty);
                    MainWindowVM.Recorder.Current.ExecuteSetProperty(layerItem.Item.Value, "EnableForSelection.Value", true);
                    MainWindowVM.Recorder.Current.ExecuteSetProperty(layerItem, "Parent.Value", Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
                                                                                                      .Where(x => x is LayerItem)
                                                                                                      .First(x => (x as LayerItem).Item == (child as LayerItem).Item)
                                                                                                      .Parent.Value
                                                                                                      .Parent.Value);
                    MainWindowVM.Recorder.Current.ExecuteSetProperty(layerItem, "IsSelected.Value", true);
                    layerItem.Parent.Value.AddChildren(MainWindowVM.Recorder, layerItem);
                }

                var clone = (GroupItemViewModel)groupRoot.Clone();

                MainWindowVM.Recorder.Current.ExecuteDispose(groupRoot, () =>
                {
                    groupRoot.Swap(clone);
                });

                Remove(groupRoot);

                var groupZIndex = groupRoot.ZIndex.Value;

                var it = from item in Layers.SelectMany(x => x.Children)
                         where (item as LayerItem).Item.Value.ZIndex.Value > groupZIndex
                         select item;

                foreach (var x in it)
                {
                    MainWindowVM.Recorder.Current.ExecuteSetProperty((x as LayerItem).Item.Value, "ZIndex.Value", (x as LayerItem).Item.Value.ZIndex.Value - 1);
                }
            }

            MainWindowVM.Recorder.EndRecode();

            UpdateStatisticsCountUngroup();
        }

        private void UpdateStatisticsCountUngroup()
        {
            var statistics = MainWindowVM.Statistics.Value;
            statistics.NumberOfUngrouped++;
            var dao = new StatisticsDao();
            dao.Update(statistics);
        }

        public bool CanExecuteUngroup()
        {
            var items = from item in SelectedItems.Value.OfType<GroupItemViewModel>()
                        select item;
            return items.Count() > 0;
        }

        #endregion //Grouping

        #region Ordering

        /// <summary>
        /// 前面へ移動
        /// </summary>
        private void ExecuteBringForwardCommand()
        {
            MainWindowVM.Recorder.BeginRecode();

            var ordered = from item in SelectedItems.Value
                          orderby item.ZIndex.Value descending
                          select item;

            int count = Layers.SelectMany(x => x.Children).Count();

            for (int i = 0; i < ordered.Count(); ++i)
            {
                int currentIndex = ordered.ElementAt(i).ZIndex.Value;
                if (SelectedLayers.Value.First().Children.Max(x => (x as LayerItem).Item.Value.ZIndex.Value) == currentIndex)
                    continue; //レイヤー内の最大ZIndex値と同じだった場合はcontinueして次の選択アイテムへ
                var next = (from x in Layers.SelectMany(x => x.Children)
                            where (x as LayerItem).Item.Value.ZIndex.Value == currentIndex + 1
                            select x).SingleOrDefault();

                if (next == null) continue;

                int newIndex = (next as LayerItem).Item.Value.ParentID != Guid.Empty 
                             ? (Layers.SelectMany(x => x.Children)
                                      .Single(x => (x as LayerItem).Item.Value.ID == (next as LayerItem).Item.Value.ParentID) as LayerItem).Item.Value.ZIndex.Value
                             : Math.Min(count - 1 - i, currentIndex + 1);
                if (currentIndex != newIndex)
                {
                    if (ordered.ElementAt(i) is GroupItemViewModel)
                    {
                        MainWindowVM.Recorder.Current.ExecuteSetProperty(ordered.ElementAt(i), "ZIndex.Value", newIndex);

                        var children = from item in Layers.SelectMany(xx => xx.Children)
                                       where (item as LayerItem).Item.Value.ParentID == ordered.ElementAt(i).ID
                                       orderby (item as LayerItem).Item.Value.ZIndex.Value descending
                                       select item;

                        int youngestChildrenZIndex = 0;

                        for (int j = 0; j < children.Count(); ++j)
                        {
                            var child = children.ElementAt(j);
                            youngestChildrenZIndex = newIndex - j - 1;
                            MainWindowVM.Recorder.Current.ExecuteSetProperty((child as LayerItem).Item.Value, "ZIndex.Value", newIndex - j - 1);
                        }

                        var younger = from item in Layers.SelectMany(xx => xx.Children)
                                      where (item as LayerItem).Item.Value.ID != ordered.ElementAt(i).ID && (item as LayerItem).Item.Value.ParentID != ordered.ElementAt(i).ID
                                      && (item as LayerItem).Item.Value.ZIndex.Value <= ordered.ElementAt(i).ZIndex.Value && (item as LayerItem).Item.Value.ZIndex.Value >= youngestChildrenZIndex
                                      select item;

                        var x = from item in Layers.SelectMany(xx => xx.Children)
                                where (item as LayerItem).Item.Value.ID != ordered.ElementAt(i).ID && (item as LayerItem).Item.Value.ParentID != ordered.ElementAt(i).ID
                                && (item as LayerItem).Item.Value.ZIndex.Value < youngestChildrenZIndex
                                select item;

                        var z = x.ToList();
                        z.AddRange(younger);

                        for (int j = 0; j < z.Count(); ++j)
                        {
                            MainWindowVM.Recorder.Current.ExecuteSetProperty((z.ElementAt(j) as LayerItem).Item.Value, "ZIndex.Value", j);
                        }
                    }
                    else
                    {
                        MainWindowVM.Recorder.Current.ExecuteSetProperty(ordered.ElementAt(i), "ZIndex.Value", newIndex);
                        var exists = Layers.SelectMany(x => x.Children).Where(item => (item as LayerItem).Item.Value.ZIndex.Value == newIndex);

                        foreach (var item in exists)
                        {
                            if ((item as LayerItem).Item.Value != ordered.ElementAt(i))
                            {
                                if ((item as LayerItem).Item.Value is GroupItemViewModel)
                                {
                                    var children = from it in Layers.SelectMany(x => x.Children)
                                                   where (it as LayerItem).Item.Value.ParentID == (item as LayerItem).Item.Value.ID
                                                   select it;

                                    foreach (var child in children)
                                    {
                                        MainWindowVM.Recorder.Current.ExecuteSetProperty((child as LayerItem).Item.Value, "ZIndex.Value", (child as LayerItem).Item.Value.ZIndex.Value - 1);
                                    }

                                    MainWindowVM.Recorder.Current.ExecuteSetProperty((item as LayerItem).Item.Value, "ZIndex.Value", currentIndex + children.Count());
                                }
                                else
                                {
                                    MainWindowVM.Recorder.Current.ExecuteSetProperty((item as LayerItem).Item.Value, "ZIndex.Value", currentIndex);
                                }
                                break;
                            }
                        }
                    }
                }
            }

            Sort(Layers);

            MainWindowVM.Recorder.EndRecode();

            UpdateStatisticsCountMoveToFront();
        }

        private void UpdateStatisticsCountMoveToFront()
        {
            var statistics = MainWindowVM.Statistics.Value;
            statistics.NumberOfMovesToTheFront++;
            var dao = new StatisticsDao();
            dao.Update(statistics);
        }

        /// <summary>
        /// 背面へ移動
        /// </summary>
        private void ExecuteSendBackwardCommand()
        {
            MainWindowVM.Recorder.BeginRecode();

            var ordered = from item in SelectedItems.Value
                          orderby item.ZIndex.Value ascending
                          select item;

            int count = Layers.SelectMany(x => x.Children).Count();

            for (int i = 0; i < ordered.Count(); ++i)
            {
                int currentIndex = ordered.ElementAt(i).ZIndex.Value;
                if (SelectedLayers.Value.First().Children.Min(x => (x as LayerItem).Item.Value.ZIndex.Value) == currentIndex)
                    continue; //レイヤー内の最小ZIndex値と同じだった場合はcontinueして次の選択アイテムへ
                var previous = (from x in Layers.SelectMany(x => x.Children)
                                where (x as LayerItem).Item.Value.ZIndex.Value == currentIndex - 1
                                select x).SingleOrDefault();

                if (previous == null) continue;

                int newIndex = (previous as LayerItem).Item.Value is GroupItemViewModel ? Layers.SelectMany(x => x.Children).Where(x => (x as LayerItem).Item.Value.ParentID == (previous as LayerItem).Item.Value.ID).Min(x => (x as LayerItem).Item.Value.ZIndex.Value) : Math.Max(i, currentIndex - 1);
                if (currentIndex != newIndex)
                {
                    if (ordered.ElementAt(i) is GroupItemViewModel)
                    {
                        var children = (from item in Layers.SelectMany(xx => xx.Children)
                                        where (item as LayerItem).Item.Value.ParentID == ordered.ElementAt(i).ID
                                        orderby (item as LayerItem).Item.Value.ZIndex.Value descending
                                        select item).ToList();

                        if (children.Any(c => (c as LayerItem).Item.Value.ZIndex.Value == 0)) continue;

                        MainWindowVM.Recorder.Current.ExecuteSetProperty(ordered.ElementAt(i), "ZIndex.Value", newIndex);

                        int youngestChildrenZIndex = 0;

                        for (int j = 0; j < children.Count(); ++j)
                        {
                            var child = children.ElementAt(j);
                            youngestChildrenZIndex = newIndex - j - 1;
                            MainWindowVM.Recorder.Current.ExecuteSetProperty((child as LayerItem).Item.Value, "ZIndex.Value", newIndex - j - 1);
                        }

                        var older = from item in Layers.SelectMany(xx => xx.Children)
                                    where (item as LayerItem).Item.Value.ID != ordered.ElementAt(i).ID && (item as LayerItem).Item.Value.ParentID != ordered.ElementAt(i).ID
                                    && (item as LayerItem).Item.Value.ZIndex.Value <= ordered.ElementAt(i).ZIndex.Value && (item as LayerItem).Item.Value.ZIndex.Value >= youngestChildrenZIndex
                                    select item;

                        var x = from item in Layers.SelectMany(xx => xx.Children)
                                where (item as LayerItem).Item.Value.ID != ordered.ElementAt(i).ID && (item as LayerItem).Item.Value.ParentID != ordered.ElementAt(i).ID
                                && (item as LayerItem).Item.Value.ZIndex.Value > ordered.ElementAt(i).ZIndex.Value
                                select item;

                        var z = older.ToList();
                        z.AddRange(x);
                        z.Reverse();

                        for (int j = 0; j < z.Count(); ++j)
                        {
                            var elm = z.ElementAt(j);
                            MainWindowVM.Recorder.Current.ExecuteSetProperty((elm as LayerItem).Item.Value, "ZIndex.Value", Layers.SelectMany(xx => xx.Children).Count() - j - 1);
                        }
                    }
                    else
                    {
                        MainWindowVM.Recorder.Current.ExecuteSetProperty(ordered.ElementAt(i), "ZIndex.Value", newIndex);
                        var exists = Layers.SelectMany(x => x.Children).Where(item => (item as LayerItem).Item.Value.ZIndex.Value == newIndex);

                        foreach (var item in exists)
                        {
                            if ((item as LayerItem).Item.Value != ordered.ElementAt(i))
                            {
                                if ((item as LayerItem).Item.Value.ParentID != Guid.Empty)
                                {
                                    var children = from it in Layers.SelectMany(x => x.Children)
                                                   where (it as LayerItem).Item.Value.ParentID == (item as LayerItem).Item.Value.ParentID
                                                   select it;

                                    foreach (var child in children)
                                    {
                                        MainWindowVM.Recorder.Current.ExecuteSetProperty((child as LayerItem).Item.Value, "ZIndex.Value", (child as LayerItem).Item.Value.ZIndex.Value + 1);
                                    }

                                    var parent = (from it in Layers.SelectMany(x => x.Children)
                                                  where (it as LayerItem).Item.Value.ID == (item as LayerItem).Item.Value.ParentID
                                                  select it).Single();

                                    (parent as LayerItem).Item.Value.ZIndex.Value = children.Max(x => (x as LayerItem).Item.Value.ZIndex.Value) + 1;
                                    MainWindowVM.Recorder.Current.ExecuteSetProperty((parent as LayerItem).Item.Value, "ZIndex.Value", children.Max(x => (x as LayerItem).Item.Value.ZIndex.Value) + 1);
                                }
                                else
                                {
                                    MainWindowVM.Recorder.Current.ExecuteSetProperty((item as LayerItem).Item.Value, "ZIndex.Value", currentIndex);
                                }
                                break;
                            }
                        }
                    }
                }
            }

            Sort(Layers);

            MainWindowVM.Recorder.EndRecode();

            UpdateStatisticsCountMoveToBack();
        }

        private void UpdateStatisticsCountMoveToBack()
        {
            var statistics = MainWindowVM.Statistics.Value;
            statistics.NumberOfMovesToTheBack++;
            var dao = new StatisticsDao();
            dao.Update(statistics);
        }

        /// <summary>
        /// 最前面へ移動
        /// </summary>
        private void ExecuteBringForegroundCommand()
        {
            MainWindowVM.Recorder.BeginRecode();

            var ordered = from item in SelectedItems.Value
                          orderby item.ZIndex.Value descending
                          select item;

            int count = Layers.SelectMany(x => x.Children).Count();

            for (int i = 0; i < ordered.Count(); ++i)
            {
                var current = ordered.ElementAt(i);
                int currentIndex = current.ZIndex.Value;
                int newIndex = SelectedLayers.Value.SelectMany(x => x.Children).Count() - 1;
                if (currentIndex != newIndex)
                {
                    var oldCurrentIndex = current.ZIndex.Value;
                    MainWindowVM.Recorder.Current.ExecuteSetProperty(current, "ZIndex.Value", newIndex);

                    if (current is GroupItemViewModel)
                    {
                        var children = from item in Layers.SelectMany(x => x.Children)
                                       where (item as LayerItem).Item.Value.ParentID == current.ID
                                       orderby (item as LayerItem).Item.Value.ZIndex.Value descending
                                       select item;

                        for (int j = 0; j < children.Count(); ++j)
                        {
                            var child = children.ElementAt(j);
                            MainWindowVM.Recorder.Current.ExecuteSetProperty((child as LayerItem).Item.Value, "ZIndex.Value", current.ZIndex.Value - j - 1);
                        }

                        var minValue = children.Min(x => (x as LayerItem).Item.Value.ZIndex.Value);

                        var other = (from item in Layers.SelectMany(x => x.Children)
                                     where (item as LayerItem).Item.Value.ParentID != current.ID && (item as LayerItem).Item.Value.ID != current.ID
                                     orderby (item as LayerItem).Item.Value.ZIndex.Value descending
                                     select item).ToList();

                        for (int j = 0; j < other.Count(); ++j)
                        {
                            var item = other.ElementAt(j);
                            MainWindowVM.Recorder.Current.ExecuteSetProperty((item as LayerItem).Item.Value, "ZIndex.Value", minValue - j - 1);
                        }
                    }
                    else
                    {
                        var exists = Layers.SelectMany(x => x.Children).Where(item => (item as LayerItem).Item.Value.ZIndex.Value <= newIndex && (item as LayerItem).Item.Value.ZIndex.Value > oldCurrentIndex);

                        foreach (var item in exists)
                        {
                            if ((item as LayerItem).Item.Value != current)
                            {
                                MainWindowVM.Recorder.Current.ExecuteSetProperty((item as LayerItem).Item.Value, "ZIndex.Value", (item as LayerItem).Item.Value.ZIndex.Value - 1);
                            }
                        }
                    }
                }
            }

            Sort(Layers);

            MainWindowVM.Recorder.EndRecode();

            UpdateStatisticsCountMoveToFrontend();
        }

        private void UpdateStatisticsCountMoveToFrontend()
        {
            var statistics = MainWindowVM.Statistics.Value;
            statistics.NumberOfMovesToTheFrontend++;
            var dao = new StatisticsDao();
            dao.Update(statistics);
        }

        /// <summary>
        /// 最背面へ移動
        /// </summary>
        private void ExecuteSendBackgroundCommand()
        {
            MainWindowVM.Recorder.BeginRecode();

            var ordered = from item in SelectedItems.Value
                          orderby item.ZIndex.Value ascending
                          select item;

            int count = Layers.SelectMany(x => x.Children).Count();

            for (int i = 0; i < ordered.Count(); ++i)
            {
                var current = ordered.ElementAt(i);
                int currentIndex = current.ZIndex.Value;
                int newIndex = current is GroupItemViewModel
                             ? Layers.SelectMany(x => x.Children).Where(x => (x as LayerItem).Item.Value.ParentID == current.ID).Count()
                             : SelectedLayers.Value.First().Children.Min(x => (x as LayerItem).Item.Value.ZIndex.Value);
                if (currentIndex != newIndex)
                {
                    var oldCurrentIndex = current.ZIndex.Value;
                    MainWindowVM.Recorder.Current.ExecuteSetProperty(current, "ZIndex.Value", newIndex);

                    if (current is GroupItemViewModel)
                    {
                        var children = (from item in Layers.SelectMany(x => x.Children)
                                        where (item as LayerItem).Item.Value.ParentID == current.ID
                                        orderby (item as LayerItem).Item.Value.ZIndex.Value descending
                                        select item).ToList();

                        for (int j = 0; j < children.Count(); ++j)
                        {
                            var child = children.ElementAt(j);
                            MainWindowVM.Recorder.Current.ExecuteSetProperty((child as LayerItem).Item.Value, "ZIndex.Value", current.ZIndex.Value - j - 1);
                        }

                        var other = (from item in Layers.SelectMany(x => x.Children)
                                     where (item as LayerItem).Item.Value.ParentID != current.ID && (item as LayerItem).Item.Value.ID != current.ID
                                     orderby (item as LayerItem).Item.Value.ZIndex.Value descending
                                     select item).ToList();

                        var maxValue = Layers.SelectMany(x => x.Children).Count() - 1;

                        for (int j = 0; j < other.Count(); ++j)
                        {
                            var item = other.ElementAt(j);
                            MainWindowVM.Recorder.Current.ExecuteSetProperty((item as LayerItem).Item.Value, "ZIndex.Value", maxValue - j);
                        }
                    }
                    else
                    {
                        var exists = Layers.SelectMany(x => x.Children).Where(item => (item as LayerItem).Item.Value.ZIndex.Value >= newIndex && (item as LayerItem).Item.Value.ZIndex.Value < oldCurrentIndex);

                        foreach (var item in exists)
                        {
                            if ((item as LayerItem).Item.Value != current)
                            {
                                MainWindowVM.Recorder.Current.ExecuteSetProperty((item as LayerItem).Item.Value, "ZIndex.Value", (item as LayerItem).Item.Value.ZIndex.Value + 1);
                            }
                        }
                    }
                }
            }

            Sort(Layers);

            MainWindowVM.Recorder.EndRecode();

            UpdateStatisticsCountMoveToBackend();
        }

        private void UpdateStatisticsCountMoveToBackend()
        {
            var statistics = MainWindowVM.Statistics.Value;
            statistics.NumberOfMovesToTheBackend++;
            var dao = new StatisticsDao();
            dao.Update(statistics);
        }

        private void Sort(ReactiveCollection<LayerTreeViewItemBase> target)
        {
            var list = target.ToList();

            foreach (var layer in list)
            {
                target.Remove(layer);
            }

            list.Sort();

            foreach (var layer in list)
            {
                Sort(layer.Children);
                target.Add(layer);
            }
        }

        public bool CanExecuteOrder()
        {
            return SelectedItems.Value.Count() > 0;
        }

        #endregion //Ordering

        #region Alignment

        private void ExecuteAlignTopCommand()
        {
            if (SelectedItems.Value.Count() > 1)
            {
                MainWindowVM.Recorder.BeginRecode();

                var first = SelectedItems.Value.First();
                double top = GetTop(first);

                foreach (var item in SelectedItems.Value)
                {
                    double delta = top - GetTop(item);
                    SetTop(item, GetTop(item) + delta);
                }

                MainWindowVM.Recorder.EndRecode();

                UpdateStatisticsCountAlignTop();
            }
        }

        private void UpdateStatisticsCountAlignTop()
        {
            var statistics = MainWindowVM.Statistics.Value;
            statistics.NumberOfTopAlignment++;
            var dao = new StatisticsDao();
            dao.Update(statistics);
        }

        private void ExecuteAlignVerticalCenterCommand()
        {
            if (SelectedItems.Value.Count() > 1)
            {
                MainWindowVM.Recorder.BeginRecode();

                var first = SelectedItems.Value.First();
                double bottom = GetTop(first) + GetHeight(first) / 2;

                foreach (var item in SelectedItems.Value)
                {
                    double delta = bottom - (GetTop(item) + GetHeight(item) / 2);
                    SetTop(item, GetTop(item) + delta);
                }

                MainWindowVM.Recorder.EndRecode();
                UpdateStatisticsCountAlignVerticalCenter();
            }
        }

        private void UpdateStatisticsCountAlignVerticalCenter()
        {
            var statistics = MainWindowVM.Statistics.Value;
            statistics.NumberOfTimesTheTopAndBottomAreCentered++;
            var dao = new StatisticsDao();
            dao.Update(statistics);
        }

        private void ExecuteAlignBottomCommand()
        {
            if (SelectedItems.Value.Count() > 1)
            {
                MainWindowVM.Recorder.BeginRecode();

                var first = SelectedItems.Value.First();
                double bottom = GetTop(first) + GetHeight(first);

                foreach (var item in SelectedItems.Value)
                {
                    double delta = bottom - (GetTop(item) + GetHeight(item));
                    SetTop(item, GetTop(item) + delta);
                }

                MainWindowVM.Recorder.EndRecode();
                UpdateStatisticsCountAlignBottom();
            }
        }

        private void UpdateStatisticsCountAlignBottom()
        {
            var statistics = MainWindowVM.Statistics.Value;
            statistics.NumberOfBottomAlignment++;
            var dao = new StatisticsDao();
            dao.Update(statistics);
        }

        private void ExecuteAlignLeftCommand()
        {
            if (SelectedItems.Value.Count() > 1)
            {
                MainWindowVM.Recorder.BeginRecode();

                var first = SelectedItems.Value.First();
                double left = GetLeft(first);

                foreach (var item in SelectedItems.Value)
                {
                    double delta = left - GetLeft(item);
                    SetLeft(item, GetLeft(item) + delta);
                }

                MainWindowVM.Recorder.EndRecode();
                UpdateStatisticsCountAlignLeft();
            }
        }

        private void UpdateStatisticsCountAlignLeft()
        {
            var statistics = MainWindowVM.Statistics.Value;
            statistics.NumberOfLeftAlignment++;
            var dao = new StatisticsDao();
            dao.Update(statistics);
        }

        private void ExecuteAlignHorizontalCenterCommand()
        {
            if (SelectedItems.Value.Count() > 1)
            {
                MainWindowVM.Recorder.BeginRecode();

                var first = SelectedItems.Value.First();
                double center = GetLeft(first) + GetWidth(first) / 2;

                foreach (var item in SelectedItems.Value)
                {
                    double delta = center - (GetLeft(item) + GetWidth(item) / 2);
                    SetLeft(item, GetLeft(item) + delta);
                }

                MainWindowVM.Recorder.EndRecode();
                UpdateStatisticsCountAlignHorizontalCenter();
            }
        }

        private void UpdateStatisticsCountAlignHorizontalCenter()
        {
            var statistics = MainWindowVM.Statistics.Value;
            statistics.NumberOfTimesLeftAndRightCentered++;
            var dao = new StatisticsDao();
            dao.Update(statistics);
        }

        private void ExecuteAlignRightCommand()
        {
            if (SelectedItems.Value.Count() > 1)
            {
                MainWindowVM.Recorder.BeginRecode();

                var first = SelectedItems.Value.First();
                double right = GetLeft(first) + GetWidth(first);

                foreach (var item in SelectedItems.Value)
                {
                    double delta = right - (GetLeft(item) + GetWidth(item));
                    SetLeft(item, GetLeft(item) + delta);
                }

                MainWindowVM.Recorder.EndRecode();
                UpdateStatisticsCountAlignRight();
            }
        }

        private void UpdateStatisticsCountAlignRight()
        {
            var statistics = MainWindowVM.Statistics.Value;
            statistics.NumberOfRightAlignment++;
            var dao = new StatisticsDao();
            dao.Update(statistics);
        }

        /// <summary>
        /// 左右に整列
        /// </summary>
        private void ExecuteDistributeHorizontalCommand()
        {
            var selectedItems = from item in SelectedItems.Value
                                let itemLeft = GetLeft(item)
                                orderby itemLeft
                                select item;

            if (selectedItems.Count() > 1)
            {
                MainWindowVM.Recorder.BeginRecode();

                double left = double.MaxValue;
                double right = double.MinValue;
                double sumWidth = 0;

                foreach (var item in selectedItems)
                {
                    left = Math.Min(left, GetLeft(item));
                    right = Math.Max(right, GetLeft(item) + GetWidth(item));
                    sumWidth += GetWidth(item);
                }

                double distance = Math.Max(0, (right - left - sumWidth) / (selectedItems.Count() - 1));
                double offset = GetLeft(selectedItems.First());

                foreach (var item in selectedItems)
                {
                    double delta = offset - GetLeft(item);
                    SetLeft(item, GetLeft(item) + delta);
                    offset = offset + GetWidth(item) + distance;
                }

                MainWindowVM.Recorder.EndRecode();
                UpdateStatisticsCountAlignLeftAndRight();
            }
        }

        private void UpdateStatisticsCountAlignLeftAndRight()
        {
            var statistics = MainWindowVM.Statistics.Value;
            statistics.NumberOfTimesAlignedLeftAndRight++;
            var dao = new StatisticsDao();
            dao.Update(statistics);
        }

        /// <summary>
        /// 上下に整列
        /// </summary>
        private void ExecuteDistributeVerticalCommand()
        {
            var selectedItems = from item in SelectedItems.Value
                                let itemTop = GetTop(item)
                                orderby itemTop
                                select item;

            if (selectedItems.Count() > 1)
            {
                MainWindowVM.Recorder.BeginRecode();

                double top = double.MaxValue;
                double bottom = double.MinValue;
                double sumHeight = 0;

                foreach (var item in selectedItems)
                {
                    top = Math.Min(top, GetTop(item));
                    bottom = Math.Max(bottom, GetTop(item) + GetHeight(item));
                    sumHeight += GetHeight(item);
                }

                double distance = Math.Max(0, (bottom - top - sumHeight) / (selectedItems.Count() - 1));
                double offset = GetTop(selectedItems.First());

                foreach (var item in selectedItems)
                {
                    double delta = offset - GetTop(item);
                    SetTop(item, GetTop(item) + delta);
                    offset = offset + GetHeight(item) + distance;
                }

                MainWindowVM.Recorder.EndRecode();
                UpdateStatisticsCountAlignUpAndDown();
            }
        }

        private void UpdateStatisticsCountAlignUpAndDown()
        {
            var statistics = MainWindowVM.Statistics.Value;
            statistics.NumberOfTimesAlignedUpAndDown++;
            var dao = new StatisticsDao();
            dao.Update(statistics);
        }

        public bool CanExecuteAlign()
        {
            return SelectedItems.Value.Count() > 1;
        }

        public bool CanExecuteDistribute()
        {
            return SelectedItems.Value.Count() > 1;
        }

        private double GetWidth(SelectableDesignerItemViewModelBase item)
        {
            return item is DesignerItemViewModelBase ? (item as DesignerItemViewModelBase).Width.Value
                 : item is ConnectorBaseViewModel ? Math.Max((item as ConnectorBaseViewModel).Points[0].X - (item as ConnectorBaseViewModel).Points[1].X, (item as ConnectorBaseViewModel).Points[1].X - (item as ConnectorBaseViewModel).Points[0].X)
                 : (item as GroupItemViewModel).Width.Value;
        }

        private void SetLeft(SelectableDesignerItemViewModelBase item, double value)
        {
            if (item is DesignerItemViewModelBase di)
            {
                MainWindowVM.Recorder.Current.ExecuteSetProperty(di, "Left.Value", value);
            }
            else if (item is ConnectorBaseViewModel connector)
            {
                //do nothing
            }
        }

        private double GetLeft(SelectableDesignerItemViewModelBase item)
        {
            return item is DesignerItemViewModelBase ? (item as DesignerItemViewModelBase).Left.Value
                : item is ConnectorBaseViewModel ? Math.Min((item as ConnectorBaseViewModel).Points[0].X, (item as ConnectorBaseViewModel).Points[1].X)
                : Layers.SelectMany(x => x.Children).Where(x => (x as LayerItem).Item.Value.ParentID == (item as GroupItemViewModel).ID).Min(x => GetLeft((x as LayerItem).Item.Value));
        }

        private double GetHeight(SelectableDesignerItemViewModelBase item)
        {
            return item is DesignerItemViewModelBase ? (item as DesignerItemViewModelBase).Height.Value
                 : item is ConnectorBaseViewModel ? Math.Max((item as ConnectorBaseViewModel).Points[0].Y - (item as ConnectorBaseViewModel).Points[1].Y, (item as ConnectorBaseViewModel).Points[1].Y - (item as ConnectorBaseViewModel).Points[0].Y)
                 : (item as GroupItemViewModel).Height.Value;
        }

        private void SetTop(SelectableDesignerItemViewModelBase item, double value)
        {
            if (item is DesignerItemViewModelBase di)
            {
                MainWindowVM.Recorder.Current.ExecuteSetProperty(di, "Top.Value", value);
            }
            else if (item is ConnectorBaseViewModel connector)
            {
                //do nothing
            }
        }

        private double GetTop(SelectableDesignerItemViewModelBase item)
        {
            return item is DesignerItemViewModelBase ? (item as DesignerItemViewModelBase).Top.Value
                : item is ConnectorBaseViewModel ? Math.Min((item as ConnectorBaseViewModel).Points[0].Y, (item as ConnectorBaseViewModel).Points[1].Y)
                : Layers.SelectMany(x => x.Children).Where(x => (x as LayerItem).Item.Value.ParentID == (item as GroupItemViewModel).ID).Min(x => GetTop((x as LayerItem).Item.Value));
        }

        #endregion //Alignment

        private void ExecuteSelectAllCommand()
        {
            Layers.SelectMany(x => x.Children).ToList().ForEach(x => (x as LayerItem).Item.Value.IsSelected.Value = true);
        }

        #region Uniform

        /// <summary>
        /// 幅を合わせる
        /// </summary>
        private void ExecuteUniformWidthCommand()
        {
            MainWindowVM.Recorder.BeginRecode();

            var selectedItems = SelectedItems.Value.OfType<DesignerItemViewModelBase>();
            if (selectedItems.Count() > 1)
            {
                var first = selectedItems.First();
                double width = first.Width.Value;

                foreach (var item in selectedItems)
                {
                    double delta = width - item.Width.Value;
                    MainWindowVM.Recorder.Current.ExecuteSetProperty(item, "Width.Value", item.Width.Value + delta);
                }
            }

            MainWindowVM.Recorder.EndRecode();
            UpdateStatisticsCountMatchWidth();
        }

        private void UpdateStatisticsCountMatchWidth()
        {
            var statistics = MainWindowVM.Statistics.Value;
            statistics.NumberOfTimesToMatchTheWidth++;
            var dao = new StatisticsDao();
            dao.Update(statistics);
        }

        /// <summary>
        /// 高さを合わせる
        /// </summary>
        private void ExecuteUniformHeightCommand()
        {
            MainWindowVM.Recorder.BeginRecode();

            var selectedItems = SelectedItems.Value.OfType<DesignerItemViewModelBase>();
            if (selectedItems.Count() > 1)
            {
                var first = selectedItems.First();
                double height = first.Height.Value;

                foreach (var item in selectedItems)
                {
                    double delta = height - item.Height.Value;
                    MainWindowVM.Recorder.Current.ExecuteSetProperty(item, "Height.Value", item.Height.Value + delta);
                }
            }

            MainWindowVM.Recorder.EndRecode();
            UpdateStatisticsCountMatchHeight();
        }

        private void UpdateStatisticsCountMatchHeight()
        {
            var statistics = MainWindowVM.Statistics.Value;
            statistics.NumberOfTimesToMatchTheHeight++;
            var dao = new StatisticsDao();
            dao.Update(statistics);
        }

        public bool CanExecuteUniform()
        {
            return SelectedItems.Value.OfType<DesignerItemViewModelBase>().Count() > 1;
        }

        #endregion //Uniform

        #region Duplicate

        private void ExecuteDuplicateCommand()
        {
            DuplicateObjects(SelectedItems.Value);
        }

        private void DuplicateObjects(IEnumerable<SelectableDesignerItemViewModelBase> items)
        {
            var selectedItems = from item in items.OfType<DesignerItemViewModelBase>()
                                orderby item.ZIndex.Value ascending
                                select item;

            var oldNewList = new List<Tuple<SelectableDesignerItemViewModelBase, SelectableDesignerItemViewModelBase>>();

            foreach (var item in selectedItems)
            {
                DuplicateDesignerItem(selectedItems, oldNewList, item);
            }

            var selectedConnectors = (from item in items.OfType<SnapPointViewModel>().Select(x => x.Parent.Value).OfType<ConnectorBaseViewModel>()
                                     orderby item.ZIndex.Value ascending
                                     select item).Distinct();

            foreach (var connector in selectedConnectors)
            {
                DuplicateConnector(oldNewList, connector);
            }
        }

        private void DuplicateDesignerItem(IOrderedEnumerable<DesignerItemViewModelBase> selectedItems, List<Tuple<SelectableDesignerItemViewModelBase, SelectableDesignerItemViewModelBase>> oldNewList, DesignerItemViewModelBase item, GroupItemViewModel parent = null)
        {
            if (item is GroupItemViewModel groupItem)
            {
                var cloneGroup = groupItem.Clone() as GroupItemViewModel;
                if (parent != null)
                {
                    cloneGroup.ParentID = parent.ID;
                    cloneGroup.EnableForSelection.Value = false;
                    parent.AddGroup(MainWindowVM.Recorder, cloneGroup);
                }

                var children = from it in Layers.SelectMany(x => x.Children).OfType<DesignerItemViewModelBase>()
                               where it.ParentID == item.ID
                               orderby it.ZIndex.Value ascending
                               select it;

                foreach (var child in children)
                {
                    DuplicateDesignerItem(selectedItems, oldNewList, child, cloneGroup);
                }

                var childrenConnectors = from it in Layers.SelectMany(x => x.Children).OfType<ConnectorBaseViewModel>()
                                         where it.ParentID == item.ID
                                         orderby it.ZIndex.Value ascending
                                         select it;

                foreach (var connector in childrenConnectors)
                {
                    //DuplicateConnector(childrenConnectedItems, oldNewList, connector, cloneGroup);
                }

                oldNewList.Add(new Tuple<SelectableDesignerItemViewModelBase, SelectableDesignerItemViewModelBase>(groupItem, cloneGroup));
                cloneGroup.ZIndex.Value = Layers.SelectMany(x => x.Children).Count();
                Add(cloneGroup);
            }
            else
            {
                var clone = item.Clone() as DesignerItemViewModelBase;
                clone.ZIndex.Value = Layers.SelectMany(x => x.Children).Count();
                clone.EdgeThickness.Value = item.EdgeThickness.Value;
                clone.IsHitTestVisible.Value = true;
                if (parent != null)
                {
                    clone.ParentID = parent.ID;
                    clone.EnableForSelection.Value = false;
                    parent.AddGroup(MainWindowVM.Recorder, clone);
                }
                oldNewList.Add(new Tuple<SelectableDesignerItemViewModelBase, SelectableDesignerItemViewModelBase>(item, clone));
                Add(clone);
            }
        }

        private void DuplicateConnector(List<Tuple<SelectableDesignerItemViewModelBase, SelectableDesignerItemViewModelBase>> oldNewList, ConnectorBaseViewModel connector, GroupItemViewModel groupItem = null)
        {
            var clone = connector.Clone() as ConnectorBaseViewModel;
            clone.ZIndex.Value = Layers.SelectMany(x => x.Children).Count();
            clone.IsHitTestVisible.Value = true;
            clone.SnapPoint0VM.Value.IsHitTestVisible.Value = true;
            clone.SnapPoint1VM.Value.IsHitTestVisible.Value = true;
            if (groupItem != null)
            {
                clone.ParentID = groupItem.ID;
                clone.EnableForSelection.Value = false;
                groupItem.AddGroup(MainWindowVM.Recorder, clone);
            }
            Add(clone);
        }

        [Obsolete]
        private void DuplicateConnector(IEnumerable<DesignerItemViewModelBase> connectedItems, List<Tuple<SelectableDesignerItemViewModelBase, SelectableDesignerItemViewModelBase>> oldNewList, ConnectorBaseViewModel connector, GroupItemViewModel groupItem = null)
        {
            var clone = connector.Clone() as ConnectorBaseViewModel;
            clone.ZIndex.Value = Layers.SelectMany(x => x.Children).Count();
            if (groupItem != null)
            {
                clone.ParentID = groupItem.ID;
                clone.EnableForSelection.Value = false;
                groupItem.AddGroup(MainWindowVM.Recorder, clone);
            }
            Add(clone);
        }

        public bool CanExecuteDuplicate()
        {
            return SelectedItems.Value.Count() > 0;
        }

        #endregion //Duplicate

        private IEnumerable<SelectableDesignerItemViewModelBase> GetGroupMembers(SelectableDesignerItemViewModelBase item)
        {
            var list = new List<SelectableDesignerItemViewModelBase>();
            list.Add(item);
            var children = Layers.SelectMany(x => x.Children)
                                 .Where(x => (x as LayerItem).Item.Value.ParentID == item.ID)
                                 .Select(x => (x as LayerItem).Item.Value);
            list.AddRange(children);
            return list;
        }

        public static Rect GetBoundingRectangle(IEnumerable<SelectableDesignerItemViewModelBase> items)
        {
            double x1 = Double.MaxValue;
            double y1 = Double.MaxValue;
            double x2 = Double.MinValue;
            double y2 = Double.MinValue;

            foreach (var item in items)
            {
                if (item is DesignerItemViewModelBase designerItem)
                {
                    var centerPoint = designerItem.CenterPoint.Value;
                    var angleInDegrees = designerItem.RotationAngle.Value;

                    var p0 = new Point(designerItem.Left.Value + designerItem.Width.Value, designerItem.Top.Value + designerItem.Height.Value / 2);
                    var p1 = new Point(designerItem.Left.Value, designerItem.Top.Value);
                    var p2 = new Point(designerItem.Left.Value + designerItem.Width.Value, designerItem.Top.Value);
                    var p3 = new Point(designerItem.Left.Value + designerItem.Width.Value, designerItem.Top.Value + designerItem.Height.Value);
                    var p4 = new Point(designerItem.Left.Value, designerItem.Top.Value + designerItem.Height.Value);

                    var vector_p0_center = p0 - centerPoint;
                    var vector_p1_center = p1 - centerPoint;
                    var vector_p2_center = p2 - centerPoint;
                    var vector_p3_center = p3 - centerPoint;
                    var vector_p4_center = p4 - centerPoint;

                    UpdateBoundary(ref x1, ref y1, ref x2, ref y2, centerPoint, angleInDegrees + Vector.AngleBetween(vector_p0_center, vector_p1_center), p1);
                    UpdateBoundary(ref x1, ref y1, ref x2, ref y2, centerPoint, angleInDegrees + Vector.AngleBetween(vector_p0_center, vector_p2_center), p2);
                    UpdateBoundary(ref x1, ref y1, ref x2, ref y2, centerPoint, angleInDegrees + Vector.AngleBetween(vector_p0_center, vector_p3_center), p3);
                    UpdateBoundary(ref x1, ref y1, ref x2, ref y2, centerPoint, angleInDegrees + Vector.AngleBetween(vector_p0_center, vector_p4_center), p4);
                }
                else if (item is ConnectorBaseViewModel connector)
                {
                    x1 = Math.Min(Math.Min(connector.Points[0].X, connector.Points[1].X), x1);
                    y1 = Math.Min(Math.Min(connector.Points[0].Y, connector.Points[1].Y), y1);

                    x2 = Math.Max(Math.Max(connector.Points[0].X, connector.Points[1].X), x2);
                    y2 = Math.Max(Math.Max(connector.Points[0].Y, connector.Points[1].Y), y2);
                }
            }

            return new Rect(new Point(x1, y1), new Point(x2, y2));
        }

        private static void UpdateBoundary(ref double x1, ref double y1, ref double x2, ref double y2, Point centerPoint, double angleInDegrees, Point point)
        {
            var rad = angleInDegrees * Math.PI / 180;

            var t = RotatePoint(centerPoint, point, rad);

            x1 = Math.Min(t.Item1, x1);
            y1 = Math.Min(t.Item2, y1);
            x2 = Math.Max(t.Item1, x2);
            y2 = Math.Max(t.Item2, y2);
        }

        private static Tuple<double, double> RotatePoint(Point center, Point point, double rad)
        {
            var z1 = point.X - center.X;
            var z2 = point.Y - center.Y;
            var x = center.X + Math.Sqrt(Math.Pow(z1, 2) + Math.Pow(z2, 2)) * Math.Cos(rad);
            var y = center.Y + Math.Sqrt(Math.Pow(z1, 2) + Math.Pow(z2, 2)) * Math.Sin(rad);

            return new Tuple<double, double>(x, y);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Layers.Dispose();
                    AllItems.Dispose();
                    SelectedItems.Dispose();
                    EdgeThickness.Dispose();
                    EnableMiniMap.Dispose();
                    FileName.Dispose();
                    CanvasBackground.Dispose();
                    EnablePointSnap.Dispose();
                    if (_AutoSaveTimerDisposableObj != null)
                        _AutoSaveTimerDisposableObj.Dispose();
                    MainWindowVM.Recorder.Current.StackChanged -= Current_StackChanged;
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
