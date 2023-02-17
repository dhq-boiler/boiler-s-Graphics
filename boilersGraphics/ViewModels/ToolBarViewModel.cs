using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
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

namespace boilersGraphics.ViewModels;

public class ToolBarViewModel
{
    private readonly IDialogService dlgService;

    public ToolBarViewModel(IDialogService dialogService)
    {
        dlgService = dialogService;
        InitializeToolItems(dialogService);
        InitializeToolItems2();
    }

    public ReactiveCollection<ToolItemData> ToolItems { get; } = new();
    public ReactiveCollection<ToolItemData> ToolItems2 { get; } = new();

    public BehaviorCollection Behaviors =>
        Interaction.GetBehaviors(App.GetCurrentApp().MainWindow.GetChildOfType<DesignerCanvas>());

    public ReactivePropertySlim<bool> CurrentHitTestVisibleState { get; } = new();

    public DeselectBehavior DeselectBehavior { get; } = new();
    public LassoBehavior RubberbandBehavior { get; } = new();
    public NDrawStraightLineBehavior NDrawStraightLineBehavior { get; } = new();
    public NDrawRectangleBehavior NDrawRectangleBehavior { get; } = new();
    public NDrawEllipseBehavior NDrawEllipseBehavior { get; } = new();
    public PictureBehavior PictureBehavior { get; private set; }
    public LetterBehavior LetterBehavior { get; } = new();
    public LetterVerticalBehavior LetterVerticalBehavior { get; } = new();
    public NDrawPolygonBehavior NDrawPolygonBehavior { get; private set; }
    public NDrawBezierCurveBehavior NDrawBezierCurveBehavior { get; } = new();
    public SetSnapPointBehavior SetSnapPointBehavior { get; } = new();
    public BrushBehavior BrushBehavior { get; private set; }
    public EraserBehavior EraserBehavior { get; } = new();
    public SliceBehavior SliceBehavior { get; private set; }
    public NDrawPieBehavior NDrawPieBehavior { get; } = new();
    public DropperBehavior DropperBehavior { get; } = new();
    public CanvasModifierBehavior CanvasModifierBehavior { get; } = new();
    public MosaicBehavior MosaicBehavior { get; } = new();
    public BlurBehavior BlurBehavior { get; } = new();
    public ColorCorrectBehavior ColorCorrectBehavior { get; } = new();

    public NDrawPolyBezierBehavior PolyBezierBehavior { get; } = new();

