using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.Views.Behaviors;
using Microsoft.Win32;
using Prism.Commands;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Xaml.Behaviors;
using System.IO;
using System.Windows.Media.Imaging;
using Prism.Services.Dialogs;
using boilersGraphics.Views;
using boilersGraphics.Models;
using System.Windows.Media;
using System.Windows;
using Reactive.Bindings;
using System.Diagnostics;

namespace boilersGraphics.ViewModels
{
    public class ToolBarViewModel
    {
        private IDialogService dlgService = null;
        public ObservableCollection<ToolItemData> ToolItems { get; } = new ObservableCollection<ToolItemData>();

        public BehaviorCollection Behaviors { get { return Interaction.GetBehaviors(App.Current.MainWindow.GetChildOfType<DesignerCanvas>()); } }

        public ReactivePropertySlim<bool> CurrentHitTestVisibleState { get; } = new ReactivePropertySlim<bool>();

        public ToolBarViewModel(IDialogService dialogService)
        {
            this.dlgService = dialogService;
            ToolItems.Add(new ToolItemData("pointer", "pack://application:,,,/Assets/img/pointer.png", new DelegateCommand(() =>
            {
                var deselectBehavior = new DeselectBehavior();
                Behaviors.Clear();
                if (!Behaviors.Contains(deselectBehavior))
                {
                    Behaviors.Add(deselectBehavior);
                }
                ChangeHitTestToEnable();
                SelectOneToolItem("pointer");
            })));
            ToolItems.Add(new ToolItemData("rubberband", "pack://application:,,,/Assets/img/rubberband.png", new DelegateCommand(() =>
            {
                var behavior = new RubberbandBehavior();
                Behaviors.Clear();
                if (!Behaviors.Contains(behavior))
                {
                    Behaviors.Add(behavior);
                }
                ChangeHitTestToEnable();
                SelectOneToolItem("rubberband");
            })));
            ToolItems.Add(new ToolItemData("straightline", "pack://application:,,,/Assets/img/straightline.png", new DelegateCommand(() =>
            {
                var behavior = new NDrawStraightLineBehavior();
                Behaviors.Clear();
                if (!Behaviors.Contains(behavior))
                {
                    Behaviors.Add(behavior);
                }
                ChangeHitTestToDisable();
                SelectOneToolItem("straightline");
            })));
            ToolItems.Add(new ToolItemData("rectangle", "pack://application:,,,/Assets/img/rectangle.png", new DelegateCommand(() =>
            {
                var behavior = new NDrawRectangleBehavior();
                Behaviors.Clear();
                if (!Behaviors.Contains(behavior))
                {
                    Behaviors.Add(behavior);
                }
                ChangeHitTestToDisable();
                SelectOneToolItem("rectangle");
            })));
            ToolItems.Add(new ToolItemData("ellipse", "pack://application:,,,/Assets/img/ellipse.png", new DelegateCommand(() =>
            {
                var behavior = new NDrawEllipseBehavior();
                Behaviors.Clear();
                if (!Behaviors.Contains(behavior))
                {
                    Behaviors.Add(behavior);
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
                    var behavior = new PictureBehavior(dialog.FileName, bitmap.Width, bitmap.Height);
                    Behaviors.Clear();
                    if (!Behaviors.Contains(behavior))
                    {
                        Behaviors.Add(behavior);
                    }
                    ChangeHitTestToDisable();
                    SelectOneToolItem("picture");
                }
            })));
            ToolItems.Add(new ToolItemData("letter", "pack://application:,,,/Assets/img/A.png", new DelegateCommand(() =>
            {
                var behavior = new LetterBehavior();
                Behaviors.Clear();
                if (!Behaviors.Contains(behavior))
                {
                    Behaviors.Add(behavior);
                }
                ChangeHitTestToDisable();
                SelectOneToolItem("letter");
            })));
            ToolItems.Add(new ToolItemData("letter-vertical", "pack://application:,,,/Assets/img/A_Vertical.png", new DelegateCommand(() =>
            {
                var behavior = new LetterVerticalBehavior();
                Behaviors.Clear();
                if (!Behaviors.Contains(behavior))
                {
                    Behaviors.Add(behavior);
                }
                ChangeHitTestToDisable();
                SelectOneToolItem("letter-vertical");
            })));
            ToolItems.Add(new ToolItemData("polygon", "pack://application:,,,/Assets/img/pentagon.png", new DelegateCommand(() =>
            {
                IDialogResult result = null;
                this.dlgService.ShowDialog(nameof(PolygonSetting), ret => result = ret);
                if (result != null && result.Result == ButtonResult.OK)
                {
                    var corners = result.Parameters.GetValue<ObservableCollection<Corner>>("Corners");
                    var data = result.Parameters.GetValue<string>("Data");
                    var startPoint = result.Parameters.GetValue<Point>("StartPoint");
                    var behavior = new NDrawPolygonBehavior(corners, data, startPoint);
                    Behaviors.Clear();
                    if (!Behaviors.Contains(behavior))
                    {
                        Behaviors.Add(behavior);
                    }
                    ChangeHitTestToDisable();
                    SelectOneToolItem("polygon");
                }
            })));
            ToolItems.Add(new ToolItemData("bezier", "pack://application:,,,/Assets/img/BezierCurve.png", new DelegateCommand(() =>
            {
                var behavior = new NDrawBezierCurveBehavior();
                Behaviors.Clear();
                if (!Behaviors.Contains(behavior))
                {
                    Behaviors.Add(behavior);
                }
                ChangeHitTestToDisable();
                SelectOneToolItem("bezier");
            })));
            ToolItems.Add(new ToolItemData("snappoint", "pack://application:,,,/Assets/img/SnapPoint.png", new DelegateCommand(() =>
            {
                var behavior = new SetSnapPointBehavior();
                Behaviors.Clear();
                if (!Behaviors.Contains(behavior))
                {
                    Behaviors.Add(behavior);
                }
                ChangeHitTestToDisable();
                SelectOneToolItem("snappoint");
            })));
            ToolItems.Add(new ToolItemData("brush", "pack://application:,,,/Assets/img/brush.png", new DelegateCommand(() =>
            {
                var behavior = new BrushBehavior(dlgService);
                behavior.CurrentBrush.OpenThicknessDialog();
                Behaviors.Clear();
                if (!Behaviors.Contains(behavior))
                {
                    Behaviors.Add(behavior);
                }
                ChangeHitTestToDisable();
                SelectOneToolItem("brush");
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
                    Trace.WriteLine($"{layerItem.Name.Value}.IsHitTestVisible={layerItem.Item.Value.IsHitTestVisible.Value}");
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
