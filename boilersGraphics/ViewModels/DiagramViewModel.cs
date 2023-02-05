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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Linq;
using TsOperationHistory;
using TsOperationHistory.Extensions;
using Unity;
using Layers = boilersGraphics.Views.Layers;
using Line = System.Windows.Shapes.Line;
using Path = System.Windows.Shapes.Path;
using Preference = boilersGraphics.Models.Preference;
using Version = System.Version;

namespace boilersGraphics.ViewModels;

public class DiagramViewModel : BindableBase, IDiagramViewModel, IDisposable
{
    private IDisposable _AutoSaveTimerDisposableObj;
    private double _CanvasBorderThickness;
    private readonly CompositeDisposable _CompositeDisposable = new();
    private Point _CurrentPoint;
    private ObservableCollection<Color> _FillColors = new();
    private int _Height;
    private bool _MiddleButtonIsPressed;
    private Point _MousePointerPosition;
    private int _Width;
    private DesignerCanvas designerCanvas;
    private bool disposedValue;
    private readonly IDialogService dlgService;

    public DiagramViewModel(MainWindowViewModel mainWindowViewModel, bool isPreview = false)
    {
        MainWindowVM = mainWindowViewModel;
        Instance = this;

        if (!App.IsTest)
        {
            RenderWidth = Observable.Return(Application.Current.MainWindow.GetChildOfType<DiagramControl>())
                .Where(x => x != null)
                .Select(x => x.ActualWidth)
                .ToReadOnlyReactivePropertySlim(1000);
            RenderHeight = Observable.Return(Application.Current.MainWindow.GetChildOfType<DiagramControl>())
                .Where(x => x != null)
                .Select(x => x.ActualHeight)
                .ToReadOnlyReactivePropertySlim(1000);
        }

        if (!isPreview)
        {
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
            BringForegroundCommand =
                new DelegateCommand(() => ExecuteBringForegroundCommand(), () => CanExecuteOrder());
            SendBackgroundCommand = new DelegateCommand(() => ExecuteSendBackgroundCommand(), () => CanExecuteOrder());
            AlignTopCommand = new DelegateCommand(() => ExecuteAlignTopCommand(), () => CanExecuteAlign());
            AlignVerticalCenterCommand =
                new DelegateCommand(() => ExecuteAlignVerticalCenterCommand(), () => CanExecuteAlign());
            AlignBottomCommand = new DelegateCommand(() => ExecuteAlignBottomCommand(), () => CanExecuteAlign());
            AlignLeftCommand = new DelegateCommand(() => ExecuteAlignLeftCommand(), () => CanExecuteAlign());
            AlignHorizontalCenterCommand =
                new DelegateCommand(() => ExecuteAlignHorizontalCenterCommand(), () => CanExecuteAlign());
            AlignRightCommand = new DelegateCommand(() => ExecuteAlignRightCommand(), () => CanExecuteAlign());
            DistributeHorizontalCommand =
                new DelegateCommand(() => ExecuteDistributeHorizontalCommand(), () => CanExecuteDistribute());
            DistributeVerticalCommand =
                new DelegateCommand(() => ExecuteDistributeVerticalCommand(), () => CanExecuteDistribute());
            SelectAllCommand = new DelegateCommand(() => ExecuteSelectAllCommand());
            SettingCommand = new DelegateCommand(() => ExecuteSettingCommand());
            UniformWidthCommand = new DelegateCommand(() => ExecuteUniformWidthCommand(), () => CanExecuteUniform());
            UniformHeightCommand = new DelegateCommand(() => ExecuteUniformHeightCommand(), () => CanExecuteUniform());
            DuplicateCommand = new DelegateCommand(() => ExecuteDuplicateCommand(), () => CanExecuteDuplicate());
            CutCommand = new DelegateCommand(() => ExecuteCutCommand(), () => CanExecuteCut());
            CopyCommand = new DelegateCommand(() => ExecuteCopyCommand(), () => CanExecuteCopy());
            CopyCanvasToClipboardCommand = new DelegateCommand(() => ExecuteCopyCanvasToClipboardCommand());
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
                LogManager.GetCurrentClassLogger().Trace("MouseWheelCommand");
                var diagramControl = Application.Current.MainWindow.GetChildOfType<DiagramControl>();
                var zoomBox = diagramControl.GetChildOfType<ZoomBox>();
                if (args.Delta > 0)
                    zoomBox.ZoomSliderPlus();
                else if (args.Delta < 0)
                    zoomBox.ZoomSliderMinus();
                args.Handled = true;
            });
            PreviewMouseDownCommand = new DelegateCommand<MouseEventArgs>(args =>
            {
                LogManager.GetCurrentClassLogger().Trace("PreviewMouseDownCommand");
                if (args.MiddleButton == MouseButtonState.Pressed)
                {
                    _MiddleButtonIsPressed = true;
                    var diagramControl = Application.Current.MainWindow.GetChildOfType<DiagramControl>();
                    _MousePointerPosition = args.GetPosition(diagramControl);
                    diagramControl.Cursor = Cursors.SizeAll;
                }
            });
            PreviewMouseUpCommand = new DelegateCommand<MouseEventArgs>(args =>
            {
                LogManager.GetCurrentClassLogger().Trace("PreviewMouseUpCommand");
                ReleaseMiddleButton(args);
            });
            MouseMoveCommand = new DelegateCommand<MouseEventArgs>(args =>
            {
                LogManager.GetCurrentClassLogger().Trace("MouseMoveCommand");
                if (_MiddleButtonIsPressed)
                {
                    var diagramControl = Application.Current.MainWindow.GetChildOfType<DiagramControl>();
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
                LogManager.GetCurrentClassLogger().Trace("MouseLeaveCommand");
                if (_MiddleButtonIsPressed) ReleaseMiddleButton(args);
            });
            MouseEnterCommand = new DelegateCommand<MouseEventArgs>(args =>
            {
                LogManager.GetCurrentClassLogger().Trace("MouseEnterCommand");
                if (_MiddleButtonIsPressed) ReleaseMiddleButton(args);
            });
            PreviewKeyDownCommand = new DelegateCommand<KeyEventArgs>(args =>
            {
                LogManager.GetCurrentClassLogger().Trace("PreviewKeyDownCommand");
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
                LogManager.GetCurrentClassLogger().Trace("EditMenuOpenedCommand");
                CutCommand.RaiseCanExecuteChanged();
                CopyCommand.RaiseCanExecuteChanged();
                PasteCommand.RaiseCanExecuteChanged();
            });
            PropertyCommand = new DelegateCommand(() =>
                {
                    LogManager.GetCurrentClassLogger().Trace("PropertyCommand");
                    var first = SelectedItems.Value.First();
                    first.OpenPropertyDialog();
                },
                () => CanOpenPropertyDialog());
            MouseDownStraightLineCommand = new DelegateCommand<Line>(line =>
            {
                LogManager.GetCurrentClassLogger().Trace("MouseDownStraightLineCommand");
                var straightLineVM = line.DataContext as StraightConnectorViewModel;
                straightLineVM.IsSelected.Value = true;
                straightLineVM.SnapPoint0VM.Value.IsSelected.Value = true;
                straightLineVM.SnapPoint1VM.Value.IsSelected.Value = true;
            });
            MouseDownBezierCurveCommand = new DelegateCommand<Path>(line =>
            {
                LogManager.GetCurrentClassLogger().Trace("MouseDownBezierCurveCommand");
                var bezierCurveVM = line.DataContext as BezierCurveViewModel;
                bezierCurveVM.IsSelected.Value = true;
                bezierCurveVM.SnapPoint0VM.Value.IsSelected.Value = true;
                bezierCurveVM.SnapPoint1VM.Value.IsSelected.Value = true;
            });
            MouseDownPolyBezierCommand = new DelegateCommand<Path>(line =>
            {
                LogManager.GetCurrentClassLogger().Trace("MouseDownPolyBezierCommand");
                var polyBezierVM = line.DataContext as PolyBezierViewModel;
                polyBezierVM.IsSelected.Value = true;
                polyBezierVM.SnapPoint0VM.Value.IsSelected.Value = true;
                polyBezierVM.SnapPoint1VM.Value.IsSelected.Value = true;
            });
            LoadedCommand = new DelegateCommand(() => { LogManager.GetCurrentClassLogger().Trace("LoadedCommand"); });
            FitCanvasCommand = new DelegateCommand(() =>
                {
                    LogManager.GetCurrentClassLogger().Trace("FitCanvasCommand");
                    var horizontalGap = AllItems.Value.OfType<DesignerItemViewModelBase>()
                        .Except(new DesignerItemViewModelBase[] { BackgroundItem.Value }).Count() > 0
                        ? AllItems.Value.OfType<DesignerItemViewModelBase>()
                            .Except(new DesignerItemViewModelBase[] { BackgroundItem.Value }).Min(x => x.Left.Value)
                        : 0;
                    var verticalGap = AllItems.Value.OfType<DesignerItemViewModelBase>()
                        .Except(new DesignerItemViewModelBase[] { BackgroundItem.Value }).Count() > 0
                        ? AllItems.Value.OfType<DesignerItemViewModelBase>()
                            .Except(new DesignerItemViewModelBase[] { BackgroundItem.Value }).Min(x => x.Top.Value)
                        : 0;
                    foreach (var item in AllItems.Value.OfType<ConnectorBaseViewModel>())
                    foreach (var p in item.Points)
                    {
                        horizontalGap = Math.Min(p.X, horizontalGap);
                        verticalGap = Math.Min(p.Y, verticalGap);
                    }

                    foreach (var item in AllItems.Value.OfType<DesignerItemViewModelBase>()
                                 .Except(new DesignerItemViewModelBase[] { BackgroundItem.Value }))
                    {
                        item.Left.Value += -horizontalGap;
                        item.Top.Value += -verticalGap;
                    }

                    foreach (var item in AllItems.Value.OfType<ConnectorBaseViewModel>())
                        for (var i = 0; i < item.Points.Count; i++)
                        {
                            var p = item.Points[i];
                            var newP = new Point(p.X - horizontalGap, p.Y - verticalGap);
                            item.Points[i] = newP;
                        }

                    var horizontalMax = AllItems.Value.OfType<DesignerItemViewModelBase>()
                        .Except(new DesignerItemViewModelBase[] { BackgroundItem.Value }).Count() > 0
                        ? AllItems.Value.OfType<DesignerItemViewModelBase>()
                            .Except(new DesignerItemViewModelBase[] { BackgroundItem.Value }).Max(x => x.Right.Value)
                        : 0;
                    var verticalMax = AllItems.Value.OfType<DesignerItemViewModelBase>()
                        .Except(new DesignerItemViewModelBase[] { BackgroundItem.Value }).Count() > 0
                        ? AllItems.Value.OfType<DesignerItemViewModelBase>()
                            .Except(new DesignerItemViewModelBase[] { BackgroundItem.Value }).Max(x => x.Bottom.Value)
                        : 0;
                    foreach (var item in AllItems.Value.OfType<ConnectorBaseViewModel>())
                    foreach (var p in item.Points)
                    {
                        horizontalMax = Math.Max(p.X, horizontalMax);
                        verticalMax = Math.Max(p.Y, verticalMax);
                    }

                    var horizontalMin = AllItems.Value.OfType<DesignerItemViewModelBase>()
                        .Except(new DesignerItemViewModelBase[] { BackgroundItem.Value }).Count() > 0
                        ? AllItems.Value.OfType<DesignerItemViewModelBase>()
                            .Except(new DesignerItemViewModelBase[] { BackgroundItem.Value }).Min(x => x.Left.Value)
                        : 0;
                    var verticalMin = AllItems.Value.OfType<DesignerItemViewModelBase>()
                        .Except(new DesignerItemViewModelBase[] { BackgroundItem.Value }).Count() > 0
                        ? AllItems.Value.OfType<DesignerItemViewModelBase>()
                            .Except(new DesignerItemViewModelBase[] { BackgroundItem.Value }).Min(x => x.Top.Value)
                        : 0;

                    BackgroundItem.Value.Left.Value = Math.Round(horizontalMin);
                    BackgroundItem.Value.Top.Value = Math.Round(verticalMin);
                    BackgroundItem.Value.Width.Value = Math.Round(horizontalMax);
                    BackgroundItem.Value.Height.Value = Math.Round(verticalMax);
                },
                () => AllItems.Value.OfType<DesignerItemViewModelBase>()
                        .Except(new DesignerItemViewModelBase[] { BackgroundItem.Value }).Count() +
                    AllItems.Value.OfType<ConnectorBaseViewModel>().Count() > 0);
            ClearCanvasCommand = new DelegateCommand(() => { InitialSetting(mainWindowViewModel, true); });
            OnLoaded = new ReactiveCommand().WithSubscribe(() =>
            {
                DesignerCanvas = Application.Current.MainWindow.GetChildOfType<DesignerCanvas>();
                Layers.ToList().ForEach(x => x.UpdateAppearanceBothParentAndChild());
            }).AddTo(_CompositeDisposable);
        }

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
                .Where(x => x is not null)
                .OrderBy(x => x.ZIndex.Value)
                .ToArray())
            .ToReadOnlyReactivePropertySlim(Array.Empty<SelectableDesignerItemViewModelBase>());

        if (!isPreview)
        {
            AllItems.Subscribe(x =>
                {
                    FitCanvasCommand.RaiseCanExecuteChanged();
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
                    .OfType<LayerItem>()
                    .Select(y => y.Item.Value)
                    .Except(Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
                        .OfType<LayerItem>()
                        .Select(y => y.Item.Value)
                        .OfType<ConnectorBaseViewModel>())
                    .Union(Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
                        .OfType<LayerItem>()
                        .Select(y => y.Item.Value)
                        .OfType<ConnectorBaseViewModel>()
                        .SelectMany(x => new[] { x.SnapPoint0VM.Value, x.SnapPoint1VM.Value })
                        .Where(y => y.IsSelected.Value)
                    )
                    .Where(z => z.IsSelected.Value)
                    .OrderBy(z => z.SelectedOrder.Value)
                    .ToArray()
                ).ToReadOnlyReactivePropertySlim(Array.Empty<SelectableDesignerItemViewModelBase>());

            SelectedItems.Subscribe(selectedItems =>
                {
                    LogManager.GetCurrentClassLogger()
                        .Debug(
                            $"SelectedItems changed {string.Join(", ", selectedItems.Select(x => x?.ToString() ?? "null"))}");

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

                    ClipCommand.RaiseCanExecuteChanged();

                    PropertyCommand.RaiseCanExecuteChanged();
                })
                .AddTo(_CompositeDisposable);

            SelectedLayers = Layers.ObserveElementObservableProperty(x => x.IsSelected)
                .Select(_ => Layers.Where(x => x.IsSelected.Value).ToArray())
                .ToReadOnlyReactivePropertySlim(Array.Empty<LayerTreeViewItemBase>());

            SelectedLayers.Subscribe(x =>
                {
                    LogManager.GetCurrentClassLogger()
                        .Trace($"SelectedLayers changed {string.Join(", ", x.Select(x => x.ToString()))}");
                })
                .AddTo(_CompositeDisposable);

            Layers.ObserveAddChanged()
                .Subscribe(x =>
                {
                    RootLayer.Value.Children = new ReactiveCollection<LayerTreeViewItemBase>(Layers.ToObservable());
                    x.SetParentToChildren(RootLayer.Value);
                })
                .AddTo(_CompositeDisposable);
        }

        if (!isPreview)
        {
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
            var logSettings = dao.FindBy(new Dictionary<string, object> { { "ID", id } });
            if (logSettings.Count() == 0)
            {
                var newLogSetting = new LogSetting();
                newLogSetting.ID = id;
                newLogSetting.LogLevel = LogLevel.Info.ToString();
                dao.Insert(newLogSetting);
            }

            logSettings = dao.FindBy(new Dictionary<string, object> { { "ID", id } });
            var logSetting = logSettings.First();
            MainWindowVM.LogLevel.Value = LogLevel.FromString(logSetting.LogLevel);
            PackAutoSaveFiles();
        }

        AngleType.Value = Helpers.AngleType.Minus180To180;
        EnableImageEmbedding.Value = true;
        ColorSpots.Value = new ColorSpots();
        EnableCombine.Value = true;
        EnableLayers.Value = true;
        EnableWorkHistory.Value = true;

        SettingIfDebug();
    }

    public DiagramViewModel(MainWindowViewModel MainWindowVM, IDialogService dlgService)
        : this(MainWindowVM)
    {
        this.dlgService = dlgService;

        Mediator.Instance.Register(this);
    }

    public static DiagramViewModel Instance { get; private set; }
    public DelegateCommand<object> CreateNewDiagramCommand { get; }
    public DelegateCommand LoadCommand { get; }
    public DelegateCommand<string> LoadFileCommand { get; }
    public DelegateCommand SaveCommand { get; }
    public DelegateCommand OverwriteCommand { get; }
    public DelegateCommand ExportCommand { get; }
    public DelegateCommand GroupCommand { get; }
    public DelegateCommand UngroupCommand { get; }
    public DelegateCommand BringForegroundCommand { get; }
    public DelegateCommand BringForwardCommand { get; }
    public DelegateCommand SendBackwardCommand { get; }
    public DelegateCommand SendBackgroundCommand { get; }
    public DelegateCommand AlignTopCommand { get; }
    public DelegateCommand AlignVerticalCenterCommand { get; }
    public DelegateCommand AlignBottomCommand { get; }
    public DelegateCommand AlignLeftCommand { get; }
    public DelegateCommand AlignHorizontalCenterCommand { get; }
    public DelegateCommand AlignRightCommand { get; }
    public DelegateCommand DistributeHorizontalCommand { get; }
    public DelegateCommand DistributeVerticalCommand { get; }
    public DelegateCommand SelectAllCommand { get; }
    public DelegateCommand SettingCommand { get; }
    public DelegateCommand UniformWidthCommand { get; }
    public DelegateCommand UniformHeightCommand { get; }
    public DelegateCommand DuplicateCommand { get; }
    public DelegateCommand CutCommand { get; }
    public DelegateCommand CopyCommand { get; }
    public DelegateCommand CopyCanvasToClipboardCommand { get; }
    public DelegateCommand PasteCommand { get; }
    public DelegateCommand EditMenuOpenedCommand { get; }
    public DelegateCommand UnionCommand { get; }
    public DelegateCommand IntersectCommand { get; }
    public DelegateCommand XorCommand { get; }
    public DelegateCommand ExcludeCommand { get; }
    public DelegateCommand ClipCommand { get; }
    public DelegateCommand UndoCommand { get; }
    public DelegateCommand RedoCommand { get; }
    public DelegateCommand<MouseWheelEventArgs> MouseWheelCommand { get; }
    public DelegateCommand<MouseEventArgs> PreviewMouseDownCommand { get; }
    public DelegateCommand<MouseEventArgs> PreviewMouseUpCommand { get; }
    public DelegateCommand<MouseEventArgs> MouseMoveCommand { get; }
    public DelegateCommand<MouseEventArgs> MouseLeaveCommand { get; }
    public DelegateCommand<MouseEventArgs> MouseEnterCommand { get; }
    public DelegateCommand<KeyEventArgs> PreviewKeyDownCommand { get; }
    public DelegateCommand PropertyCommand { get; }
    public DelegateCommand<Line> MouseDownStraightLineCommand { get; }
    public DelegateCommand<Path> MouseDownBezierCurveCommand { get; }
    public DelegateCommand<Path> MouseDownPolyBezierCommand { get; }
    public DelegateCommand LoadedCommand { get; }
    public DelegateCommand FitCanvasCommand { get; }
    public DelegateCommand ClearCanvasCommand { get; }
    public ReactiveCommand OnLoaded { get; }
    public MainWindowViewModel MainWindowVM { get; }

    public DelegateCommand<object> AddItemCommand { get; }
    public DelegateCommand<object> RemoveItemCommand { get; }
    public DelegateCommand<object> ClearSelectedItemsCommand { get; }

    public void DeselectAll()
    {
        foreach (var layerItem in Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
                     .OfType<LayerItem>())
        {
            layerItem.Item.Value.IsSelected.Value = false;
            layerItem.IsSelected.Value = false;
            if (layerItem.Item.Value is ConnectorBaseViewModel c)
            {
                c.SnapPoint0VM.Value.IsSelected.Value = false;
                c.SnapPoint1VM.Value.IsSelected.Value = false;
            }
        }
    }

    public void Dispose()
    {
        // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public IEnumerable<Tuple<SnapPoint, Point>> GetSnapPoints(IEnumerable<SnapPoint> exceptSnapPoints)
    {
        var resizeThumbs = DesignerCanvas.EnumerateChildOfType<SnapPoint>();
        var sets = resizeThumbs
            .Where(x => !exceptSnapPoints.Contains(x))
            .Select(x => new Tuple<SnapPoint, Point>(x, GetCenter(x)))
            .Distinct();
        return sets;
    }

    public IEnumerable<Tuple<SnapPoint, Point>> GetSnapPoints(Point exceptPoint)
    {
        var resizeThumbs = DesignerCanvas.EnumerateChildOfType<SnapPoint>();
        var sets = resizeThumbs
            .Where(x => x.InputHitTest(exceptPoint) == null)
            .Select(x => new Tuple<SnapPoint, Point>(x, GetCenter(x)))
            .Distinct();
        return sets;
    }

    private void ExecuteCopyCanvasToClipboardCommand()
    {
        var renderer = new Renderer(new WpfVisualTreeHelper());
        var bitmap = renderer.Render(null, DesignerCanvas.GetInstance(), this, BackgroundItem.Value);
        Clipboard.SetImage(bitmap);
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
            var files = Directory.EnumerateFiles(
                System.IO.Path.Combine(Helpers.Path.GetRoamingDirectory(), "dhq_boiler\\boilersGraphics\\AutoSave"),
                "AutoSave-*-*-*-*-*-*.xml");
            foreach (var file in files.OrderByDescending(x => new FileInfo(x).LastWriteTime))
                AutoSaveFiles.AddOnScheduler(file);
        }
        catch (DirectoryNotFoundException)
        {
            //Ignore it as it only happens on Azure DevOps
        }
    }

    private bool CanOpenPropertyDialog()
    {
        return (SelectedItems.Value.Length == 1 && SelectedItems.Value.First().SupportsPropertyDialog)
               || (SelectedItems.Value.OfType<SnapPointViewModel>().Count() == 2 && SelectedItems.Value
                   .OfType<SnapPointViewModel>().First().Parent.Value.SupportsPropertyDialog);
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

    public void Initialize(bool isPreview = false)
    {
        MainWindowVM.Recorder.BeginRecode();

        InitialSetting(MainWindowVM, true, true, isPreview);

        MainWindowVM.Recorder.EndRecode();

        MainWindowVM.Controller.Flush();

        SetAutoSave();
    }

    private void SetAutoSave()
    {
        if (_AutoSaveTimerDisposableObj != null) _AutoSaveTimerDisposableObj.Dispose();

        MainWindowVM.Recorder.Current.StackChanged -= Current_StackChanged;

        if (EnableAutoSave.Value)
        {
            if (AutoSaveType.Value == Models.AutoSaveType.SetInterval)
            {
                var source = Observable.Timer(AutoSaveInterval.Value, AutoSaveInterval.Value);

                _AutoSaveTimerDisposableObj = source.Subscribe(_ => { AutoSave(); });
                _CompositeDisposable.Add(_AutoSaveTimerDisposableObj);
            }
            else if (AutoSaveType.Value == Models.AutoSaveType.EveryTimeCampusChanges)
            {
                MainWindowVM.Recorder.Current.StackChanged += Current_StackChanged;
            }
        }
    }

    private void Current_StackChanged(object sender, OperationStackChangedEventArgs e)
    {
        AutoSave();
    }

    private void AutoSave()
    {
        if (App.IsTest)
        {
            LogManager.GetCurrentClassLogger().Warn("AutoSave()が呼び出されましたが、App.IsTest=trueのため、処理を実行しませんでした。");
            return;
        }

        AutoSavedDateTime.Value = DateTime.Now;
        var path = System.IO.Path.Combine(Helpers.Path.GetRoamingDirectory(),
            $"dhq_boiler\\boilersGraphics\\AutoSave\\AutoSave-{AutoSavedDateTime.Value.Year}-{AutoSavedDateTime.Value.Month}-{AutoSavedDateTime.Value.Day}-{AutoSavedDateTime.Value.Hour}-{AutoSavedDateTime.Value.Minute}-{AutoSavedDateTime.Value.Second}.xml");
        var autoSaveDir = System.IO.Path.GetDirectoryName(path);
        if (!Directory.Exists(autoSaveDir)) Directory.CreateDirectory(autoSaveDir);

        App.GetCurrentApp().Dispatcher.Invoke(() =>
        {
            var versionXML = new XElement("Version", BGSXFileVersion.ToString());
            var layersXML = new XElement("Layers", ObjectSerializer.SerializeLayers(Layers));
            var configurationXML = new XElement("Configuration", ObjectSerializer.SerializeConfiguration(this));

            var root = new XElement("boilersGraphics");
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
        var leftTop = snapPoint.TransformToAncestor(DesignerCanvas).Transform(new Point(0, 0));
        switch (snapPoint.Tag)
        {
            case "左上":
                return new Point(leftTop.X + snapPoint.Width, leftTop.Y + snapPoint.Height);
            case "右上":
                return new Point(leftTop.X, leftTop.Y + snapPoint.Height);
            case "左下":
                return new Point(leftTop.X + snapPoint.Width, leftTop.Y);
            case "右下":
                return new Point(leftTop.X, leftTop.Y);
            case "左":
            case "上":
            case "右":
            case "下":
                return new Point(leftTop.X + snapPoint.Width / 2, leftTop.Y + snapPoint.Height / 2);
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
        var rtb = new RenderTargetBitmap((int)DesignerCanvas.ActualWidth, (int)DesignerCanvas.ActualHeight, 96, 96,
            PixelFormats.Pbgra32);

        var visual = new DrawingVisual();
        using (var context = visual.RenderOpen())
        {
            var brush = new VisualBrush(DesignerCanvas);
            context.DrawRectangle(brush, null,
                new Rect(new Point(), new Size(DesignerCanvas.Width, DesignerCanvas.Height)));

            var rand = new Random();
            foreach (var set in sets)
            {
                context.DrawText(
                    new FormattedText((string)set.Item1.Tag, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                        new Typeface("メイリオ"), 12, Randomizer.RandomColorBrush(rand),
                        VisualTreeHelper.GetDpi(designerCanvas).PixelsPerDip), set.Item2);
                context.DrawEllipse(Brushes.Red, new Pen(Brushes.Red, 1), set.Item2, 2, 2);
            }
        }

        rtb.Render(visual);

        OpenCvSharpHelper.ImShow("DebugPrint", rtb);
    }

    private void InitialSetting(MainWindowViewModel mainwindowViewModel, bool addingLayer = false,
        bool initCanvasBackground = false, bool isPreview = false)
    {
        mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "EdgeBrush.Value", Brushes.Black as Brush);
        mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "FillBrush.Value", Brushes.White as Brush);
        mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "EdgeThickness.Value", 1.0);
        mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "CanvasBorderThickness", 0.0);
        if (initCanvasBackground)
            mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "CanvasFillBrush.Value",
                Brushes.White as Brush);
        mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "BackgroundItem.Value",
            new BackgroundViewModel(this));
        mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "BackgroundItem.Value.ZIndex.Value", -1);
        mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "BackgroundItem.Value.FillBrush.Value",
            CanvasFillBrush.Value);
        mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "BackgroundItem.Value.Left.Value", 0d);
        mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "BackgroundItem.Value.Top.Value", 0d);
        BackgroundItem.Value.Width.Subscribe(width =>
            {
                if (Application.Current == null || Application.Current.MainWindow == null)
                    return;
                if (DesignerCanvas is null)
                    return;
                DesignerCanvas.Width = width;
            })
            .AddTo(_CompositeDisposable);
        BackgroundItem.Value.Height.Subscribe(height =>
            {
                if (Application.Current == null || Application.Current.MainWindow == null)
                    return;
                if (DesignerCanvas is null)
                    return;
                DesignerCanvas.Height = height;
            })
            .AddTo(_CompositeDisposable);
        mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "BackgroundItem.Value.Width.Value", 1000d);
        mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "BackgroundItem.Value.Height.Value", 1000d);
        mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "BackgroundItem.Value.Owner", this);
        mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "BackgroundItem.Value.EdgeBrush.Value",
            Brushes.Black as Brush);
        mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "BackgroundItem.Value.EdgeThickness.Value", 1d);
        mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "BackgroundItem.Value.EnableForSelection.Value",
            false);
        mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "BackgroundItem.Value.IsVisible.Value", true);
        mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "EnablePointSnap.Value", true);
        mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "LayerCount", 1);
        mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "LayerItemCount", 1);
        RootLayer.Dispose();
        RootLayer = new ReactivePropertySlim<LayerTreeViewItemBase>(new RootLayer());
        Layers.ToClearOperation().ExecuteTo(mainwindowViewModel.Recorder.Current);
        if (addingLayer)
        {
            var layer = new Layer(isPreview);
            layer.IsVisible.Value = true;
            layer.IsSelected.Value = true;
            layer.Name.Value = Name.GetNewLayerName(this);
            var rand = new Random();
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
        MainWindowVM.Recorder.BeginRecode();
        var picture = SelectedItems.Value.OfType<PictureDesignerItemViewModel>().First();
        var other = SelectedItems.Value.OfType<DesignerItemViewModelBase>().Last();
        var left = -(other.Left.Value - picture.Left.Value);
        var top = -(other.Top.Value - picture.Top.Value);
        var right = -(picture.Right.Value - other.Right.Value);
        var bottom = -(picture.Bottom.Value - other.Bottom.Value);
        var image = new Image();
        image.BeginInit();
        image.Width = picture.Width.Value;
        image.Height = picture.Height.Value;
        image.Source = picture.EmbeddedImage.Value != null
            ? picture.EmbeddedImage.Value
            : ToBitmapSource(picture.FileName);
        var g = GeometryCreator.Translate(other.PathGeometry.Value, -left, -top);
        image.Clip = g;
        image.Stretch = Stretch.Fill;
        image.EndInit();
        var canvas = new Canvas();
        canvas.Width = picture.Width.Value;
        canvas.Height = picture.Height.Value;
        canvas.Children.Add(image);
        var size = new Size(canvas.Width + other.EdgeThickness.Value, canvas.Height + other.EdgeThickness.Value);
        canvas.Measure(size);
        canvas.Arrange(new Rect(size));
        canvas.RenderTransform = new TranslateTransform(left, top);
        canvas.UpdateLayout();
        var newCroppedPicture = new CroppedPictureDesignerItemViewModel();
        newCroppedPicture.PathGeometryNoRotate.Value = other.PathGeometryNoRotate.Value;
        newCroppedPicture.PathGeometryRotate.Value = other.PathGeometryRotate.Value;
        newCroppedPicture.EnablePathGeometryUpdate.Value = false;
        newCroppedPicture.Left.Value = other.Left.Value;
        newCroppedPicture.Top.Value = other.Top.Value;
        newCroppedPicture.Width.Value = other.Width.Value;
        newCroppedPicture.Height.Value = other.Height.Value;
        newCroppedPicture.EnablePathGeometryUpdate.Value = true;
        newCroppedPicture.EdgeBrush.Value = other.EdgeBrush.Value;
        newCroppedPicture.EdgeThickness.Value = other.EdgeThickness.Value;
        newCroppedPicture.FillBrush.Value = new SolidColorBrush(Colors.Transparent);
        var encoder = new PngBitmapEncoder();
        var bitmap =
            new RenderTargetBitmap((int)other.Width.Value, (int)other.Height.Value, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(canvas);
        OpenCvSharpHelper.ImShow("Clipping", bitmap);
        var frame = BitmapFrame.Create(bitmap);
        encoder.Frames.Add(frame);
        var bitmapImage = new BitmapImage();
        using (var stream = new MemoryStream())
        {
            encoder.Save(stream);
            stream.Seek(0, SeekOrigin.Begin);
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = stream;
            bitmapImage.EndInit();
        }

        newCroppedPicture.EmbeddedImage.Value = bitmapImage;
        newCroppedPicture.Owner = this;
        Add(newCroppedPicture);
        Remove(picture);
        Remove(other);
        MainWindowVM.Recorder.EndRecode();
    }

    private BitmapSource ToBitmapSource(string fileName)
    {
        using (var stream = File.OpenRead(fileName))
        {
            return BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
        }
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
            var firstElementTypeIsCorrect =
                SelectedItems.ElementAt(0).GetType() != typeof(PictureDesignerItemViewModel);
            var secondElementTypeIsCorrect =
                SelectedItems.ElementAt(1).GetType() != typeof(PictureDesignerItemViewModel);
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
            var firstElementTypeIsCorrect =
                SelectedItems.ElementAt(0).GetType() != typeof(PictureDesignerItemViewModel);
            var secondElementTypeIsCorrect =
                SelectedItems.ElementAt(1).GetType() != typeof(PictureDesignerItemViewModel);
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
            var firstElementTypeIsCorrect =
                SelectedItems.ElementAt(0).GetType() != typeof(PictureDesignerItemViewModel);
            var secondElementTypeIsCorrect =
                SelectedItems.ElementAt(1).GetType() != typeof(PictureDesignerItemViewModel);
            return countIsCorrent && firstElementTypeIsCorrect && secondElementTypeIsCorrect;
        }

        return false;
    }

    private void ExecuteUnionCommand()
    {
        CombineAndAddItem(GeometryCombineMode.Union);
        UpdateStatisticsCountUnion();
    }

    public bool CanExecuteUnion()
    {
        var countIsCorrent = GetCountIsCorrent();
        if (countIsCorrent)
        {
            var firstElementTypeIsCorrect =
                SelectedItems.ElementAt(0).GetType() != typeof(PictureDesignerItemViewModel);
            var secondElementTypeIsCorrect =
                SelectedItems.ElementAt(1).GetType() != typeof(PictureDesignerItemViewModel);
            return countIsCorrent && firstElementTypeIsCorrect && secondElementTypeIsCorrect;
        }

        var polyBezier = GetSelectedItemsForCombine().FirstOrDefault() as PolyBezierViewModel;
        if (polyBezier != null) return true;
        return false;
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
            MainWindowVM.Recorder.Current.ExecuteSetProperty(combine, "ZIndex.Value",
                Layers.SelectMany(x => x.Children).Count());
            MainWindowVM.Recorder.Current.ExecuteSetProperty(combine, "IsHitTestVisible.Value",
                MainWindowVM.ToolBarViewModel.CurrentHitTestVisibleState.Value);
            MainWindowVM.Recorder.Current.ExecuteSetProperty(combine, "PathGeometry.Value",
                GeometryCreator.CreateCombineGeometry(pb));
            MainWindowVM.Recorder.Current.ExecuteSetProperty(combine, "Left.Value",
                combine.PathGeometry.Value.Bounds.Left);
            MainWindowVM.Recorder.Current.ExecuteSetProperty(combine, "Top.Value",
                combine.PathGeometry.Value.Bounds.Top);
            MainWindowVM.Recorder.Current.ExecuteSetProperty(combine, "Width.Value",
                combine.PathGeometry.Value.Bounds.Width);
            MainWindowVM.Recorder.Current.ExecuteSetProperty(combine, "Height.Value",
                combine.PathGeometry.Value.Bounds.Height);
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
            MainWindowVM.Recorder.Current.ExecuteSetProperty(combine, "ZIndex.Value",
                Layers.SelectMany(x => x.Children).Count());
            MainWindowVM.Recorder.Current.ExecuteSetProperty(combine, "IsHitTestVisible.Value",
                MainWindowVM.ToolBarViewModel.CurrentHitTestVisibleState.Value);
            MainWindowVM.Recorder.Current.ExecuteSetProperty(combine, "PathGeometryNoRotate.Value",
                GeometryCreator.CreateCombineGeometry(item1, item2));
            if (combine.PathGeometryNoRotate.Value == null || combine.PathGeometryNoRotate.Value.Figures.Count() == 0)
            {
                var item1PathGeometry = item1.PathGeometryNoRotate.Value;
                var item2PathGeometry = item2.PathGeometryNoRotate.Value;

                if (item1 is DesignerItemViewModelBase designerItem1)
                {
                    if (item1.RotationAngle.Value != 0) item1PathGeometry = designerItem1.PathGeometryRotate.Value;
                    if (designerItem1 is not CombineGeometryViewModel)
                        item1PathGeometry = GeometryCreator.Translate(item1PathGeometry, designerItem1.Left.Value,
                            designerItem1.Top.Value);
                }

                if (item2 is DesignerItemViewModelBase designerItem2)
                {
                    if (item2.RotationAngle.Value != 0) item2PathGeometry = designerItem2.PathGeometryRotate.Value;

                    if (designerItem2 is not CombineGeometryViewModel)
                        item2PathGeometry = GeometryCreator.Translate(item2PathGeometry, designerItem2.Left.Value,
                            designerItem2.Top.Value);
                }


                CastToLetterAndSetTransform(item1, item2, item1PathGeometry, item2PathGeometry);

                MainWindowVM.Recorder.Current.ExecuteSetProperty(combine, "PathGeometryNoRotate.Value",
                    Geometry.Combine(item1PathGeometry, item2PathGeometry, mode, null));
            }

            MainWindowVM.Recorder.Current.ExecuteSetProperty(combine, "Left.Value",
                combine.PathGeometryNoRotate.Value.Bounds.Left);
            MainWindowVM.Recorder.Current.ExecuteSetProperty(combine, "Top.Value",
                combine.PathGeometryNoRotate.Value.Bounds.Top);
            MainWindowVM.Recorder.Current.ExecuteSetProperty(combine, "Width.Value",
                combine.PathGeometryNoRotate.Value.Bounds.Width);
            MainWindowVM.Recorder.Current.ExecuteSetProperty(combine, "Height.Value",
                combine.PathGeometryNoRotate.Value.Bounds.Height);
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

    private void CastToLetterAndSetTransform(SelectableDesignerItemViewModelBase item1,
        SelectableDesignerItemViewModelBase item2, PathGeometry item1PathGeometry, PathGeometry item2PathGeometry)
    {
        InternalCastToLetterAndSetTransform(item1, item1PathGeometry);
        InternalCastToLetterVerticalAndSetTransform(item1, item1PathGeometry);
        InternalCastToLetterAndSetTransform(item2, item2PathGeometry);
        InternalCastToLetterVerticalAndSetTransform(item2, item2PathGeometry);
        InternalCastToPolygonAndSetTransform(item1, item1PathGeometry);
        InternalCastToPolygonAndSetTransform(item2, item2PathGeometry);
    }

    private void InternalCastToPolygonAndSetTransform(SelectableDesignerItemViewModelBase item,
        PathGeometry itemPathGeometry)
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
            MainWindowVM.Recorder.Current.ExecuteSetPropertyWithEnforcePropertyType<PathGeometry, Transform>(
                itemPathGeometry, "Transform", transformGroup);
        }
    }

    private void InternalCastToLetterVerticalAndSetTransform(SelectableDesignerItemViewModelBase item,
        PathGeometry itemPathGeometry)
    {
        if (item is LetterVerticalDesignerItemViewModel)
        {
            var item_ = item as LetterVerticalDesignerItemViewModel;
            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(new TranslateTransform(item_.Left.Value, item_.Top.Value));
            if (itemPathGeometry.Transform != null)
                transformGroup.Children.Add(itemPathGeometry.Transform);
            MainWindowVM.Recorder.Current.ExecuteSetPropertyWithEnforcePropertyType<PathGeometry, Transform>(
                itemPathGeometry, "Transform", transformGroup);
            item_.CloseLetterSettingDialog();
        }
    }

    private void InternalCastToLetterAndSetTransform(SelectableDesignerItemViewModelBase item,
        PathGeometry itemPathGeometry)
    {
        if (item is LetterDesignerItemViewModel)
        {
            var item_ = item as LetterDesignerItemViewModel;
            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(new TranslateTransform(item_.Left.Value, item_.Top.Value));
            if (itemPathGeometry.Transform != null)
                transformGroup.Children.Add(itemPathGeometry.Transform);
            MainWindowVM.Recorder.Current.ExecuteSetPropertyWithEnforcePropertyType<PathGeometry, Transform>(
                itemPathGeometry, "Transform", transformGroup);
            item_.CloseLetterSettingDialog();
        }
    }

    private bool GetCountIsCorrent()
    {
        var newlist = GetSelectedItemsForCombine();
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
            var encoder = new JpegBitmapEncoder();
            var memoryStream = new MemoryStream();
            var bImg = new BitmapImage();

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
            pic.PathGeometryNoRotate.Value = null;
            pic.Height.Value = bImg.PixelHeight;
            pic.FileWidth = bImg.PixelWidth;
            pic.FileHeight = bImg.PixelHeight;
            pic.IsVisible.Value = true;
            pic.IsSelected.Value = true;
            pic.IsHitTestVisible.Value = true;
            pic.ZIndex.Value = pic.Owner.Layers
                .SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children).Count();
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

                        if (copyObjsHasItems)
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
            SelectedLayers.Value.ToList().ForEach(x =>
            {
                foreach (var selectedItem in SelectedItems.Value)
                {
                    x.RemoveItem(MainWindowVM, selectedItem);
                    selectedItem.Dispose();
                }
            });
        else if (SelectedLayers.Value.Count() > 0)
            //Copy Layer and LayerItem
            foreach (var selectedLayer in SelectedLayers.Value)
                Layers.Remove(selectedLayer);

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
            copyObj.Add(ObjectSerializer.ExtractItems(Layers.SelectMany(x => x.Children)
                .Where(x => (x as LayerItem).IsSelected.Value).Cast<LayerItem>()));
            Clipboard.SetDataObject(new ClipboardDTO(root.ToString()), false);
        }
        else if (SelectedLayers.Value.Count() > 0)
        {
            //Copy Layer and LayerItem
            var root = new XElement("boilersGraphics");
            var copyObj = new XElement("CopyObjects");
            root.Add(copyObj);
            copyObj.Add(new XElement("Layers"));
            copyObj.Element("Layers")
                .Add(ObjectSerializer.SerializeLayers(SelectedLayers.Value.ToObservableCollection()));
            Clipboard.SetDataObject(new ClipboardDTO(root.ToString()), false);
        }
    }

    public bool CanExecuteCut()
    {
        return (SelectedLayers.Value.Count() > 0 && SelectedItems.Value.Count() > 0)
               || SelectedLayers.Value.Count() > 0;
    }

    private void ExecuteSettingCommand()
    {
        IDialogResult result = null;
        var preferences = new Preference();
        preferences.Width.Value = (int)BackgroundItem.Value.Width.Value;
        preferences.Height.Value = (int)BackgroundItem.Value.Height.Value;
        Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
            .Where(x => x.GetType() == typeof(LayerItem))
            .Select(y => (y as LayerItem).Item.Value)
            .Where(z => z is BrushViewModel)
            .Cast<BrushViewModel>()
            .ToList()
            .ForEach(z =>
            {
                z.Width.Value = BackgroundItem.Value.Width.Value;
                z.Height.Value = BackgroundItem.Value.Height.Value;
            });
        preferences.CanvasFillBrush.Value = CanvasFillBrush.Value;
        preferences.CanvasEdgeThickness.Value = BackgroundItem.Value.EdgeThickness.Value;
        preferences.CanvasEdgeBrush.Value = BackgroundItem.Value.EdgeBrush.Value;
        preferences.EnablePointSnap.Value = EnablePointSnap.Value;
        preferences.SnapPower.Value =
            (Application.Current.MainWindow.DataContext as MainWindowViewModel).SnapPower.Value;
        preferences.EnableAutoSave.Value = EnableAutoSave.Value;
        preferences.AutoSaveType.Value = AutoSaveType.Value;
        preferences.AutoSaveInterval.Value = AutoSaveInterval.Value;
        preferences.AngleType.Value = AngleType.Value;
        preferences.EnableImageEmbedding.Value = EnableImageEmbedding.Value;
        dlgService.ShowDialog(nameof(Views.Preference), new DialogParameters { { "Preferences", preferences } },
            ret => result = ret);
        if (result != null && result.Result == ButtonResult.OK)
        {
            var s = result.Parameters.GetValue<Preference>("Preferences");
            CanvasFillBrush.Value = s.CanvasFillBrush.Value;
            BackgroundItem.Value.EdgeThickness.Value = s.CanvasEdgeThickness.Value;
            BackgroundItem.Value.FillBrush.Value = CanvasFillBrush.Value;
            BackgroundItem.Value.EdgeBrush.Value = s.CanvasEdgeBrush.Value;
            EnablePointSnap.Value = s.EnablePointSnap.Value;
            (Application.Current.MainWindow.DataContext as MainWindowViewModel).SnapPower.Value = s.SnapPower.Value;
            BackgroundItem.Value.Width.Value = s.Width.Value;
            BackgroundItem.Value.Height.Value = s.Height.Value;
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
            var diagramControl = Application.Current.MainWindow.GetChildOfType<DiagramControl>();
            diagramControl.Cursor = Cursors.Arrow;
        }
    }

    private void ExecuteAddItemCommand(object parameter)
    {
        if (parameter is SelectableDesignerItemViewModelBase item)
        {
            var targetLayer = GetSelectedLayer();
            if (targetLayer == null)
                return;
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

    private LayerTreeViewItemBase GetSelectedLayer()
    {
        var targetLayer = SelectedLayers.Value.FirstOrDefault();
        if (targetLayer == null)
            targetLayer = Layers.FirstOrDefault();
        if (targetLayer == null)
        {
            LogManager.GetCurrentClassLogger().Warn("レイヤーが選択されていません。");
            return null;
        }

        return targetLayer;
    }

    private void ExecuteRemoveItemCommand(object parameter)
    {
        if (parameter is SelectableDesignerItemViewModelBase)
        {
            var item = (SelectableDesignerItemViewModelBase)parameter;
            if (item is SnapPointViewModel snapPoint && !(snapPoint.Parent.Value is null))
                item = snapPoint.Parent.Value;
            RemoveGroupMembers(item);
            Remove(item);
            if (item is LetterDesignerItemViewModel) (item as LetterDesignerItemViewModel).CloseLetterSettingDialog();
            if (item is LetterVerticalDesignerItemViewModel)
                (item as LetterVerticalDesignerItemViewModel).CloseLetterSettingDialog();
            item.Dispose();
            UpdateZIndex();
        }
    }

    private void UpdateZIndex()
    {
        var items = (from item in Layers.SelectMany(x => x.Children)
            orderby (item as LayerItem).Item.Value.ZIndex.Value
            select item).ToList();

        for (var i = 0; i < items.Count; ++i) (items.ElementAt(i) as LayerItem).Item.Value.ZIndex.Value = i;
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
        foreach (var layerItem in Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
                     .OfType<LayerItem>())
        {
            layerItem.Item.Value.IsSelected.Value = false;
            if (layerItem.Item.Value is ConnectorBaseViewModel c)
            {
                c.SnapPoint0VM.Value.IsSelected.Value = false;
                c.SnapPoint1VM.Value.IsSelected.Value = false;
            }
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
        dlgService.ShowDialog(nameof(Export), ret => result = ret);
        if (result != null)
        {
        }
    }

    private void Add(SelectableDesignerItemViewModelBase item, string layerItemName = null)
    {
        var selectedLayer = GetSelectedLayer();
        selectedLayer.AddItem(MainWindowVM, this, item, layerItemName);
    }

    private void Add(LayerItem item)
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

    private void ExecuteSelectAllCommand()
    {
        Layers.SelectMany(x => x.Children).ToList().ForEach(x => (x as LayerItem).Item.Value.IsSelected.Value = true);
    }

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
        var x1 = double.MaxValue;
        var y1 = double.MaxValue;
        var x2 = double.MinValue;
        var y2 = double.MinValue;

        foreach (var item in items)
            if (item is DesignerItemViewModelBase designerItem)
            {
                var centerPoint = designerItem.CenterPoint.Value;
                var angleInDegrees = designerItem.RotationAngle.Value;

                var p0 = new Point(designerItem.Left.Value + designerItem.Width.Value,
                    designerItem.Top.Value + designerItem.Height.Value / 2);
                var p1 = new Point(designerItem.Left.Value, designerItem.Top.Value);
                var p2 = new Point(designerItem.Left.Value + designerItem.Width.Value, designerItem.Top.Value);
                var p3 = new Point(designerItem.Left.Value + designerItem.Width.Value,
                    designerItem.Top.Value + designerItem.Height.Value);
                var p4 = new Point(designerItem.Left.Value, designerItem.Top.Value + designerItem.Height.Value);

                var vector_p0_center = p0 - centerPoint;
                var vector_p1_center = p1 - centerPoint;
                var vector_p2_center = p2 - centerPoint;
                var vector_p3_center = p3 - centerPoint;
                var vector_p4_center = p4 - centerPoint;

                UpdateBoundary(ref x1, ref y1, ref x2, ref y2, centerPoint,
                    angleInDegrees + Vector.AngleBetween(vector_p0_center, vector_p1_center), p1);
                UpdateBoundary(ref x1, ref y1, ref x2, ref y2, centerPoint,
                    angleInDegrees + Vector.AngleBetween(vector_p0_center, vector_p2_center), p2);
                UpdateBoundary(ref x1, ref y1, ref x2, ref y2, centerPoint,
                    angleInDegrees + Vector.AngleBetween(vector_p0_center, vector_p3_center), p3);
                UpdateBoundary(ref x1, ref y1, ref x2, ref y2, centerPoint,
                    angleInDegrees + Vector.AngleBetween(vector_p0_center, vector_p4_center), p4);
            }
            else if (item is ConnectorBaseViewModel connector)
            {
                x1 = Math.Min(Math.Min(connector.Points[0].X, connector.Points[1].X), x1);
                y1 = Math.Min(Math.Min(connector.Points[0].Y, connector.Points[1].Y), y1);

                x2 = Math.Max(Math.Max(connector.Points[0].X, connector.Points[1].X), x2);
                y2 = Math.Max(Math.Max(connector.Points[0].Y, connector.Points[1].Y), y2);
            }

        return new Rect(new Point(x1, y1), new Point(x2, y2));
    }

    private static void UpdateBoundary(ref double x1, ref double y1, ref double x2, ref double y2, Point centerPoint,
        double angleInDegrees, Point point)
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
                EnableCombine.Dispose();
                EnableLayers.Dispose();
                FileName.Dispose();
                CanvasFillBrush.Dispose();
                EnablePointSnap.Dispose();
                if (_AutoSaveTimerDisposableObj != null)
                    _AutoSaveTimerDisposableObj.Dispose();
                MainWindowVM.Recorder.Current.StackChanged -= Current_StackChanged;
            }

            disposedValue = true;
        }
    }

    #region Property

    public ReactivePropertySlim<LayerTreeViewItemBase> RootLayer { get; set; } = new(new RootLayer());

    public ReactiveCollection<LayerTreeViewItemBase> Layers { get; }

    public ReadOnlyReactivePropertySlim<LayerTreeViewItemBase[]> SelectedLayers { get; }

    public ReadOnlyReactivePropertySlim<SelectableDesignerItemViewModelBase[]> AllItems { get; }

    public ReadOnlyReactivePropertySlim<SelectableDesignerItemViewModelBase[]> SelectedItems { get; }

    public ReactivePropertySlim<BackgroundViewModel> BackgroundItem { get; } = new();

    public ReactivePropertySlim<double?> EdgeThickness { get; } = new();

    public ReactivePropertySlim<bool> EnableMiniMap { get; } = new();

    public ReactivePropertySlim<bool> EnableCombine { get; } = new();

    public ReactivePropertySlim<bool> EnableLayers { get; } = new();

    public ReactivePropertySlim<bool> EnableWorkHistory { get; } = new();

    public ReactivePropertySlim<bool> EnableBrushThickness { get; } = new();

    public ReactivePropertySlim<string> FileName { get; } = new();

    public ReactivePropertySlim<Brush> CanvasFillBrush { get; } = new();

    public ReactivePropertySlim<bool> EnablePointSnap { get; } = new();

    public ReactivePropertySlim<bool> EnableAutoSave { get; } = new();

    public ReactivePropertySlim<DateTime> AutoSavedDateTime { get; } = new();

    public ReactivePropertySlim<AutoSaveType> AutoSaveType { get; } = new();

    public ReactivePropertySlim<TimeSpan> AutoSaveInterval { get; } = new(TimeSpan.FromMinutes(1));

    public ReactiveCollection<string> AutoSaveFiles { get; set; } = new();

    public ReactivePropertySlim<AngleType> AngleType { get; set; } = new();

    public ReactivePropertySlim<bool> EnableImageEmbedding { get; set; } = new();

    public ReactivePropertySlim<Visibility> ContextMenuVisibility { get; } = new();

    public ReactivePropertySlim<ColorSpots> ColorSpots { get; } = new();

    public ReactivePropertySlim<Brush> EdgeBrush { get; } = new();
    public ReactivePropertySlim<Brush> FillBrush { get; } = new();

    public ReadOnlyReactivePropertySlim<double> RenderWidth { get; }
    public ReadOnlyReactivePropertySlim<double> RenderHeight { get; }

    /// <summary>
    ///     現在ポインティングしている座標
    ///     ステータスバー上の座標インジケーターに使用される
    /// </summary>
    public Point CurrentPoint
    {
        get => _CurrentPoint;
        set => SetProperty(ref _CurrentPoint, value);
    }

    public double CanvasBorderThickness
    {
        get => _CanvasBorderThickness;
        set => SetProperty(ref _CanvasBorderThickness, value);
    }

    public double ScaleX { get; set; } = 1.0;
    public double ScaleY { get; set; } = 1.0;
    public Version BGSXFileVersion { get; } = new(2, 4);

    public int LayerCount { get; set; } = 1;

    public int LayerItemCount { get; set; } = 1;

    public DesignerCanvas DesignerCanvas { get; private set; }

    public IEnumerable<Tuple<SnapPoint, Point>> SnapPoints
    {
        get
        {
            var resizeThumbs = DesignerCanvas.EnumerateChildOfType<SnapPoint>();
            var sets = resizeThumbs
                .Select(x => new Tuple<SnapPoint, Point>(x, GetCenter(x)))
                .Distinct();
            return sets;
        }
    }

    /// <summary>
    ///     拡大率
    /// </summary>
    public ReactivePropertySlim<double> MagnificationRate { get; } = new(100);

    #endregion //Property

    #region Save

    private void ExecuteSaveCommand()
    {
        var versionXML = new XElement("Version", BGSXFileVersion.ToString());
        var layersXML = new XElement("Layers", ObjectSerializer.SerializeLayers(Layers));
        var configurationXML = new XElement("Configuration", ObjectSerializer.SerializeConfiguration(this));

        var root = new XElement("boilersGraphics");
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
        var versionXML = new XElement("Version", BGSXFileVersion.ToString());
        var layersXML = new XElement("Layers", ObjectSerializer.SerializeLayers(Layers));
        var configurationXML = new XElement("Configuration", ObjectSerializer.SerializeConfiguration(this));

        var root = new XElement("boilersGraphics");
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

    private async Task ExecuteLoadCommand()
    {
        var result = MessageBox.Show(Resources.Message_CanvasWillDiscardedConfirm, Resources.DialogTitle_Confirm,
            MessageBoxButton.OKCancel);
        if (result == MessageBoxResult.Cancel)
            return;
        var (root, filename) = LoadSerializedDataFromFile();
        await LoadInternal(root, filename);
        var statistics = MainWindowVM.Statistics.Value;
        statistics.NumberOfTimesTheFileWasOpenedBySpecifyingIt++;
        var dao = new StatisticsDao();
        dao.Update(statistics);
    }

    private async Task LoadInternal(XElement root, string filename, bool isPreview = false)
    {
        if (root == null) return;

        if (root.Element("Version") != null)
        {
            var version = new Version(root.Element("Version").Value);
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

        var willLoadingObjCount = ObjectDeserializer.CountObjectsFromXML(root);
        
        Func<ProgressBarWithOutputViewModel, Task> loadAction = async (vm) =>
        {
            var mainwindowViewModel = MainWindowVM;
            try
            {

                LogManager.GetCurrentClassLogger().Info(Resources.Log_BeginLoadFromXml);
                mainwindowViewModel.Recorder.BeginRecode();

                var configuration = root.Element("Configuration");
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "CanvasFillBrush.Value",
                    WpfObjectSerializer.Deserialize(configuration.Element("CanvasFillBrush").Nodes().First()
                        .ToString()) ??
                    new SolidColorBrush((Color)ColorConverter.ConvertFromString(configuration
                        .Element("CanvasFillBrush")
                        .Nodes().First().ToString())));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "EnablePointSnap.Value",
                    bool.Parse(configuration.Element("EnablePointSnap").Value));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(mainwindowViewModel, "SnapPower.Value",
                    double.Parse(configuration.Element("SnapPower").Value));
                if (configuration.Element("ColorSpots") != null)
                {
                    var colorSpots = configuration.Element("ColorSpots");
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot0",
                        WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot0").Nodes().First()
                            .ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot1",
                        WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot1").Nodes().First()
                            .ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot2",
                        WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot2").Nodes().First()
                            .ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot3",
                        WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot3").Nodes().First()
                            .ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot4",
                        WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot4").Nodes().First()
                            .ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot5",
                        WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot5").Nodes().First()
                            .ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot6",
                        WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot6").Nodes().First()
                            .ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot7",
                        WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot7").Nodes().First()
                            .ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot8",
                        WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot8").Nodes().First()
                            .ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot9",
                        WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot9").Nodes().First()
                            .ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot10",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot10").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot11",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot11").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot12",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot12").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot13",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot13").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot14",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot14").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot15",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot15").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot16",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot16").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot17",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot17").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot18",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot18").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot19",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot19").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot20",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot20").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot21",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot21").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot22",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot22").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot23",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot23").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot24",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot24").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot25",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot25").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot26",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot26").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot27",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot27").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot28",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot28").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot29",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot29").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot30",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot30").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot31",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot31").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot32",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot32").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot33",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot33").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot34",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot34").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot35",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot35").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot36",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot36").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot37",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot37").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot38",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot38").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot39",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot39").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot40",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot40").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot41",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot41").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot42",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot42").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot43",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot43").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot44",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot44").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot45",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot45").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot46",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot46").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot47",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot47").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot48",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot48").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot49",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot49").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot50",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot50").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot51",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot51").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot52",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot52").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot53",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot53").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot54",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot54").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot55",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot55").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot56",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot56").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot57",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot57").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot58",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot58").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot59",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot59").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot60",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot60").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot61",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot61").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot62",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot62").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot63",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot63").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot64",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot64").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot65",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot65").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot66",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot66").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot67",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot67").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot68",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot68").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot69",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot69").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot70",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot70").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot71",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot71").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot72",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot72").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot73",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot73").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot74",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot74").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot75",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot75").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot76",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot76").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot77",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot77").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot78",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot78").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot79",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot79").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot80",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot80").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot81",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot81").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot82",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot82").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot83",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot83").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot84",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot84").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot85",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot85").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot86",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot86").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot87",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot87").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot88",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot88").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot89",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot89").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot90",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot90").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot91",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot91").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot92",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot92").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot93",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot93").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot94",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot94").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot95",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot95").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot96",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot96").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot97",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot97").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot98",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot98").Nodes().First().ToString()));
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot99",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot99").Nodes().First().ToString()));
                }

                InitialSetting(mainwindowViewModel, false, false, isPreview);

                if (configuration.Element("Left") != null)
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "BackgroundItem.Value.Left.Value",
                        double.Parse(configuration.Element("Left").Value));
                if (configuration.Element("Top") != null)
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "BackgroundItem.Value.Top.Value",
                        double.Parse(configuration.Element("Top").Value));
                if (configuration.Element("Width") != null)
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this,
                        "BackgroundItem.Value.Width.Value",
                        double.Parse(configuration.Element("Width").Value));
                if (configuration.Element("Height") != null)
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this,
                        "BackgroundItem.Value.Height.Value",
                        double.Parse(configuration.Element("Height").Value));
                ObjectDeserializer.ReadObjectsFromXML(this, vm, root,
                    isPreview);

                await PostProcessInFileLoadingSequence(mainwindowViewModel).ConfigureAwait(false);

                LogManager.GetCurrentClassLogger().Info(Resources.Log_FinishLoadFromXml);
            }
            catch (Exception)
            {
                MessageBox.Show(Resources.Message_FileCannotOpenBecauseTooOldOrCorrupted,
                    Resources.DialogTitle_ReadError);
                LogManager.GetCurrentClassLogger().Error(Resources.Log_FileCannotOpenBecauseTooOldOrCorrupted);
                FileName.Value = "*";
                return;
            }
            finally
            {
                mainwindowViewModel.Recorder.EndRecode();
                mainwindowViewModel.Controller.Flush();
            }
            LogManager.GetCurrentClassLogger().Info(string.Format(Resources.Log_LoadedFile, filename));
        };
        IDialogResult dialogResult = new DialogResult();
        dlgService.ShowDialog(nameof(ProgressBarWithOutput),
            new DialogParameters() { { "LoadAction", loadAction }, { "Maximum", willLoadingObjCount } },
            ret => dialogResult = ret);
        if (dialogResult.Result != ButtonResult.OK)
        {
            FileName.Value = "*";
        }
    }

    private async Task PostProcessInFileLoadingSequence(MainWindowViewModel mainwindowViewModel)
    {
        LogManager.GetCurrentClassLogger().Info(Resources.Log_BeginPostProcessInFileLoadingSequence);
        ScanEffectViewModelObjects();

        var layersViewModel = Application.Current.MainWindow.GetChildOfType<Layers>().DataContext as LayersViewModel;
        layersViewModel.InitializeHitTestVisible(mainwindowViewModel);
        Layers.First().IsSelected.Value = true;

        LogManager.GetCurrentClassLogger().Info(Resources.Log_FinishPostProcessInFileLoadingSequence);
    }

    private int Count(List<FrameworkElement> allViews)
    {
        int count = 0;
        foreach (var item in AllItems.Value)
        {
            var view = allViews.FirstOrDefault(x => x.DataContext == item);
            if (view is not null)
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    ///     ファイルロード後にこのメソッドを実行することで、すべての EffectViewModel オブジェクトをレンダリングします。
    ///     注意：ZIndex の小さい方から順にレンダリングが実施されます。
    /// </summary>
    private void ScanEffectViewModelObjects()
    {
        App.Current.Dispatcher.BeginInvoke(() =>
        {
            foreach (var item in AllItems.Value.OrderBy(x => x.ZIndex.Value))
            {
                if (item is EffectViewModel effect)
                {
                    effect.Initialize();
                    effect.Render();
                }
            }
        });
    }

    private void ExecuteLoadCommand(string file, bool showConfirmDialog = true)
    {
        if (showConfirmDialog)
        {
            var result = MessageBox.Show(Resources.Message_CanvasWillDiscardedConfirm, Resources.DialogTitle_Confirm,
                MessageBoxButton.OKCancel);
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
        LoadInternal(root, file, true);
    }

    private (XElement, string) LoadSerializedDataFromFile()
    {
        var openFile = new OpenFileDialog();
        openFile.Filter = "Designer Files (*.xml)|*.xml|All Files (*.*)|*.*";

        var oldFileName = FileName.Value;

        if (openFile.ShowDialog() == true)
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

        return (null, string.Empty);
    }

    #endregion //Load

    #region Grouping

    private void ExecuteGroupItemsCommand()
    {
        MainWindowVM.Recorder.BeginRecode();

        var items = SelectedItems.Value.Select(x => x is SnapPointViewModel sp
            ? sp.Parent.Value
            : x
        ).Distinct().Where(x => Equals(x.ParentID, Guid.Empty)).ToList();

        var rect = GetBoundingRectangle(items);

        var groupItem = new GroupItemViewModel();
        groupItem.Width.Value = rect.Width;
        groupItem.Height.Value = rect.Height;
        groupItem.Left.Value = rect.Left;
        groupItem.Top.Value = rect.Top;
        groupItem.IsHitTestVisible.Value = MainWindowVM.ToolBarViewModel.CurrentHitTestVisibleState.Value;

        AddItemCommand.Execute(groupItem);

        var groupItemLayerItem =
            Layers.SelectMany(x => x.Children).First(x => (x as LayerItem).Item.Value == groupItem);

        var list = new List<Tuple<LayerItem, LayerTreeViewItemBase>>();

        foreach (var item in items)
        {
            var layerItem =
                Layers.SelectMany(x => x.Children).First(x => (x as LayerItem).Item.Value == item) as LayerItem;
            list.Add(new Tuple<LayerItem, LayerTreeViewItemBase>(layerItem, layerItem.Parent.Value));
            MainWindowVM.Recorder.Current.ExecuteSetProperty(layerItem, "Parent.Value", groupItemLayerItem);
            MainWindowVM.Recorder.Current.ExecuteAdd(groupItemLayerItem.Children, layerItem);
            groupItem.AddGroup(MainWindowVM.Recorder, item);
            MainWindowVM.Recorder.Current.ExecuteSetProperty(item, "ParentID", groupItem.ID);
            MainWindowVM.Recorder.Current.ExecuteSetProperty(item, "EnableForSelection.Value", false);
        }

        foreach (var item in list)
        {
            var layerItem = item.Item1;
            var parent = item.Item2;
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
            .First(x => x as LayerItem == layerItem)
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
            var children =
                (from child in Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
                    where child is LayerItem && (child as LayerItem).Item.Value.ParentID == groupRoot.ID
                    select child).ToList();

            var group = groupRoot as GroupItemViewModel;

            foreach (var child in children)
            {
                var layerItem = child as LayerItem;
                MainWindowVM.Recorder.Current.ExecuteDispose(layerItem.Item.Value.GroupDisposable,
                    () => group.GroupDisposable = group.Subscribe(layerItem.Item.Value));
                MainWindowVM.Recorder.Current.ExecuteSetProperty(layerItem.Item.Value, "ParentID", Guid.Empty);
                MainWindowVM.Recorder.Current.ExecuteSetProperty(layerItem.Item.Value, "EnableForSelection.Value",
                    true);
                MainWindowVM.Recorder.Current.ExecuteSetProperty(layerItem, "Parent.Value", Layers
                    .SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
                    .Where(x => x is LayerItem)
                    .First(x => (x as LayerItem).Item == (child as LayerItem).Item)
                    .Parent.Value
                    .Parent.Value);
                MainWindowVM.Recorder.Current.ExecuteSetProperty(layerItem, "IsSelected.Value", true);
                layerItem.Parent.Value.AddChildren(MainWindowVM.Recorder, layerItem);
            }

            var clone = (GroupItemViewModel)groupRoot.Clone();

            MainWindowVM.Recorder.Current.ExecuteDispose(groupRoot, () => { groupRoot.Swap(clone); });

            Remove(groupRoot);

            var groupZIndex = groupRoot.ZIndex.Value;

            var it = from item in Layers.SelectMany(x => x.Children)
                where (item as LayerItem).Item.Value.ZIndex.Value > groupZIndex
                select item;

            foreach (var x in it)
                MainWindowVM.Recorder.Current.ExecuteSetProperty((x as LayerItem).Item.Value, "ZIndex.Value",
                    (x as LayerItem).Item.Value.ZIndex.Value - 1);
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
    ///     前面へ移動
    /// </summary>
    private void ExecuteBringForwardCommand()
    {
        MainWindowVM.Recorder.BeginRecode();

        var ordered = from item in SelectedItems.Value
            orderby item.ZIndex.Value descending
            select item;

        var count = Layers.SelectMany(x => x.Children).Count();

        for (var i = 0; i < ordered.Count(); ++i)
        {
            var currentIndex = ordered.ElementAt(i).ZIndex.Value;
            if (SelectedLayers.Value.First().Children.Max(x => (x as LayerItem).Item.Value.ZIndex.Value) ==
                currentIndex)
                continue; //レイヤー内の最大ZIndex値と同じだった場合はcontinueして次の選択アイテムへ
            var next = (from x in Layers.SelectMany(x => x.Children)
                where (x as LayerItem).Item.Value.ZIndex.Value == currentIndex + 1
                select x).SingleOrDefault();

            if (next == null) continue;

            var newIndex = (next as LayerItem).Item.Value.ParentID != Guid.Empty
                ? (Layers.SelectMany(x => x.Children)
                        .Single(x => (x as LayerItem).Item.Value.ID == (next as LayerItem).Item.Value.ParentID) as
                    LayerItem).Item.Value.ZIndex.Value
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

                    var youngestChildrenZIndex = 0;

                    for (var j = 0; j < children.Count(); ++j)
                    {
                        var child = children.ElementAt(j);
                        youngestChildrenZIndex = newIndex - j - 1;
                        MainWindowVM.Recorder.Current.ExecuteSetProperty((child as LayerItem).Item.Value,
                            "ZIndex.Value", newIndex - j - 1);
                    }

                    var younger = from item in Layers.SelectMany(xx => xx.Children)
                        where (item as LayerItem).Item.Value.ID != ordered.ElementAt(i).ID &&
                              (item as LayerItem).Item.Value.ParentID != ordered.ElementAt(i).ID
                              && (item as LayerItem).Item.Value.ZIndex.Value <= ordered.ElementAt(i).ZIndex.Value &&
                              (item as LayerItem).Item.Value.ZIndex.Value >= youngestChildrenZIndex
                        select item;

                    var x = from item in Layers.SelectMany(xx => xx.Children)
                        where (item as LayerItem).Item.Value.ID != ordered.ElementAt(i).ID &&
                              (item as LayerItem).Item.Value.ParentID != ordered.ElementAt(i).ID
                              && (item as LayerItem).Item.Value.ZIndex.Value < youngestChildrenZIndex
                        select item;

                    var z = x.ToList();
                    z.AddRange(younger);

                    for (var j = 0; j < z.Count(); ++j)
                        MainWindowVM.Recorder.Current.ExecuteSetProperty((z.ElementAt(j) as LayerItem).Item.Value,
                            "ZIndex.Value", j);
                }
                else
                {
                    MainWindowVM.Recorder.Current.ExecuteSetProperty(ordered.ElementAt(i), "ZIndex.Value", newIndex);
                    var exists = Layers.SelectMany(x => x.Children)
                        .Where(item => (item as LayerItem).Item.Value.ZIndex.Value == newIndex);

                    foreach (var item in exists)
                    {
                        ((item as LayerItem).Item.Value as EffectViewModel)?.DisposeMonitoringItem(ordered.ElementAt(i));
                        if ((item as LayerItem).Item.Value != ordered.ElementAt(i))
                        {
                            if ((item as LayerItem).Item.Value is GroupItemViewModel)
                            {
                                var children = from it in Layers.SelectMany(x => x.Children)
                                               where (it as LayerItem).Item.Value.ParentID == (item as LayerItem).Item.Value.ID
                                               select it;

                                foreach (var child in children)
                                    MainWindowVM.Recorder.Current.ExecuteSetProperty((child as LayerItem).Item.Value,
                                        "ZIndex.Value", (child as LayerItem).Item.Value.ZIndex.Value - 1);

                                MainWindowVM.Recorder.Current.ExecuteSetProperty((item as LayerItem).Item.Value,
                                    "ZIndex.Value", currentIndex + children.Count());
                            }
                            else
                            {
                                MainWindowVM.Recorder.Current.ExecuteSetProperty((item as LayerItem).Item.Value,
                                    "ZIndex.Value", currentIndex);
                            }

                            if ((item as LayerItem).Item.Value is EffectViewModel effect)
                            {
                                effect.Render();
                            }

                            break;
                        }
                    }

                    (ordered.ElementAt(i) as EffectViewModel)?.Render();
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
    ///     背面へ移動
    /// </summary>
    private void ExecuteSendBackwardCommand()
    {
        MainWindowVM.Recorder.BeginRecode();

        var ordered = from item in SelectedItems.Value
            orderby item.ZIndex.Value
            select item;

        var count = Layers.SelectMany(x => x.Children).Count();

        for (var i = 0; i < ordered.Count(); ++i)
        {
            var currentIndex = ordered.ElementAt(i).ZIndex.Value;
            if (SelectedLayers.Value.First().Children.Min(x => (x as LayerItem).Item.Value.ZIndex.Value) ==
                currentIndex)
                continue; //レイヤー内の最小ZIndex値と同じだった場合はcontinueして次の選択アイテムへ
            var previous = (from x in Layers.SelectMany(x => x.Children)
                where (x as LayerItem).Item.Value.ZIndex.Value == currentIndex - 1
                select x).SingleOrDefault();

            if (previous == null) continue;

            var newIndex = (previous as LayerItem).Item.Value is GroupItemViewModel
                ? Layers.SelectMany(x => x.Children)
                    .Where(x => (x as LayerItem).Item.Value.ParentID == (previous as LayerItem).Item.Value.ID)
                    .Min(x => (x as LayerItem).Item.Value.ZIndex.Value)
                : Math.Max(i, currentIndex - 1);
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

                    var youngestChildrenZIndex = 0;

                    for (var j = 0; j < children.Count(); ++j)
                    {
                        var child = children.ElementAt(j);
                        youngestChildrenZIndex = newIndex - j - 1;
                        MainWindowVM.Recorder.Current.ExecuteSetProperty((child as LayerItem).Item.Value,
                            "ZIndex.Value", newIndex - j - 1);
                    }

                    var older = from item in Layers.SelectMany(xx => xx.Children)
                        where (item as LayerItem).Item.Value.ID != ordered.ElementAt(i).ID &&
                              (item as LayerItem).Item.Value.ParentID != ordered.ElementAt(i).ID
                              && (item as LayerItem).Item.Value.ZIndex.Value <= ordered.ElementAt(i).ZIndex.Value &&
                              (item as LayerItem).Item.Value.ZIndex.Value >= youngestChildrenZIndex
                        select item;

                    var x = from item in Layers.SelectMany(xx => xx.Children)
                        where (item as LayerItem).Item.Value.ID != ordered.ElementAt(i).ID &&
                              (item as LayerItem).Item.Value.ParentID != ordered.ElementAt(i).ID
                              && (item as LayerItem).Item.Value.ZIndex.Value > ordered.ElementAt(i).ZIndex.Value
                        select item;

                    var z = older.ToList();
                    z.AddRange(x);
                    z.Reverse();

                    for (var j = 0; j < z.Count(); ++j)
                    {
                        var elm = z.ElementAt(j);
                        MainWindowVM.Recorder.Current.ExecuteSetProperty((elm as LayerItem).Item.Value, "ZIndex.Value",
                            Layers.SelectMany(xx => xx.Children).Count() - j - 1);
                    }
                }
                else
                {
                    MainWindowVM.Recorder.Current.ExecuteSetProperty(ordered.ElementAt(i), "ZIndex.Value", newIndex);
                    
                    (ordered.ElementAt(i) as EffectViewModel)?.Render();
                    
                    var exists = Layers.SelectMany(x => x.Children)
                        .Where(item => (item as LayerItem).Item.Value.ZIndex.Value == newIndex);

                    foreach (var item in exists)
                    {
                        if ((item as LayerItem).Item.Value != ordered.ElementAt(i))
                        {
                            if ((item as LayerItem).Item.Value.ParentID != Guid.Empty)
                            {
                                var children = from it in Layers.SelectMany(x => x.Children)
                                    where (it as LayerItem).Item.Value.ParentID ==
                                          (item as LayerItem).Item.Value.ParentID
                                    select it;

                                foreach (var child in children)
                                    MainWindowVM.Recorder.Current.ExecuteSetProperty((child as LayerItem).Item.Value,
                                        "ZIndex.Value", (child as LayerItem).Item.Value.ZIndex.Value + 1);

                                var parent = (from it in Layers.SelectMany(x => x.Children)
                                    where (it as LayerItem).Item.Value.ID == (item as LayerItem).Item.Value.ParentID
                                    select it).Single();

                                (parent as LayerItem).Item.Value.ZIndex.Value =
                                    children.Max(x => (x as LayerItem).Item.Value.ZIndex.Value) + 1;
                                MainWindowVM.Recorder.Current.ExecuteSetProperty((parent as LayerItem).Item.Value,
                                    "ZIndex.Value", children.Max(x => (x as LayerItem).Item.Value.ZIndex.Value) + 1);
                            }
                            else
                            {
                                MainWindowVM.Recorder.Current.ExecuteSetProperty((item as LayerItem).Item.Value,
                                    "ZIndex.Value", currentIndex);
                            }
                            ((item as LayerItem)?.Item.Value as EffectViewModel)?.BeginMonitoring(ordered.ElementAt(i));
                            ((item as LayerItem)?.Item.Value as EffectViewModel)?.Render();
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
    ///     最前面へ移動
    /// </summary>
    private void ExecuteBringForegroundCommand()
    {
        MainWindowVM.Recorder.BeginRecode();

        var ordered = from item in SelectedItems.Value
            orderby item.ZIndex.Value descending
            select item;

        var count = Layers.SelectMany(x => x.Children).Count();

        for (var i = 0; i < ordered.Count(); ++i)
        {
            var current = ordered.ElementAt(i);
            var currentIndex = current.ZIndex.Value;
            var newIndex = SelectedLayers.Value.SelectMany(x => x.Children).Count() - 1;
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

                    for (var j = 0; j < children.Count(); ++j)
                    {
                        var child = children.ElementAt(j);
                        MainWindowVM.Recorder.Current.ExecuteSetProperty((child as LayerItem).Item.Value,
                            "ZIndex.Value", current.ZIndex.Value - j - 1);
                    }

                    var minValue = children.Min(x => (x as LayerItem).Item.Value.ZIndex.Value);

                    var other = (from item in Layers.SelectMany(x => x.Children)
                        where (item as LayerItem).Item.Value.ParentID != current.ID &&
                              (item as LayerItem).Item.Value.ID != current.ID
                        orderby (item as LayerItem).Item.Value.ZIndex.Value descending
                        select item).ToList();

                    for (var j = 0; j < other.Count(); ++j)
                    {
                        var item = other.ElementAt(j);
                        MainWindowVM.Recorder.Current.ExecuteSetProperty((item as LayerItem).Item.Value, "ZIndex.Value",
                            minValue - j - 1);
                    }
                }
                else
                {
                    var exists = Layers.SelectMany(x => x.Children).Where(item =>
                        (item as LayerItem).Item.Value.ZIndex.Value <= newIndex &&
                        (item as LayerItem).Item.Value.ZIndex.Value > oldCurrentIndex);

                    foreach (var item in exists)
                        if ((item as LayerItem).Item.Value != current)
                            MainWindowVM.Recorder.Current.ExecuteSetProperty((item as LayerItem).Item.Value,
                                "ZIndex.Value", (item as LayerItem).Item.Value.ZIndex.Value - 1);
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
    ///     最背面へ移動
    /// </summary>
    private void ExecuteSendBackgroundCommand()
    {
        MainWindowVM.Recorder.BeginRecode();

        var ordered = from item in SelectedItems.Value
            orderby item.ZIndex.Value
            select item;

        var count = Layers.SelectMany(x => x.Children).Count();

        for (var i = 0; i < ordered.Count(); ++i)
        {
            var current = ordered.ElementAt(i);
            var currentIndex = current.ZIndex.Value;
            var newIndex = current is GroupItemViewModel
                ? Layers.SelectMany(x => x.Children).Where(x => (x as LayerItem).Item.Value.ParentID == current.ID)
                    .Count()
                : SelectedLayers.Value.First().Children.Min(x => (x as LayerItem).Item.Value.ZIndex.Value);
            if (currentIndex != newIndex)
            {
                var oldCurrentIndex = current.ZIndex.Value;
                MainWindowVM.Recorder.Current.ExecuteSetProperty(current, "ZIndex.Value", newIndex);
                (current as EffectViewModel)?.Render();

                if (current is GroupItemViewModel)
                {
                    var children = (from item in Layers.SelectMany(x => x.Children)
                        where (item as LayerItem).Item.Value.ParentID == current.ID
                        orderby (item as LayerItem).Item.Value.ZIndex.Value descending
                        select item).ToList();

                    for (var j = 0; j < children.Count(); ++j)
                    {
                        var child = children.ElementAt(j);
                        MainWindowVM.Recorder.Current.ExecuteSetProperty((child as LayerItem).Item.Value,
                            "ZIndex.Value", current.ZIndex.Value - j - 1);
                    }

                    var other = (from item in Layers.SelectMany(x => x.Children)
                        where (item as LayerItem).Item.Value.ParentID != current.ID &&
                              (item as LayerItem).Item.Value.ID != current.ID
                        orderby (item as LayerItem).Item.Value.ZIndex.Value descending
                        select item).ToList();

                    var maxValue = Layers.SelectMany(x => x.Children).Count() - 1;

                    for (var j = 0; j < other.Count(); ++j)
                    {
                        var item = other.ElementAt(j);
                        MainWindowVM.Recorder.Current.ExecuteSetProperty((item as LayerItem).Item.Value, "ZIndex.Value",
                            maxValue - j);
                    }
                }
                else
                {
                    var exists = Layers.SelectMany(x => x.Children).Where(item =>
                        (item as LayerItem).Item.Value.ZIndex.Value >= newIndex &&
                        (item as LayerItem).Item.Value.ZIndex.Value < oldCurrentIndex)
                        .Select(x => (x as LayerItem).Item.Value)
                        .Except(new List<SelectableDesignerItemViewModelBase>() { current })
                        .OrderBy(x => x.ZIndex.Value).ToList();

                    foreach (var item in exists)
                    {
                        if (item != current)
                        {
                            MainWindowVM.Recorder.Current.ExecuteSetProperty(item,
                                "ZIndex.Value", item.ZIndex.Value + 1);
                            (item as EffectViewModel)?.BeginMonitoring(current);
                            (item as EffectViewModel)?.Render();
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

    public static void Sort(ReactiveCollection<LayerTreeViewItemBase> target)
    {
        var list = target.ToList();

        foreach (var layer in list) target.Remove(layer);

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
            var top = GetTop(first);

            foreach (var item in SelectedItems.Value)
            {
                var delta = top - GetTop(item);
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
            var bottom = GetTop(first) + GetHeight(first) / 2;

            foreach (var item in SelectedItems.Value)
            {
                var delta = bottom - (GetTop(item) + GetHeight(item) / 2);
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
            var bottom = GetTop(first) + GetHeight(first);

            foreach (var item in SelectedItems.Value)
            {
                var delta = bottom - (GetTop(item) + GetHeight(item));
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
            var left = GetLeft(first);

            foreach (var item in SelectedItems.Value)
            {
                var delta = left - GetLeft(item);
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
            var center = GetLeft(first) + GetWidth(first) / 2;

            foreach (var item in SelectedItems.Value)
            {
                var delta = center - (GetLeft(item) + GetWidth(item) / 2);
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
            var right = GetLeft(first) + GetWidth(first);

            foreach (var item in SelectedItems.Value)
            {
                var delta = right - (GetLeft(item) + GetWidth(item));
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
    ///     左右に整列
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

            var left = double.MaxValue;
            var right = double.MinValue;
            double sumWidth = 0;

            foreach (var item in selectedItems)
            {
                left = Math.Min(left, GetLeft(item));
                right = Math.Max(right, GetLeft(item) + GetWidth(item));
                sumWidth += GetWidth(item);
            }

            var distance = Math.Max(0, (right - left - sumWidth) / (selectedItems.Count() - 1));
            var offset = GetLeft(selectedItems.First());

            foreach (var item in selectedItems)
            {
                var delta = offset - GetLeft(item);
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
    ///     上下に整列
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

            var top = double.MaxValue;
            var bottom = double.MinValue;
            double sumHeight = 0;

            foreach (var item in selectedItems)
            {
                top = Math.Min(top, GetTop(item));
                bottom = Math.Max(bottom, GetTop(item) + GetHeight(item));
                sumHeight += GetHeight(item);
            }

            var distance = Math.Max(0, (bottom - top - sumHeight) / (selectedItems.Count() - 1));
            var offset = GetTop(selectedItems.First());

            foreach (var item in selectedItems)
            {
                var delta = offset - GetTop(item);
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
        return item is DesignerItemViewModelBase
            ? (item as DesignerItemViewModelBase).Width.Value
            : item is ConnectorBaseViewModel
                ? Math.Max((item as ConnectorBaseViewModel).Points[0].X - (item as ConnectorBaseViewModel).Points[1].X,
                    (item as ConnectorBaseViewModel).Points[1].X - (item as ConnectorBaseViewModel).Points[0].X)
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
            : item is ConnectorBaseViewModel ? Math.Min((item as ConnectorBaseViewModel).Points[0].X,
                (item as ConnectorBaseViewModel).Points[1].X)
            : Layers.SelectMany(x => x.Children)
                .Where(x => (x as LayerItem).Item.Value.ParentID == (item as GroupItemViewModel).ID)
                .Min(x => GetLeft((x as LayerItem).Item.Value));
    }

    private double GetHeight(SelectableDesignerItemViewModelBase item)
    {
        return item is DesignerItemViewModelBase
            ? (item as DesignerItemViewModelBase).Height.Value
            : item is ConnectorBaseViewModel
                ? Math.Max((item as ConnectorBaseViewModel).Points[0].Y - (item as ConnectorBaseViewModel).Points[1].Y,
                    (item as ConnectorBaseViewModel).Points[1].Y - (item as ConnectorBaseViewModel).Points[0].Y)
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
            : item is ConnectorBaseViewModel ? Math.Min((item as ConnectorBaseViewModel).Points[0].Y,
                (item as ConnectorBaseViewModel).Points[1].Y)
            : Layers.SelectMany(x => x.Children)
                .Where(x => (x as LayerItem).Item.Value.ParentID == (item as GroupItemViewModel).ID)
                .Min(x => GetTop((x as LayerItem).Item.Value));
    }

    #endregion //Alignment

    #region Uniform

    /// <summary>
    ///     幅を合わせる
    /// </summary>
    private void ExecuteUniformWidthCommand()
    {
        MainWindowVM.Recorder.BeginRecode();

        var selectedItems = SelectedItems.Value.OfType<DesignerItemViewModelBase>();
        if (selectedItems.Count() > 1)
        {
            var first = selectedItems.First();
            var width = first.Width.Value;

            foreach (var item in selectedItems)
            {
                var delta = width - item.Width.Value;
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
    ///     高さを合わせる
    /// </summary>
    private void ExecuteUniformHeightCommand()
    {
        MainWindowVM.Recorder.BeginRecode();

        var selectedItems = SelectedItems.Value.OfType<DesignerItemViewModelBase>();
        if (selectedItems.Count() > 1)
        {
            var first = selectedItems.First();
            var height = first.Height.Value;

            foreach (var item in selectedItems)
            {
                var delta = height - item.Height.Value;
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
        Sort(Layers);
    }

    private void DuplicateObjects(IEnumerable<SelectableDesignerItemViewModelBase> items)
    {
        var selectedItems = from item in items.OfType<DesignerItemViewModelBase>()
            orderby item.ZIndex.Value
            select item;

        var oldNewList = new List<Tuple<SelectableDesignerItemViewModelBase, SelectableDesignerItemViewModelBase>>();

        foreach (var item in selectedItems) DuplicateDesignerItem(selectedItems, oldNewList, item);

        var selectedConnectors = (from item in items.OfType<SnapPointViewModel>().Select(x => x.Parent.Value)
                .OfType<ConnectorBaseViewModel>()
            orderby item.ZIndex.Value
            select item).Distinct();

        foreach (var connector in selectedConnectors) DuplicateConnector(oldNewList, connector);

        EssentialCodeForBugAvoidance();
    }

    private void EssentialCodeForBugAvoidance()
    {
        var list = new List<LayerTreeViewItemBase>();
        foreach (var layer in Layers) list.Add(layer);
        Layers.Clear();
        foreach (var item in list) Layers.Add(item);
    }

    private void DuplicateDesignerItem(IOrderedEnumerable<DesignerItemViewModelBase> selectedItems,
        List<Tuple<SelectableDesignerItemViewModelBase, SelectableDesignerItemViewModelBase>> oldNewList,
        SelectableDesignerItemViewModelBase item, GroupItemViewModel parent = null, string layerItemName = null,
        LayerItem parentLayerItem = null)
    {
        if (item is GroupItemViewModel groupItem)
        {
            var cloneGroup = groupItem.Clone() as GroupItemViewModel;
            cloneGroup.IsHitTestVisible.Value = true;
            cloneGroup.CanDrag.Value = true;
            if (parent != null)
            {
                cloneGroup.ParentID = parent.ID;
                cloneGroup.EnableForSelection.Value = false;
                parent.AddGroup(MainWindowVM.Recorder, cloneGroup);
            }

            var items = Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children);
            var children = (from it in items.OfType<LayerItem>().Where(x => x.Item.Value is DesignerItemViewModelBase)
                where it.Item.Value.ParentID.Equals(groupItem.ID)
                orderby it.Item.Value.ZIndex.Value
                select new { DesignerItem = it.Item.Value, LayerItemName = it.Name.Value }).ToList();

            var childrenConnectors =
                (from it in items.OfType<LayerItem>().Where(x => x.Item.Value is ConnectorBaseViewModel)
                    where it.Item.Value.ParentID.Equals(groupItem.ID)
                    orderby it.Item.Value.ZIndex.Value
                    select new { DesignerItem = it.Item.Value, LayerItemName = it.Name.Value }).ToList();
            var unions = children.Union(childrenConnectors)
                .OrderBy(x => x.DesignerItem.ZIndex.Value);

            oldNewList.Add(
                new Tuple<SelectableDesignerItemViewModelBase, SelectableDesignerItemViewModelBase>(groupItem,
                    cloneGroup));
            var groupItemName = Name.GetNewLayerItemName(this);
            var _parentLayerItem = new LayerItem(cloneGroup, SelectedLayers.Value.First(), groupItemName);

            foreach (var u in unions)
                if (u.DesignerItem is DesignerItemViewModelBase)
                    DuplicateDesignerItem(selectedItems, oldNewList, u.DesignerItem, cloneGroup, u.LayerItemName,
                        _parentLayerItem);
                else if (u.DesignerItem is ConnectorBaseViewModel)
                    DuplicateConnector(oldNewList, u.DesignerItem, cloneGroup, u.LayerItemName, _parentLayerItem);

            Add(_parentLayerItem);
            cloneGroup.ZIndex.Value =
                Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
                    .OfType<LayerItem>().Where(x => x.Item.Value.ParentID == cloneGroup.ID)
                    .Max(x => x.Item.Value.ZIndex.Value) + 1;
        }
        else
        {
            var clone = item.Clone() as DesignerItemViewModelBase;
            var items = Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children);
            if (parentLayerItem != null)
                items = items.Union(
                    parentLayerItem.Children.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x =>
                        x.Children));
            clone.ZIndex.Value = items.OfType<LayerItem>().Max(x => x.Item.Value.ZIndex.Value) + 1;
            clone.EdgeThickness.Value = item.EdgeThickness.Value;
            clone.IsHitTestVisible.Value = true;
            clone.EnableForSelection.Value = true;
            clone.IsVisible.Value = true;
            clone.CanDrag.Value = true;
            if (parent != null)
            {
                clone.ParentID = parent.ID;
                clone.EnableForSelection.Value = false;
                parent.AddGroup(MainWindowVM.Recorder, clone);
                var newLayerItem = new LayerItem(clone, parentLayerItem, layerItemName);
                newLayerItem.Color.Value = Layers
                    .SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children).OfType<LayerItem>()
                    .First(x => x.Item.Value.ID == item.ID).Color.Value;
                parentLayerItem.Children.Add(newLayerItem);
            }
            else
            {
                Add(clone);
            }

            oldNewList.Add(
                new Tuple<SelectableDesignerItemViewModelBase, SelectableDesignerItemViewModelBase>(item, clone));
        }
    }

    private void DuplicateConnector(
        List<Tuple<SelectableDesignerItemViewModelBase, SelectableDesignerItemViewModelBase>> oldNewList,
        SelectableDesignerItemViewModelBase connector, GroupItemViewModel groupItem = null, string layerItemName = null,
        LayerItem parentLayerItem = null)
    {
        var clone = connector.Clone() as ConnectorBaseViewModel;
        var items = Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children);
        if (parentLayerItem != null)
            items = items.Union(
                parentLayerItem.Children
                    .SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children));
        clone.ZIndex.Value = items.OfType<LayerItem>().Max(x => x.Item.Value.ZIndex.Value) + 1;
        clone.IsHitTestVisible.Value = true;
        clone.SnapPoint0VM.Value.IsHitTestVisible.Value = true;
        clone.SnapPoint1VM.Value.IsHitTestVisible.Value = true;
        if (groupItem != null)
        {
            clone.ParentID = groupItem.ID;
            clone.EnableForSelection.Value = false;
            groupItem.AddGroup(MainWindowVM.Recorder, clone);
            var newLayerItem = new LayerItem(clone, parentLayerItem, layerItemName);
            newLayerItem.Color.Value = Layers
                .SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children).OfType<LayerItem>()
                .First(x => x.Item.Value.ID == connector.ID).Color.Value;
            parentLayerItem.Children.Add(newLayerItem);
        }
        else
        {
            Add(clone);
        }

        oldNewList.Add(
            new Tuple<SelectableDesignerItemViewModelBase, SelectableDesignerItemViewModelBase>(connector, clone));
    }

    [Obsolete]
    private void DuplicateConnector(IEnumerable<DesignerItemViewModelBase> connectedItems,
        List<Tuple<SelectableDesignerItemViewModelBase, SelectableDesignerItemViewModelBase>> oldNewList,
        ConnectorBaseViewModel connector, GroupItemViewModel groupItem = null)
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
}