    public void InitializeToolItems(IDialogService dialogService)
    {
        SliceBehavior = new SliceBehavior(dialogService);
        ToolItems.Add(new ToolItemData("pointer", "pack://application:,,,/Assets/img/pointer.png",
            Resources.Tool_Pointer, new DelegateCommand(() =>
            {
                MainWindowViewModel.Instance.ClearCurrentOperationAndDetails();
                Behaviors.Clear();
                if (!Behaviors.Contains(DeselectBehavior)) Behaviors.Add(DeselectBehavior);
                ChangeHitTestToEnable();
                SelectOneToolItem("pointer");
            })));
        ToolItems.Add(new ToolItemData("lasso", "pack://application:,,,/Assets/img/rubberband_dark.png",
            Resources.Tool_Lasso, new DelegateCommand(() =>
            {
                MainWindowViewModel.Instance.ClearCurrentOperationAndDetails();
                Behaviors.Clear();
                if (!Behaviors.Contains(RubberbandBehavior)) Behaviors.Add(RubberbandBehavior);
                ChangeHitTestToDisable();
                SelectOneToolItem("lasso");
            })));
        ToolItems.Add(new ToolItemData("straightline", "pack://application:,,,/Assets/img/straightline_dark.png",
            Resources.Tool_StraightLine, new DelegateCommand(() =>
            {
                MainWindowViewModel.Instance.ClearCurrentOperationAndDetails();
                Behaviors.Clear();
                if (!Behaviors.Contains(NDrawStraightLineBehavior)) Behaviors.Add(NDrawStraightLineBehavior);
                ChangeHitTestToDisable();
                SelectOneToolItem("straightline");
            })));
        ToolItems.Add(new ToolItemData("bezier", "pack://application:,,,/Assets/img/BezierCurve_dark.png",
            Resources.Tool_BezierCurve, new DelegateCommand(() =>
            {
                MainWindowViewModel.Instance.ClearCurrentOperationAndDetails();
                Behaviors.Clear();
                if (!Behaviors.Contains(NDrawBezierCurveBehavior)) Behaviors.Add(NDrawBezierCurveBehavior);
                ChangeHitTestToDisable();
                SelectOneToolItem("bezier");
            })));
        ToolItems.Add(new ToolItemData("polybezier", "pack://application:,,,/Assets/img/poly_bezier_dark.png",
            Resources.Tool_FreeHand, new DelegateCommand(() =>
            {
                MainWindowViewModel.Instance.ClearCurrentOperationAndDetails();
                Behaviors.Clear();
                if (!Behaviors.Contains(PolyBezierBehavior)) Behaviors.Add(PolyBezierBehavior);
                ChangeHitTestToDisable();
                SelectOneToolItem("polybezier");
            })));
        ToolItems.Add(new ToolItemData("rectangle", "pack://application:,,,/Assets/img/rectangle_dark.png",
            Resources.Tool_Rectangle, new DelegateCommand(() =>
            {
                MainWindowViewModel.Instance.ClearCurrentOperationAndDetails();
                Behaviors.Clear();
                if (!Behaviors.Contains(NDrawRectangleBehavior)) Behaviors.Add(NDrawRectangleBehavior);
                ChangeHitTestToDisable();
                SelectOneToolItem("rectangle");
            })));
        ToolItems.Add(new ToolItemData("ellipse", "pack://application:,,,/Assets/img/ellipse_dark.png",
            Resources.Tool_Ellipse, new DelegateCommand(() =>
            {
                MainWindowViewModel.Instance.ClearCurrentOperationAndDetails();
                Behaviors.Clear();
                if (!Behaviors.Contains(NDrawEllipseBehavior)) Behaviors.Add(NDrawEllipseBehavior);
                ChangeHitTestToDisable();
                SelectOneToolItem("ellipse");
            })));
        ToolItems.Add(new ToolItemData("polygon", "pack://application:,,,/Assets/img/polygon_dark.png",
            Resources.Tool_Polygon, new DelegateCommand(() =>
            {
                MainWindowViewModel.Instance.ClearCurrentOperationAndDetails();
                IDialogResult result = null;
                dlgService.ShowDialog(nameof(PolygonSetting), ret => result = ret);
                if (result != null && result.Result == ButtonResult.OK)
                {
                    var corners = result.Parameters.GetValue<ObservableCollection<Corner>>("Corners");
                    var data = result.Parameters.GetValue<string>("Data");
                    NDrawPolygonBehavior = new NDrawPolygonBehavior(corners, data);
                    Behaviors.Clear();
                    if (!Behaviors.Contains(NDrawPolygonBehavior)) Behaviors.Add(NDrawPolygonBehavior);
                    ChangeHitTestToDisable();
                    SelectOneToolItem("polygon");
                }
            })));
        ToolItems.Add(new ToolItemData("pie", "pack://application:,,,/Assets/img/FoldingFan_dark.png",
            Resources.Tool_Pie, new DelegateCommand(() =>
            {
                MainWindowViewModel.Instance.ClearCurrentOperationAndDetails();
                Behaviors.Clear();
                if (!Behaviors.Contains(NDrawPieBehavior)) Behaviors.Add(NDrawPieBehavior);
                ChangeHitTestToDisable();
                SelectOneToolItem("pie");
                MainWindowViewModel.Instance.Details.Value =
                    Resources.String_Pie_DetermineCenterPoint;
            })));
        ToolItems.Add(new ToolItemData("picture", "pack://application:,,,/Assets/img/Picture_dark.png",
            Resources.Tool_Picture, new DelegateCommand(() =>
            {
                MainWindowViewModel.Instance.ClearCurrentOperationAndDetails();
                var dialog = new OpenFileDialog();
                dialog.Multiselect = false;
                dialog.Filter = Resources.String_SupportImage;
                if (dialog.ShowDialog() == true)
                {
                    var bitmap =
                        BitmapFactory.FromStream(new FileStream(dialog.FileName, FileMode.Open, FileAccess.Read));
                    PictureBehavior = new PictureBehavior(dialog.FileName, bitmap.Width, bitmap.Height);
                    Behaviors.Clear();
                    if (!Behaviors.Contains(PictureBehavior)) Behaviors.Add(PictureBehavior);
                    ChangeHitTestToDisable();
                    SelectOneToolItem("picture");
                }
            })));
        ToolItems.Add(new ToolItemData("mosaic", "pack://application:,,,/Assets/img/mosaic.png", Resources.Tool_Mosaic,
            new DelegateCommand(() =>
            {
                MainWindowViewModel.Instance.ClearCurrentOperationAndDetails();
                Behaviors.Clear();
                if (!Behaviors.Contains(MosaicBehavior)) Behaviors.Add(MosaicBehavior);
                ChangeHitTestToDisable();
                SelectOneToolItem("mosaic");
            })));
        ToolItems.Add(new ToolItemData("blur", "pack://application:,,,/Assets/img/GaussianFilter.png",
            Resources.Tool_GaussianFilter, new DelegateCommand(() =>
            {
                MainWindowViewModel.Instance.ClearCurrentOperationAndDetails();
                Behaviors.Clear();
                if (!Behaviors.Contains(BlurBehavior)) Behaviors.Add(BlurBehavior);
                ChangeHitTestToDisable();
                SelectOneToolItem("blur");
            })));
        ToolItems.Add(new ToolItemData("colorCorrect", "pack://application:,,,/Assets/img/colorCorrect.png",
            Resources.Tool_ColorCorrect, new DelegateCommand(() =>
            {
                MainWindowViewModel.Instance.ClearCurrentOperationAndDetails();
                Behaviors.Clear();
                if (!Behaviors.Contains(BlurBehavior)) Behaviors.Add(ColorCorrectBehavior);
                ChangeHitTestToDisable();
                SelectOneToolItem("colorCorrect");
            })));
        ToolItems.Add(new ToolItemData("letter", "pack://application:,,,/Assets/img/A_dark.png",
            Resources.Tool_Lettering, new DelegateCommand(() =>
            {
                MainWindowViewModel.Instance.ClearCurrentOperationAndDetails();
                Behaviors.Clear();
                if (!Behaviors.Contains(LetterBehavior)) Behaviors.Add(LetterBehavior);
                ChangeHitTestToDisable();
                SelectOneToolItem("letter");
            })));
        ToolItems.Add(new ToolItemData("letter-vertical", "pack://application:,,,/Assets/img/A_Vertical_dark.png",
            Resources.Tool_VerticalLettering, new DelegateCommand(() =>
            {
                MainWindowViewModel.Instance.ClearCurrentOperationAndDetails();
                Behaviors.Clear();
                if (!Behaviors.Contains(LetterVerticalBehavior)) Behaviors.Add(LetterVerticalBehavior);
                ChangeHitTestToDisable();
                SelectOneToolItem("letter-vertical");
            })));
        ToolItems.Add(new ToolItemData("brush", "pack://application:,,,/Assets/img/brush_dark.png",
            Resources.Tool_Brush, new DelegateCommand(() =>
            {
                MainWindowViewModel.Instance.ClearCurrentOperationAndDetails();
                BrushBehavior = new BrushBehavior(dlgService);
                BrushBehavior.CurrentBrush.OpenThicknessDialog();
                Behaviors.Clear();
                if (!Behaviors.Contains(BrushBehavior)) Behaviors.Add(BrushBehavior);
                ChangeHitTestToDisable();
                SelectOneToolItem("brush");
            })));
        ToolItems.Add(new ToolItemData("eraser", "pack://application:,,,/Assets/img/eraser_dark.png",
            Resources.Tool_Eraser, new DelegateCommand(() =>
            {
                MainWindowViewModel.Instance.ClearCurrentOperationAndDetails();
                Behaviors.Clear();
                if (!Behaviors.Contains(EraserBehavior)) Behaviors.Add(EraserBehavior);
                ChangeHitTestToDisable();
                SelectOneToolItem("eraser");
            })));
        ToolItems.Add(new ToolItemData("snappoint", "pack://application:,,,/Assets/img/SnapPoint_dark.png",
            Resources.Tool_SnapPoint, new DelegateCommand(() =>
            {
                MainWindowViewModel.Instance.ClearCurrentOperationAndDetails();
                Behaviors.Clear();
                if (!Behaviors.Contains(SetSnapPointBehavior)) Behaviors.Add(SetSnapPointBehavior);
                ChangeHitTestToDisable();
                SelectOneToolItem("snappoint");
            })));
        ToolItems.Add(new ToolItemData("slice", "pack://application:,,,/Assets/img/slice_tool_dark.png",
            Resources.Tool_Slice, new DelegateCommand(() =>
            {
                MainWindowViewModel.Instance.ClearCurrentOperationAndDetails();
                Behaviors.Clear();
                if (!Behaviors.Contains(SliceBehavior)) Behaviors.Add(SliceBehavior);
                ChangeHitTestToDisable();
                SelectOneToolItem("slice");
            })));
        ToolItems.Add(new ToolItemData("dropper", "pack://application:,,,/Assets/img/dropper_dark.png",
            Resources.Tool_Dropper, new DelegateCommand(() =>
            {
                MainWindowViewModel.Instance.ClearCurrentOperationAndDetails();
                Behaviors.Clear();
                if (!Behaviors.Contains(DropperBehavior)) Behaviors.Add(DropperBehavior);
                ChangeHitTestToDisable();
                SelectOneToolItem("dropper");
            })));
        ToolItems.Add(new ToolItemData("canvasModifier", "pack://application:,,,/Assets/img/icons8-canvas-64_dark.png",
            Resources.Tool_CanvasResize, new DelegateCommand(() =>
            {
                MainWindowViewModel.Instance.ClearCurrentOperationAndDetails();
                Behaviors.Clear();
                if (!Behaviors.Contains(CanvasModifierBehavior)) Behaviors.Add(CanvasModifierBehavior);
                SelectOneToolItem("canvasModifier");
            })));
    }

