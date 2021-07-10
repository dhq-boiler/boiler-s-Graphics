using boilersGraphics.Exceptions;
using boilersGraphics.Extensions;
using boilersGraphics.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace boilersGraphics.Controls
{
    public class RotateThumb : Thumb
    {
        private Matrix _initialMatrix;
        private MatrixTransform _rotateTransform;
        private Vector _startVector;
        private Point _centerPoint;
        private FrameworkElement _designerItem;
        private Canvas _canvas;
        private double _previousAngleInDegrees;

        public RotateThumb()
        {
            DragDelta += new DragDeltaEventHandler(this.RotateThumb_DragDelta);
            DragStarted += new DragStartedEventHandler(this.RotateThumb_DragStarted);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            (App.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = "回転";
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            (App.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = "";
            (App.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = "";
        }

        private void RotateThumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            _designerItem = this.GetParentOfType("selectedGrid") as FrameworkElement;

            if (_designerItem != null)
            {
                _canvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();

                if (_canvas != null)
                {
                    _centerPoint = _designerItem.TranslatePoint(
                        new Point(_designerItem.ActualWidth * _designerItem.RenderTransformOrigin.X,
                                  _designerItem.ActualHeight * _designerItem.RenderTransformOrigin.Y),
                                  _canvas);

                    Point startPoint = Mouse.GetPosition(_canvas);
                    _startVector = Point.Subtract(startPoint, _centerPoint);

                    _rotateTransform = _designerItem.RenderTransform as MatrixTransform;
                    if (_rotateTransform == null)
                    {
                        throw new UnexpectedException();
                    }
                    else
                    {
                        _initialMatrix = _rotateTransform.Matrix;
                        _previousAngleInDegrees = 0;
                    }
                }
            }
        }

        private void RotateThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var viewModel = DataContext as DesignerItemViewModelBase;

            if (_designerItem != null && _canvas != null)
            {
                Point currentPoint = Mouse.GetPosition(_canvas);
                Vector deltaVector = Point.Subtract(currentPoint, _centerPoint);

                double angleInDegrees = Vector.AngleBetween(_startVector, deltaVector);

                var diff = angleInDegrees - _previousAngleInDegrees;
                diff = Math.Round(diff, 0);
                viewModel.RotationAngle.Value += diff;
                _initialMatrix.RotateAt(diff, 0, 0);
                viewModel.Matrix.Value = _initialMatrix;
                _previousAngleInDegrees = angleInDegrees;

                (App.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = $"角度 {viewModel.RotationAngle.Value}°";

                _designerItem.InvalidateMeasure();
            }
        }
    }
}
