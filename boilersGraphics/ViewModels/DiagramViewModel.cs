using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.Messenger;
using boilersGraphics.Models;
using boilersGraphics.UserControls;
using boilersGraphics.Views;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
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
        private MainWindowViewModel mainWindowViewModel;
        private IDialogService dlgService;
        private Point _CurrentPoint;
        private ObservableCollection<Color> _EdgeColors = new ObservableCollection<Color>();
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
        public DelegateCommand<MouseWheelEventArgs> MouseWheelCommand { get; private set; }
        public DelegateCommand<MouseEventArgs> PreviewMouseDownCommand { get; private set; }
        public DelegateCommand<MouseEventArgs> PreviewMouseUpCommand { get; private set; }
        public DelegateCommand<MouseEventArgs> MouseMoveCommand { get; private set; }
        public DelegateCommand<MouseEventArgs> MouseLeaveCommand { get; private set; }
        public DelegateCommand<MouseEventArgs> MouseEnterCommand { get; private set; }

        #region Property

        public ReactivePropertySlim<LayerTreeViewItemBase> RootLayer { get; set; } = new ReactivePropertySlim<LayerTreeViewItemBase>(new LayerTreeViewItemBase());

        public ReactiveCollection<LayerTreeViewItemBase> Layers { get; }

        public ReadOnlyReactivePropertySlim<LayerTreeViewItemBase[]> SelectedLayers { get; }

        public ReadOnlyReactivePropertySlim<SelectableDesignerItemViewModelBase[]> AllItems { get; }

        public ReadOnlyReactivePropertySlim<SelectableDesignerItemViewModelBase[]> SelectedItems { get; }

        public ReactivePropertySlim<BackgroundViewModel> BackgroundItem { get; } = new ReactivePropertySlim<BackgroundViewModel>();

        public ReactiveProperty<double?> EdgeThickness { get; } = new ReactiveProperty<double?>();

        public ReactiveProperty<bool> EnableMiniMap { get; } = new ReactiveProperty<bool>();

        public ReactiveProperty<string> FileName { get; } = new ReactiveProperty<string>();

        public ReactiveProperty<Color> CanvasBackground { get; } = new ReactiveProperty<Color>();

        public ReactiveProperty<bool> EnablePointSnap { get; } = new ReactiveProperty<bool>();

        public ObservableCollection<Color> EdgeColors
        {
            get { return _EdgeColors; }
            set { SetProperty(ref _EdgeColors, value); }
        }

        public ObservableCollection<Color> FillColors
        {
            get { return _FillColors; }
            set { SetProperty(ref _FillColors, value); }
        }

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
        public System.Version BGSXFileVersion { get; } = new System.Version(2, 1);

        public int LayerCount { get; set; } = 1;

        public int LayerItemCount { get; set; } = 1;

        public IEnumerable<Point> SnapPoints
        {
            get
            {
                var designerCanvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();
                var resizeThumbs = designerCanvas.EnumerateChildOfType<SnapPoint>();
                var sets = resizeThumbs
                                .Select(x => new Tuple<SnapPoint, Point>(x, GetCenter(x)))
                                .Distinct();
                return sets.Select(x => x.Item2);
            }
        }

        public IEnumerable<Point> GetSnapPoints(IEnumerable<SnapPoint> exceptSnapPoints)
        {
            var designerCanvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();
            var resizeThumbs = designerCanvas.EnumerateChildOfType<SnapPoint>();
            var sets = resizeThumbs
                            .Where(x => !exceptSnapPoints.Contains(x))
                            .Select(x => new Tuple<SnapPoint, Point>(x, GetCenter(x)))
                            .Distinct();
            return sets.Select(x => x.Item2);
        }

        #endregion //Property

        public DiagramViewModel(MainWindowViewModel mainWindowViewModel, int width, int height)
        {
            this.mainWindowViewModel = mainWindowViewModel;

            AddItemCommand = new DelegateCommand<object>(p => ExecuteAddItemCommand(p));
            RemoveItemCommand = new DelegateCommand<object>(p => ExecuteRemoveItemCommand(p));
            ClearSelectedItemsCommand = new DelegateCommand<object>(p => ExecuteClearSelectedItemsCommand(p));
            CreateNewDiagramCommand = new DelegateCommand<object>(p => ExecuteCreateNewDiagramCommand(p));
            LoadCommand = new DelegateCommand(() => ExecuteLoadCommand());
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
            EditMenuOpenedCommand = new DelegateCommand(() =>
            {
                CutCommand.RaiseCanExecuteChanged();
                CopyCommand.RaiseCanExecuteChanged();
                PasteCommand.RaiseCanExecuteChanged();
            });


            EdgeColors.CollectionChangedAsObservable()
                .Subscribe(_ => RaisePropertyChanged("EdgeColors"))
                .AddTo(_CompositeDisposable);
            FillColors.CollectionChangedAsObservable()
                .Subscribe(_ => RaisePropertyChanged("FillColors"))
                .AddTo(_CompositeDisposable);

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
                Trace.WriteLine($"{x.Length} items in AllItems.");
                Trace.WriteLine(string.Join(", ", x.Select(y => y?.ToString() ?? "null")));
            })
            .AddTo(_CompositeDisposable);

            SelectedItems = Layers.CollectionChangedAsObservable()
                                  .Select(_ => Layers.Select(x => x.SelectedLayerItemsChangedAsObservable()).Merge())
                                  .Switch()
                                  .Select(_ => Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
                                                     .Where(x => x.GetType() == typeof(LayerItem))
                                                     .Select(y => (y as LayerItem).Item.Value)
                                                     .Where(z => z.IsSelected.Value == true)
                                                     .OrderBy(z => z.SelectedOrder.Value)
                                                     .ToArray())
                                  .ToReadOnlyReactivePropertySlim(Array.Empty<SelectableDesignerItemViewModelBase>());

            SelectedItems.Subscribe(selectedItems =>
            {
                Trace.WriteLine($"SelectedItems changed {string.Join(", ", selectedItems.Select(x => x?.ToString() ?? "null"))}");

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
            })
            .AddTo(_CompositeDisposable);

            SelectedLayers = Layers.ObserveElementObservableProperty(x => x.IsSelected)
                                   .Select(_ => Layers.Where(x => x.IsSelected.Value == true).ToArray())
                                   .ToReadOnlyReactivePropertySlim(Array.Empty<LayerTreeViewItemBase>());

            SelectedLayers.Subscribe(x =>
            {
                Trace.WriteLine($"SelectedLayers changed {string.Join(", ", x.Select(x => x.ToString()))}");
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

            InitialSetting(mainWindowViewModel, true, true);
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
                    return new Point(leftTop.X, leftTop.Y + snapPoint.Height - 1);
                case "左下":
                    return new Point(leftTop.X + snapPoint.Width - 1, leftTop.Y);
                case "右下":
                    return new Point(leftTop.X, leftTop.Y);
                case "左":
                case "上":
                case "右":
                case "下":
                    return new Point(leftTop.X, leftTop.Y);
                case "始点":
                case "終点":
                case "制御点":
                case "独立点":
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
            mainwindowViewModel.Recorder.Current.ExecuteAdd(EdgeColors, Colors.Black);
            mainwindowViewModel.Recorder.Current.ExecuteAdd(FillColors, Colors.White);
            mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "EdgeThickness.Value", 1.0);
            mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "CanvasBorderThickness", 0.0);
            if (initCanvasBackground)
            {
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "CanvasBackground.Value", Colors.White);
            }
            BackgroundItem.Value = new BackgroundViewModel();
            mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "BackgroundItem.Value.ZIndex.Value", -1);
            mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "BackgroundItem.Value.FillColor.Value", CanvasBackground.Value);
            mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "BackgroundItem.Value.Left.Value", 0d);
            mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "BackgroundItem.Value.Top.Value", 0d);
            mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "BackgroundItem.Value.Width.Value", (double)Width);
            mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "BackgroundItem.Value.Height.Value", (double)Height);
            mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "BackgroundItem.Value.Owner", this);
            mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "BackgroundItem.Value.EdgeColor.Value", Colors.Black);
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
        private void ExecuteUndoCommand()
        {
            (App.Current.MainWindow.DataContext as MainWindowViewModel).Controller.Undo();
        }

        private bool CanExecuteUndo()
        {
            return (App.Current.MainWindow.DataContext as MainWindowViewModel).Controller.CanUndo;
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

        private bool CanExecuteClip()
        {
            return SelectedItems.Value.Count() == 2 &&
                   SelectedItems.Value.First().GetType() == typeof(PictureDesignerItemViewModel);
        }

        private void ExecuteExcludeCommand()
        {
            CombineAndAddItem(GeometryCombineMode.Exclude);
        }

        private bool CanExecuteExclude()
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
        }

        private bool CanExecuteXor()
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
        }

        private bool CanExecuteIntersect()
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
        }

        private void CombineAndAddItem(GeometryCombineMode mode)
        {
            var item1 = SelectedItems.Value.OfType<SelectableDesignerItemViewModelBase>().First();
            var item2 = SelectedItems.Value.OfType<SelectableDesignerItemViewModelBase>().Last();
            var combine = new CombineGeometryViewModel();
            Remove(item1);
            Remove(item2);
            combine.EdgeColor.Value = item1.EdgeColor.Value;
            combine.EdgeThickness.Value = item1.EdgeThickness.Value;
            combine.IsSelected.Value = true;
            combine.Owner = this;
            combine.ZIndex.Value = Layers.SelectMany(x => x.Children).Count();
            combine.PathGeometry.Value = GeometryCreator.CreateCombineGeometry(item1, item2);
            if (combine.PathGeometry.Value == null)
            {
                var item1PathGeometry = item1.PathGeometry.Value;
                var item2PathGeometry = item2.PathGeometry.Value;

                if (item1.RotationAngle.Value != 0)
                    item1PathGeometry = item1.RotatePathGeometry.Value;
                if (item2.RotationAngle.Value != 0)
                    item2PathGeometry = item2.RotatePathGeometry.Value;
                
                CastToLetterAndSetTransform(item1, item2, item1PathGeometry, item2PathGeometry);

                combine.PathGeometry.Value = Geometry.Combine(item1PathGeometry, item2PathGeometry, mode, null);
            }
            combine.Left.Value = combine.PathGeometry.Value.Bounds.Left;
            combine.Top.Value = combine.PathGeometry.Value.Bounds.Top;
            combine.Width.Value = combine.PathGeometry.Value.Bounds.Width;
            combine.Height.Value = combine.PathGeometry.Value.Bounds.Height;
            Add(combine);
        }

        private static void CastToLetterAndSetTransform(SelectableDesignerItemViewModelBase item1, SelectableDesignerItemViewModelBase item2, PathGeometry item1PathGeometry, PathGeometry item2PathGeometry)
        {
            InternalCastToLetterAndSetTransform(item1, item1PathGeometry);
            InternalCastToLetterVerticalAndSetTransform(item1, item1PathGeometry);
            InternalCastToLetterAndSetTransform(item2, item2PathGeometry);
            InternalCastToLetterVerticalAndSetTransform(item2, item2PathGeometry);
            InternalCastToPolygonAndSetTransform(item1, item1PathGeometry);
            InternalCastToPolygonAndSetTransform(item2, item2PathGeometry);
        }

        private static void InternalCastToPolygonAndSetTransform(SelectableDesignerItemViewModelBase item, PathGeometry itemPathGeometry)
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
                itemPathGeometry.Transform = transformGroup;
            }
        }

        private static void InternalCastToLetterVerticalAndSetTransform(SelectableDesignerItemViewModelBase item, PathGeometry itemPathGeometry)
        {
            if (item is LetterVerticalDesignerItemViewModel)
            {
                var item_ = item as LetterVerticalDesignerItemViewModel;
                var transformGroup = new TransformGroup();
                transformGroup.Children.Add(new TranslateTransform(item_.Left.Value, item_.Top.Value));
                if (itemPathGeometry.Transform != null)
                    transformGroup.Children.Add(itemPathGeometry.Transform);
                itemPathGeometry.Transform = transformGroup;
                item_.CloseLetterSettingDialog();
            }
        }

        private static void InternalCastToLetterAndSetTransform(SelectableDesignerItemViewModelBase item, PathGeometry itemPathGeometry)
        {
            if (item is LetterDesignerItemViewModel)
            {
                var item_ = item as LetterDesignerItemViewModel;
                var transformGroup = new TransformGroup();
                transformGroup.Children.Add(new TranslateTransform(item_.Left.Value, item_.Top.Value));
                if (itemPathGeometry.Transform != null)
                    transformGroup.Children.Add(itemPathGeometry.Transform);
                itemPathGeometry.Transform = transformGroup;
                item_.CloseLetterSettingDialog();
            }
        }

        private bool CanExecuteUnion()
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

        private void ExecuteCopyCommand()
        {
            CopyToClipboard();
        }

        private bool CanExecuteCopy()
        {
            return SelectedItems.Value.Count() > 0;
        }

        private void ExecutePasteCommand()
        {
            var obj = Clipboard.GetDataObject();
            var clipboardDTO = obj.GetData(typeof(ClipboardDTO)) as ClipboardDTO;
            var root = XElement.Parse(clipboardDTO.Root);
            ObjectDeserializer.ReadCopyObjectsFromXML(this, root);
        }

        private bool CanExecutePaste()
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
                        x.RemoveItem(selectedItem);
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

        private bool CanExecuteCut()
        {
            return (SelectedLayers.Value.Count() > 0 && SelectedItems.Value.Count() > 0)
                || (SelectedLayers.Value.Count() > 0);
        }

        private void ExecuteSettingCommand()
        {
            IDialogResult result = null;
            var setting = new Models.Setting();
            setting.Width.Value = this.Width;
            setting.Height.Value = this.Height;
            setting.CanvasBackground.Value = this.CanvasBackground.Value;
            setting.EnablePointSnap.Value = this.EnablePointSnap.Value;
            setting.SnapPower.Value = (App.Current.MainWindow.DataContext as MainWindowViewModel).SnapPower.Value;
            dlgService.ShowDialog(nameof(Views.Setting), new DialogParameters() { { "Setting",  setting} }, ret => result = ret);
            if (result != null && result.Result == ButtonResult.OK)
            {
                var s = result.Parameters.GetValue<Models.Setting>("Setting");
                Width = s.Width.Value;
                Height = s.Height.Value;
                CanvasBackground.Value = s.CanvasBackground.Value;
                BackgroundItem.Value.FillColor.Value = CanvasBackground.Value;
                EnablePointSnap.Value = s.EnablePointSnap.Value;
                (App.Current.MainWindow.DataContext as MainWindowViewModel).SnapPower.Value = s.SnapPower.Value;
                BackgroundItem.Value.Width.Value = Width;
                BackgroundItem.Value.Height.Value = Height;
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

        public DiagramViewModel(MainWindowViewModel mainWindowViewModel, IDialogService dlgService, int width, int height)
            : this(mainWindowViewModel, width, height)
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
                      .ForEach(x => x.PushZIndex(newZIndex));
                item.ZIndex.Value = newZIndex;
                item.Owner = this;
                SelectedLayers.Value.First().AddItem(mainWindowViewModel, this, item);
            }
        }

        private void ExecuteRemoveItemCommand(object parameter)
        {
            if (parameter is SelectableDesignerItemViewModelBase)
            {
                SelectableDesignerItemViewModelBase item = (SelectableDesignerItemViewModelBase)parameter;
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
            SelectedLayers.Value.First().AddItem(mainWindowViewModel, this, item);
        }

        private void Remove(SelectableDesignerItemViewModelBase item)
        {
            Layers.ToList().ForEach(x => x.RemoveItem(item));
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
                }
                catch (Exception ex)
                {
                    FileName.Value = oldFileName;
                    MessageBox.Show(ex.StackTrace, ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
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
                    }
                    catch (Exception ex)
                    {
                        FileName.Value = "*";
                        MessageBox.Show(ex.StackTrace, ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                root.Save(FileName.Value);
            }
        }

        #endregion

        #region Load

        private void ExecuteLoadCommand()
        {
            var root = LoadSerializedDataFromFile();

            if (root == null)
            {
                return;
            }

            if (root.Element("Version") != null)
            {
                var version = new System.Version(root.Element("Version").Value);
                if (version > BGSXFileVersion)
                {
                    MessageBox.Show("ファイルが新しすぎて開けません。");
                    return;
                }
            }
            else
            {
                Trace.WriteLine("強制読み込みモードでファイルを読み込みます。このモードはVersion要素が見つからない時に実施されます。");
            }

            var mainwindowViewModel = (App.Current.MainWindow.DataContext as MainWindowViewModel);
            mainwindowViewModel.Recorder.BeginRecode();

            try
            {
                var configuration = root.Element("Configuration");
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "Width", int.Parse(configuration.Element("Width").Value));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "Height", int.Parse(configuration.Element("Height").Value));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "CanvasBackground.Value", (Color)ColorConverter.ConvertFromString(configuration.Element("CanvasBackground").Value));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(this, "EnablePointSnap.Value", bool.Parse(configuration.Element("EnablePointSnap").Value));
                mainwindowViewModel.Recorder.Current.ExecuteSetProperty(mainwindowViewModel, "SnapPower.Value", double.Parse(configuration.Element("SnapPower").Value));

                InitialSetting(mainwindowViewModel, false, false);

                ObjectDeserializer.ReadObjectsFromXML(this, root);
            }
            catch (Exception)
            {
                MessageBox.Show("このファイルは古すぎるか壊れているため開けません。", "読み込みエラー");
                FileName.Value = "*";
                return;
            }

            (App.Current.MainWindow.DataContext as MainWindowViewModel).Recorder.EndRecode("ExecuteLoadCommand complete");

            var layersViewModel = App.Current.MainWindow.GetChildOfType<Views.Layers>().DataContext as LayersViewModel;
            layersViewModel.InitializeHitTestVisible();
            Layers.First().IsSelected.Value = true;
        }

        private XElement LoadSerializedDataFromFile()
        {
            var openFile = new OpenFileDialog();
            openFile.Filter = "Designer Files (*.xml)|*.xml|All Files (*.*)|*.*";

            var oldFileName = FileName.Value;

            if (openFile.ShowDialog() == true)
            {
                try
                {
                    FileName.Value = openFile.FileName;
                    return XElement.Load(openFile.FileName);
                }
                catch (Exception e)
                {
                    FileName.Value = oldFileName;
                    MessageBox.Show(e.StackTrace, e.Message, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            return null;
        }

        #endregion //Load

        #region Grouping

        private void ExecuteGroupItemsCommand()
        {
            var items = (from item in SelectedItems.Value
                        where item.ParentID == Guid.Empty
                        select item).ToList();

            var rect = GetBoundingRectangle(items);

            var groupItem = new GroupItemViewModel();
            groupItem.Width.Value = rect.Width;
            groupItem.Height.Value = rect.Height;
            groupItem.Left.Value = rect.Left;
            groupItem.Top.Value = rect.Top;
            groupItem.IsHitTestVisible.Value = (App.Current.MainWindow.DataContext as MainWindowViewModel).ToolBarViewModel.CurrentHitTestVisibleState.Value;

            AddItemCommand.Execute(groupItem);

            var groupItemLayerItem = Layers.SelectMany(x => x.Children).First(x => (x as LayerItem).Item.Value == groupItem);

            var list = new List<Tuple<LayerItem, LayerTreeViewItemBase>>();

            foreach (var item in items)
            {
                LayerItem layerItem = Layers.SelectMany(x => x.Children).First(x => (x as LayerItem).Item.Value == item) as LayerItem;
                list.Add(new Tuple<LayerItem, LayerTreeViewItemBase>(layerItem, layerItem.Parent.Value));
                layerItem.Parent.Value = groupItemLayerItem;
                groupItemLayerItem.Children.Add(layerItem);
                groupItem.AddGroup(item);
                item.ParentID = groupItem.ID;
                item.EnableForSelection.Value = false;
            }

            var groupItems = from it in Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
                             where it is LayerItem && (it as LayerItem).Item.Value.ParentID == groupItem.ID
                             select it;

            var theMostForwardItem = (from it in groupItems
                                      orderby (it as LayerItem).Item.Value.ZIndex.Value descending
                                      select it).Take(1).SingleOrDefault();

            var sortList = (from it in Layers.SelectMany(x => x.Children)
                            where (it as LayerItem).Item.Value.ZIndex.Value < (theMostForwardItem as LayerItem).Item.Value.ZIndex.Value && (it as LayerItem).Item.Value.ID != groupItem.ID
                            orderby (it as LayerItem).Item.Value.ZIndex.Value descending
                            select it).ToList();

            var swapItems = (from it in groupItems
                             orderby (it as LayerItem).Item.Value.ZIndex.Value descending
                             select it).Skip(1);

            for (int i = 0; i < swapItems.Count(); ++i)
            {
                var it = swapItems.ElementAt(i);
                (it as LayerItem).Item.Value.ZIndex.Value = (theMostForwardItem as LayerItem).Item.Value.ZIndex.Value - i - 1;
            }

            var swapItemsCount = swapItems.Count();

            for (int i = 0, j = 0; i < sortList.Count(); ++i)
            {
                var it = sortList.ElementAt(i);
                if ((it as LayerItem).Item.Value.ParentID == groupItem.ID)
                {
                    j++;
                    continue;
                }
                (it as LayerItem).Item.Value.ZIndex.Value = (theMostForwardItem as LayerItem).Item.Value.ZIndex.Value - swapItemsCount - (i - j) - 1;
            }

            groupItem.ZIndex.Value = (theMostForwardItem as LayerItem).Item.Value.ZIndex.Value + 1;

            var adds = from item in Layers.SelectMany(x => x.Children)
                       where (item as LayerItem).Item.Value.ID != groupItem.ID && (item as LayerItem).Item.Value.ZIndex.Value >= groupItem.ZIndex.Value
                       select item;

            foreach (var add in adds)
            {
                (add as LayerItem).Item.Value.ZIndex.Value += 1;
            }

            foreach (var item in list)
            {
                LayerItem layerItem = item.Item1;
                LayerTreeViewItemBase parent = item.Item2;
                parent.RemoveChildren(layerItem);
            }

            groupItem.SelectItemCommand.Execute(true);
        }

        private void Remove(LayerItem layerItem)
        {
            var layer = Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
                              .Where(x => x is LayerItem)
                              .First(x => (x as LayerItem) == layerItem)
                              .Parent.Value;
            layer.RemoveChildren(layerItem);
        }

        private bool CanExecuteGroup()
        {
            var items = from item in SelectedItems.Value
                        where item.ParentID == Guid.Empty
                        select item;
            return items.Count() > 1;
        }

        private void ExecuteUngroupItemsCommand()
        {
            var groups = from item in SelectedItems.Value
                         where item.ParentID == Guid.Empty
                         select item;

            foreach (var groupRoot in groups.ToList())
            {
                var children = (from child in Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
                               where child is LayerItem && (child as LayerItem).Item.Value.ParentID == groupRoot.ID
                               select child).ToList();

                foreach (var child in children)
                {
                    var layerItem = child as LayerItem;
                    layerItem.Item.Value.GroupDisposable.Dispose();
                    layerItem.Item.Value.ParentID = Guid.Empty;
                    layerItem.Item.Value.EnableForSelection.Value = true;
                    layerItem.Parent.Value = Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
                                                   .Where(x => x is LayerItem)
                                                   .First(x => (x as LayerItem).Item == (child as LayerItem).Item)
                                                   .Parent.Value
                                                   .Parent.Value;
                    layerItem.Parent.Value.AddChildren(layerItem);
                }

                groupRoot.Dispose();

                Remove(groupRoot);

                var groupZIndex = groupRoot.ZIndex.Value;

                var it = from item in Layers.SelectMany(x => x.Children)
                         where (item as LayerItem).Item.Value.ZIndex.Value > groupZIndex
                         select item;

                foreach (var x in it)
                {
                    (x as LayerItem).Item.Value.ZIndex.Value -= 1;
                }
            }
        }

        private bool CanExecuteUngroup()
        {
            var items = from item in SelectedItems.Value.OfType<GroupItemViewModel>()
                        select item;
            return items.Count() > 0;
        }

        #endregion //Grouping

        #region Ordering

        private void ExecuteBringForwardCommand()
        {
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
                        ordered.ElementAt(i).ZIndex.Value = newIndex;

                        var children = from item in Layers.SelectMany(xx => xx.Children)
                                       where (item as LayerItem).Item.Value.ParentID == ordered.ElementAt(i).ID
                                       orderby (item as LayerItem).Item.Value.ZIndex.Value descending
                                       select item;

                        int youngestChildrenZIndex = 0;

                        for (int j = 0; j < children.Count(); ++j)
                        {
                            var child = children.ElementAt(j);
                            youngestChildrenZIndex = (child as LayerItem).Item.Value.ZIndex.Value = newIndex - j - 1;
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
                            (z.ElementAt(j) as LayerItem).Item.Value.ZIndex.Value = j;
                        }
                    }
                    else
                    {
                        ordered.ElementAt(i).ZIndex.Value = newIndex;
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
                                        (child as LayerItem).Item.Value.ZIndex.Value -= 1;
                                    }

                                    (item as LayerItem).Item.Value.ZIndex.Value = currentIndex + children.Count();
                                }
                                else
                                {
                                    (item as LayerItem).Item.Value.ZIndex.Value = currentIndex;
                                }
                                break;
                            }
                        }
                    }
                }
            }

            Sort(Layers);
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

        private void ExecuteSendBackwardCommand()
        {
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

                        ordered.ElementAt(i).ZIndex.Value = newIndex;

                        int youngestChildrenZIndex = 0;

                        for (int j = 0; j < children.Count(); ++j)
                        {
                            var child = children.ElementAt(j);
                            youngestChildrenZIndex = (child as LayerItem).Item.Value.ZIndex.Value = newIndex - j - 1;
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
                            (elm as LayerItem).Item.Value.ZIndex.Value = Layers.SelectMany(xx => xx.Children).Count() - j - 1;
                        }
                    }
                    else
                    {
                        ordered.ElementAt(i).ZIndex.Value = newIndex;
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
                                        (child as LayerItem).Item.Value.ZIndex.Value += 1;
                                    }

                                    var parent = (from it in Layers.SelectMany(x => x.Children)
                                                  where (it as LayerItem).Item.Value.ID == (item as LayerItem).Item.Value.ParentID
                                                  select it).Single();

                                    (parent as LayerItem).Item.Value.ZIndex.Value = children.Max(x => (x as LayerItem).Item.Value.ZIndex.Value) + 1;
                                }
                                else
                                {
                                    (item as LayerItem).Item.Value.ZIndex.Value = currentIndex;
                                }
                                break;
                            }
                        }
                    }
                }
            }

            Sort(Layers);
        }

        private void ExecuteBringForegroundCommand()
        {
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
                    current.ZIndex.Value = newIndex;

                    if (current is GroupItemViewModel)
                    {
                        var children = from item in Layers.SelectMany(x => x.Children)
                                       where (item as LayerItem).Item.Value.ParentID == current.ID
                                       orderby (item as LayerItem).Item.Value.ZIndex.Value descending
                                       select item;

                        for (int j = 0; j < children.Count(); ++j)
                        {
                            var child = children.ElementAt(j);
                            (child as LayerItem).Item.Value.ZIndex.Value = current.ZIndex.Value - j - 1;
                        }

                        var minValue = children.Min(x => (x as LayerItem).Item.Value.ZIndex.Value);

                        var other = (from item in Layers.SelectMany(x => x.Children)
                                     where (item as LayerItem).Item.Value.ParentID != current.ID && (item as LayerItem).Item.Value.ID != current.ID
                                     orderby (item as LayerItem).Item.Value.ZIndex.Value descending
                                     select item).ToList();

                        for (int j = 0; j < other.Count(); ++j)
                        {
                            var item = other.ElementAt(j);
                            (item as LayerItem).Item.Value.ZIndex.Value = minValue - j - 1;
                        }
                    }
                    else
                    {
                        var exists = Layers.SelectMany(x => x.Children).Where(item => (item as LayerItem).Item.Value.ZIndex.Value <= newIndex && (item as LayerItem).Item.Value.ZIndex.Value > oldCurrentIndex);

                        foreach (var item in exists)
                        {
                            if ((item as LayerItem).Item.Value != current)
                            {
                                (item as LayerItem).Item.Value.ZIndex.Value -= 1;
                            }
                        }
                    }
                }
            }

            Sort(Layers);
        }

        private void ExecuteSendBackgroundCommand()
        {
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
                    current.ZIndex.Value = newIndex;

                    if (current is GroupItemViewModel)
                    {
                        var children = (from item in Layers.SelectMany(x => x.Children)
                                        where (item as LayerItem).Item.Value.ParentID == current.ID
                                        orderby (item as LayerItem).Item.Value.ZIndex.Value descending
                                        select item).ToList();

                        for (int j = 0; j < children.Count(); ++j)
                        {
                            var child = children.ElementAt(j);
                            (child as LayerItem).Item.Value.ZIndex.Value = current.ZIndex.Value - j - 1;
                        }

                        var other = (from item in Layers.SelectMany(x => x.Children)
                                     where (item as LayerItem).Item.Value.ParentID != current.ID && (item as LayerItem).Item.Value.ID != current.ID
                                     orderby (item as LayerItem).Item.Value.ZIndex.Value descending
                                     select item).ToList();

                        var maxValue = Layers.SelectMany(x => x.Children).Count() - 1;

                        for (int j = 0; j < other.Count(); ++j)
                        {
                            var item = other.ElementAt(j);
                            (item as LayerItem).Item.Value.ZIndex.Value = maxValue - j;
                        }
                    }
                    else
                    {
                        var exists = Layers.SelectMany(x => x.Children).Where(item => (item as LayerItem).Item.Value.ZIndex.Value >= newIndex && (item as LayerItem).Item.Value.ZIndex.Value < oldCurrentIndex);

                        foreach (var item in exists)
                        {
                            if ((item as LayerItem).Item.Value != current)
                            {
                                (item as LayerItem).Item.Value.ZIndex.Value += 1;
                            }
                        }
                    }
                }
            }

            Sort(Layers);
        }

        private bool CanExecuteOrder()
        {
            return SelectedItems.Value.Count() > 0;
        }

        #endregion //Ordering

        #region Alignment

        private void ExecuteAlignTopCommand()
        {
            if (SelectedItems.Value.Count() > 1)
            {
                var first = SelectedItems.Value.First();
                double top = GetTop(first);

                foreach (var item in SelectedItems.Value)
                {
                    double delta = top - GetTop(item);
                    SetTop(item, GetTop(item) + delta);
                }
            }
        }

        private void ExecuteAlignVerticalCenterCommand()
        {
            if (SelectedItems.Value.Count() > 1)
            {
                var first = SelectedItems.Value.First();
                double bottom = GetTop(first) + GetHeight(first) / 2;

                foreach (var item in SelectedItems.Value)
                {
                    double delta = bottom - (GetTop(item) + GetHeight(item) / 2);
                    SetTop(item, GetTop(item) + delta);
                }
            }
        }

        private void ExecuteAlignBottomCommand()
        {
            if (SelectedItems.Value.Count() > 1)
            {
                var first = SelectedItems.Value.First();
                double bottom = GetTop(first) + GetHeight(first);

                foreach (var item in SelectedItems.Value)
                {
                    double delta = bottom - (GetTop(item) + GetHeight(item));
                    SetTop(item, GetTop(item) + delta);
                }
            }
        }

        private void ExecuteAlignLeftCommand()
        {
            if (SelectedItems.Value.Count() > 1)
            {
                var first = SelectedItems.Value.First();
                double left = GetLeft(first);

                foreach (var item in SelectedItems.Value)
                {
                    double delta = left - GetLeft(item);
                    SetLeft(item, GetLeft(item) + delta);
                }
            }
        }

        private void ExecuteAlignHorizontalCenterCommand()
        {
            if (SelectedItems.Value.Count() > 1)
            {
                var first = SelectedItems.Value.First();
                double center = GetLeft(first) + GetWidth(first) / 2;

                foreach (var item in SelectedItems.Value)
                {
                    double delta = center - (GetLeft(item) + GetWidth(item) / 2);
                    SetLeft(item, GetLeft(item) + delta);
                }
            }
        }

        private void ExecuteAlignRightCommand()
        {
            if (SelectedItems.Value.Count() > 1)
            {
                var first = SelectedItems.Value.First();
                double right = GetLeft(first) + GetWidth(first);

                foreach (var item in SelectedItems.Value)
                {
                    double delta = right - (GetLeft(item) + GetWidth(item));
                    SetLeft(item, GetLeft(item) + delta);
                }
            }
        }

        private void ExecuteDistributeHorizontalCommand()
        {
            var selectedItems = from item in SelectedItems.Value
                                let itemLeft = GetLeft(item)
                                orderby itemLeft
                                select item;

            if (selectedItems.Count() > 1)
            {
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
            }
        }

        private void ExecuteDistributeVerticalCommand()
        {
            var selectedItems = from item in SelectedItems.Value
                                let itemTop = GetTop(item)
                                orderby itemTop
                                select item;

            if (selectedItems.Count() > 1)
            {
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
            }
        }

        private bool CanExecuteAlign()
        {
            return SelectedItems.Value.Count() > 1;
        }

        private bool CanExecuteDistribute()
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
                di.Left.Value = value;
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
                di.Top.Value = value;
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

        private void ExecuteUniformWidthCommand()
        {
            var selectedItems = SelectedItems.Value.OfType<DesignerItemViewModelBase>();
            if (selectedItems.Count() > 1)
            {
                var first = selectedItems.First();
                double width = first.Width.Value;

                foreach (var item in selectedItems)
                {
                    double delta = width - item.Width.Value;
                    item.Width.Value += delta;
                }
            }
        }

        private void ExecuteUniformHeightCommand()
        {
            var selectedItems = SelectedItems.Value.OfType<DesignerItemViewModelBase>();
            if (selectedItems.Count() > 1)
            {
                var first = selectedItems.First();
                double height = first.Height.Value;

                foreach (var item in selectedItems)
                {
                    double delta = height - item.Height.Value;
                    item.Height.Value += delta;
                }
            }
        }

        private bool CanExecuteUniform()
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

            var selectedConnectors = from item in items.OfType<ConnectorBaseViewModel>()
                                     orderby item.ZIndex.Value ascending
                                     select item;

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
                    parent.AddGroup(cloneGroup);
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
                if (parent != null)
                {
                    clone.ParentID = parent.ID;
                    clone.EnableForSelection.Value = false;
                    parent.AddGroup(clone);
                }
                oldNewList.Add(new Tuple<SelectableDesignerItemViewModelBase, SelectableDesignerItemViewModelBase>(item, clone));
                Add(clone);
            }
        }

        private void DuplicateConnector(List<Tuple<SelectableDesignerItemViewModelBase, SelectableDesignerItemViewModelBase>> oldNewList, ConnectorBaseViewModel connector, GroupItemViewModel groupItem = null)
        {
            var clone = connector.Clone() as ConnectorBaseViewModel;
            clone.ZIndex.Value = Layers.SelectMany(x => x.Children).Count();
            if (groupItem != null)
            {
                clone.ParentID = groupItem.ID;
                clone.EnableForSelection.Value = false;
                groupItem.AddGroup(clone);
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
                groupItem.AddGroup(clone);
            }
            Add(clone);
        }

        private bool CanExecuteDuplicate()
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