    private void InitializeToolItems2()
    {
        var toolItemData = new ToolItemData("minimap", "pack://application:,,,/Assets/img/minimap.png",
            Resources.MenuItem_MiniMap,
            new DelegateCommand(() =>
            {
                MainWindowViewModel.Instance.DiagramViewModel.EnableMiniMap.Value =
                    !MainWindowViewModel.Instance.DiagramViewModel.EnableMiniMap.Value;
            }));
        ToolItems2.Add(toolItemData);
        toolItemData.IsChecked = MainWindowViewModel.Instance.DiagramViewModel.EnableMiniMap.Value;

        toolItemData = new ToolItemData("combine", "pack://application:,,,/Assets/img/icon_Combine_union_dark.png",
            Resources.MenuItem_Combine,
            new DelegateCommand(() =>
            {
                MainWindowViewModel.Instance.DiagramViewModel.EnableCombine.Value =
                    !MainWindowViewModel.Instance.DiagramViewModel.EnableCombine.Value;
            }));
        ToolItems2.Add(toolItemData);
        toolItemData.IsChecked = MainWindowViewModel.Instance.DiagramViewModel.EnableCombine.Value;

        toolItemData = new ToolItemData("layers", "pack://application:,,,/Assets/img/icon_Layers.png",
            Resources.MenuItem_Layering,
            new DelegateCommand(() =>
            {
                MainWindowViewModel.Instance.DiagramViewModel.EnableLayers.Value =
                    !MainWindowViewModel.Instance.DiagramViewModel.EnableLayers.Value;
            }));
        ToolItems2.Add(toolItemData);
        toolItemData.IsChecked = MainWindowViewModel.Instance.DiagramViewModel.EnableLayers.Value;

        toolItemData = new ToolItemData("workHistory", "pack://application:,,,/Assets/img/work_history_dark.png",
            Resources.MenuItem_WorkHistory,
            new DelegateCommand(() =>
            {
                MainWindowViewModel.Instance.DiagramViewModel.EnableWorkHistory.Value =
                    !MainWindowViewModel.Instance.DiagramViewModel.EnableWorkHistory.Value;
            }));
        ToolItems2.Add(toolItemData);
        toolItemData.IsChecked = MainWindowViewModel.Instance.DiagramViewModel.EnableWorkHistory.Value;
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
        var diagramViewModel = MainWindowViewModel.Instance.DiagramViewModel;
        diagramViewModel.AllItems.Value.ToList().ForEach(x => x.IsHitTestVisible.Value = false);
        CurrentHitTestVisibleState.Value = false;
    }

    private void ChangeHitTestToEnable()
    {
        var diagramViewModel = MainWindowViewModel.Instance.DiagramViewModel;
        diagramViewModel.SelectedLayers.Value.ToList().ForEach(x =>
            (x as Layer).Children.ToList().ForEach(y =>
            {
                var layerItem = y as LayerItem;
                layerItem.Item.Value.IsHitTestVisible.Value = true;
                LogManager.GetCurrentClassLogger()
                    .Trace($"{layerItem.Name.Value}.IsHitTestVisible={layerItem.Item.Value.IsHitTestVisible.Value}");
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
                MainWindowViewModel.Instance.DiagramViewModel
                    .ContextMenuVisibility.Value = Visibility.Visible;
                break;
            case "dropper":
                MainWindowViewModel.Instance.DiagramViewModel
                    .ContextMenuVisibility.Value = Visibility.Collapsed;
                break;
        }
    }
}