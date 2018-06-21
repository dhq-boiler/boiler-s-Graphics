using grapher.ViewModels;
using System.Windows;
using System.Windows.Interactivity;
using System.Windows.Shapes;

namespace grapher.Views.Behaviors
{
    internal class EllipseSelectBehavior : Behavior<FrameworkElement>
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
            var ellipse = AssociatedObject as Ellipse;
            var viewModel = ellipse.DataContext as RenderItemViewModel;
            viewModel.Model.IsSelected = true;
        }
    }
}
