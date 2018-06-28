using grapher.Extensions;
using grapher.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace grapher.Controls
{
    public class RotateThumb : Thumb
    {
        private double _initialAngle;
        private RotateTransform _rotateTransform;
        private Vector _startVector;
        private Point _centerPoint;
        private FrameworkElement _designerItem;
        private Canvas _canvas;

        public RotateThumb()
        {
            DragDelta += new DragDeltaEventHandler(this.RotateThumb_DragDelta);
            DragStarted += new DragStartedEventHandler(this.RotateThumb_DragStarted);
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

                    _rotateTransform = _designerItem.RenderTransform as RotateTransform;
                    if (_rotateTransform == null)
                    {
                        _designerItem.RenderTransform = new RotateTransform(0);
                        _initialAngle = 0;
                    }
                    else
                    {
                        _initialAngle = _rotateTransform.Angle;
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

                double angle = Vector.AngleBetween(_startVector, deltaVector);

                viewModel.RotateAngle.Value = _initialAngle + Math.Round(angle, 0);
                _designerItem.InvalidateMeasure();
            }
        }
    }
}
