using grapher.Controls;
using grapher.ViewModels;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace grapher.Views.Behaviors
{
    internal class NDrawStraightLineBehavior : Behavior<DesignerCanvas>
    {
        private Point? _straightLineStartPoint;

        protected override void OnAttached()
        {
            this.AssociatedObject.MouseDown += AssociatedObject_MouseDown;
            this.AssociatedObject.MouseMove += AssociatedObject_MouseMove;
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            this.AssociatedObject.MouseDown -= AssociatedObject_MouseDown;
            this.AssociatedObject.MouseMove -= AssociatedObject_MouseMove;
            base.OnDetaching();
        }

        private void AssociatedObject_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (e.Source == AssociatedObject)
                {
                    _straightLineStartPoint = e.GetPosition(AssociatedObject);

                    e.Handled = true;
                }
            }
        }

        private void AssociatedObject_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var canvas = AssociatedObject as DesignerCanvas;
            if (canvas.SourceConnector == null)
            {
                if (e.LeftButton != MouseButtonState.Pressed)
                    _straightLineStartPoint = null;

                if (_straightLineStartPoint.HasValue)
                {
                    (App.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = "描画";

                    AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(canvas);
                    if (adornerLayer != null)
                    {
                        var adorner = new Adorners.StraightLineAdorner(canvas, _straightLineStartPoint);
                        if (adorner != null)
                        {
                            adornerLayer.Add(adorner);
                        }
                    }
                }
            }
        }
    }
}
