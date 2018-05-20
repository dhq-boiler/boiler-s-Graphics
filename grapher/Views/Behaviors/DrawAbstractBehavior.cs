using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace grapher.Views.Behaviors
{
    abstract class DrawAbstractBehavior<T> : Behavior<Canvas> where T : AbstractAdorner
    {
        private Point origin;
        private Point dragStartPos;

        public T Adorner { get; private set; }

        protected override void OnAttached()
        {
            this.AssociatedObject.PreviewMouseDown += PreviewMouseDownHandler;
            this.AssociatedObject.PreviewMouseMove += PreviewMouseMoveHandler;
            this.AssociatedObject.PreviewMouseUp += PreviewMouseUpHandler;
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            this.AssociatedObject.PreviewMouseDown -= PreviewMouseDownHandler;
            this.AssociatedObject.PreviewMouseMove -= PreviewMouseMoveHandler;
            this.AssociatedObject.PreviewMouseUp -= PreviewMouseUpHandler;
            base.OnDetaching();
        }

        private void PreviewMouseDownHandler(object sender, MouseButtonEventArgs e)
        {
            origin = e.GetPosition(this.AssociatedObject);

            if (sender is IInputElement)
            {
                dragStartPos = e.GetPosition(sender as IInputElement);
            }

            Adorner = CreateAdornerObject(AssociatedObject, AssociatedObject, origin);

            AssociatedObject.CaptureMouse();
        }

        protected abstract T CreateAdornerObject(Visual visual, UIElement adornedElement, Point beginPoint);

        private void PreviewMouseMoveHandler(object sender, MouseEventArgs e)
        {
            if (Adorner != null)
            {
                if (AssociatedObject.IsMouseCaptured && e.LeftButton == MouseButtonState.Pressed)
                {
                    var pt = e.GetPosition(AssociatedObject);
                    Adorner.EndPoint = pt;
                }
            }
        }

        private void PreviewMouseUpHandler(object sender, MouseButtonEventArgs e)
        {
            if (Adorner != null)
            {
                Draw();

                AssociatedObject.ReleaseMouseCapture();
                Adorner.Detach();
                Adorner = null;
            }
        }

        public abstract void Draw();

        private bool CheckDistance(Point x, Point y)
        {
            return Math.Abs(x.X - y.X) >= SystemParameters.MinimumHorizontalDragDistance ||
                   Math.Abs(x.Y - y.Y) >= SystemParameters.MinimumVerticalDragDistance;
        }
    }
}
