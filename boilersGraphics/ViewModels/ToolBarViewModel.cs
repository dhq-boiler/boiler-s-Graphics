using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.Models;
using boilersGraphics.Properties;
using boilersGraphics.Views;
using boilersGraphics.Views.Behaviors;
using Microsoft.Win32;
using Microsoft.Xaml.Behaviors;
using NLog;
using Prism.Commands;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace boilersGraphics.ViewModels
{
    public class ToolBarViewModel
    {
        private IDialogService dlgService = null;
        public ReactiveCollection<ToolItemData> ToolItems { get; } = new ReactiveCollection<ToolItemData>();
        public ReactiveCollection<ToolItemData> ToolItems2 { get; } = new ReactiveCollection<ToolItemData>();

        public BehaviorCollection Behaviors { get { return Interaction.GetBehaviors(App.GetCurrentApp().MainWindow.GetChildOfType<DesignerCanvas>()); } }

        public ReactivePropertySlim<bool> CurrentHitTestVisibleState { get; } = new ReactivePropertySlim<bool>();

        public DeselectBehavior DeselectBehavior { get; } = new DeselectBehavior();
        public LassoBehavior RubberbandBehavior { get; } = new LassoBehavior();
        public NDrawStraightLineBehavior NDrawStraightLineBehavior { get; } = new NDrawStraightLineBehavior();
        public NDrawRectangleBehavior NDrawRectangleBehavior { get; } = new NDrawRectangleBehavior();
        public NDrawEllipseBehavior NDrawEllipseBehavior { get; } = new NDrawEllipseBehavior();
        public PictureBehavior PictureBehavior { get; private set; }
        public LetterBehavior LetterBehavior { get; } = new LetterBehavior();
        public LetterVerticalBehavior LetterVerticalBehavior { get; } = new LetterVerticalBehavior();
        public NDrawPolygonBehavior NDrawPolygonBehavior { get; private set; }
        public NDrawBezierCurveBehavior NDrawBezierCurveBehavior { get; } = new NDrawBezierCurveBehavior();
        public SetSnapPointBehavior SetSnapPointBehavior { get; } = new SetSnapPointBehavior();
        public BrushBehavior BrushBehavior { get; private set; }
        public EraserBehavior EraserBehavior { get; } = new EraserBehavior();
        public SliceBehavior SliceBehavior { get; private set; }
        public NDrawPieBehavior NDrawPieBehavior { get; } = new NDrawPieBehavior();
        public DropperBehavior DropperBehavior { get; } = new DropperBehavior();
        public CanvasModifierBehavior CanvasModifierBehavior { get; } = new CanvasModifierBehavior();

        public NDrawPolyBezierBehavior PolyBezierBehavior { get; } = new NDrawPolyBezierBehavior();

        public ToolBarViewModel(IDialogService dialogService, MainWindowViewModel mainWindowViewModel)
        {
            this.dlgService = dialogService;
            InitializeToolItems(dialogService);
            InitializeToolItems2(mainWindowViewModel);
        }

        public void InitializeToolItems(IDialogService dialogService)
        {
            SliceBehavior = new SliceBehavior(dialogService);
            ToolItems.Add(new ToolItemData("pointer", "pack://application:,,,/Assets/img/pointer.png", Resources.Tool_Pointer, new DelegateCommand(() =>
            {
                var mainWindowViewModel = (App.Current.MainWindow.DataContext as MainWindowViewModel);
                mainWindowViewModel.ClearCurrentOperationAndDetails();
                Behaviors.Clear();
                if (!Behaviors.Contains(DeselectBehavior))
                {
                    Behaviors.Add(DeselectBehavior);
                }
                ChangeHitTestToEnable();
                SelectOneToolItem("pointer");
            })));
            ToolItems.Add(new ToolItemData("lasso", "pack://application:,,,/Assets/img/rubberband.png", Resources.Tool_Lasso, new DelegateCommand(() =>
            {
                var mainWindowViewModel = (App.Current.MainWindow.DataContext as MainWindowViewModel);
                mainWindowViewModel.ClearCurrentOperationAndDetails();
                Behaviors.Clear();
                if (!Behaviors.Contains(RubberbandBehavior))
                {
                    Behaviors.Add(RubberbandBehavior);
                }
                ChangeHitTestToDisable();
                SelectOneToolItem("lasso");
            })));
            ToolItems.Add(new ToolItemData("straightline", "pack://application:,,,/Assets/img/straightline.png", Resources.Tool_StraightLine, new DelegateCommand(() =>
            {
                var mainWindowViewModel = (App.Current.MainWindow.DataContext as MainWindowViewModel);
                mainWindowViewModel.ClearCurrentOperationAndDetails();
                Behaviors.Clear();
                if (!Behaviors.Contains(NDrawStraightLineBehavior))
                {
                    Behaviors.Add(NDrawStraightLineBehavior);
                }
                ChangeHitTestToDisable();
                SelectOneToolItem("straightline");
            })));
            ToolItems.Add(new ToolItemData("rectangle", "pack://application:,,,/Assets/img/rectangle.png", Resources.Tool_Rectangle, new DelegateCommand(() =>
            {
                var mainWindowViewModel = (App.Current.MainWindow.DataContext as MainWindowViewModel);
                mainWindowViewModel.ClearCurrentOperationAndDetails();
                Behaviors.Clear();
                if (!Behaviors.Contains(NDrawRectangleBehavior))
                {
                    Behaviors.Add(NDrawRectangleBehavior);
                }
                ChangeHitTestToDisable();
                SelectOneToolItem("rectangle");
            })));
            ToolItems.Add(new ToolItemData("ellipse", "pack://application:,,,/Assets/img/ellipse.png", Resources.Tool_Ellipse, new DelegateCommand(() =>
            {
                var mainWindowViewModel = (App.Current.MainWindow.DataContext as MainWindowViewModel);
                mainWindowViewModel.ClearCurrentOperationAndDetails();
                Behaviors.Clear();
                if (!Behaviors.Contains(NDrawEllipseBehavior))
                {
                    Behaviors.Add(NDrawEllipseBehavior);
                }
                ChangeHitTestToDisable();
                SelectOneToolItem("ellipse");
            })));
            ToolItems.Add(new ToolItemData("picture", "pack://application:,,,/Assets/img/Picture.png", Resources.Tool_Picture, new DelegateCommand(() =>
            {
                var mainWindowViewModel = (App.Current.MainWindow.DataContext as MainWindowViewModel);
                mainWindowViewModel.ClearCurrentOperationAndDetails();
                var dialog = new OpenFileDialog();
                dialog.Multiselect = false;
                dialog.Filter = boilersGraphics.Properties.Resources.String_SupportImage;
                if (dialog.ShowDialog() == true)
                {
                    var bitmap = BitmapFactory.FromStream(new FileStream(dialog.FileName, FileMode.Open, FileAccess.Read));
                    PictureBehavior = new PictureBehavior(dialog.FileName, bitmap.Width, bitmap.Height);
                    Behaviors.Clear();
                    if (!Behaviors.Contains(PictureBehavior))
                    {
                        Behaviors.Add(PictureBehavior);
                    }
                    ChangeHitTestToDisable();
                    SelectOneToolItem("picture");
                }
            })));
            ToolItems.Add(new ToolItemData("letter", "pack://application:,,,/Assets/img/A.png", Resources.Tool_Lettering, new DelegateCommand(() =>
            {
                var mainWindowViewModel = (App.Current.MainWindow.DataContext as MainWindowViewModel);
                mainWindowViewModel.ClearCurrentOperationAndDetails();
                Behaviors.Clear();
                if (!Behaviors.Contains(LetterBehavior))
                {
                    Behaviors.Add(LetterBehavior);
                }
                ChangeHitTestToDisable();
                SelectOneToolItem("letter");
            })));
            ToolItems.Add(new ToolItemData("letter-vertical", "pack://application:,,,/Assets/img/A_Vertical.png", Resources.Tool_VerticalLettering, new DelegateCommand(() =>
            {
                var mainWindowViewModel = (App.Current.MainWindow.DataContext as MainWindowViewModel);
                mainWindowViewModel.ClearCurrentOperationAndDetails();
                Behaviors.Clear();
                if (!Behaviors.Contains(LetterVerticalBehavior))
                {
                    Behaviors.Add(LetterVerticalBehavior);
                }
                ChangeHitTestToDisable();
                SelectOneToolItem("letter-vertical");
            })));
            ToolItems.Add(new ToolItemData("polygon", "pack://application:,,,/Assets/img/polygon.png", Resources.Tool_Polygon, new DelegateCommand(() =>
            {
                var mainWindowViewModel = (App.Current.MainWindow.DataContext as MainWindowViewModel);
                mainWindowViewModel.ClearCurrentOperationAndDetails();
                IDialogResult result = null;
                this.dlgService.ShowDialog(nameof(PolygonSetting), ret => result = ret);
                if (result != null && result.Result == ButtonResult.OK)
                {
                    var corners = result.Parameters.GetValue<ObservableCollection<Corner>>("Corners");
                    var data = result.Parameters.GetValue<string>("Data");
                    NDrawPolygonBehavior = new NDrawPolygonBehavior(corners, data);
                    Behaviors.Clear();
                    if (!Behaviors.Contains(NDrawPolygonBehavior))
                    {
                        Behaviors.Add(NDrawPolygonBehavior);
                    }
                    ChangeHitTestToDisable();
                    SelectOneToolItem("polygon");
                }
            })));
            ToolItems.Add(new ToolItemData("bezier", "pack://application:,,,/Assets/img/BezierCurve.png", Resources.Tool_BezierCurve, new DelegateCommand(() =>
            {
                var mainWindowViewModel = (App.Current.MainWindow.DataContext as MainWindowViewModel);
                mainWindowViewModel.ClearCurrentOperationAndDetails();
                Behaviors.Clear();
                if (!Behaviors.Contains(NDrawBezierCurveBehavior))
                {
                    Behaviors.Add(NDrawBezierCurveBehavior);
                }
                ChangeHitTestToDisable();
                SelectOneToolItem("bezier");
            })));
            ToolItems.Add(new ToolItemData("snappoint", "pack://application:,,,/Assets/img/SnapPoint.png", Resources.Tool_SnapPoint, new DelegateCommand(() =>
            {
                var mainWindowViewModel = (App.Current.MainWindow.DataContext as MainWindowViewModel);
                mainWindowViewModel.ClearCurrentOperationAndDetails();
                Behaviors.Clear();
                if (!Behaviors.Contains(SetSnapPointBehavior))
                {
                    Behaviors.Add(SetSnapPointBehavior);
                }
                ChangeHitTestToDisable();
                SelectOneToolItem("snappoint");
            })));
            ToolItems.Add(new ToolItemData("brush", "pack://application:,,,/Assets/img/brush.png", Resources.Tool_Brush, new DelegateCommand(() =>
            {
                var mainWindowViewModel = (App.Current.MainWindow.DataContext as MainWindowViewModel);
                mainWindowViewModel.ClearCurrentOperationAndDetails();
                BrushBehavior = new BrushBehavior(dlgService);
                BrushBehavior.CurrentBrush.OpenThicknessDialog();
                Behaviors.Clear();
                if (!Behaviors.Contains(BrushBehavior))
                {
                    Behaviors.Add(BrushBehavior);
                }
                ChangeHitTestToDisable();
                SelectOneToolItem("brush");
            })));
            ToolItems.Add(new ToolItemData("eraser", "pack://application:,,,/Assets/img/eraser.png", Resources.Tool_Eraser, new DelegateCommand(() =>
            {
                var mainWindowViewModel = (App.Current.MainWindow.DataContext as MainWindowViewModel);
                mainWindowViewModel.ClearCurrentOperationAndDetails();
                Behaviors.Clear();
                if (!Behaviors.Contains(EraserBehavior))
                {
                    Behaviors.Add(EraserBehavior);
                }
                ChangeHitTestToDisable();
                SelectOneToolItem("eraser");
            })));
            ToolItems.Add(new ToolItemData("slice", "pack://application:,,,/Assets/img/slice_tool.png", Resources.Tool_Slice, new DelegateCommand(() =>
            {
                var mainWindowViewModel = (App.Current.MainWindow.DataContext as MainWindowViewModel);
                mainWindowViewModel.ClearCurrentOperationAndDetails();
                Behaviors.Clear();
                if (!Behaviors.Contains(SliceBehavior))
                {
                    Behaviors.Add(SliceBehavior);
                }
                ChangeHitTestToDisable();
                SelectOneToolItem("slice");
            })));
            ToolItems.Add(new ToolItemData("polybezier", "pack://application:,,,/Assets/img/poly_bezier.png", Resources.Tool_FreeHand, new DelegateCommand(() =>
            {
                var mainWindowViewModel = (App.Current.MainWindow.DataContext as MainWindowViewModel);
                mainWindowViewModel.ClearCurrentOperationAndDetails();
                Behaviors.Clear();
                if (!Behaviors.Contains(PolyBezierBehavior))
                {
                    Behaviors.Add(PolyBezierBehavior);
                }
                ChangeHitTestToDisable();
                SelectOneToolItem("polybezier");
            })));
            ToolItems.Add(new ToolItemData("pie", "pack://application:,,,/Assets/img/FoldingFan.png", Resources.Tool_Pie, new DelegateCommand(() =>
            {
                var mainWindowViewModel = (App.Current.MainWindow.DataContext as MainWindowViewModel);
                mainWindowViewModel.ClearCurrentOperationAndDetails();
                Behaviors.Clear();
                if (!Behaviors.Contains(NDrawPieBehavior))
                {
                    Behaviors.Add(NDrawPieBehavior);
                }
                ChangeHitTestToDisable();
                SelectOneToolItem("pie");
                (App.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = Resources.String_Pie_DetermineCenterPoint;
            })));
            ToolItems.Add(new ToolItemData("dropper", "pack://application:,,,/Assets/img/dropper.png", null, new DelegateCommand(() =>
            {
                var mainWindowViewModel = (App.Current.MainWindow.DataContext as MainWindowViewModel);
                mainWindowViewModel.ClearCurrentOperationAndDetails();
                Behaviors.Clear();
                if (!Behaviors.Contains(DropperBehavior))
                {
                    Behaviors.Add(DropperBehavior);
                }
                ChangeHitTestToDisable();
                SelectOneToolItem("dropper");
            })));
            ToolItems.Add(new ToolItemData("canvasModifier", null, null, new DelegateCommand(() =>
            {
                var mainWindowViewModel = (App.Current.MainWindow.DataContext as MainWindowViewModel);
                mainWindowViewModel.ClearCurrentOperationAndDetails();
                Behaviors.Clear();
                if (!Behaviors.Contains(CanvasModifierBehavior))
                {
                    Behaviors.Add(CanvasModifierBehavior);
                }
                SelectOneToolItem("canvasModifier");
            })));
        }

        private void InitializeToolItems2(MainWindowViewModel mainWindowViewModel)
        {
            var toolItemData = new ToolItemData("minimap", "pack://application:,,,/Assets/img/minimap.png", Resources.MenuItem_MiniMap, new DelegateCommand(() =>
            {
                mainWindowViewModel.DiagramViewModel.EnableMiniMap.Value = !mainWindowViewModel.DiagramViewModel.EnableMiniMap.Value;
            }));
            ToolItems2.Add(toolItemData);
            toolItemData.IsChecked = mainWindowViewModel.DiagramViewModel.EnableMiniMap.Value;
            
            toolItemData = new ToolItemData("combine", "pack://application:,,,/Assets/img/icon_Combine_union.png", Resources.MenuItem_Combine, new DelegateCommand(() =>
            {
                mainWindowViewModel.DiagramViewModel.EnableCombine.Value = !mainWindowViewModel.DiagramViewModel.EnableCombine.Value;
            }));
            ToolItems2.Add(toolItemData);
            toolItemData.IsChecked = mainWindowViewModel.DiagramViewModel.EnableCombine.Value;
            
            toolItemData = new ToolItemData("layers", "pack://application:,,,/Assets/img/icon_Layers.png", Resources.MenuItem_Layering, new DelegateCommand(() =>
            {
                mainWindowViewModel.DiagramViewModel.EnableLayers.Value = !mainWindowViewModel.DiagramViewModel.EnableLayers.Value;
            }));
            ToolItems2.Add(toolItemData);
            toolItemData.IsChecked = mainWindowViewModel.DiagramViewModel.EnableLayers.Value;

            toolItemData = new ToolItemData("workHistory", "pack://application:,,,/Assets/img/work_history.png", Resources.MenuItem_WorkHistory, new DelegateCommand(() =>
            {
                mainWindowViewModel.DiagramViewModel.EnableWorkHistory.Value = !mainWindowViewModel.DiagramViewModel.EnableWorkHistory.Value;
            }));
            ToolItems2.Add(toolItemData);
            toolItemData.IsChecked = mainWindowViewModel.DiagramViewModel.EnableWorkHistory.Value;
        }

        public void FinalizeToolItems()
        {
            ToolItems.Clear();
        }

        public void ReinitializeToolItems()
        {
            FinalizeToolItems();
            InitializeToolItems(dlgService);
        }

        private void ChangeHitTestToDisable()
        {
            var diagramViewModel = (App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel;
            diagramViewModel.AllItems.Value.ToList().ForEach(x => x.IsHitTestVisible.Value = false);
            CurrentHitTestVisibleState.Value = false;
        }

        private void ChangeHitTestToEnable()
        {
            var diagramViewModel = (App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel;
            diagramViewModel.SelectedLayers.Value.ToList().ForEach(x => 
                (x as Layer).Children.ToList().ForEach(y =>
                {
                    var layerItem = y as LayerItem;
                    layerItem.Item.Value.IsHitTestVisible.Value = true;
                    LogManager.GetCurrentClassLogger().Trace($"{layerItem.Name.Value}.IsHitTestVisible={layerItem.Item.Value.IsHitTestVisible.Value}");
                })
            );
            CurrentHitTestVisibleState.Value = true;
        }

        private void SelectOneToolItem(string toolName)
        {
            var toolItem = ToolItems.Where(i => i.Name.Value == toolName).Single();
            toolItem.IsChecked = true;

            ToolItems.Where(i => i.Name.Value != toolName).ToList().ForEach(i => i.IsChecked = false);

            switch (toolName)
            {
                case "pointer":
                case "lasso":
                case "straightline":
                case "rectangle":
                case "ellipse":
                case "picture":
                case "letter":
                case "letter-vertical":
                case "polygon":
                case "bezier":
                case "snappoint":
                case "brush":
                case "eraser":
                case "slice":
                case "polybezier":
                case "pie":
                    (App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel.ContextMenuVisibility.Value = Visibility.Visible;
                    break;
                case "dropper":
                    (App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel.ContextMenuVisibility.Value = Visibility.Collapsed;
                    break;
            }
        }
    }
}
