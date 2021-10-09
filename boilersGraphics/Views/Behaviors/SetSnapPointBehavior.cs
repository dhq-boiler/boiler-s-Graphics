using boilersGraphics.Controls;
using boilersGraphics.ViewModels;
using Microsoft.Xaml.Behaviors;
using System.Windows.Input;

namespace boilersGraphics.Views.Behaviors
{
    public class SetSnapPointBehavior : Behavior<DesignerCanvas>
    {
        protected override void OnAttached()
        {
            this.AssociatedObject.StylusDown += AssociatedObject_StylusDown;
            this.AssociatedObject.TouchDown += AssociatedObject_TouchDown;
            this.AssociatedObject.MouseDown += AssociatedObject_MouseDown;
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            this.AssociatedObject.StylusDown -= AssociatedObject_StylusDown;
            this.AssociatedObject.TouchDown -= AssociatedObject_TouchDown;
            this.AssociatedObject.MouseDown -= AssociatedObject_MouseDown;
            base.OnDetaching();
        }

        private void AssociatedObject_StylusDown(object sender, StylusDownEventArgs e)
        {
            if (e.Source == AssociatedObject)
            {
                var setSnapPoint = e.GetPosition(AssociatedObject);
                var item = new SnapPointViewModel();
                item.Owner = (AssociatedObject as DesignerCanvas).DataContext as IDiagramViewModel;
                item.Left.Value = setSnapPoint.X;
                item.Top.Value = setSnapPoint.Y;
                item.Width.Value = 3;
                item.Height.Value = 3;
                item.Opacity.Value = 0.5;
                item.IsVisible.Value = true;
                ((AssociatedObject as DesignerCanvas).DataContext as IDiagramViewModel).AddItemCommand.Execute(item);

                e.Handled = true;
            }
        }

        private void AssociatedObject_TouchDown(object sender, TouchEventArgs e)
        {
            if (e.Source == AssociatedObject)
            {
                var touchPoint = e.GetTouchPoint(AssociatedObject);
                var setSnapPoint = touchPoint.Position;
                var item = new SnapPointViewModel();
                item.Owner = (AssociatedObject as DesignerCanvas).DataContext as IDiagramViewModel;
                item.Left.Value = setSnapPoint.X;
                item.Top.Value = setSnapPoint.Y;
                item.Width.Value = 3;
                item.Height.Value = 3;
                item.Opacity.Value = 0.5;
                item.IsVisible.Value = true;
                ((AssociatedObject as DesignerCanvas).DataContext as IDiagramViewModel).AddItemCommand.Execute(item);
            }
        }

        private void AssociatedObject_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.StylusDevice != null)
                return;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (e.Source == AssociatedObject)
                {
                    var setSnapPoint = e.GetPosition(AssociatedObject);
                    var item = new SnapPointViewModel();
                    item.Owner = (AssociatedObject as DesignerCanvas).DataContext as IDiagramViewModel;
                    item.Left.Value = setSnapPoint.X;
                    item.Top.Value = setSnapPoint.Y;
                    item.Width.Value = 3;
                    item.Height.Value = 3;
                    item.Opacity.Value = 0.5;
                    item.IsVisible.Value = true;
                    ((AssociatedObject as DesignerCanvas).DataContext as IDiagramViewModel).AddItemCommand.Execute(item);

                    e.Handled = true;
                }
            }
        }
    }
}
