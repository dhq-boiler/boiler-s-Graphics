using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;
using System.Windows.Media;

namespace boilersGraphics.Views.Behaviors
{
    [Obsolete]
    internal abstract class DrawAbstractBehavior<T> : Behavior<Canvas> where T : AbstractAdorner
    {
        private Point _origin;
        private Point _dragStartPos;

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
            _origin = e.GetPosition(this.AssociatedObject);

            if (sender is IInputElement)
            {
                _dragStartPos = e.GetPosition(sender as IInputElement);
            }

            Adorner = CreateAdornerObject(AssociatedObject, AssociatedObject, _origin);

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
