using grapher.Models;
using grapher.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace grapher.Views.Behaviors
{
    public class DrawRectangleBehavior : Behavior<Canvas>
    {
        private Point origin;
        private Point dragStartPos;
        private RectangleAdorner _adorner;

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

            _adorner = new RectangleAdorner(AssociatedObject, AssociatedObject, origin);

            AssociatedObject.CaptureMouse();
        }

        private void PreviewMouseMoveHandler(object sender, MouseEventArgs e)
        {
            if (_adorner != null)
            {
                if (AssociatedObject.IsMouseCaptured && e.LeftButton == MouseButtonState.Pressed)
                {
                    var pt = e.GetPosition(AssociatedObject);
                    _adorner.EndPoint = pt;
                }
            }
        }

        private void PreviewMouseUpHandler(object sender, MouseButtonEventArgs e)
        {
            if (_adorner != null)
            {
                DrawRectangle();

                AssociatedObject.ReleaseMouseCapture();
                _adorner.Detach();
                _adorner = null;
            }
        }

        private void DrawRectangle()
        {
            var renderItem = new Rectangle
            {
                X = Math.Min(_adorner.BeginPoint.X, _adorner.EndPoint.X),
                Y = Math.Min(_adorner.BeginPoint.Y, _adorner.EndPoint.Y),
                Width = Math.Max(_adorner.EndPoint.X - _adorner.BeginPoint.X, _adorner.BeginPoint.X - _adorner.EndPoint.X),
                Height = Math.Max(_adorner.EndPoint.Y - _adorner.BeginPoint.Y, _adorner.BeginPoint.Y - _adorner.EndPoint.Y),
                Stroke = new SolidColorBrush(Colors.Black),
                Fill = new SolidColorBrush(Colors.Transparent)
            };
            var viewModel = new RectangleViewModel(renderItem);
            (App.Current.MainWindow.DataContext as MainWindowViewModel).RenderItems.Add(viewModel);
        }

        private bool CheckDistance(Point x, Point y)
        {
            return Math.Abs(x.X - y.X) >= SystemParameters.MinimumHorizontalDragDistance ||
                   Math.Abs(x.Y - y.Y) >= SystemParameters.MinimumVerticalDragDistance;
        }
    }
}
