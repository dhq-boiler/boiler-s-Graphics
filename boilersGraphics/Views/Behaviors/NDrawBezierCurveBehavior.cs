using boilersGraphics.Controls;
using boilersGraphics.Helpers;
using boilersGraphics.ViewModels;
using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace boilersGraphics.Views.Behaviors
{
    internal class NDrawBezierCurveBehavior : Behavior<DesignerCanvas>
    {
        private Point? _rectangleStartPoint;
        private SnapAction snapAction;

        public NDrawBezierCurveBehavior()
        {
            snapAction = new SnapAction();
        }

        protected override void OnAttached()
        {
            this.AssociatedObject.MouseDown += AssociatedObject_MouseDown;
            this.AssociatedObject.MouseMove += AssociatedObject_MouseMove;
            this.AssociatedObject.MouseUp += AssociatedObject_MouseUp;
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            this.AssociatedObject.MouseDown -= AssociatedObject_MouseDown;
            this.AssociatedObject.MouseMove -= AssociatedObject_MouseMove;
            this.AssociatedObject.MouseUp -= AssociatedObject_MouseUp;
            base.OnDetaching();
        }

        private void AssociatedObject_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (e.Source == AssociatedObject)
                {
                    _rectangleStartPoint = e.GetPosition(AssociatedObject);

                    e.Handled = true;
                }
            }
        }

        private void AssociatedObject_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var canvas = AssociatedObject as DesignerCanvas;
            Point current = e.GetPosition(canvas);
            snapAction.OnMouseMove(ref current);

            if (e.LeftButton != MouseButtonState.Pressed)
                _rectangleStartPoint = null;

            if (_rectangleStartPoint.HasValue)
            {
                _rectangleStartPoint = current;
                (App.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = "描画";

                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(canvas);
                if (adornerLayer != null)
                {
                    var adorner = new Adorners.BezierCurveAdorner(canvas, _rectangleStartPoint);
                    if (adorner != null)
                    {
                        adornerLayer.Add(adorner);
                    }
                }
            }
        }

        private void AssociatedObject_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // release mouse capture
            if (AssociatedObject.IsMouseCaptured) AssociatedObject.ReleaseMouseCapture();
        }
    }
}
