using boilersGraphics.Extensions;
using boilersGraphics.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Microsoft.Xaml.Behaviors;
using System.Windows.Media;
using System.Windows.Shapes;

namespace boilersGraphics.Views.Behaviors
{
    internal class StraightLineSelectBehavior : Behavior<FrameworkElement>
    {
        private StraightLineResizeHandle _adorner;

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

                if (_adorner == null)
                {
                    var layer = AdornerLayer.GetAdornerLayer(line);
                    _adorner = new StraightLineResizeHandle(line);
                    layer.Add(_adorner);
                }
            }
            else
            {
                if (_adorner != null)
                {
                    var layer = AdornerLayer.GetAdornerLayer(line);
                    layer.Remove(_adorner);
                    _adorner = null;
                }
            }
        }
    }
}
