using boilersGraphics.Controls;
using boilersGraphics.Dao;
using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.Messenger;
using boilersGraphics.Models;
using boilersGraphics.Properties;
using boilersGraphics.UserControls;
using boilersGraphics.Views;
using boilersGraphics.Views.Behaviors;
using Microsoft.Win32;
using NLog;
using ObservableCollections;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using R3;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;
using TsOperationHistory;
using TsOperationHistory.Extensions;
using ZLinq;
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
    private boilersGraphics.Helpers.Anchors.NodeHighlightController _nodeHighlightController;
    private int _Width;
    private DesignerCanvas designerCanvas;
    private bool disposedValue;
    private readonly IDialogService dlgService;

    public DiagramViewModel(MainWindowViewModel mainWindowViewModel, bool isPreview = false)
    {
        MainWindowVM = mainWindowViewModel;

        PartDefinitions.CollectionChanged += OnPartDefinitionsCollectionChanged;

        // Phase 4-c: 組込テーマ 4 種 (Bladerunner / Matrix / MedicalBlueWhite / AmberCrt) をロード。
        // ユーザー追加テーマは Phase 4-f のシリアライズ復元時に追加する。
        foreach (var t in boilersGraphics.Models.Themes.ThemeRepository.CreateBuiltIn())
        {
            AvailableThemes.Add(t);
        }

        if (!App.IsTest)
        {
            RenderWidth = Observable.Return(Application.Current.MainWindow.GetChildOfType<DiagramControl>())
                .Where(x => x != null)
                .Select(x => x.ActualWidth)
                .ToReadOnlyBindableReactiveProperty(1000);
            RenderHeight = Observable.Return(Application.Current.MainWindow.GetChildOfType<DiagramControl>())
                .Where(x => x != null)
                .Select(x => x.ActualHeight)
                .ToReadOnlyBindableReactiveProperty(1000);
        }

        if (!isPreview)
        {
            AddItemCommand = new DelegateCommand<object>(p => ExecuteAddItemCommand(p));
            RemoveItemCommand = new DelegateCommand<object>(p => ExecuteRemoveItemCommand(p));
            ClearSelectedItemsCommand = new DelegateCommand<object>(p => ExecuteClearSelectedItemsCommand(p));
            CreateNewDiagramCommand = new DelegateCommand<object>(p => ExecuteCreateNewDiagramCommand(p));
            LoadCommand = new DelegateCommand(() => ExecuteLoadCommand());
            LoadFileCommand = new DelegateCommand<string>(file => ExecuteLoadCommand(file));
            SaveCommand = new DelegateCommand(() => ExecuteSaveAsCommand());
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
            // Phase 4-c: テーマ選択ダイアログ起動。組込テーマは worktree 初期化時に AvailableThemes へロード。
            OpenThemeManagerCommand = new DelegateCommand(() => ExecuteOpenThemeManagerCommand());
            // Phase 5-f-2: PNG 連番書出ダイアログ起動。
            OpenPngSequenceExportDialogCommand = new DelegateCommand(() => ExecuteOpenPngSequenceExportDialogCommand());
            // Phase 5.5-c: WPF Storyboard XAML 書出ダイアログ起動。
            OpenWpfXamlExportDialogCommand = new DelegateCommand(() => ExecuteOpenWpfXamlExportDialogCommand());
            UniformWidthCommand = new DelegateCommand(() => ExecuteUniformWidthCommand(), () => CanExecuteUniform());
            UniformHeightCommand = new DelegateCommand(() => ExecuteUniformHeightCommand(), () => CanExecuteUniform());
            DuplicateCommand = new DelegateCommand(() => ExecuteDuplicateCommand(), () => CanExecuteDuplicate());
            PromoteToPartCommand = new DelegateCommand(() => ExecutePromoteToPartCommand(), () => CanExecutePromoteToPart());
            ToggleNodeCommand = new DelegateCommand(() => ExecuteToggleNodeCommand(), () => CanExecuteToggleNode());
            DetachPartCommand = new DelegateCommand(() => ExecuteDetachPartCommand(), () => CanExecuteDetachPart());
            ClonePartDefinitionCommand = new DelegateCommand(() => ExecuteClonePartDefinitionCommand(), () => CanExecuteClonePartDefinition());
            EditPartDefinitionCommand = new DelegateCommand(() => ExecuteEditPartDefinitionCommand(), () => CanExecuteEditPartDefinition());
            ExportPartCommand = new DelegateCommand(() => ExecuteExportPartCommand(), () => CanExecuteExportPart());
            ImportPartCommand = new DelegateCommand(() => ExecuteImportPartCommand());
            RemoveUnusedPartDefinitionsCommand = new DelegateCommand(() => ExecuteRemoveUnusedPartDefinitionsCommand());
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
            MouseDoubleClickCommand = new DelegateCommand<MouseEventArgs>(args =>
            {
                LogManager.GetCurrentClassLogger().Trace("MouseDoubleClickCommand");
                var first = SelectedItems.Value.AsValueEnumerable().FirstOrDefault();

                //色調補正ツールのトーンカーブの白スポイトおよび黒スポイト機能使用中は、ダブルクリックイベントを抑制する
                if (first is ColorCorrectViewModel cc && cc.CCType.Value == ColorCorrectType.ToneCurve)
                {
                    if (MainWindowViewModel.Instance.ToolBarViewModel.Behaviors.Contains(MainWindowViewModel.Instance.ToolBarViewModel.BlackDropperBehavior)
                     || MainWindowViewModel.Instance.ToolBarViewModel.Behaviors.Contains(MainWindowViewModel.Instance.ToolBarViewModel.WhiteDropperBehavior))
                    {
                        return;
                    }
                }

                if (first is not null && first.IsOpenedInstructionDialog.Value)
                {
                    return;
                }

                first?.OpenInstructionDialog();
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
                    var first = SelectedItems.Value.AsValueEnumerable().First();
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
            LoadedCommand = new DelegateCommand(() =>
            {
                LogManager.GetCurrentClassLogger().Trace("LoadedCommand");

                while (LoadedEventActions.Count > 0)
                {
                    var action = LoadedEventActions.Dequeue();
                    action();
                }

                App.Current.Dispatcher.Invoke(() =>
                {
                    RootLayer.Value.UpdateAppearanceBothParentAndChildBatched();
                }, DispatcherPriority.Render);
            });
            FitCanvasCommand = new DelegateCommand(() =>
                {
                    LogManager.GetCurrentClassLogger().Trace("FitCanvasCommand");
                    var horizontalGap = AllItems.Value.AsValueEnumerable().OfType<DesignerItemViewModelBase>()
                        .Except(new DesignerItemViewModelBase[] { BackgroundItem.Value }).Count() > 0
                        ? AllItems.Value.AsValueEnumerable().OfType<DesignerItemViewModelBase>()
                            .Except(new DesignerItemViewModelBase[] { BackgroundItem.Value }).Min(x => x.Left.Value)
                        : 0;
                    var verticalGap = AllItems.Value.AsValueEnumerable().OfType<DesignerItemViewModelBase>()
                        .Except(new DesignerItemViewModelBase[] { BackgroundItem.Value }).Count() > 0
                        ? AllItems.Value.AsValueEnumerable().OfType<DesignerItemViewModelBase>()
                            .Except(new DesignerItemViewModelBase[] { BackgroundItem.Value }).Min(x => x.Top.Value)
                        : 0;
                    foreach (var item in AllItems.Value.AsValueEnumerable().OfType<ConnectorBaseViewModel>())
                    foreach (var p in item.Points)
                    {
                        horizontalGap = Math.Min(p.X, horizontalGap);
                        verticalGap = Math.Min(p.Y, verticalGap);
                    }

                    foreach (var item in AllItems.Value.AsValueEnumerable().OfType<DesignerItemViewModelBase>()
                                 .Except(new DesignerItemViewModelBase[] { BackgroundItem.Value }))
                    {
                        item.Left.Value += -horizontalGap;
                        item.Top.Value += -verticalGap;
                    }

                    foreach (var item in AllItems.Value.AsValueEnumerable().OfType<ConnectorBaseViewModel>())
                        for (var i = 0; i < item.Points.Count; i++)
                        {
                            var p = item.Points[i];
                            var newP = new Point(p.X - horizontalGap, p.Y - verticalGap);
                            item.Points[i] = newP;
                        }

                    var horizontalMax = AllItems.Value.AsValueEnumerable().OfType<DesignerItemViewModelBase>()
                        .Except(new DesignerItemViewModelBase[] { BackgroundItem.Value }).Count() > 0
                        ? AllItems.Value.AsValueEnumerable().OfType<DesignerItemViewModelBase>()
                            .Except(new DesignerItemViewModelBase[] { BackgroundItem.Value }).Max(x => x.Right.Value)
                        : 0;
                    var verticalMax = AllItems.Value.AsValueEnumerable().OfType<DesignerItemViewModelBase>()
                        .Except(new DesignerItemViewModelBase[] { BackgroundItem.Value }).Count() > 0
                        ? AllItems.Value.AsValueEnumerable().OfType<DesignerItemViewModelBase>()
                            .Except(new DesignerItemViewModelBase[] { BackgroundItem.Value }).Max(x => x.Bottom.Value)
                        : 0;
                    foreach (var item in AllItems.Value.AsValueEnumerable().OfType<ConnectorBaseViewModel>())
                    foreach (var p in item.Points)
                    {
                        horizontalMax = Math.Max(p.X, horizontalMax);
                        verticalMax = Math.Max(p.Y, verticalMax);
                    }

                    var horizontalMin = AllItems.Value.AsValueEnumerable().OfType<DesignerItemViewModelBase>()
                        .Except(new DesignerItemViewModelBase[] { BackgroundItem.Value }).Count() > 0
                        ? AllItems.Value.AsValueEnumerable().OfType<DesignerItemViewModelBase>()
                            .Except(new DesignerItemViewModelBase[] { BackgroundItem.Value }).Min(x => x.Left.Value)
                        : 0;
                    var verticalMin = AllItems.Value.AsValueEnumerable().OfType<DesignerItemViewModelBase>()
                        .Except(new DesignerItemViewModelBase[] { BackgroundItem.Value }).Count() > 0
                        ? AllItems.Value.AsValueEnumerable().OfType<DesignerItemViewModelBase>()
                            .Except(new DesignerItemViewModelBase[] { BackgroundItem.Value }).Min(x => x.Top.Value)
                        : 0;

                    BackgroundItem.Value.Left.Value = Math.Round(horizontalMin);
                    BackgroundItem.Value.Top.Value = Math.Round(verticalMin);
                    BackgroundItem.Value.Width.Value = Math.Round(horizontalMax);
                    BackgroundItem.Value.Height.Value = Math.Round(verticalMax);
                },
                () => AllItems.Value.AsValueEnumerable().OfType<DesignerItemViewModelBase>()
                        .Except(new DesignerItemViewModelBase[] { BackgroundItem.Value }).Count() +
                    AllItems.Value.AsValueEnumerable().OfType<ConnectorBaseViewModel>().Count() > 0);
            ClearCanvasCommand = new DelegateCommand(() => { InitialSetting(mainWindowViewModel, true); });
            OnLoaded = new ReactiveCommand().WithSubscribe(() =>
            {
                DesignerCanvas = Application.Current.MainWindow.GetChildOfType<DesignerCanvas>();
                Layers.AsValueEnumerable().ToList().ForEach(x => x.UpdateAppearanceBothParentAndChildBatched());
            }).AddTo(_CompositeDisposable);
            Image2TextEngCommand = new ReactiveCommand().WithSubscribe(() =>
            {
                MainWindowViewModel.Instance.ToolBarViewModel.SelectOneToolItem(string.Empty);
                MainWindowViewModel.Instance.ToolBarViewModel.Behaviors.Add(new Image2TextBehavior("eng"));
                MainWindowViewModel.Instance.ToolBarViewModel.ChangeHitTestToDisable();
            }).AddTo(_CompositeDisposable);
            Image2TextJpnCommand = new ReactiveCommand().WithSubscribe(() =>
            {
                MainWindowViewModel.Instance.ToolBarViewModel.SelectOneToolItem(string.Empty);
                MainWindowViewModel.Instance.ToolBarViewModel.Behaviors.Add(new Image2TextBehavior("jpn"));
                MainWindowViewModel.Instance.ToolBarViewModel.ChangeHitTestToDisable();
            }).AddTo(_CompositeDisposable);
        }

        Layers = Observable.Merge(RootLayer.Value.Children.CollectionChangedAsObservable().ToUnit(),
                RootLayer.Value.LayerChangedAsObservable())
            .SelectMany(_ => RootLayer.Value.Children.ToObservable())
            .ToObservableList()
            .ToWritableNotifyCollectionChanged();

        //AllItems = Layers.CollectionChangedAsObservable()
        //    .Select(_ => Layers.Select(x => x.LayerItemsChangedAsObservable()).Merge()
        //        .Merge(this.ObservePropertyChanged(y => y.BackgroundItem).ToUnit()))
        //    .Switch()
        //    .Select(_ => Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
        //        .AsValueEnumerable()
        //        .Where(x => x.GetType() == typeof(LayerItem))
        //        .Select(y => (y as LayerItem).Item.Value)
        //        .Union(new SelectableDesignerItemViewModelBase[] { BackgroundItem.Value })
        //        .Where(x => x is not null)
        //        .OrderBy(x => x.ZIndex.Value)
        //        .ToArray())
        //    .ToReadOnlyReactiveProperty(Array.Empty<SelectableDesignerItemViewModelBase>());

        //AllItems = Observable.CombineLatest(
        //        Layers.CollectionChangedAsObservable()
        //            .Select(_ => Layers.Select(x => x.LayerItemsChangedAsObservable()).Merge())
        //            .Switch()
        //            .Prepend(Unit.Default),
        //        this.ObservePropertyChanged(y => y.BackgroundItem.Value).ToUnit().Prepend(Unit.Default)
        //    )
        //    .Select(_ => Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
        //        .AsValueEnumerable()
        //        .Where(x => x.GetType() == typeof(LayerItem))
        //        .Select(y => (y as LayerItem).Item.Value)
        //        .Union(new SelectableDesignerItemViewModelBase[] { BackgroundItem.Value })
        //        .Where(x => x is not null)
        //        .OrderBy(x => x.ZIndex.Value)
        //        .ToArray())
        //    .ToReadOnlyReactiveProperty(Array.Empty<SelectableDesignerItemViewModelBase>());

        //AllItems = Observable.Merge(
        //        Layers.CollectionChangedAsObservable()
        //            .Select(_ => Layers.Select(x => x.LayerItemsChangedAsObservable()).Merge())
        //            .Switch(),
        //        this.ObservePropertyChanged(y => y.BackgroundItem.Value).ToUnit(),
        //        Observable.Return(Unit.Default)  // 初期値として
        //    )
        //    .Select(_ => Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
        //        .AsValueEnumerable()
        //        .Where(x => x.GetType() == typeof(LayerItem))
        //        .Select(y => (y as LayerItem).Item.Value)
        //        .Union(new SelectableDesignerItemViewModelBase[] { BackgroundItem.Value })
        //        .Where(x => x is not null)
        //        .OrderBy(x => x.ZIndex.Value)
        //        .ToArray())
        //    .ToReadOnlyReactiveProperty(Array.Empty<SelectableDesignerItemViewModelBase>());

        AllItems = Observable.Create<SelectableDesignerItemViewModelBase[]>(observer =>
        {
            var disposable = new CompositeDisposable();
            var layerSubscriptions = new CompositeDisposable();

            void UpdateAllItems()
            {
                Debug.WriteLine("UpdateAllItems called");
                var items = new List<SelectableDesignerItemViewModelBase>();

                foreach (var layer in Layers)
                {
                    foreach (var child in layer.Children)
                    {
                        if (child is LayerItem layerItem && layerItem.Item.Value != null)
                        {
                            items.Add(layerItem.Item.Value);
                            Debug.WriteLine($"Added from layer: {layerItem.Item.Value.GetType().Name}");
                        }
                    }
                }

                if (BackgroundItem.Value != null)
                {
                    items.Add(BackgroundItem.Value);
                    Debug.WriteLine($"Added background: {BackgroundItem.Value.GetType().Name}");
                }

                var result = items.OrderBy(x => x.ZIndex.Value).ToArray();
                Debug.WriteLine($"AllItems updated: {result.Length} items total");
                observer.OnNext(result);
            }

            void SetupLayerSubscriptions()
            {
                Debug.WriteLine($"SetupLayerSubscriptions called for {Layers.Count} layers");
                layerSubscriptions.Clear();

                foreach (var layer in Layers)
                {
                    Debug.WriteLine($"Setting up subscription for layer: {layer.Name.Value}");
                    layer.Children.CollectionChangedAsObservable()
                        .Subscribe(_ => {
                            Debug.WriteLine($"Layer '{layer.Name.Value}' children changed - triggering UpdateAllItems");
                            UpdateAllItems();
                        })
                        .AddTo(layerSubscriptions);
                }
            }

            // 初期更新
            UpdateAllItems();

            // レイヤーコレクションの変更を監視（ADD/REMOVEの両方）
            Layers.CollectionChangedAsObservable()
                .Subscribe(e => {
                    Debug.WriteLine($"Layers collection changed: {e.Action} - setting up layer subscriptions");

                    // レイヤーの監視を再設定
                    SetupLayerSubscriptions();

                    // アイテムリストを更新
                    UpdateAllItems();
                })
                .AddTo(disposable);

            // バックグラウンドアイテムの変更を監視
            BackgroundItem
                .Subscribe(_ => {
                    Debug.WriteLine("BackgroundItem changed");
                    UpdateAllItems();
                })
                .AddTo(disposable);

            // 初期レイヤー監視設定（この時点では0個でも後でLayersが追加されたときに再設定される）
            SetupLayerSubscriptions();

            layerSubscriptions.AddTo(disposable);

            return disposable;
        })
        .ToReadOnlyBindableReactiveProperty(Array.Empty<SelectableDesignerItemViewModelBase>());

        AllItems.AsObservable().Subscribe(x =>
            {
                Debug.WriteLine($"=== AllItems UPDATED ===");
                Debug.WriteLine($"Count: {x.Length}");
                foreach (var item in x)
                {
                    Debug.WriteLine($"  - {item?.GetType().Name}: {item}");
                }
                Debug.WriteLine("=== AllItems END ===");

                FitCanvasCommand.RaiseCanExecuteChanged();
                LogManager.GetCurrentClassLogger().Trace($"{x.Length} items in AllItems.");
                LogManager.GetCurrentClassLogger().Trace(string.Join(", ", x.AsValueEnumerable().Select(y => y?.ToString() ?? "null").ToArray()));
            })
            .AddTo(_CompositeDisposable);

        if (!isPreview)
        {
            AllItems.AsObservable().Subscribe(x =>
                {
                    FitCanvasCommand.RaiseCanExecuteChanged();
                    LogManager.GetCurrentClassLogger().Trace($"{x.Length} items in AllItems.");
                    LogManager.GetCurrentClassLogger().Trace(string.Join(", ", x.AsValueEnumerable().Select(y => y?.ToString() ?? "null").ToArray()));
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
                .Select(_ => GetSelectedItemsCore())
                .ToReadOnlyBindableReactiveProperty(Array.Empty<SelectableDesignerItemViewModelBase>());

            SelectedItems.AsObservable().Subscribe(selectedItems =>
                {
                    LogManager.GetCurrentClassLogger()
                        .Debug(
                            $"SelectedItems changed {string.Join(", ", selectedItems.AsValueEnumerable().Select(x => x?.ToString() ?? "null").ToArray())}");

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

                    PromoteToPartCommand.RaiseCanExecuteChanged();
                    ToggleNodeCommand.RaiseCanExecuteChanged();
                    EditPartDefinitionCommand.RaiseCanExecuteChanged();
                    ClonePartDefinitionCommand.RaiseCanExecuteChanged();
                    DetachPartCommand.RaiseCanExecuteChanged();
                    ExportPartCommand.RaiseCanExecuteChanged();
                })
                .AddTo(_CompositeDisposable);

            // Phase 3-g: IsNode=true な DesignerItem が選択されたとき、関連コネクタを強調表示する。
            // Dispose は _CompositeDisposable に乗せて DiagramViewModel と寿命を合わせる。
            _nodeHighlightController = new boilersGraphics.Helpers.Anchors.NodeHighlightController(this);
            Disposable.Create(() => _nodeHighlightController.Dispose()).AddTo(_CompositeDisposable);

            SelectedLayers = Layers.ObserveElementObservableProperty(x => x.IsSelected)
                .Select(_ => Layers.AsValueEnumerable().Where(x => x.IsSelected.Value).ToArray())
                .ToReadOnlyBindableReactiveProperty([]);

            SelectedLayers.AsObservable().Subscribe(x =>
                {
                    LogManager.GetCurrentClassLogger()
                        .Trace($"SelectedLayers changed {string.Join(", ", x.AsValueEnumerable().Select(x => x.ToString()).ToArray())}");
                })
                .AddTo(_CompositeDisposable);

            Layers.CollectionChangedAsObservable()
                .Subscribe(x =>
                {
                    RootLayer.Value.Children = new ObservableList<LayerTreeViewItemBase>(Layers.AsValueEnumerable().ToList()).ToWritableNotifyCollectionChanged();
                    x.NewItems?.OfType<LayerTreeViewItemBase>().ToList().ForEach(x => x.SetParentToChildren(RootLayer.Value));
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
            if (logSettings.AsValueEnumerable().Count() == 0)
            {
                var newLogSetting = new LogSetting();
                newLogSetting.ID = id;
                newLogSetting.LogLevel = LogLevel.Info.ToString();
                dao.Insert(newLogSetting);
            }

            logSettings = dao.FindBy(new Dictionary<string, object> { { "ID", id } });
            var logSetting = logSettings.AsValueEnumerable().First();
            MainWindowVM.LogLevel.Value = LogLevel.FromString(logSetting.LogLevel);
            PackAutoSaveFiles();
        }

        AngleType.Value = Helpers.AngleType.Minus180To180;
        EnableImageEmbedding.Value = true;
        ColorSpots.Value = new ColorSpots();
        EnableCombine.Value = true;
        EnableLayers.Value = true;
        EnableWorkHistory.Value = true;

        // Phase 5-e-2: Timeline 再生エンジンに DesignerItem 解決関数を差し込む。
        // ItemId -> 該当 VM の lookup は AllItems を線形検索する単純実装 (Tracks の総数 << AllItems の総数想定)。
        Timeline.ItemResolver = guid =>
        {
            var arr = AllItems?.Value;
            if (arr is null) return null;
            foreach (var it in arr)
            {
                if (it is not null && it.ID == guid) return it;
            }
            return null;
        };

        SettingIfDebug();
    }

    private SelectableDesignerItemViewModelBase[] GetSelectedItemsCore()
    {
        var allLayerItems = Layers
            .SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
            .AsValueEnumerable()
            .OfType<LayerItem>()
            .ToArray();

        var selectedItems = new List<SelectableDesignerItemViewModelBase>();

        foreach (var layerItem in allLayerItems)
        {
            var item = layerItem.Item.Value;
            if (item == null) continue;

            // 通常のアイテムが選択されている場合
            if (item.IsSelected.Value && !(item is ConnectorBaseViewModel))
            {
                selectedItems.Add(item);
            }
            // ConnectorBaseViewModelの場合、選択されたSnapPointを追加
            else if (item is ConnectorBaseViewModel connector)
            {
                if (connector.SnapPoint0VM.Value?.IsSelected.Value == true)
                {
                    selectedItems.Add(connector.SnapPoint0VM.Value);
                }
                if (connector.SnapPoint1VM.Value?.IsSelected.Value == true)
                {
                    selectedItems.Add(connector.SnapPoint1VM.Value);
                }
            }
        }

        return selectedItems
            .OrderBy(x => x.SelectedOrder.Value)
            .ToArray();
    }

    public DiagramViewModel(MainWindowViewModel MainWindowVM, IDialogService dlgService)
        : this(MainWindowVM)
    {
        this.dlgService = dlgService;

        Mediator.Instance.Register(this);
    }

    public Renderer Renderer { get; } = new(new WpfVisualTreeHelper());
    public DelegateCommand<object> CreateNewDiagramCommand { get; }
    public DelegateCommand LoadCommand { get; }
    public DelegateCommand<string> LoadFileCommand { get; }
    public DelegateCommand SaveCommand { get; }
    public DelegateCommand OverwriteCommand { get; }
    public DelegateCommand ExportCommand { get; }
    public DelegateCommand GroupCommand { get; }
    public DelegateCommand UngroupCommand { get; }
    public DelegateCommand PromoteToPartCommand { get; }
    public DelegateCommand ToggleNodeCommand { get; }
    public DelegateCommand DetachPartCommand { get; }
    public DelegateCommand ClonePartDefinitionCommand { get; }
    public DelegateCommand EditPartDefinitionCommand { get; }
    public DelegateCommand ExportPartCommand { get; }
    public DelegateCommand ImportPartCommand { get; }
    public DelegateCommand RemoveUnusedPartDefinitionsCommand { get; }
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
    /// <summary>Phase 4-c: テーマ選択 / パレット適用ダイアログ起動コマンド。</summary>
    public DelegateCommand OpenThemeManagerCommand { get; }
    public DelegateCommand OpenPngSequenceExportDialogCommand { get; }
    /// <summary>Phase 5.5-c: WPF Storyboard XAML 書出ダイアログ起動コマンド。</summary>
    public DelegateCommand OpenWpfXamlExportDialogCommand { get; }
    /// <summary>Phase 4-c: 利用可能なテーマ一覧 (組込 4 種 + ユーザー追加)。</summary>
    public ObservableList<boilersGraphics.Models.Themes.Theme> AvailableThemes { get; } = new();
    /// <summary>Phase 4-c: 現在アクティブなテーマ。null 許容。</summary>
    public R3.BindableReactiveProperty<boilersGraphics.Models.Themes.Theme> ActiveTheme { get; } = new();
    /// <summary>
    /// Phase 5-d: 現在編集中のシーンのアニメーションタイムライン。
    /// CanvasPage 切替時の swap は Phase 5-d-2 で実装予定 (現状はシーン依存なく単一)。
    /// </summary>
    public boilersGraphics.ViewModels.Animation.TimelineViewModel Timeline { get; } =
        new boilersGraphics.ViewModels.Animation.TimelineViewModel();
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
    public DelegateCommand<MouseEventArgs> MouseDoubleClickCommand { get; }
    public DelegateCommand<KeyEventArgs> PreviewKeyDownCommand { get; }
    public DelegateCommand PropertyCommand { get; }
    public DelegateCommand<Line> MouseDownStraightLineCommand { get; }
    public DelegateCommand<Path> MouseDownBezierCurveCommand { get; }
    public DelegateCommand<Path> MouseDownPolyBezierCommand { get; }
    public DelegateCommand LoadedCommand { get; }
    public DelegateCommand FitCanvasCommand { get; }
    public DelegateCommand ClearCanvasCommand { get; }
    public ReactiveCommand OnLoaded { get; }
    public ReactiveCommand Image2TextEngCommand { get; }
    public ReactiveCommand Image2TextJpnCommand { get; }
    public MainWindowViewModel MainWindowVM { get; }

    public DelegateCommand<object> AddItemCommand { get; }
    public DelegateCommand<object> RemoveItemCommand { get; }
    public DelegateCommand<object> ClearSelectedItemsCommand { get; }

    public void DeselectAll()
    {
        foreach (var layerItem in Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
                     .AsValueEnumerable()
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
        var sets = resizeThumbs.AsValueEnumerable()
            .Where(x => !exceptSnapPoints.AsValueEnumerable().Contains(x))
            .Select(x => new Tuple<SnapPoint, Point>(x, GetCenter(x)))
            .Distinct()
            .ToArray();
        return sets;
    }

    public IEnumerable<Tuple<SnapPoint, Point>> GetSnapPoints(Point exceptPoint)
    {
        var resizeThumbs = DesignerCanvas.EnumerateChildOfType<SnapPoint>();
        var sets = resizeThumbs.AsValueEnumerable()
            .Where(x => x.InputHitTest(exceptPoint) == null)
            .Select(x => new Tuple<SnapPoint, Point>(x, GetCenter(x)))
            .Distinct()
            .ToArray();
        return sets;
    }

    private void ExecuteCopyCanvasToClipboardCommand()
    {
        var renderer = this.Renderer;
        var bitmap = renderer.Render(null, DesignerCanvas.GetInstance(), this, BackgroundItem.Value, BackgroundItem.Value);
        ClipboardHelper.SetImage(bitmap);
    }

    [Conditional("DEBUG")]
    private void SettingIfDebug()
    {
        EnableAutoSave.Value = false;
    }

    private void PackAutoSaveFiles()
    {
        if (App.IsTest || App.Current is null) return;
        App.Current.Dispatcher.Invoke(() =>
        {
            AutoSaveFiles?.Clear();
            try
            {
                var files = Directory.EnumerateFiles(
                    System.IO.Path.Combine(Helpers.Path.GetRoamingDirectory(), "dhq_boiler\\boilersGraphics\\AutoSave"),
                    "AutoSave-*-*-*-*-*-*.bgff");
                foreach (var file in files.AsValueEnumerable().OrderByDescending(x => new FileInfo(x).LastWriteTime))
                    AutoSaveFiles.Add(file);
            }
            catch (DirectoryNotFoundException)
            {
                //Ignore it as it only happens on Azure DevOps
            }
        });
    }

    private bool CanOpenPropertyDialog()
    {
        return (SelectedItems.Value.Length == 1 && SelectedItems.Value.AsValueEnumerable().First().SupportsPropertyDialog)
               || (SelectedItems.Value.AsValueEnumerable().OfType<SnapPointViewModel>().Count() == 2 && SelectedItems.Value
                   .AsValueEnumerable()
                   .OfType<SnapPointViewModel>().First().Parent.Value.SupportsPropertyDialog);
    }

    private void MoveSelectedItems(int horizontalDiff, int verticalDiff)
    {
        MainWindowVM.Recorder.BeginRecode();
        SelectedItems.Value.AsValueEnumerable().OfType<DesignerItemViewModelBase>().ToList().ForEach(x =>
        {
            MainWindowVM.Recorder.Current.ExecuteSetProperty(x, "Left.Value", x.Left.Value + horizontalDiff);
            MainWindowVM.Recorder.Current.ExecuteSetProperty(x, "Top.Value", x.Top.Value + verticalDiff);
        });
        SelectedItems.Value.AsValueEnumerable().OfType<SnapPointViewModel>().ToList().ForEach(x =>
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
            $"dhq_boiler\\boilersGraphics\\AutoSave\\AutoSave-{AutoSavedDateTime.Value.Year}-{AutoSavedDateTime.Value.Month}-{AutoSavedDateTime.Value.Day}-{AutoSavedDateTime.Value.Hour}-{AutoSavedDateTime.Value.Minute}-{AutoSavedDateTime.Value.Second}.bgff");
        var autoSaveDir = System.IO.Path.GetDirectoryName(path);
        if (!Directory.Exists(autoSaveDir)) Directory.CreateDirectory(autoSaveDir);

        App.GetCurrentApp().Dispatcher.Invoke(() =>
        {
            var versionXML = new XElement("Version", BGSXFileVersion.ToString());
            var layersXML = new XElement("Layers", ObjectSerializer.SerializeLayers(Layers));
            var configurationXML = new XElement("Configuration", ObjectSerializer.SerializeConfiguration(this));
            var attachmentsXML = new XElement("Attachments", ObjectSerializer.SerializeAttachments(this));
            // Phase 4-f-2: ActiveThemeId を保存。
            var themesXML = ObjectSerializer.SerializeThemes(this);

            var root = new XElement("boilersGraphics");
            root.Add(versionXML);
            root.Add(layersXML);
            root.Add(configurationXML);
            root.Add(attachmentsXML);
            root.Add(themesXML);

            // Phase 5-d-1: 空でなければ <Timeline> セクションを追加。
            if (!Timeline.IsEmpty)
            {
                root.Add(boilersGraphics.Helpers.Animation.TimelineSerializer.SerializeTimeline(Timeline));
            }

            //自動保存なので、FileNameは更新しないでセーブだけする
            SaveFileAndNoFileNameUpdatingWithoutSaveFileDialog(root, path);
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
            .AsValueEnumerable()
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
        if (this.BackgroundItem.Value is null)
        {
            mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "BackgroundItem.Value",
                new BackgroundViewModel(this));
        }
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
        RootLayer = new BindableReactiveProperty<LayerTreeViewItemBase>(new RootLayer());
        Layers.ToClearOperation().ExecuteTo(mainwindowViewModel.Recorder.Current);
        if (addingLayer || Layers.Count == 0)
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
        var picture = SelectedItems.Value.AsValueEnumerable().OfType<PictureDesignerItemViewModel>().First();
        var other = SelectedItems.Value.AsValueEnumerable().OfType<DesignerItemViewModelBase>().Last();
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
        newCroppedPicture.UpdatingStrategy.Value = SelectableDesignerItemViewModelBase.PathGeometryUpdatingStrategy.Fixed;
        newCroppedPicture.Left.Value = other.Left.Value;
        newCroppedPicture.Top.Value = other.Top.Value;
        newCroppedPicture.Width.Value = other.Width.Value;
        newCroppedPicture.Height.Value = other.Height.Value;
        newCroppedPicture.UpdatingStrategy.Value = SelectableDesignerItemViewModelBase.PathGeometryUpdatingStrategy.Initial;
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
        return SelectedItems.Value.AsValueEnumerable().Count() == 2 &&
               SelectedItems.Value.AsValueEnumerable().First().GetType() == typeof(PictureDesignerItemViewModel);
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
        var countIsCorrent = SelectedItems.Value.AsValueEnumerable().Count() == 2;
        if (countIsCorrent)
        {
            var firstElementTypeIsCorrect =
                SelectedItems.Value.ElementAt(0).GetType() != typeof(PictureDesignerItemViewModel);
            var secondElementTypeIsCorrect =
                SelectedItems.Value.ElementAt(1).GetType() != typeof(PictureDesignerItemViewModel);
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
        var countIsCorrent = SelectedItems.Value.AsValueEnumerable().Count() == 2;
        if (countIsCorrent)
        {
            var firstElementTypeIsCorrect =
                SelectedItems.Value.ElementAt(0).GetType() != typeof(PictureDesignerItemViewModel);
            var secondElementTypeIsCorrect =
                SelectedItems.Value.ElementAt(1).GetType() != typeof(PictureDesignerItemViewModel);
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
        var countIsCorrent = SelectedItems.Value.AsValueEnumerable().Count() == 2;
        if (countIsCorrent)
        {
            var firstElementTypeIsCorrect =
                SelectedItems.Value.ElementAt(0).GetType() != typeof(PictureDesignerItemViewModel);
            var secondElementTypeIsCorrect =
                SelectedItems.Value.ElementAt(1).GetType() != typeof(PictureDesignerItemViewModel);
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
                SelectedItems.Value.ElementAt(0).GetType() != typeof(PictureDesignerItemViewModel);
            var secondElementTypeIsCorrect =
                SelectedItems.Value.ElementAt(1).GetType() != typeof(PictureDesignerItemViewModel);
            return countIsCorrent && firstElementTypeIsCorrect && secondElementTypeIsCorrect;
        }

        var polyBezier = GetSelectedItemsForCombine().AsValueEnumerable().FirstOrDefault() as PolyBezierViewModel;
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
        if (selectedItems.AsValueEnumerable().Count() == 1 && item1 is PolyBezierViewModel pb)
        {
            Remove(pb);
            var combine = new CombineGeometryViewModel();
            MainWindowVM.Recorder.Current.ExecuteSetProperty(combine, "EdgeBrush.Value", pb.EdgeBrush.Value);
            MainWindowVM.Recorder.Current.ExecuteSetProperty(combine, "EdgeThickness.Value", pb.EdgeThickness.Value);
            MainWindowVM.Recorder.Current.ExecuteSetProperty(combine, "IsSelected.Value", true);
            MainWindowVM.Recorder.Current.ExecuteSetProperty(combine, "Owner", this);
            MainWindowVM.Recorder.Current.ExecuteSetProperty(combine, "ZIndex.Value",
                Layers.AsValueEnumerable().SelectMany(x => x.Children).Count());
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
        else if (selectedItems.AsValueEnumerable().Count() == 2 && (item1 is EffectViewModel || GetSelectedItemLast() is EffectViewModel))
        {
            var item2 = GetSelectedItemLast();

            // EffectViewModelを保持し、もう一方を削除する
            EffectViewModel effect;
            SelectableDesignerItemViewModelBase otherItem;
            if (item1 is EffectViewModel effect1)
            {
                effect = effect1;
                otherItem = item2;
            }
            else
            {
                effect = (EffectViewModel)item2;
                otherItem = item1;
            }
            Remove(otherItem);

            var designerEffect = effect as DesignerItemViewModelBase;
            var designerOther = otherItem as DesignerItemViewModelBase;

            var effectPathGeometry = effect.PathGeometryNoRotate.Value;
            var otherPathGeometry = otherItem.PathGeometryNoRotate.Value;

            if (effect.RotationAngle.Value != 0) effectPathGeometry = designerEffect.PathGeometryRotate.Value;
            if (designerEffect is not CombineGeometryViewModel)
                effectPathGeometry = GeometryCreator.Translate(effectPathGeometry, designerEffect.Left.Value,
                    designerEffect.Top.Value);

            if (otherItem.RotationAngle.Value != 0) otherPathGeometry = designerOther.PathGeometryRotate.Value;

            if (designerOther is not CombineGeometryViewModel)
                otherPathGeometry = GeometryCreator.Translate(otherPathGeometry, designerOther.Left.Value,
                    designerOther.Top.Value);

            var combinedAbsolute = Geometry.Combine(effectPathGeometry, otherPathGeometry, mode, null);
            var combinedLocal = GeometryCreator.Translate(combinedAbsolute, -designerEffect.Left.Value, -designerEffect.Top.Value);

            MainWindowVM.Recorder.Current.ExecuteSetProperty(effect, "UpdatingStrategy.Value", SelectableDesignerItemViewModelBase.PathGeometryUpdatingStrategy.Fixed);
            MainWindowVM.Recorder.Current.ExecuteSetProperty(effect, "PathGeometryNoRotate.Value", combinedLocal);
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
                Layers.AsValueEnumerable().SelectMany(x => x.Children).Count());
            MainWindowVM.Recorder.Current.ExecuteSetProperty(combine, "IsHitTestVisible.Value",
                MainWindowVM.ToolBarViewModel.CurrentHitTestVisibleState.Value);
            MainWindowVM.Recorder.Current.ExecuteSetProperty(combine, "PathGeometryNoRotate.Value",
                GeometryCreator.CreateCombineGeometry(item1, item2));
            if (combine.PathGeometryNoRotate.Value == null || combine.PathGeometryNoRotate.Value.Figures.AsValueEnumerable().Count() == 0)
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

            var bounds = combine.PathGeometryNoRotate.Value.Bounds;
            MainWindowVM.Recorder.Current.ExecuteSetProperty(combine, "Left.Value", bounds.Left);
            MainWindowVM.Recorder.Current.ExecuteSetProperty(combine, "Top.Value", bounds.Top);
            MainWindowVM.Recorder.Current.ExecuteSetProperty(combine, "PathGeometryNoRotate.Value",
                GeometryCreator.Translate(combine.PathGeometryNoRotate.Value, -bounds.Left, -bounds.Top));
            MainWindowVM.Recorder.Current.ExecuteSetProperty(combine, "Width.Value", bounds.Width);
            MainWindowVM.Recorder.Current.ExecuteSetProperty(combine, "Height.Value", bounds.Height);
            Add(combine);
        }

        MainWindowVM.Recorder.EndRecode();
    }

    private SelectableDesignerItemViewModelBase GetSelectedItemFirst()
    {
        return GetSelectedItemsForCombine().AsValueEnumerable().First();
    }

    private SelectableDesignerItemViewModelBase GetSelectedItemLast()
    {
        return GetSelectedItemsForCombine().AsValueEnumerable().Skip(1).Take(1).First();
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
        return newlist.AsValueEnumerable().Count() == 2;
    }

    private List<SelectableDesignerItemViewModelBase> GetSelectedItemsForCombine()
    {
        var list = SelectedItems.Value.AsValueEnumerable().ToList();
        var newlist = new List<SelectableDesignerItemViewModelBase>();
        foreach (var item in list)
        {
            if (item is DesignerItemViewModelBase)
                newlist.Add(item);
            if (item is SnapPointViewModel snapPoint)
                newlist.Add(snapPoint.Parent.Value);
        }

        newlist = newlist.AsValueEnumerable().Distinct().ToList();
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
        return SelectedItems.Value.Any();
    }

    private void ExecutePasteCommand()
    {
        var obj = ClipboardHelper.GetDataObject();
        if (ClipboardHelper.GetDataPresent(obj, ClipboardDTO.ClipboardFormat))
        {
            var str = ClipboardHelper.GetData(obj, ClipboardDTO.ClipboardFormat) as string;
            var root = XElement.Parse(str);
            ObjectDeserializer.ReadCopyObjectsFromXML(this, root);
        }
        else if (ClipboardHelper.ContainsImage())
        {
            var bitmap = ClipboardHelper.GetImage();
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
        var obj = ClipboardHelper.GetDataObject();
        if (ClipboardHelper.GetDataPresent(obj, ClipboardDTO.ClipboardFormat))
        {
            var str = ClipboardHelper.GetData(obj, ClipboardDTO.ClipboardFormat) as string;
            try
            {
                var root = XElement.Parse(str);
                var rootNameIsCopyObjects = root.Name == "boilersGraphics";
                var rootHasElements = root.HasElements;
                if (rootNameIsCopyObjects && rootHasElements)
                {
                    var copyObjsEnumerable = root.Elements().AsValueEnumerable().Where(x => x.Name == "CopyObjects");
                    var copyObjs = copyObjsEnumerable.FirstOrDefault();
                    var rootHasCopyObjects = copyObjs != null;
                    if (rootHasCopyObjects)
                    {
                        var copyObjsHasLayers = copyObjs.Elements().AsValueEnumerable().Where(x => x.Name == "Layers").Count() == 1;
                        var copyObjsHasItems = copyObjs.Elements().AsValueEnumerable().Where(x => x.Name == "LayerItems").Count() == 1;
                        if (copyObjsHasLayers)
                        {
                            var layers = copyObjs.Elements().AsValueEnumerable().Where(x => x.Name == "Layers").FirstOrDefault();
                            return layers.Elements().AsValueEnumerable().Count() >= 1;
                        }

                        if (copyObjsHasItems)
                        {
                            var items = copyObjs.Elements().AsValueEnumerable().Where(x => x.Name == "LayerItems").FirstOrDefault();
                            return items.Elements().AsValueEnumerable().Count() >= 1;
                        }
                    }
                }
            }
            catch (XmlException)
            {
                return false;
            }
        }
        else if (ClipboardHelper.ContainsImage())
        {
            return true;
        }

        return false;
    }

    private void ExecuteCutCommand()
    {
        CopyToClipboard();

        if (SelectedLayers.Value.AsValueEnumerable().Count() > 0 && SelectedItems.Value.AsValueEnumerable().Count() > 0)
            SelectedLayers.Value.AsValueEnumerable().ToList().ForEach(x =>
            {
                foreach (var selectedItem in SelectedItems.Value)
                {
                    x.RemoveItem(MainWindowVM, selectedItem);
                    selectedItem.Dispose();
                }
            });
        else if (SelectedLayers.Value.AsValueEnumerable().Count() > 0)
            //Copy Layer and LayerItem
            foreach (var selectedLayer in SelectedLayers.Value)
                Layers.Remove(selectedLayer);

        ScanEffectViewModelObjects();
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
        if (SelectedLayers.Value.AsValueEnumerable().Count() > 0 && SelectedItems.Value.AsValueEnumerable().Count() > 0)
        {
            //Copy only LayerItem
            var root = new XElement("boilersGraphics");
            var copyObj = new XElement("CopyObjects");
            root.Add(copyObj);
            copyObj.Add(ObjectSerializer.ExtractItems(Layers.AsValueEnumerable().SelectMany(x => x.Children)
                .Where(x => (x as LayerItem).IsSelected.Value).Cast<LayerItem>().ToArray()));
            var dataObject = new DataObject();
            dataObject.SetData(ClipboardDTO.ClipboardFormat, root.ToString());
            ClipboardHelper.SetDataObject(dataObject, false);
        }
        else if (SelectedLayers.Value.AsValueEnumerable().Count() > 0)
        {
            //Copy Layer and LayerItem
            var root = new XElement("boilersGraphics");
            var copyObj = new XElement("CopyObjects");
            root.Add(copyObj);
            copyObj.Add(new XElement("Layers"));
            copyObj.Element("Layers")
                .Add(ObjectSerializer.SerializeLayers(new ObservableList<LayerTreeViewItemBase>(SelectedLayers.Value).ToNotifyCollectionChangedSlim()));
            var dataObject = new DataObject();
            dataObject.SetData(ClipboardDTO.ClipboardFormat, root.ToString());
            ClipboardHelper.SetDataObject(dataObject, false);
        }
    }

    public bool CanExecuteCut()
    {
        return (SelectedLayers.Value.AsValueEnumerable().Count() > 0 && SelectedItems.Value.AsValueEnumerable().Count() > 0)
               || SelectedLayers.Value.AsValueEnumerable().Count() > 0;
    }

    private void ExecuteSettingCommand()
    {
        IDialogResult result = null;
        var preferences = new Preference();
        preferences.Width.Value = (int)BackgroundItem.Value.Width.Value;
        preferences.Height.Value = (int)BackgroundItem.Value.Height.Value;
        Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
            .AsValueEnumerable()
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
        preferences.EnableAutoScrollOnDrag.Value = EnableAutoScrollOnDrag.Value;
        preferences.AutoScrollOnDragSpeed.Value = AutoScrollOnDragSpeed.Value;
        // Phase 3-i / Q-7 案 C: グローバル設定の AnchorSnapDistance を初期値として渡す。
        preferences.AnchorSnapDistance.Value = boilersGraphics.Helpers.Anchors.AnchorSnapSettings.SnapDistance.Value;
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
            EnableAutoScrollOnDrag.Value = s.EnableAutoScrollOnDrag.Value;
            AutoScrollOnDragSpeed.Value = s.AutoScrollOnDragSpeed.Value;
            // Phase 3-i / Q-7 案 C: ダイアログ確定時に AnchorSnap グローバル設定へ反映。
            // 負値を防いで 0 以上にクランプ (0 は事実上「常に最寄り」になる)。
            boilersGraphics.Helpers.Anchors.AnchorSnapSettings.SnapDistance.Value =
                System.Math.Max(0, s.AnchorSnapDistance.Value);
            SetAutoSave();
        }
    }

    /// <summary>
    /// Phase 4-c: テーマ選択 / パレット適用ダイアログを起動し、結果に応じて適用する。
    /// </summary>
    private void ExecuteOpenThemeManagerCommand()
    {
        IDialogResult result = null;
        dlgService.ShowDialog(nameof(Views.ThemeManager),
            new DialogParameters
            {
                { "Themes", (System.Collections.Generic.IReadOnlyList<boilersGraphics.Models.Themes.Theme>)AvailableThemes.ToList() },
                { "ActiveTheme", ActiveTheme.Value },
            },
            ret => result = ret);
        if (result == null || result.Result != ButtonResult.OK) return;

        var theme = result.Parameters.GetValue<boilersGraphics.Models.Themes.Theme>("Theme");
        var scope = result.Parameters.GetValue<boilersGraphics.Models.Themes.ThemeApplyScope>("Scope");
        var target = result.Parameters.GetValue<boilersGraphics.Models.Themes.ThemeApplyTarget>("Target");
        var lineStyle = result.Parameters.GetValue<boilersGraphics.Models.Themes.LineStyle>("LineStyle");
        var applyGlow = result.Parameters.GetValue<bool>("ApplyGlow");
        ApplyThemeToScope(theme, scope, target, lineStyle, applyGlow);
    }

    /// <summary>
    /// Phase 5-f-2: PNG 連番書出ダイアログを開き、OK なら <see cref="boilersGraphics.Helpers.Animation.PngSequenceExporter.Export"/>
    /// で各フレームを Renderer.Render + PngBitmapEncoder で保存する。再生中の場合は Pause させてから走らせる方が安全だが
    /// ここでは Snapshot/Restore に任せている (Export 内で Snapshot を取り、終了時に Restore する)。
    /// </summary>
    private void ExecuteOpenPngSequenceExportDialogCommand()
    {
        IDialogResult result = null;
        var end = Timeline.PlayRangeEnd.Value > 0 ? Timeline.PlayRangeEnd.Value : Timeline.Duration.Value;
        if (end <= Timeline.PlayRangeStart.Value) end = Timeline.PlayRangeStart.Value + 1.0;
        dlgService.ShowDialog(nameof(boilersGraphics.Views.Animation.PngSequenceExportDialog),
            new DialogParameters
            {
                { "Start", Timeline.PlayRangeStart.Value },
                { "End", end },
                { "Fps", Timeline.Fps.Value > 0 ? Timeline.Fps.Value : 30 },
                { "Duration", Timeline.Duration.Value },
            },
            ret => result = ret);
        if (result == null || result.Result != ButtonResult.OK) return;

        var settings = result.Parameters.GetValue<boilersGraphics.Models.Animation.PngSequenceExportSettings>("Settings");
        if (settings == null) return;

        try
        {
            var designerCanvas = System.Windows.Application.Current.MainWindow?.GetChildOfType<boilersGraphics.Controls.DesignerCanvas>();
            if (designerCanvas == null)
            {
                MainWindowVM.Message.Value = "DesignerCanvas が取得できなかったっす";
                return;
            }
            var saved = boilersGraphics.Helpers.Animation.PngSequenceExporter.Export(
                Timeline,
                settings,
                Timeline.ItemResolver,
                (time, path) => RenderFrameAndSavePng(designerCanvas, path));
            MainWindowVM.Message.Value = $"PNG {saved} 枚を {settings.OutputDirectory} に書き出したっす";
        }
        catch (Exception ex)
        {
            LogManager.GetCurrentClassLogger().Warn(ex, "PNG sequence export failed");
            MainWindowVM.Message.Value = $"書き出し失敗: {ex.Message}";
        }
    }

    private void RenderFrameAndSavePng(boilersGraphics.Controls.DesignerCanvas designerCanvas, string filePath)
    {
        var background = BackgroundItem.Value;
        var rtb = Renderer.Render(null, designerCanvas, this, background, background);
        if (rtb == null) return;
        using var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Create);
        var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
        encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(rtb));
        encoder.Save(stream);
    }

    /// <summary>
    /// Phase 5.5-c: WPF Storyboard XAML 書出ダイアログを開き、OK なら
    /// <see cref="boilersGraphics.Helpers.Animation.Export.WpfStoryboardXamlExporter"/> で
    /// <c>.xaml</c> (+ オプションで <c>.xaml.cs</c>) をファイル出力する。
    /// PathGeometry は各図形が描画用に保持している <c>PathGeometryNoRotate.Value</c> を利用 (= 既に流れた状態の Geometry)。
    /// </summary>
    private void ExecuteOpenWpfXamlExportDialogCommand()
    {
        IDialogResult result = null;
        dlgService.ShowDialog(nameof(boilersGraphics.Views.Animation.WpfXamlExportDialog),
            new DialogParameters(),
            ret => result = ret);
        if (result == null || result.Result != ButtonResult.OK) return;

        var settings = result.Parameters.GetValue<boilersGraphics.Helpers.Animation.Export.XamlExportSettings>("Settings");
        var outputPath = result.Parameters.GetValue<string>("OutputPath");
        if (settings == null || string.IsNullOrWhiteSpace(outputPath))
        {
            MainWindowVM.Message.Value = "XAML 書出に必要なパラメータが揃っていなかったっす";
            return;
        }

        try
        {
            var allItems = (AllItems?.Value as System.Collections.Generic.IEnumerable<SelectableDesignerItemViewModelBase>)
                ?? System.Array.Empty<SelectableDesignerItemViewModelBase>();
            var allItemsList = new System.Collections.Generic.List<SelectableDesignerItemViewModelBase>(allItems);
            var exporter = new boilersGraphics.Helpers.Animation.Export.WpfStoryboardXamlExporter(
                allItemsList,
                BuildPathGeometryResolverForXamlExport());

            var options = new System.Collections.Generic.Dictionary<string, object>
            {
                { "TargetNamespace", settings.TargetNamespace },
                { "ClassName", settings.ClassName },
                { "AccessModifier", settings.AccessModifier },
                { "GenerateCodeBehind", settings.GenerateCodeBehind },
                { "IndentWidth", settings.IndentWidth },
                { "NewLine", settings.NewLine },
                { "IncludeHeaderComment", settings.IncludeHeaderComment },
            };

            var written = exporter.Export(Timeline, outputPath, Timeline.ItemResolver, options);
            MainWindowVM.Message.Value = written == 1
                ? $"WPF XAML 1 ファイルを {outputPath} に書き出したっす"
                : $"WPF XAML 2 ファイル (.xaml + .xaml.cs) を {outputPath} 周辺に書き出したっす";
        }
        catch (System.Exception ex)
        {
            LogManager.GetCurrentClassLogger().Warn(ex, "WPF XAML export failed");
            MainWindowVM.Message.Value = $"書き出し失敗: {ex.Message}";
        }
    }

    /// <summary>
    /// Phase 5.5-c: Path 系図形に対する PathGeometry 解決。各 ViewModel が描画時に
    /// セットしている <see cref="SelectableDesignerItemViewModelBase.PathGeometryNoRotate"/> を最優先。
    /// 未初期化なら <c>CreateGeometry(false)</c> をフォールバックで試す
    /// (<see cref="PathDesignerItemViewModel"/> は NotSupportedException を投げるので null フォールバック)。
    /// </summary>
    internal static System.Func<SelectableDesignerItemViewModelBase, System.Windows.Media.PathGeometry> BuildPathGeometryResolverForXamlExport()
    {
        return item =>
        {
            if (item is null) return null;
            var stored = item.PathGeometryNoRotate?.Value;
            if (stored is not null) return stored;
            if (item is DesignerItemViewModelBase d)
            {
                try { return d.CreateGeometry(false); }
                catch { return null; }
            }
            return null;
        };
    }

    /// <summary>
    /// Phase 4-c / Q-3 案 A: 指定スコープの図形の EdgeBrush / FillBrush をテーマで直接書換。
    /// Phase 4-d: lineStyle が non-null なら StrokeDashArray / StrokeLineJoin も書換。
    /// Phase 4-e / Q-9 案 A: applyGlow=true なら DefaultGlow を各図形の GlowRadius/Intensity/Color に流し込む。
    /// </summary>
    private void ApplyThemeToScope(
        boilersGraphics.Models.Themes.Theme theme,
        boilersGraphics.Models.Themes.ThemeApplyScope scope,
        boilersGraphics.Models.Themes.ThemeApplyTarget target,
        boilersGraphics.Models.Themes.LineStyle lineStyle,
        bool applyGlow)
    {
        if (theme == null) return;
        var (edge, fill) = boilersGraphics.Helpers.Themes.ThemeApplier.ResolveBrushes(theme, target);

        var selected = SelectedItems.Value
            .AsValueEnumerable()
            .OfType<SelectableDesignerItemViewModelBase>()
            .ToList();
        var activeLayerItems = SelectedLayers.Value
            .AsValueEnumerable()
            .OfType<LayerItem>()
            .Select(li => li.Item.Value)
            .OfType<SelectableDesignerItemViewModelBase>()
            .ToList();
        var allItems = Layers
            .SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
            .AsValueEnumerable()
            .OfType<LayerItem>()
            .Select(li => li.Item.Value)
            .OfType<SelectableDesignerItemViewModelBase>()
            .ToList();

        var targets = boilersGraphics.Helpers.Themes.ThemeApplier.ResolveScope(
            scope, selected, activeLayerItems, allItems);

        var (glowRadius, glowIntensity, glowColor) = boilersGraphics.Helpers.Themes.ThemeApplier.ResolveGlow(theme);
        foreach (var item in targets)
        {
            if (edge != null) item.EdgeBrush.Value = edge;
            if (fill != null) item.FillBrush.Value = fill;
            // Phase 4-d: 線種プリセットがあれば StrokeDashArray / StrokeLineJoin を書換 (テーマ側参照は共有しない)。
            if (lineStyle != null)
            {
                item.StrokeDashArray.Value = boilersGraphics.Helpers.Themes.ThemeApplier.CopyDashArray(lineStyle);
                item.StrokeLineJoin.Value = lineStyle.StrokeLineJoin;
            }
            // Phase 4-e: グロー設定を流し込む (DataTemplate での視覚化は 4-e-2 で対応)。
            if (applyGlow)
            {
                item.GlowRadius.Value = glowRadius;
                item.GlowIntensity.Value = glowIntensity;
                item.GlowColor.Value = glowColor;
            }
        }
        ActiveTheme.Value = theme;
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
        Debug.WriteLine($"=== ExecuteAddItemCommand START ===");
        Debug.WriteLine($"Parameter: {parameter?.GetType().Name}");

        if (parameter is SelectableDesignerItemViewModelBase item)
        {
            Debug.WriteLine($"Item details: {item}");

            var targetLayer = GetSelectedLayer();
            Debug.WriteLine($"Target layer: {targetLayer?.Name?.Value ?? "NULL"}");
            Debug.WriteLine($"Target layer type: {targetLayer?.GetType().Name}");

            if (targetLayer == null)
            {
                Debug.WriteLine("ERROR: targetLayer is null - RETURNING");
                return;
            }

            Debug.WriteLine("Calculating newZIndex...");
            var newZIndex = targetLayer.GetNewZIndex(Layers.AsValueEnumerable().TakeWhile(x => x != targetLayer).ToArray());
            Debug.WriteLine($"New ZIndex: {newZIndex}");

            Debug.WriteLine("Pushing ZIndex for other layers...");
            Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
                .AsValueEnumerable()
                .Where(x => x != targetLayer)
                .ToList()
                .ForEach(x => x.PushZIndex(MainWindowVM.Recorder, newZIndex));

            Debug.WriteLine("Setting item properties...");
            item.ZIndex.Value = newZIndex;
            item.Owner = this;

            // Phase 2-f: PartInstance が Owner = this になったタイミングで、対応する PartDefinition の
            // Items を ID 引き継ぎクローンして RenderedItems に詰め、Binding 値伝搬を配線する。
            // Definition が見つからない場合は何もしない (デシリアライズ中など、Definition が後から追加される
            // ケースは別途 Definition 側の CollectionChanged で再初期化する想定 — 現状は AddItem 経由のみ対応)。
            if (item is boilersGraphics.ViewModels.Parts.PartInstanceViewModel partInstance
                && TryGetPartDefinition(partInstance.DefinitionId.Value, out var partDefinition))
            {
                partInstance.InitializeRenderedItems(partDefinition);
            }

            Debug.WriteLine($"About to call targetLayer.AddItem with item: {item}");
            Debug.WriteLine($"Target layer children count before: {targetLayer.Children.Count}");

            // レイヤーに追加
            targetLayer.AddItem(MainWindowVM, this, item);

            Debug.WriteLine($"Target layer children count after: {targetLayer.Children.Count}");
            
            //// 手動でAllItemsの更新をトリガー
            //Debug.WriteLine("Manually triggering AllItems update...");
            //RootLayer.OnNext(RootLayer.Value); // RootLayerの変更を通知

            Debug.WriteLine("=== ExecuteAddItemCommand END ===");
        }
        else
        {
            Debug.WriteLine("ERROR: Parameter is not SelectableDesignerItemViewModelBase");
        }
    }

    private LayerTreeViewItemBase GetSelectedLayer()
    {
        var targetLayer = SelectedLayers.Value.AsValueEnumerable().FirstOrDefault();
        if (targetLayer == null)
            targetLayer = Layers.AsValueEnumerable().FirstOrDefault();
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
            // Phase 3.5 / Q-8 案 B: AnchorViewModel を削除する前に、その Id を参照する
            // OrthogonalConnector / AnchorBezierConnector を先に削除 (自由端コネクタが残る
            // 状況を回避し、データの一貫性を保つ)。明示 Guid 参照のみ対象、暗黙 9 点参照 (#xx)
            // は対象外なのでオーナー DesignerItem 削除時に別途回収される。
            if (item is boilersGraphics.ViewModels.Anchors.AnchorViewModel anchor)
            {
                RemoveAnchorReferringConnectors(anchor);
            }
            RemoveGroupMembers(item);
            Remove(item);
            if (item is LetterDesignerItemViewModel) (item as LetterDesignerItemViewModel).CloseLetterSettingDialog();
            if (item is LetterVerticalDesignerItemViewModel)
                (item as LetterVerticalDesignerItemViewModel).CloseLetterSettingDialog();
            item.Dispose();
            UpdateZIndex();
        }
    }

    /// <summary>
    /// Phase 3.5 / Q-8 案 B: 指定 AnchorViewModel の Id を明示参照しているコネクタ
    /// (OrthogonalConnector / AnchorBezierConnector) を一括削除する。
    /// 検索ロジックは <see cref="boilersGraphics.Helpers.Anchors.AnchorReferenceFinder"/> に分離。
    /// </summary>
    private void RemoveAnchorReferringConnectors(boilersGraphics.ViewModels.Anchors.AnchorViewModel anchor)
    {
        if (anchor is null) return;
        // AllItems.Value はライブ参照配列なので、削除中の再列挙を避けるため snapshot を取る。
        var refs = boilersGraphics.Helpers.Anchors.AnchorReferenceFinder
            .FindReferring(AllItems.Value, anchor.ID)
            .ToList();
        foreach (var connector in refs)
        {
            ExecuteRemoveItemCommand(connector);
        }
    }

    private void UpdateZIndex()
    {
        var items = (from item in Layers.AsValueEnumerable().SelectMany(x => x.Children)
            orderby (item as LayerItem).Item.Value.ZIndex.Value
            select item).ToList();

        for (var i = 0; i < items.Count; ++i) (items.AsValueEnumerable().ElementAt(i) as LayerItem).Item.Value.ZIndex.Value = i;
    }

    private void RemoveGroupMembers(SelectableDesignerItemViewModelBase item)
    {
        if (item is GroupItemViewModel groupItem)
        {
            var children = (from it in Layers.AsValueEnumerable().SelectMany(x => x.Children)
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
                     .AsValueEnumerable()
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

    private void LoadCanvasPagesFromXml(XElement root, MainWindowViewModel mainWindowVM)
    {
        var canvasesElement = root.Element("Canvases");
        if (canvasesElement == null)
        {
            // Legacy single-canvas file: create one page with current canvas state
            mainWindowVM.CanvasPages.Clear();
            var page = new Models.CanvasPage("Canvas 1");
            page.SerializedData = SerializeCanvasState();
            mainWindowVM.CanvasPages.Add(page);
            mainWindowVM.ActiveCanvasIndex.Value = 0;
            return;
        }

        mainWindowVM.CanvasPages.Clear();
        int index = 0;
        foreach (var canvasElement in canvasesElement.Elements("Canvas"))
        {
            var name = canvasElement.Attribute("Name")?.Value ?? $"Canvas {index + 1}";
            var page = new Models.CanvasPage(name);
            page.SerializedData = new XElement(canvasElement);
            mainWindowVM.CanvasPages.Add(page);
            index++;
        }

        var activeIndexElement = root.Element("ActiveCanvasIndex");
        int activeIndex = 0;
        if (activeIndexElement != null)
            int.TryParse(activeIndexElement.Value, out activeIndex);

        activeIndex = Math.Max(0, Math.Min(activeIndex, mainWindowVM.CanvasPages.Count - 1));
        mainWindowVM.ActiveCanvasIndex.Value = activeIndex;

        // The current canvas (loaded via legacy path) represents the active canvas
        // Update its serialized data
        if (mainWindowVM.CanvasPages.Count > 0)
        {
            mainWindowVM.CanvasPages[activeIndex].SerializedData = SerializeCanvasState();
        }
    }

    public XElement SerializeCanvasState()
    {
        var canvas = new XElement("Canvas");
        canvas.Add(new XElement("Layers", ObjectSerializer.SerializeLayers(Layers)));

        var background = BackgroundItem.Value;
        var bgElement = new XElement("Background");
        bgElement.Add(new XElement("Left", background.Left.Value));
        bgElement.Add(new XElement("Top", background.Top.Value));
        bgElement.Add(new XElement("Width", background.Width.Value));
        bgElement.Add(new XElement("Height", background.Height.Value));
        bgElement.Add(new XElement("FillBrush",
            XElement.Parse(Helpers.WpfObjectSerializer.Serialize(CanvasFillBrush.Value))));
        canvas.Add(bgElement);

        return canvas;
    }

    public void RestoreCanvasState(XElement canvas)
    {
        Layers.Clear();

        var bgElement = canvas.Element("Background");
        if (bgElement != null)
        {
            var background = BackgroundItem.Value;
            background.Left.Value = double.Parse(bgElement.Element("Left").Value);
            background.Top.Value = double.Parse(bgElement.Element("Top").Value);
            background.Width.Value = double.Parse(bgElement.Element("Width").Value);
            background.Height.Value = double.Parse(bgElement.Element("Height").Value);

            var fillBrushElement = bgElement.Element("FillBrush");
            if (fillBrushElement != null && fillBrushElement.HasElements)
            {
                var brush = Helpers.WpfObjectSerializer.Deserialize(
                    fillBrushElement.Elements().First().ToString());
                if (brush is System.Windows.Media.Brush b)
                    CanvasFillBrush.Value = b;
            }
        }

        ObjectDeserializer.ReadObjectsFromXML(this, null, canvas);

        if (Layers.Count == 0)
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
        item.Item.Value.LayerItem.Value = item;
        SelectedLayers.Value.AsValueEnumerable().First().AddItem(MainWindowVM, this, item);
        LogManager.GetCurrentClassLogger().Info($"Add item {item.ShowPropertiesAndFields()}");
        UpdateStatisticsCountAdd();
    }

    private void UpdateStatisticsCountAdd()
    {
        var statistics = MainWindowVM.Statistics.Value;
        statistics.NumberOfTimesYouHaveNamedAndSaved++;
        var dao = new StatisticsDao();
        dao.Update(statistics);
    }

    private void Remove(SelectableDesignerItemViewModelBase item)
    {
        Layers.AsValueEnumerable().ToList().ForEach(x => x.RemoveItem(MainWindowVM, item));
        ScanEffectViewModelObjects();
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
        Layers.AsValueEnumerable().SelectMany(x => x.Children).ToList().ForEach(x => (x as LayerItem).Item.Value.IsSelected.Value = true);
    }

    private IEnumerable<SelectableDesignerItemViewModelBase> GetGroupMembers(SelectableDesignerItemViewModelBase item)
    {
        var list = new List<SelectableDesignerItemViewModelBase>();
        list.Add(item);
        var children = Layers.AsValueEnumerable().SelectMany(x => x.Children)
            .Where(x => (x as LayerItem).Item.Value.ParentID == item.ID)
            .Select(x => (x as LayerItem).Item.Value);
        list.AddRange(children.ToArray());
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

    public BindableReactiveProperty<LayerTreeViewItemBase> RootLayer { get; set; } = new(new RootLayer());

    public NotifyCollectionChangedSynchronizedViewList<LayerTreeViewItemBase> Layers { get; }

    public System.Collections.ObjectModel.ObservableCollection<boilersGraphics.ViewModels.Parts.PartDefinitionViewModel> PartDefinitions { get; }
        = new System.Collections.ObjectModel.ObservableCollection<boilersGraphics.ViewModels.Parts.PartDefinitionViewModel>();

    // PartDefinitions の Id 索引 (Phase 1-c-8)。PartDefinitions の CollectionChanged を購読して同期。
    private readonly Dictionary<Guid, boilersGraphics.ViewModels.Parts.PartDefinitionViewModel> _partDefinitionsById = new();

    public IReadOnlyDictionary<Guid, boilersGraphics.ViewModels.Parts.PartDefinitionViewModel> PartDefinitionsById => _partDefinitionsById;

    public bool TryGetPartDefinition(Guid id, out boilersGraphics.ViewModels.Parts.PartDefinitionViewModel definition)
        => _partDefinitionsById.TryGetValue(id, out definition);

    private void OnPartDefinitionsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (e.NewItems is null) break;
                foreach (boilersGraphics.ViewModels.Parts.PartDefinitionViewModel def in e.NewItems)
                {
                    _partDefinitionsById[def.Id.Value] = def;
                    InitializePartInstancesForDefinition(def);
                }
                break;
            case NotifyCollectionChangedAction.Remove:
                if (e.OldItems is null) break;
                foreach (boilersGraphics.ViewModels.Parts.PartDefinitionViewModel def in e.OldItems)
                    _partDefinitionsById.Remove(def.Id.Value);
                break;
            case NotifyCollectionChangedAction.Replace:
                if (e.OldItems is not null)
                    foreach (boilersGraphics.ViewModels.Parts.PartDefinitionViewModel def in e.OldItems)
                        _partDefinitionsById.Remove(def.Id.Value);
                if (e.NewItems is not null)
                    foreach (boilersGraphics.ViewModels.Parts.PartDefinitionViewModel def in e.NewItems)
                    {
                        _partDefinitionsById[def.Id.Value] = def;
                        InitializePartInstancesForDefinition(def);
                    }
                break;
            case NotifyCollectionChangedAction.Reset:
                _partDefinitionsById.Clear();
                foreach (var def in PartDefinitions)
                {
                    _partDefinitionsById[def.Id.Value] = def;
                    InitializePartInstancesForDefinition(def);
                }
                break;
        }
    }

    /// <summary>
    /// Phase 2-f-3: Definition が後から (デシリアライズ完了後 / Import 後) 追加された場合に、
    /// その Definition を参照済みの PartInstance を全部 InitializeRenderedItems で再配線する。
    /// PartDefinitions の CollectionChanged から呼ばれる。
    /// </summary>
    private void InitializePartInstancesForDefinition(boilersGraphics.ViewModels.Parts.PartDefinitionViewModel def)
    {
        var items = AllItems.Value;
        if (items is null) return;
        foreach (var item in items)
        {
            if (item is boilersGraphics.ViewModels.Parts.PartInstanceViewModel pi
                && pi.DefinitionId.Value == def.Id.Value)
            {
                pi.InitializeRenderedItems(def);
            }
        }
    }

    /// <summary>
    /// AllItems の PartInstance が PartDefinitions に存在する DefinitionId を参照しているかを検査する。
    /// Save/Load の前後で孤児参照を検出するために使う (Phase 1-c-8)。
    /// </summary>
    public PartReferenceValidationResult ValidatePartReferences()
    {
        var orphans = new List<boilersGraphics.ViewModels.Parts.PartInstanceViewModel>();
        var items = AllItems.Value;
        if (items is null) return new PartReferenceValidationResult(orphans);

        foreach (var item in items)
        {
            if (item is not boilersGraphics.ViewModels.Parts.PartInstanceViewModel pi) continue;
            if (!_partDefinitionsById.ContainsKey(pi.DefinitionId.Value))
                orphans.Add(pi);
        }
        return new PartReferenceValidationResult(orphans);
    }

    public readonly record struct PartReferenceValidationResult(
        IReadOnlyList<boilersGraphics.ViewModels.Parts.PartInstanceViewModel> OrphanedInstances)
    {
        public bool HasOrphans => OrphanedInstances.Count > 0;
    }

    /// <summary>
    /// Phase 1-c-6-d-7: 指定 Definition Id を参照する PartInstance の数を返す (キャンバス全体)。
    /// 0 なら「未使用」。
    /// </summary>
    public int GetPartInstanceReferenceCount(Guid definitionId)
    {
        var items = AllItems.Value;
        if (items is null) return 0;

        var count = 0;
        foreach (var item in items)
        {
            if (item is boilersGraphics.ViewModels.Parts.PartInstanceViewModel pi
                && pi.DefinitionId.Value == definitionId)
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// Phase 1-c-6-d-7: どの PartInstance からも参照されていない PartDefinition の一覧を返す。
    /// </summary>
    public IReadOnlyList<boilersGraphics.ViewModels.Parts.PartDefinitionViewModel> GetUnusedPartDefinitions()
    {
        var result = new List<boilersGraphics.ViewModels.Parts.PartDefinitionViewModel>();
        foreach (var def in PartDefinitions)
        {
            if (GetPartInstanceReferenceCount(def.Id.Value) == 0)
                result.Add(def);
        }
        return result;
    }

    /// <summary>
    /// Phase 1-c-6-d-7: 未使用 PartDefinition を Recorder 経由で削除する。Undo で復元可能。
    /// 戻り値は削除した件数。
    /// </summary>
    public int RemoveUnusedPartDefinitions()
    {
        var unused = GetUnusedPartDefinitions();
        if (unused.Count == 0) return 0;

        MainWindowVM.Recorder.BeginRecode();
        try
        {
            foreach (var def in unused)
                MainWindowVM.Recorder.Current.ExecuteRemove(PartDefinitions, def);
        }
        finally
        {
            MainWindowVM.Recorder.EndRecode();
        }
        return unused.Count;
    }

    public IReadOnlyBindableReactiveProperty<LayerTreeViewItemBase[]> SelectedLayers { get; }

    public IReadOnlyBindableReactiveProperty<SelectableDesignerItemViewModelBase[]> AllItems { get; }

    public IReadOnlyBindableReactiveProperty<SelectableDesignerItemViewModelBase[]> SelectedItems { get; }

    public BindableReactiveProperty<BackgroundViewModel> BackgroundItem { get; } = new();

    public BindableReactiveProperty<double?> EdgeThickness { get; } = new();

    public BindableReactiveProperty<bool> EnableMiniMap { get; } = new();

    public BindableReactiveProperty<bool> EnableCombine { get; } = new();

    public BindableReactiveProperty<bool> EnableLayers { get; } = new();

    public BindableReactiveProperty<bool> EnableWorkHistory { get; } = new();

    public BindableReactiveProperty<bool> EnableBrushThickness { get; } = new();

    public BindableReactiveProperty<string> FileName { get; } = new();

    public BindableReactiveProperty<Brush> CanvasFillBrush { get; } = new();

    public BindableReactiveProperty<bool> EnablePointSnap { get; } = new();

    public BindableReactiveProperty<bool> EnableAutoSave { get; } = new();

    public BindableReactiveProperty<DateTime> AutoSavedDateTime { get; } = new();

    public BindableReactiveProperty<AutoSaveType> AutoSaveType { get; } = new();

    public BindableReactiveProperty<TimeSpan> AutoSaveInterval { get; } = new(TimeSpan.FromMinutes(1));

    public NotifyCollectionChangedSynchronizedViewList<string> AutoSaveFiles { get; set; } = new ObservableList<string>().ToWritableNotifyCollectionChanged();

    public BindableReactiveProperty<AngleType> AngleType { get; set; } = new();

    public BindableReactiveProperty<bool> EnableImageEmbedding { get; set; } = new();

    // Default to enabled with a moderate speed (8 px / 30 ms tick).
    // Affects DragThumb's auto-scroll-on-drag behavior. Settable via the
    // Preference dialog. Speed is the per-tick scroll increment in pixels;
    // 1.0 is very slow, 30.0 is very fast.
    public BindableReactiveProperty<bool> EnableAutoScrollOnDrag { get; set; } = new(true);
    public BindableReactiveProperty<double> AutoScrollOnDragSpeed { get; set; } = new(8d);

    public BindableReactiveProperty<Visibility> ContextMenuVisibility { get; } = new();

    public BindableReactiveProperty<ColorSpots> ColorSpots { get; } = new();

    public BindableReactiveProperty<Brush> EdgeBrush { get; } = new();
    public BindableReactiveProperty<Brush> FillBrush { get; } = new();

    public IReadOnlyBindableReactiveProperty<double> RenderWidth { get; }
    public IReadOnlyBindableReactiveProperty<double> RenderHeight { get; }

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
                .AsValueEnumerable()
                .Select(x => new Tuple<SnapPoint, Point>(x, GetCenter(x)))
                .Distinct()
                .ToArray();
            return sets;
        }
    }

    /// <summary>
    ///     拡大率
    /// </summary>
    public BindableReactiveProperty<double> MagnificationRate { get; } = new(100);

    public Queue<Action> LoadedEventActions { get; } = new();

    #endregion //Property

    #region Save

    private void ExecuteSaveAsCommand()
    {
        var root = BuildSaveXElement();
        SaveFileWithSaveFileDialog(root);
    }

    internal XElement BuildSaveXElement()
    {
        var root = new XElement("boilersGraphics");
        root.Add(new XElement("Version", BGSXFileVersion.ToString()));

        var canvasPages = MainWindowVM.CanvasPages;
        if (canvasPages.Count > 1 || (canvasPages.Count == 1 && canvasPages[0].SerializedData != null))
        {
            // Save current canvas state before serializing all pages
            MainWindowVM.SaveCurrentCanvasState();

            var canvasesXml = new XElement("Canvases");
            for (int i = 0; i < canvasPages.Count; i++)
            {
                var page = canvasPages[i];
                var canvasXml = page.SerializedData ?? SerializeCanvasState();
                canvasXml.SetAttributeValue("Name", page.Name);
                canvasesXml.Add(canvasXml);
            }
            root.Add(canvasesXml);
            root.Add(new XElement("ActiveCanvasIndex", MainWindowVM.ActiveCanvasIndex.Value));
        }

        // Always include legacy-compatible single-canvas format
        root.Add(new XElement("Layers", ObjectSerializer.SerializeLayers(Layers)));
        root.Add(new XElement("Configuration", ObjectSerializer.SerializeConfiguration(this)));
        root.Add(new XElement("Attachments", ObjectSerializer.SerializeAttachments(this)));
        // Phase 4-f-2: ActiveThemeId を保存。
        root.Add(ObjectSerializer.SerializeThemes(this));

        // Phase 5-d-1: 空でなければ <Timeline> セクションを追加 (空なら Phase 4 以前互換のため省略)。
        if (!Timeline.IsEmpty)
        {
            root.Add(boilersGraphics.Helpers.Animation.TimelineSerializer.SerializeTimeline(Timeline));
        }

        if (PartDefinitions.Count > 0)
        {
            root.Add(boilersGraphics.Helpers.Parts.PartSerializer.SerializeAll(PartDefinitions));
        }

        return root;
    }

    private void SaveFileWithSaveFileDialog(XElement xElement)
    {
        var saveFile = new SaveFileDialog();
        saveFile.Filter = "boiler's Graphics Format Files (*.bgff)|*.bgff|Files (*.xml)|*.xml|All Files (*.*)|*.*";
        var oldFileName = FileName.Value;
        if (saveFile.ShowDialog() == true)
            try
            {
                FileName.Value = saveFile.FileName;
                SaveFileWithoutSaveFileDialog(xElement, saveFile.FileName);

                UpdateStatisticsCountSaveAs();
            }
            catch (Exception ex)
            {
                FileName.Value = oldFileName;
                MessageBox.Show(ex.StackTrace, ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            }
    }

    private void SaveFileAndNoFileNameUpdatingWithoutSaveFileDialog(XElement xElement, string filename)
    {
        var oldFileName = FileName.Value;
        try
        {
            xElement.Save(filename);
            UpdateStatisticsCountSaveAs();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.StackTrace, ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void SaveFileWithoutSaveFileDialog(XElement xElement, string filename)
    {
        var oldFileName = FileName.Value;
        try
        {
            FileName.Value = filename;
            xElement.Save(filename);
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
        var root = BuildSaveXElement();
        Save(root);
    }

    private void Save(XElement root)
    {
        if (FileName.Value == "*")
        {
            var saveFile = new SaveFileDialog();
            saveFile.Filter = "boiler's Graphics Format Files (*.bgff)|*.bgff|Files (*.xml)|*.xml|All Files (*.*)|*.*";
            if (saveFile.ShowDialog() == true)
                try
                {
                    FileName.Value = saveFile.FileName;
                    SaveFileWithoutSaveFileDialog(root, saveFile.FileName);
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
                SaveFileWithoutSaveFileDialog(root, FileName.Value);
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

    public async Task Load(string filename)
    {
        try
        {
            await LoadInternal(XElement.Load(filename), filename);
            FileName.Value = filename;
        }
        catch (IOException e)
        {
            MessageBox.Show(e.ToString());
            return;
        }
        var statistics = MainWindowVM.Statistics.Value;
        statistics.NumberOfTimesTheFileWasOpenedBySpecifyingIt++;
        var dao = new StatisticsDao();
        dao.Update(statistics);
    }

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

        var willLoadingObjCount = ObjectDeserializer.CountObjectsFromXML(root) + 100 + 5;

        Func<ProgressBarWithOutputViewModel, Task> loadAction = async (vm) =>
        {
            var mainwindowViewModel = MainWindowVM;
            try
            {
                vm.Output.Value += Resources.Log_BeginLoadFromFile;
                LogManager.GetCurrentClassLogger().Info(Resources.Log_BeginLoadFromFile);
                mainwindowViewModel.Recorder.BeginRecode();

                var configuration = root.Element("Configuration");
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "CanvasFillBrush.Value",
                    WpfObjectSerializer.Deserialize(configuration.Element("CanvasFillBrush").Nodes().AsValueEnumerable().First()
                        .ToString()) ??
                    new SolidColorBrush((Color)ColorConverter.ConvertFromString(configuration
                        .Element("CanvasFillBrush")
                        .Nodes().AsValueEnumerable().First().ToString())));
                App.Current.Dispatcher.Invoke(() =>
                {
                    vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(this.CanvasFillBrush)}={this.CanvasFillBrush.Value}";
                }, DispatcherPriority.ApplicationIdle);
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "EnablePointSnap.Value",
                            bool.Parse(configuration.Element("EnablePointSnap").Value));
                App.Current.Dispatcher.Invoke(() =>
                {
                    vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(this.EnablePointSnap)}={this.EnablePointSnap.Value}";
                }, DispatcherPriority.ApplicationIdle);
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(mainwindowViewModel, "SnapPower.Value",
                            double.Parse(configuration.Element("SnapPower").Value));
                App.Current.Dispatcher.Invoke(() =>
                {
                    vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(mainwindowViewModel.SnapPower)}={mainwindowViewModel.SnapPower.Value}";
                }, DispatcherPriority.ApplicationIdle);
                if (configuration.Element("ColorSpots") != null)
                {
                    var colorSpots = configuration.Element("ColorSpots");
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot0",
                        WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot0").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        vm.Current.Value++;
                        vm.Output.Value += Environment.NewLine;
                        vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot0)}={ColorSpots.Value.ColorSpot0}";
                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot1",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot1").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                                        vm.Output.Value += Environment.NewLine;
                                        vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot1)}={ColorSpots.Value.ColorSpot1}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot2",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot2").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                                        vm.Output.Value += Environment.NewLine;
                                        vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot2)}={ColorSpots.Value.ColorSpot2}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot3",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot3").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                                        vm.Output.Value += Environment.NewLine;
                                        vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot3)}={ColorSpots.Value.ColorSpot3}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot4",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot4").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                                        vm.Output.Value += Environment.NewLine;
                                        vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot4)}={ColorSpots.Value.ColorSpot4}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot5",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot5").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                                        vm.Output.Value += Environment.NewLine;
                                        vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot5)}={ColorSpots.Value.ColorSpot5}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot6",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot6").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                                        vm.Output.Value += Environment.NewLine;
                                        vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot6)}={ColorSpots.Value.ColorSpot6}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot7",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot7").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                                        vm.Output.Value += Environment.NewLine;
                                        vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot7)}={ColorSpots.Value.ColorSpot7}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot8",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot8").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                                        vm.Output.Value += Environment.NewLine;
                                        vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot8)}={ColorSpots.Value.ColorSpot8}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot9",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot9").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                                        vm.Output.Value += Environment.NewLine;
                                        vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot9)}={ColorSpots.Value.ColorSpot9}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot10",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot10").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                                        vm.Output.Value += Environment.NewLine;
                                        vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot10)}={ColorSpots.Value.ColorSpot10}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot11",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot11").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                                        vm.Output.Value += Environment.NewLine;
                                        vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot11)}={ColorSpots.Value.ColorSpot11}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot12",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot12").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                                        vm.Output.Value += Environment.NewLine;
                                        vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot12)}={ColorSpots.Value.ColorSpot12}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot13",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot13").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                                        vm.Output.Value += Environment.NewLine;
                                        vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot13)}={ColorSpots.Value.ColorSpot13}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot14",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot14").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                                        vm.Output.Value += Environment.NewLine;
                                        vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot14)}={ColorSpots.Value.ColorSpot14}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot15",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot15").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                                        vm.Output.Value += Environment.NewLine;
                                        vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot15)}={ColorSpots.Value.ColorSpot15}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot16",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot16").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                                        vm.Output.Value += Environment.NewLine;
                                        vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot16)}={ColorSpots.Value.ColorSpot16}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot17",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot17").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                                        vm.Output.Value += Environment.NewLine;
                                        vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot17)}={ColorSpots.Value.ColorSpot17}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot18",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot18").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                                        vm.Output.Value += Environment.NewLine;
                                        vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot18)}={ColorSpots.Value.ColorSpot18}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot19",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot19").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                                        vm.Output.Value += Environment.NewLine;
                                        vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot19)}={ColorSpots.Value.ColorSpot19}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot20",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot20").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                                        vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot20)}={ColorSpots.Value.ColorSpot20}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot21",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot21").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                                        vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot21)}={ColorSpots.Value.ColorSpot21}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot22",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot22").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                                        vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot22)}={ColorSpots.Value.ColorSpot22}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot23",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot23").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                                        vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot23)}={ColorSpots.Value.ColorSpot23}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot24",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot24").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                                        vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot24)}={ColorSpots.Value.ColorSpot24}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot25",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot25").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                                        vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot25)}={ColorSpots.Value.ColorSpot25}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot26",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot26").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                                        vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot26)}={ColorSpots.Value.ColorSpot26}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot27",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot27").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                                        vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot27)}={ColorSpots.Value.ColorSpot27}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot28",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot28").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                                        vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot28)}={ColorSpots.Value.ColorSpot28}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot29",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot29").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                                        vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot29)}={ColorSpots.Value.ColorSpot29}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot30",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot30").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                                        vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot30)}={ColorSpots.Value.ColorSpot30}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot31",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot31").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                                        vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot31)}={ColorSpots.Value.ColorSpot31}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot32",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot32").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                                        vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot32)}={ColorSpots.Value.ColorSpot32}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot33",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot33").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                                        vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot33)}={ColorSpots.Value.ColorSpot33}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot34",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot34").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                                        vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot34)}={ColorSpots.Value.ColorSpot34}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot35",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot35").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                                        vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot35)}={ColorSpots.Value.ColorSpot35}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot36",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot36").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                                        vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot36)}={ColorSpots.Value.ColorSpot36}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot37",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot37").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                                        vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot37)}={ColorSpots.Value.ColorSpot37}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot38",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot38").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                                        vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot38)}={ColorSpots.Value.ColorSpot38}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot39",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot39").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                                        vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot39)}={ColorSpots.Value.ColorSpot39}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot40",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot40").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                                        vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot40)}={ColorSpots.Value.ColorSpot40}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot41",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot41").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                                        vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot41)}={ColorSpots.Value.ColorSpot41}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot42",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot42").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                                        vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot42)}={ColorSpots.Value.ColorSpot42}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot43",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot43").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                                        vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot43)}={ColorSpots.Value.ColorSpot43}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot44",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot44").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                                        vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot44)}={ColorSpots.Value.ColorSpot44}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot45",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot45").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot45)}={ColorSpots.Value.ColorSpot45}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot46",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot46").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot46)}={ColorSpots.Value.ColorSpot46}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot47",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot47").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot47)}={ColorSpots.Value.ColorSpot47}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot48",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot48").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot48)}={ColorSpots.Value.ColorSpot48}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot49",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot49").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot49)}={ColorSpots.Value.ColorSpot49}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot50",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot50").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot50)}={ColorSpots.Value.ColorSpot50}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot51",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot51").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot51)}={ColorSpots.Value.ColorSpot51}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot52",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot52").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot52)}={ColorSpots.Value.ColorSpot52}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot53",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot53").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot53)}={ColorSpots.Value.ColorSpot53}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot54",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot54").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot54)}={ColorSpots.Value.ColorSpot54}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot55",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot55").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot55)}={ColorSpots.Value.ColorSpot55}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot56",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot56").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot56)}={ColorSpots.Value.ColorSpot56}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot57",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot57").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot57)}={ColorSpots.Value.ColorSpot57}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot58",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot58").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot58)}={ColorSpots.Value.ColorSpot58}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot59",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot59").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot59)}={ColorSpots.Value.ColorSpot59}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot60",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot60").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot60)}={ColorSpots.Value.ColorSpot60}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot61",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot61").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot61)}={ColorSpots.Value.ColorSpot61}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot62",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot62").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot62)}={ColorSpots.Value.ColorSpot62}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot63",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot63").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot63)}={ColorSpots.Value.ColorSpot63}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot64",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot64").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot64)}={ColorSpots.Value.ColorSpot64}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot65",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot65").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot65)}={ColorSpots.Value.ColorSpot65}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot66",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot66").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot66)}={ColorSpots.Value.ColorSpot66}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot67",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot67").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot67)}={ColorSpots.Value.ColorSpot67}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot68",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot68").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot68)}={ColorSpots.Value.ColorSpot68}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot69",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot69").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot69)}={ColorSpots.Value.ColorSpot69}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot70",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot70").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot70)}={ColorSpots.Value.ColorSpot70}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot71",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot71").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot71)}={ColorSpots.Value.ColorSpot71}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot72",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot72").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot72)}={ColorSpots.Value.ColorSpot72}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot73",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot73").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot73)}={ColorSpots.Value.ColorSpot73}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot74",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot74").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot74)}={ColorSpots.Value.ColorSpot74}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot75",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot75").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot75)}={ColorSpots.Value.ColorSpot75}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot76",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot76").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot76)}={ColorSpots.Value.ColorSpot76}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot77",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot77").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot77)}={ColorSpots.Value.ColorSpot77}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot78",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot78").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot78)}={ColorSpots.Value.ColorSpot78}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot79",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot79").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot79)}={ColorSpots.Value.ColorSpot79}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot80",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot80").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot80)}={ColorSpots.Value.ColorSpot80}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot81",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot81").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot81)}={ColorSpots.Value.ColorSpot81}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot82",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot82").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot82)}={ColorSpots.Value.ColorSpot82}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot83",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot83").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot83)}={ColorSpots.Value.ColorSpot83}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot84",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot84").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot84)}={ColorSpots.Value.ColorSpot84}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot85",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot85").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot85)}={ColorSpots.Value.ColorSpot85}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot86",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot86").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot86)}={ColorSpots.Value.ColorSpot86}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot87",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot87").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot87)}={ColorSpots.Value.ColorSpot87}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot88",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot88").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot88)}={ColorSpots.Value.ColorSpot88}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot89",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot89").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot89)}={ColorSpots.Value.ColorSpot89}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot90",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot90").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot90)}={ColorSpots.Value.ColorSpot90}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot91",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot91").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot91)}={ColorSpots.Value.ColorSpot91}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot92",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot92").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot92)}={ColorSpots.Value.ColorSpot92}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot93",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot93").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot93)}={ColorSpots.Value.ColorSpot93}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot94",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot94").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot94)}={ColorSpots.Value.ColorSpot94}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot95",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot95").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot95)}={ColorSpots.Value.ColorSpot95}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot96",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot96").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot96)}={ColorSpots.Value.ColorSpot96}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot97",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot97").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot97)}={ColorSpots.Value.ColorSpot97}";
                                    }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot98",
                                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot98").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                                                {
                                                    vm.Current.Value++;
                                                    vm.Output.Value += Environment.NewLine;
                                                    vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot98)}={ColorSpots.Value.ColorSpot98}";
                                                }, DispatcherPriority.ApplicationIdle);
                    mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot99",
                        WpfObjectSerializer.Deserialize(
                            colorSpots.Element("ColorSpot99").Nodes().AsValueEnumerable().First().ToString()));
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        vm.Current.Value++;
                        vm.Output.Value += Environment.NewLine;
                        vm.Output.Value += $"{Resources.String_Loaded}：{nameof(ColorSpots.Value.ColorSpot99)}={ColorSpots.Value.ColorSpot99}";
                    }, DispatcherPriority.ApplicationIdle);
                }

                App.Current.Dispatcher.Invoke(() =>
                {
                    vm.Current.Value++;
                    vm.Output.Value += Environment.NewLine;
                    vm.Output.Value += $"{Resources.String_SetupCanvas}...";
                }, DispatcherPriority.ApplicationIdle);
                InitialSetting(mainwindowViewModel, false, false, isPreview);
                App.Current.Dispatcher.Invoke(() =>
                {
                    vm.Current.Value++;
                    vm.Output.Value += $"{Resources.String_Completed}";
                }, DispatcherPriority.ApplicationIdle);

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

                // Load multi-canvas pages if present
                LoadCanvasPagesFromXml(root, mainwindowViewModel);

                vm.Output.Value += Environment.NewLine;
                vm.Output.Value += Resources.Log_FinishLoadFromFile;
                LogManager.GetCurrentClassLogger().Info(Resources.Log_FinishLoadFromFile);

                App.Current.Dispatcher.Invoke(() =>
                {
                    RootLayer.Value.UpdateAppearanceBothParentAndChildBatched();
                }, DispatcherPriority.Render);
            }
            catch (Exception)
            {
                vm.Output.Value += Environment.NewLine;
                vm.Output.Value += Resources.Log_FileCannotOpenBecauseTooOldOrCorrupted;
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
            vm.Output.Value += Environment.NewLine;
            vm.Output.Value += string.Format(Resources.Log_LoadedFile, filename);
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
        App.Current.Dispatcher.Invoke(() =>
        {
            RootLayer.Value.UpdateAppearanceBothParentAndChildBatched();
        }, DispatcherPriority.Render);
    }

    private async Task LoadInternalForPreview(XElement root, string filename, bool isPreview = false)
    {
        if (root == null) return;

        if (root.Element("Version") != null)
        {
            var version = new Version(root.Element("Version").Value);
            if ( version > BGSXFileVersion)
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
            LogManager.GetCurrentClassLogger().Info(Resources.Log_BeginLoadFromFile);
            mainwindowViewModel.Recorder.BeginRecode();

            var configuration = root.Element("Configuration");
            mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "CanvasFillBrush.Value",
                WpfObjectSerializer.Deserialize(configuration.Element("CanvasFillBrush").Nodes().AsValueEnumerable().First().ToString()) ??
                new SolidColorBrush((Color)ColorConverter.ConvertFromString(configuration.Element("CanvasFillBrush")
                    .Nodes().AsValueEnumerable().First().ToString())));
            mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "EnablePointSnap.Value",
                bool.Parse(configuration.Element("EnablePointSnap").Value));
            mainwindowViewModel.Recorder.Current.ExecuteSetProperty(mainwindowViewModel, "SnapPower.Value",
                double.Parse(configuration.Element("SnapPower").Value));
            if (configuration.Element("ColorSpots") != null)
            {
                var colorSpots = configuration.Element("ColorSpots");
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot0",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot0").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot1",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot1").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot2",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot2").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot3",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot3").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot4",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot4").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot5",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot5").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot6",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot6").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot7",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot7").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot8",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot8").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot9",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot9").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot10",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot10").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot11",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot11").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot12",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot12").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot13",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot13").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot14",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot14").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot15",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot15").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot16",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot16").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot17",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot17").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot18",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot18").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot19",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot19").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot20",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot20").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot21",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot21").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot22",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot22").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot23",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot23").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot24",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot24").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot25",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot25").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot26",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot26").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot27",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot27").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot28",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot28").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot29",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot29").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot30",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot30").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot31",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot31").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot32",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot32").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot33",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot33").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot34",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot34").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot35",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot35").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot36",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot36").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot37",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot37").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot38",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot38").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot39",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot39").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot40",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot40").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot41",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot41").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot42",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot42").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot43",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot43").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot44",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot44").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot45",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot45").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot46",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot46").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot47",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot47").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot48",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot48").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot49",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot49").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot50",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot50").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot51",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot51").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot52",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot52").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot53",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot53").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot54",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot54").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot55",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot55").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot56",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot56").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot57",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot57").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot58",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot58").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot59",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot59").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot60",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot60").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot61",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot61").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot62",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot62").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot63",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot63").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot64",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot64").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot65",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot65").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot66",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot66").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot67",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot67").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot68",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot68").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot69",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot69").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot70",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot70").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot71",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot71").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot72",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot72").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot73",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot73").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot74",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot74").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot75",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot75").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot76",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot76").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot77",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot77").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot78",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot78").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot79",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot79").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot80",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot80").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot81",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot81").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot82",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot82").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot83",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot83").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot84",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot84").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot85",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot85").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot86",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot86").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot87",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot87").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot88",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot88").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot89",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot89").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot90",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot90").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot91",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot91").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot92",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot92").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot93",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot93").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot94",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot94").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot95",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot95").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot96",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot96").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot97",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot97").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot98",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot98").Nodes().AsValueEnumerable().First().ToString()));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(ColorSpots.Value, "ColorSpot99",
                    WpfObjectSerializer.Deserialize(colorSpots.Element("ColorSpot99").Nodes().AsValueEnumerable().First().ToString()));
            }

            InitialSetting(mainwindowViewModel, false, false, isPreview);

            if (configuration.Element("Left") != null)
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "BackgroundItem.Value.Left.Value",
                    double.Parse(configuration.Element("Left").Value));
            if (configuration.Element("Top") != null)
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "BackgroundItem.Value.Top.Value",
                    double.Parse(configuration.Element("Top").Value));
            if (configuration.Element("Width") != null)
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "BackgroundItem.Value.Width.Value",
                    double.Parse(configuration.Element("Width").Value));
            if (configuration.Element("Height") != null)
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "BackgroundItem.Value.Height.Value",
                    double.Parse(configuration.Element("Height").Value));

            ObjectDeserializer.ReadObjectsFromXML(this, null, root, isPreview);
            LogManager.GetCurrentClassLogger().Info(Resources.Log_FinishLoadFromFile);
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
            mainwindowViewModel.Controller.Flush();
        }

        await PostProcessInFileLoadingSequence(mainwindowViewModel).ConfigureAwait(false);

        LogManager.GetCurrentClassLogger().Info(string.Format(Resources.Log_LoadedFile, filename));
    }

    private async Task PostProcessInFileLoadingSequence(MainWindowViewModel mainwindowViewModel)
    {
        LogManager.GetCurrentClassLogger().Info(Resources.Log_BeginPostProcessInFileLoadingSequence);
        ScanEffectViewModelObjects();

        var layersViewModel = Application.Current.MainWindow.GetChildOfType<Layers>().DataContext as LayersViewModel;
        layersViewModel.InitializeHitTestVisible(mainwindowViewModel);
        Layers.AsValueEnumerable().First().IsSelected.Value = true;

        LogManager.GetCurrentClassLogger().Info(Resources.Log_FinishPostProcessInFileLoadingSequence);
    }

    private int Count(List<FrameworkElement> allViews)
    {
        int count = 0;
        foreach (var item in AllItems.Value)
        {
            var view = allViews.AsValueEnumerable().FirstOrDefault(x => x.DataContext == item);
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
        if (boilersGraphics.App.IsTest)
        {
            return;
        }

        App.Current.Dispatcher.BeginInvoke(() =>
        {
            foreach (var item in AllItems.Value.AsValueEnumerable().OrderBy(x => x.ZIndex.Value))
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
        LoadInternalForPreview(root, file, true);
    }

    private (XElement, string) LoadSerializedDataFromFile()
    {
        var openFile = new OpenFileDialog();
        openFile.Filter = "boiler's Graphics Format Files (*.bgff)|*.bgff|Files (*.xml)|*.xml|All Files (*.*)|*.*";

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

        var items = SelectedItems.Value.AsValueEnumerable().Select(x => x is SnapPointViewModel sp
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
            Layers.AsValueEnumerable().SelectMany(x => x.Children).First(x => (x as LayerItem).Item.Value == groupItem);

        var list = new List<Tuple<LayerItem, LayerTreeViewItemBase>>();

        foreach (var item in items)
        {
            var layerItem =
                Layers.AsValueEnumerable().SelectMany(x => x.Children).First(x => (x as LayerItem).Item.Value == item) as LayerItem;
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
            .AsValueEnumerable()
            .Where(x => x is LayerItem)
            .First(x => x as LayerItem == layerItem)
            .Parent.Value;
        layer.RemoveChildren(MainWindowVM.Recorder, layerItem);
    }

    public bool CanExecuteGroup()
    {
        var items = SelectedItems.Value.AsValueEnumerable().Where(item => item.ParentID == Guid.Empty);
        return items.Count() > 1;
    }

    private void ExecuteUngroupItemsCommand()
    {
        MainWindowVM.Recorder.BeginRecode();
        
        var groups = SelectedItems.Value.AsValueEnumerable().Where(item => item.ParentID == Guid.Empty);

        foreach (var groupRoot in groups.ToList())
        {
            var children =
                Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
                    .AsValueEnumerable().Where(child =>
                        child is LayerItem && (child as LayerItem).Item.Value.ParentID == groupRoot.ID).ToList();

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
                    .AsValueEnumerable()
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

            var it = from item in Layers.AsValueEnumerable().SelectMany(x => x.Children)
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
        var items = from item in SelectedItems.Value.AsValueEnumerable().OfType<GroupItemViewModel>()
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

        var orderedSelected = SelectedItems.Value
            .AsValueEnumerable()
            .OrderByDescending(item => item.ZIndex.Value)
            .ToList();

        foreach (var current in orderedSelected)
        {
            var topLevel = TopLevelItemsForOrdering();
            var currentIndex = current.ZIndex.Value;

            var next = topLevel
                .Where(li => li.Item.Value.ZIndex.Value > currentIndex)
                .OrderBy(li => li.Item.Value.ZIndex.Value)
                .FirstOrDefault();
            if (next == null) continue;

            var nextItem = next.Item.Value;
            var nextZIndex = nextItem.ZIndex.Value;

            MainWindowVM.Recorder.Current.ExecuteSetProperty(current, "ZIndex.Value", nextZIndex);
            MainWindowVM.Recorder.Current.ExecuteSetProperty(nextItem, "ZIndex.Value", currentIndex);

            (current as EffectViewModel)?.Render();
        }

        Sort(Layers);

        MainWindowVM.Recorder.EndRecode();

        UpdateStatisticsCountMoveToFront();
    }

    /// <summary>
    /// Direct LayerItem children of the currently selected layers (i.e.
    /// items participating in top-level Z-order). Group children sitting
    /// inside a group's LayerItem.Children are intentionally excluded.
    /// </summary>
    private List<LayerItem> TopLevelItemsForOrdering()
    {
        return SelectedLayers.Value
            .AsValueEnumerable()
            .SelectMany(layer => layer.Children)
            .OfType<LayerItem>()
            .ToList();
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

        var orderedSelected = SelectedItems.Value
            .AsValueEnumerable()
            .OrderBy(item => item.ZIndex.Value)
            .ToList();

        foreach (var current in orderedSelected)
        {
            var topLevel = TopLevelItemsForOrdering();
            var currentIndex = current.ZIndex.Value;

            var previous = topLevel
                .Where(li => li.Item.Value.ZIndex.Value < currentIndex)
                .OrderByDescending(li => li.Item.Value.ZIndex.Value)
                .FirstOrDefault();
            if (previous == null) continue;

            var previousItem = previous.Item.Value;
            var previousZIndex = previousItem.ZIndex.Value;

            MainWindowVM.Recorder.Current.ExecuteSetProperty(current, "ZIndex.Value", previousZIndex);
            MainWindowVM.Recorder.Current.ExecuteSetProperty(previousItem, "ZIndex.Value", currentIndex);

            (current as EffectViewModel)?.Render();
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

        var orderedSelected = SelectedItems.Value
            .AsValueEnumerable()
            .OrderByDescending(item => item.ZIndex.Value)
            .ToList();

        foreach (var current in orderedSelected)
        {
            // Treat the operation as repeated swap-with-next-top-level-item
            // until current sits on top. Each step swaps ZIndex values of
            // current and its immediate top-level neighbour, leaving group
            // children's ZIndex intact (their structure within the group
            // stays put per the design).
            while (true)
            {
                var topLevel = TopLevelItemsForOrdering();
                var currentIndex = current.ZIndex.Value;
                var next = topLevel
                    .Where(li => li.Item.Value.ZIndex.Value > currentIndex)
                    .OrderBy(li => li.Item.Value.ZIndex.Value)
                    .FirstOrDefault();
                if (next == null) break;

                var nextItem = next.Item.Value;
                var nextZIndex = nextItem.ZIndex.Value;
                MainWindowVM.Recorder.Current.ExecuteSetProperty(current, "ZIndex.Value", nextZIndex);
                MainWindowVM.Recorder.Current.ExecuteSetProperty(nextItem, "ZIndex.Value", currentIndex);
            }

            (current as EffectViewModel)?.Render();
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

        var orderedSelected = SelectedItems.Value
            .AsValueEnumerable()
            .OrderBy(item => item.ZIndex.Value)
            .ToList();

        foreach (var current in orderedSelected)
        {
            // Mirror BringForeground: repeatedly swap with the next-lower
            // top-level neighbour until current sits at the bottom.
            while (true)
            {
                var topLevel = TopLevelItemsForOrdering();
                var currentIndex = current.ZIndex.Value;
                var previous = topLevel
                    .Where(li => li.Item.Value.ZIndex.Value < currentIndex)
                    .OrderByDescending(li => li.Item.Value.ZIndex.Value)
                    .FirstOrDefault();
                if (previous == null) break;

                var previousItem = previous.Item.Value;
                var previousZIndex = previousItem.ZIndex.Value;
                MainWindowVM.Recorder.Current.ExecuteSetProperty(current, "ZIndex.Value", previousZIndex);
                MainWindowVM.Recorder.Current.ExecuteSetProperty(previousItem, "ZIndex.Value", currentIndex);
            }

            (current as EffectViewModel)?.Render();
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

    public static void Sort(NotifyCollectionChangedSynchronizedViewList<LayerTreeViewItemBase> target)
    {
        var list = target.AsValueEnumerable().ToList();

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
        return SelectedItems.Value.AsValueEnumerable().Count() > 0;
    }

    #endregion //Ordering

    #region Alignment

    private void ExecuteAlignTopCommand()
    {
        if (SelectedItems.Value.AsValueEnumerable().Count() > 1)
        {
            MainWindowVM.Recorder.BeginRecode();

            var first = SelectedItems.Value.AsValueEnumerable().First();
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
        if (SelectedItems.Value.AsValueEnumerable().Count() > 1)
        {
            MainWindowVM.Recorder.BeginRecode();

            var first = SelectedItems.Value.AsValueEnumerable().First();
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
        if (SelectedItems.Value.AsValueEnumerable().Count() > 1)
        {
            MainWindowVM.Recorder.BeginRecode();

            var first = SelectedItems.Value.AsValueEnumerable().First();
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
        if (SelectedItems.Value.AsValueEnumerable().Count() > 1)
        {
            MainWindowVM.Recorder.BeginRecode();

            var first = SelectedItems.Value.AsValueEnumerable().First();
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
        if (SelectedItems.Value.AsValueEnumerable().Count() > 1)
        {
            MainWindowVM.Recorder.BeginRecode();

            var first = SelectedItems.Value.AsValueEnumerable().First();
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
        if (SelectedItems.Value.AsValueEnumerable().Count() > 1)
        {
            MainWindowVM.Recorder.BeginRecode();

            var first = SelectedItems.Value.AsValueEnumerable().First();
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
        var selectedItems = SelectedItems.Value.OrderBy(GetLeft);

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
        // Wrap the entire batch so a single Undo rolls back all duplicates at
        // once. Without this, each ExecuteAdd recorded inside DuplicateDesignerItem /
        // DuplicateConnector becomes its own undo entry and the user has to Undo
        // N times to clear an N-item duplicate.
        MainWindowVM.Recorder.BeginRecode();
        try
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
        finally
        {
            MainWindowVM.Recorder.EndRecode();
        }
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
                    parentLayerItem.Children.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children));
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
                parentLayerItem.Children.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children));
        clone.ZIndex.Value = items.OfType<LayerItem>().Max(x => x.Item.Value.ZIndex.Value) + 1;
        if (groupItem != null)
        {
            clone.ParentID = groupItem.ID;
            clone.EnableForSelection.Value = false;
            groupItem.AddGroup(MainWindowVM.Recorder, clone);
            var newLayerItem = new LayerItem(clone, parentLayerItem, layerItemName);
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
        return SelectedItems.Value.Any();
    }

    #endregion //Duplicate

    #region ToggleNode

    /// <summary>Phase 3-g: 選択中の DesignerItem の IsNode を一括反転する (全て true なら全 false、それ以外なら全 true)。</summary>
    private bool CanExecuteToggleNode()
    {
        return SelectedItems.Value
            .AsValueEnumerable()
            .OfType<DesignerItemViewModelBase>()
            .Any();
    }

    private void ExecuteToggleNodeCommand()
    {
        var items = SelectedItems.Value
            .AsValueEnumerable()
            .OfType<DesignerItemViewModelBase>()
            .ToArray();
        if (items.Length == 0) return;
        var allAreNodes = items.AsValueEnumerable().All(i => i.IsNode.Value);
        var next = !allAreNodes;
        foreach (var item in items)
            item.IsNode.Value = next;
    }

    #endregion

    #region Promote

    private bool CanExecutePromoteToPart()
    {
        return SelectedItems.Value
            .AsValueEnumerable()
            .OfType<DesignerItemViewModelBase>()
            .Any(x => x is not boilersGraphics.ViewModels.Parts.PartInstanceViewModel);
    }

    private void ExecutePromoteToPartCommand()
    {
        var selected = SelectedItems.Value
            .AsValueEnumerable()
            .OfType<DesignerItemViewModelBase>()
            .Where(x => x is not boilersGraphics.ViewModels.Parts.PartInstanceViewModel)
            .ToArray();
        if (selected.Length == 0) return;

        var defaultName = $"パーツ{PartDefinitions.Count + 1}";

        if (App.IsTest || Application.Current is null)
        {
            CompletePromote(selected, defaultName);
            return;
        }

        var container = (Application.Current as Prism.Unity.PrismApplication)?.Container
                        as Prism.Ioc.IContainerExtension;
        if (container is null)
        {
            CompletePromote(selected, defaultName);
            return;
        }

        var dialogService = new Prism.Services.Dialogs.DialogService(container);
        var parameters = new Prism.Services.Dialogs.DialogParameters
        {
            { ViewModels.PromoteToPartDialogViewModel.SelectedPartNameKey, defaultName },
        };
        dialogService.ShowDialog(nameof(Views.PromoteToPart), parameters, ret =>
        {
            if (ret.Result != Prism.Services.Dialogs.ButtonResult.OK) return;
            var name = ret.Parameters.GetValue<string>(
                ViewModels.PromoteToPartDialogViewModel.SelectedPartNameKey);
            if (string.IsNullOrWhiteSpace(name)) name = defaultName;
            CompletePromote(selected, name);
        });
    }

    private void CompletePromote(DesignerItemViewModelBase[] selected, string name)
    {
        MainWindowVM.Recorder.BeginRecode();
        try
        {
            var result = boilersGraphics.Helpers.Parts.PartOperations.Promote(selected, name);
            // Recorder 経由で Add する。Undo 時に Definition もキャンバスから消える。
            MainWindowVM.Recorder.Current.ExecuteAdd(PartDefinitions, result.Definition);

            foreach (var item in selected)
                ExecuteRemoveItemCommand(item);

            ExecuteAddItemCommand(result.Instance);
            result.Instance.IsSelected.Value = true;
        }
        finally
        {
            MainWindowVM.Recorder.EndRecode();
        }
    }

    #endregion //Promote

    #region Detach

    private bool CanExecuteDetachPart()
    {
        return SelectedItems.Value
            .AsValueEnumerable()
            .OfType<boilersGraphics.ViewModels.Parts.PartInstanceViewModel>()
            .Any();
    }

    private void ExecuteDetachPartCommand()
    {
        var instance = SelectedItems.Value
            .AsValueEnumerable()
            .OfType<boilersGraphics.ViewModels.Parts.PartInstanceViewModel>()
            .FirstOrDefault();
        if (instance is null) return;

        if (!TryGetPartDefinition(instance.DefinitionId.Value, out var definition)) return;

        MainWindowVM.Recorder.BeginRecode();
        try
        {
            var detachedItems = boilersGraphics.Helpers.Parts.PartOperations
                .Detach(instance, definition);

            ExecuteRemoveItemCommand(instance);

            foreach (var item in detachedItems)
                ExecuteAddItemCommand(item);
        }
        finally
        {
            MainWindowVM.Recorder.EndRecode();
        }
    }

    #endregion //Detach

    #region ClonePartDefinition

    private bool CanExecuteClonePartDefinition()
    {
        return SelectedItems.Value
            .AsValueEnumerable()
            .OfType<boilersGraphics.ViewModels.Parts.PartInstanceViewModel>()
            .Any();
    }

    private void ExecuteClonePartDefinitionCommand()
    {
        var instance = SelectedItems.Value
            .AsValueEnumerable()
            .OfType<boilersGraphics.ViewModels.Parts.PartInstanceViewModel>()
            .FirstOrDefault();
        if (instance is null) return;

        if (!TryGetPartDefinition(instance.DefinitionId.Value, out var definition)) return;

        var newName = GenerateCloneName(definition.Name.Value);
        var clone = boilersGraphics.Helpers.Parts.PartOperations.Clone(definition, newName);

        MainWindowVM.Recorder.BeginRecode();
        try
        {
            MainWindowVM.Recorder.Current.ExecuteAdd(PartDefinitions, clone);
        }
        finally
        {
            MainWindowVM.Recorder.EndRecode();
        }

        OpenPartEditor(clone);
    }

    private string GenerateCloneName(string originalName)
    {
        var baseName = string.IsNullOrWhiteSpace(originalName) ? "パーツ" : originalName;
        var candidate = $"{baseName}のコピー";
        if (!PartDefinitions.Any(d => d.Name.Value == candidate))
            return candidate;
        for (var i = 2; ; i++)
        {
            candidate = $"{baseName}のコピー{i}";
            if (!PartDefinitions.Any(d => d.Name.Value == candidate))
                return candidate;
        }
    }

    #endregion //ClonePartDefinition

    #region EditPartDefinition

    /// <summary>
    /// Test hook: when running with App.IsTest = true, OpenPartEditor records the
    /// requested definition here instead of opening a window. Tests can inspect
    /// this to verify the editor was triggered. In production this stays null.
    /// </summary>
    internal boilersGraphics.ViewModels.Parts.PartDefinitionViewModel LastRequestedEditorTarget { get; set; }

    private bool CanExecuteEditPartDefinition()
    {
        return SelectedItems.Value
            .AsValueEnumerable()
            .OfType<boilersGraphics.ViewModels.Parts.PartInstanceViewModel>()
            .Any();
    }

    private void ExecuteEditPartDefinitionCommand()
    {
        var instance = SelectedItems.Value
            .AsValueEnumerable()
            .OfType<boilersGraphics.ViewModels.Parts.PartInstanceViewModel>()
            .FirstOrDefault();
        if (instance is null) return;

        if (!TryGetPartDefinition(instance.DefinitionId.Value, out var definition)) return;

        OpenPartEditor(definition);
    }

    internal void OpenPartEditor(boilersGraphics.ViewModels.Parts.PartDefinitionViewModel definition)
    {
        if (definition is null) return;

        if (App.IsTest || Application.Current is null)
        {
            LastRequestedEditorTarget = definition;
            return;
        }

        var container = (Application.Current as Prism.Unity.PrismApplication)?.Container
                        as Prism.Ioc.IContainerExtension;
        if (container is null)
        {
            LastRequestedEditorTarget = definition;
            return;
        }

        var dialogService = new Prism.Services.Dialogs.DialogService(container);
        var parameters = new Prism.Services.Dialogs.DialogParameters
        {
            { ViewModels.Parts.PartEditorViewModel.PartDefinitionKey, definition },
            { ViewModels.Parts.PartEditorViewModel.DiagramKey, this },
        };
        dialogService.Show(nameof(Views.PartEditor), parameters, _ => { });
    }

    /// <summary>
    /// Phase 1-c-6-d-6: Definition に公開パラメータを追加し、その Definition を参照する全 PartInstance の
    /// ParameterValues にも DefaultValue を投入する。Recorder 経由で Undo に対応。
    /// </summary>
    public void AddExposedPropertyToDefinition(
        boilersGraphics.ViewModels.Parts.PartDefinitionViewModel definition,
        boilersGraphics.ViewModels.Parts.ExposedPropertyViewModel exposedProperty)
    {
        if (definition is null || exposedProperty is null) return;

        MainWindowVM.Recorder.BeginRecode();
        try
        {
            MainWindowVM.Recorder.Current.ExecuteAdd(definition.ExposedProperties, exposedProperty);
            SyncExposedPropertyAddedToInstances(definition, exposedProperty);
        }
        finally
        {
            MainWindowVM.Recorder.EndRecode();
        }
    }

    /// <summary>
    /// Phase 1-c-6-d-6: Definition から公開パラメータを削除し、その Definition を参照する全 PartInstance の
    /// ParameterValues からも除去する。
    /// </summary>
    public void RemoveExposedPropertyFromDefinition(
        boilersGraphics.ViewModels.Parts.PartDefinitionViewModel definition,
        boilersGraphics.ViewModels.Parts.ExposedPropertyViewModel exposedProperty)
    {
        if (definition is null || exposedProperty is null) return;

        MainWindowVM.Recorder.BeginRecode();
        try
        {
            MainWindowVM.Recorder.Current.ExecuteRemove(definition.ExposedProperties, exposedProperty);
            SyncExposedPropertyRemovedFromInstances(definition, exposedProperty);
        }
        finally
        {
            MainWindowVM.Recorder.EndRecode();
        }
    }

    private void SyncExposedPropertyAddedToInstances(
        boilersGraphics.ViewModels.Parts.PartDefinitionViewModel definition,
        boilersGraphics.ViewModels.Parts.ExposedPropertyViewModel exposedProperty)
    {
        var items = AllItems.Value;
        if (items is null) return;

        var defId = definition.Id.Value;
        var epId = exposedProperty.Id.Value;
        var defaultValue = exposedProperty.DefaultValue.Value;

        foreach (var item in items)
        {
            if (item is boilersGraphics.ViewModels.Parts.PartInstanceViewModel pi
                && pi.DefinitionId.Value == defId)
            {
                pi.GetOrCreateParameterValue(epId, defaultValue);
            }
        }
    }

    private void SyncExposedPropertyRemovedFromInstances(
        boilersGraphics.ViewModels.Parts.PartDefinitionViewModel definition,
        boilersGraphics.ViewModels.Parts.ExposedPropertyViewModel exposedProperty)
    {
        var items = AllItems.Value;
        if (items is null) return;

        var defId = definition.Id.Value;
        var epId = exposedProperty.Id.Value;

        foreach (var item in items)
        {
            if (item is boilersGraphics.ViewModels.Parts.PartInstanceViewModel pi
                && pi.DefinitionId.Value == defId)
            {
                pi.RemoveParameterValue(epId);
            }
        }
    }

    #endregion //EditPartDefinition

    #region ImportExportPart (.bgpart)

    public const string PartFileExtension = ".bgpart";
    public const string PartFileFilter = "boiler's Graphics Part Files (*.bgpart)|*.bgpart|All Files (*.*)|*.*";

    /// <summary>
    /// Test hook: when running with App.IsTest = true, ExecuteImportPartCommand /
    /// ExecuteExportPartCommand record the path they would have used here and skip
    /// the file dialog. Tests can pre-set this to drive the import/export flow.
    /// </summary>
    internal string LastPartFilePath { get; set; }

    private bool CanExecuteExportPart()
    {
        return SelectedItems.Value
            .AsValueEnumerable()
            .OfType<boilersGraphics.ViewModels.Parts.PartInstanceViewModel>()
            .Any();
    }

    private void ExecuteExportPartCommand()
    {
        var instance = SelectedItems.Value
            .AsValueEnumerable()
            .OfType<boilersGraphics.ViewModels.Parts.PartInstanceViewModel>()
            .FirstOrDefault();
        if (instance is null) return;

        if (!TryGetPartDefinition(instance.DefinitionId.Value, out var definition)) return;

        var path = ResolveExportPath(definition.Name.Value);
        if (string.IsNullOrEmpty(path)) return;

        try
        {
            var xml = boilersGraphics.Helpers.Parts.PartSerializer.SerializePartFileFromViewModel(definition);
            xml.Save(path);
            LastPartFilePath = path;
        }
        catch (Exception ex) when (!App.IsTest)
        {
            MessageBox.Show(ex.Message, "パーツのエクスポート失敗", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ExecuteImportPartCommand()
    {
        var path = ResolveImportPath();
        if (string.IsNullOrEmpty(path)) return;

        try
        {
            var root = System.Xml.Linq.XElement.Load(path);
            // 同じ .bgpart を二度 Import しても両方が独立した PartDefinition として残るように、
            // Id を毎回振り直す。既存 Definition を上書きしたい場合は手動で削除してから Import。
            var vm = boilersGraphics.Helpers.Parts.PartDeserializer.DeserializePartFileToViewModel(root, this, assignNewId: true);

            MainWindowVM.Recorder.BeginRecode();
            try
            {
                MainWindowVM.Recorder.Current.ExecuteAdd(PartDefinitions, vm);
            }
            finally
            {
                MainWindowVM.Recorder.EndRecode();
            }

            LastPartFilePath = path;
        }
        catch (Exception ex) when (!App.IsTest)
        {
            MessageBox.Show(ex.Message, "パーツのインポート失敗", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private string ResolveExportPath(string defaultFileName)
    {
        if (App.IsTest || Application.Current is null) return LastPartFilePath;

        var dlg = new Microsoft.Win32.SaveFileDialog
        {
            Filter = PartFileFilter,
            DefaultExt = PartFileExtension,
            FileName = SanitizeFileName(defaultFileName),
        };
        return dlg.ShowDialog() == true ? dlg.FileName : null;
    }

    private string ResolveImportPath()
    {
        if (App.IsTest || Application.Current is null) return LastPartFilePath;

        var dlg = new Microsoft.Win32.OpenFileDialog
        {
            Filter = PartFileFilter,
            DefaultExt = PartFileExtension,
        };
        return dlg.ShowDialog() == true ? dlg.FileName : null;
    }

    private static string SanitizeFileName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "Part";
        var invalid = System.IO.Path.GetInvalidFileNameChars();
        var chars = name.Select(c => Array.IndexOf(invalid, c) >= 0 ? '_' : c).ToArray();
        return new string(chars);
    }

    #endregion //ImportExportPart

    #region RemoveUnusedPartDefinitions

    /// <summary>
    /// Test hook: ExecuteRemoveUnusedPartDefinitionsCommand が確認ダイアログをスキップした上で
    /// 実際に何件を削除したかを記録する。App.IsTest = true の時、確認 MessageBox を出さず即削除する。
    /// </summary>
    internal int LastUnusedRemovalCount { get; set; }

    private void ExecuteRemoveUnusedPartDefinitionsCommand()
    {
        var unused = GetUnusedPartDefinitions();
        if (unused.Count == 0)
        {
            if (!App.IsTest && Application.Current is not null)
            {
                MessageBox.Show(
                    "未使用のパーツ定義はありません。",
                    "未使用パーツ定義の削除",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            LastUnusedRemovalCount = 0;
            return;
        }

        if (!App.IsTest && Application.Current is not null)
        {
            var result = MessageBox.Show(
                $"未使用のパーツ定義が {unused.Count} 件あります。削除しますか?\n(Undo で復元できます)",
                "未使用パーツ定義の削除",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Question);
            if (result != MessageBoxResult.OK)
            {
                LastUnusedRemovalCount = 0;
                return;
            }
        }

        LastUnusedRemovalCount = RemoveUnusedPartDefinitions();
    }

    #endregion //RemoveUnusedPartDefinitions

    public void OverwriteColorSpot(Brush brush)
    {
        for (int i = 0; i < 100; i++)
        {
            if (OverwriteColorSpotIf(i, brush))
            {
                return;
            }
        }
    }

    private bool OverwriteColorSpotIf(int index, Brush brush)
    {
        var target = ColorSpots.Value.Get(index);
        var white = new SolidColorBrush(Colors.White);
        if (target is SolidColorBrush scb && scb.Color.Equals(white.Color) && !target.Equals(brush))
        {
            ColorSpots.Value.Set(index, brush);
            return true;
        }
        return false;
    }
}