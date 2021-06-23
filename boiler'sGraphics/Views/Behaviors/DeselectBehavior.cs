using boiler_sGraphics.ViewModels;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace boiler_sGraphics.Views.Behaviors
{
    public class DeselectBehavior : Behavior<Canvas>
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
            foreach (var item in this.AssociatedObject.Children)
            {
                ((item as FrameworkElement).DataContext as RenderItemViewModel).Model.IsSelected = false;
            }
        }
    }
}
