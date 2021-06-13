using grapher.Controls;
using grapher.Extensions;
using grapher.Helpers;
using grapher.Views.Behaviors;
using Prism.Commands;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Xaml.Behaviors;

namespace grapher.ViewModels
{
    public class ToolBarViewModel
    {
        public ObservableCollection<ToolItemData> ToolItems { get; } = new ObservableCollection<ToolItemData>();

        public ToolBarViewModel()
        {
            ToolItems.Add(new ToolItemData("pointer", "pack://application:,,,/Assets/img/pointer.png", new DelegateCommand(() =>
            {
                var designerCanvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();
                var behaviors = Interaction.GetBehaviors(designerCanvas);
                behaviors.Clear();
                SelectOneToolItem("pointer");
            })));
            ToolItems.Add(new ToolItemData("rubberband", "pack://application:,,,/Assets/img/rubberband.png", new DelegateCommand(() =>
            {
                var behavior = new RubberbandBehavior();
                var designerCanvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();
                var behaviors = Interaction.GetBehaviors(designerCanvas);
                behaviors.Clear();
                if (!behaviors.Contains(behavior))
                {
                    behaviors.Add(behavior);
                }
                SelectOneToolItem("rubberband");
            })));
            ToolItems.Add(new ToolItemData("straightline", "pack://application:,,,/Assets/img/straightline.png", new DelegateCommand(() =>
            {
                var behavior = new NDrawStraightLineBehavior();
                var designerCanvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();
                var behaviors = Interaction.GetBehaviors(designerCanvas);
                behaviors.Clear();
                if (!behaviors.Contains(behavior))
                {
                    behaviors.Add(behavior);
                }
                SelectOneToolItem("straightline");
            })));
            ToolItems.Add(new ToolItemData("rectangle", "pack://application:,,,/Assets/img/rectangle.png", new DelegateCommand(() =>
            {
                var behavior = new NDrawRectangleBehavior();
                var designerCanvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();
                var behaviors = Interaction.GetBehaviors(designerCanvas);
                behaviors.Clear();
                if (!behaviors.Contains(behavior))
                {
                    behaviors.Add(behavior);
                }
                SelectOneToolItem("rectangle");
            })));
            ToolItems.Add(new ToolItemData("ellipse", "pack://application:,,,/Assets/img/ellipse.png", null));
            ToolItems.Add(new ToolItemData("symbol-a", "pack://application:,,,/Assets/img/Setting.png", new DelegateCommand(() =>
            {
                var behavior = new DrawSettingBehavior();
                var designerCanvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();
                var behaviors = Interaction.GetBehaviors(designerCanvas);
                behaviors.Clear();
                if (!behaviors.Contains(behavior))
                {
                    behaviors.Add(behavior);
                }
                SelectOneToolItem("symbol-a");
            })));
        }

        private void SelectOneToolItem(string toolName)
        {
            var toolItem = ToolItems.Where(i => i.Name == toolName).Single();
            toolItem.IsChecked = true;

            ToolItems.Where(i => i.Name != toolName).ToList().ForEach(i => i.IsChecked = false);
        }
    }
}
