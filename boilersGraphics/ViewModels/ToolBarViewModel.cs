using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.Models;
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
        public ObservableCollection<ToolItemData> ToolItems { get; } = new ObservableCollection<ToolItemData>();

        public BehaviorCollection Behaviors { get { return Interaction.GetBehaviors(App.Current.MainWindow.GetChildOfType<DesignerCanvas>()); } }

        public ReactivePropertySlim<bool> CurrentHitTestVisibleState { get; } = new ReactivePropertySlim<bool>();

        public DeselectBehavior DeselectBehavior { get; } = new DeselectBehavior();
        public RubberbandBehavior RubberbandBehavior { get; } = new RubberbandBehavior();
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

        public ToolBarViewModel(IDialogService dialogService)
        {
            this.dlgService = dialogService;
            ToolItems.Add(new ToolItemData("pointer", "pack://application:,,,/Assets/img/pointer.png", new DelegateCommand(() =>
            {
                Behaviors.Clear();
                if (!Behaviors.Contains(DeselectBehavior))
                {
                    Behaviors.Add(DeselectBehavior);
                }
                ChangeHitTestToEnable();
                SelectOneToolItem("pointer");
            })));
            ToolItems.Add(new ToolItemData("rubberband", "pack://application:,,,/Assets/img/rubberband.png", new DelegateCommand(() =>
            {
                Behaviors.Clear();
                if (!Behaviors.Contains(RubberbandBehavior))
                {
                    Behaviors.Add(RubberbandBehavior);
                }
                ChangeHitTestToEnable();
                SelectOneToolItem("rubberband");
            })));
            ToolItems.Add(new ToolItemData("straightline", "pack://application:,,,/Assets/img/straightline.png", new DelegateCommand(() =>
            {
                Behaviors.Clear();
                if (!Behaviors.Contains(NDrawStraightLineBehavior))
                {
                    Behaviors.Add(NDrawStraightLineBehavior);
                }
                ChangeHitTestToDisable();
                SelectOneToolItem("straightline");
            })));
            ToolItems.Add(new ToolItemData("rectangle", "pack://application:,,,/Assets/img/rectangle.png", new DelegateCommand(() =>
            {
                Behaviors.Clear();
                if (!Behaviors.Contains(NDrawRectangleBehavior))
                {
                    Behaviors.Add(NDrawRectangleBehavior);
                }
                ChangeHitTestToDisable();
                SelectOneToolItem("rectangle");
            })));
            ToolItems.Add(new ToolItemData("ellipse", "pack://application:,,,/Assets/img/ellipse.png", new DelegateCommand(() =>
            {
                Behaviors.Clear();
                if (!Behaviors.Contains(NDrawEllipseBehavior))
                {
                    Behaviors.Add(NDrawEllipseBehavior);
                }
                ChangeHitTestToDisable();
                SelectOneToolItem("ellipse");
            })));
            ToolItems.Add(new ToolItemData("picture", "pack://application:,,,/Assets/img/Picture.png", new DelegateCommand(() =>
            {
                var dialog = new OpenFileDialog();
                dialog.Multiselect = false;
                dialog.Filter = "JPEG file|*.jpg;*.jpeg|PNG file|*.png|GIF file|*.gif|BMP file|*.bmp|ALL|*.*";
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
            ToolItems.Add(new ToolItemData("letter", "pack://application:,,,/Assets/img/A.png", new DelegateCommand(() =>
            {
                Behaviors.Clear();
                if (!Behaviors.Contains(LetterBehavior))
                {
                    Behaviors.Add(LetterBehavior);
                }
                ChangeHitTestToDisable();
                SelectOneToolItem("letter");
            })));
            ToolItems.Add(new ToolItemData("letter-vertical", "pack://application:,,,/Assets/img/A_Vertical.png", new DelegateCommand(() =>
            {
                Behaviors.Clear();
                if (!Behaviors.Contains(LetterVerticalBehavior))
                {
                    Behaviors.Add(LetterVerticalBehavior);
                }
                ChangeHitTestToDisable();
                SelectOneToolItem("letter-vertical");
            })));
            ToolItems.Add(new ToolItemData("polygon", "pack://application:,,,/Assets/img/polygon.png", new DelegateCommand(() =>
            {
                IDialogResult result = null;
                this.dlgService.ShowDialog(nameof(PolygonSetting), ret => result = ret);
                if (result != null && result.Result == ButtonResult.OK)
                {
                    var corners = result.Parameters.GetValue<ObservableCollection<Corner>>("Corners");
                    var data = result.Parameters.GetValue<string>("Data");
                    var startPoint = result.Parameters.GetValue<Point>("StartPoint");
                    NDrawPolygonBehavior = new NDrawPolygonBehavior(corners, data, startPoint);
                    Behaviors.Clear();
                    if (!Behaviors.Contains(NDrawPolygonBehavior))
                    {
                        Behaviors.Add(NDrawPolygonBehavior);
                    }
                    ChangeHitTestToDisable();
                    SelectOneToolItem("polygon");
                }
            })));
            ToolItems.Add(new ToolItemData("bezier", "pack://application:,,,/Assets/img/BezierCurve.png", new DelegateCommand(() =>
            {
                Behaviors.Clear();
                if (!Behaviors.Contains(NDrawBezierCurveBehavior))
                {
                    Behaviors.Add(NDrawBezierCurveBehavior);
                }
                ChangeHitTestToDisable();
                SelectOneToolItem("bezier");
            })));
            ToolItems.Add(new ToolItemData("snappoint", "pack://application:,,,/Assets/img/SnapPoint.png", new DelegateCommand(() =>
            {
                Behaviors.Clear();
                if (!Behaviors.Contains(SetSnapPointBehavior))
                {
                    Behaviors.Add(SetSnapPointBehavior);
                }
                ChangeHitTestToDisable();
                SelectOneToolItem("snappoint");
            })));
            ToolItems.Add(new ToolItemData("brush", "pack://application:,,,/Assets/img/brush.png", new DelegateCommand(() =>
            {
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
            ToolItems.Add(new ToolItemData("eraser", "pack://application:,,,/Assets/img/eraser.png", new DelegateCommand(() =>
            {
                Behaviors.Clear();
                if (!Behaviors.Contains(EraserBehavior))
                {
                    Behaviors.Add(EraserBehavior);
                }
                ChangeHitTestToDisable();
                SelectOneToolItem("eraser");
            })));
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
            var toolItem = ToolItems.Where(i => i.Name == toolName).Single();
            toolItem.IsChecked = true;

            ToolItems.Where(i => i.Name != toolName).ToList().ForEach(i => i.IsChecked = false);
        }
    }
}
