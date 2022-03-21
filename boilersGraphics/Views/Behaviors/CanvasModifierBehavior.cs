using boilersGraphics.Controls;
using boilersGraphics.ViewModels;
using Microsoft.Xaml.Behaviors;
using System.Windows.Media;

namespace boilersGraphics.Views.Behaviors
{
    public class CanvasModifierBehavior : Behavior<DesignerCanvas>
    {
        public double SavedOpacity { get; set; }

        protected override void OnAttached()
        {
            var mainWindowViewModel = App.Current.MainWindow.DataContext as MainWindowViewModel;
            var background = mainWindowViewModel.DiagramViewModel.BackgroundItem.Value;
            background.EdgeBrush.Value = Brushes.Magenta;
            background.EdgeThickness.Value = 10;
            SavedOpacity = background.FillBrush.Value.Opacity;
            background.FillBrush.Value = background.FillBrush.Value.Clone();
            background.FillBrush.Value.Opacity = 0.5;
            background.ZIndex.Value = int.MaxValue;
            background.CanDrag.Value = true;
            background.IsHitTestVisible.Value = true;
            background.EnableForSelection.Value = true;
            background.IsSelected.Value = true;
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            var mainWindowViewModel = App.Current.MainWindow.DataContext as MainWindowViewModel;
            var background = mainWindowViewModel.DiagramViewModel.BackgroundItem.Value;
            background.EdgeBrush.Value = Brushes.Black;
            background.EdgeThickness.Value = 1;
            background.FillBrush.Value.Opacity = SavedOpacity;
            background.ZIndex.Value = -1;
            background.CanDrag.Value = false;
            background.IsHitTestVisible.Value = false;
            background.EnableForSelection.Value = false;
            background.IsSelected.Value = false;
            base.OnDetaching();
        }
    }
}
