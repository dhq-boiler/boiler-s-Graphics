using boilersGraphics.Controls;
using boilersGraphics.ViewModels;
using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace boilersGraphics.Views.Behaviors
{
    public class CanvasModifierBehavior : Behavior<DesignerCanvas>
    {
        protected override void OnAttached()
        {
            var mainWindowViewModel = App.Current.MainWindow.DataContext as MainWindowViewModel;
            var background = mainWindowViewModel.DiagramViewModel.BackgroundItem.Value;
            background.EdgeBrush.Value = Brushes.Magenta;
            background.EdgeThickness.Value = 10;
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
            background.CanDrag.Value = false;
            background.IsHitTestVisible.Value = false;
            background.EnableForSelection.Value = false;
            background.IsSelected.Value = false;
            base.OnDetaching();
        }
    }
}
