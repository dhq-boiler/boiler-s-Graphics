using boiler_sGraphics.ViewModels;
using System.Windows;
using Microsoft.Xaml.Behaviors;
using System.Windows.Shapes;

namespace boiler_sGraphics.Views.Behaviors
{
    public class RectangleSelectBehavior : Behavior<FrameworkElement>
    {
        protected override void OnAttached()
        {
            this.AssociatedObject.PreviewMouseDown += AssociatedObject_PreviewMouseDown;
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            this.AssociatedObject.PreviewMouseDown -= AssociatedObject_PreviewMouseDown;
            base.OnDetaching();
        }

        private void AssociatedObject_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var rectangle = AssociatedObject as Rectangle;
            var viewModel = rectangle.DataContext as RenderItemViewModel;
            viewModel.Model.IsSelected = true;
        }
    }
}
