using grapher.Extensions;
using grapher.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Shapes;

namespace grapher.Views.Behaviors
{
    class StraightLineSelectBehavior : Behavior<FrameworkElement>
    {
        private StraightLineResizeHandle adorner;

        protected override void OnAttached()
        {
            var canvas = this.AssociatedObject.GetParentOfType<Canvas>();
            canvas.PreviewMouseDown += Canvas_PreviewMouseDown;
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            var canvas = this.AssociatedObject.GetParentOfType<Canvas>();

            canvas.PreviewMouseDown -= Canvas_PreviewMouseDown;
            base.OnDetaching();
        }

        private void Canvas_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var line = AssociatedObject as Line;
            var position = e.GetPosition((UIElement)sender);
            bool result = false;
            var hitRect = new Rect(position.X - 2, position.Y - 2, 4, 4);
            VisualTreeHelper.HitTest(line, null, htr => { result = true; return HitTestResultBehavior.Stop; }, new GeometryHitTestParameters(new RectangleGeometry(hitRect)));

            if (result)
            {
                var viewModel = line.DataContext as ConnectorBaseViewModel;
                viewModel.IsSelected = true;

                if (adorner == null)
                {
                    var layer = AdornerLayer.GetAdornerLayer(line);
                    adorner = new StraightLineResizeHandle(line);
                    layer.Add(adorner);
                }
            }
            else
            {
                if (adorner != null)
                {
                    var layer = AdornerLayer.GetAdornerLayer(line);
                    layer.Remove(adorner);
                    adorner = null;
                }
            }
        }
    }
}
