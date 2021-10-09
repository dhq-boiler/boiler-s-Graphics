using boilersGraphics.Adorners;
using boilersGraphics.Controls;
using boilersGraphics.Helpers;
using boilersGraphics.ViewModels;
using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace boilersGraphics.Views.Behaviors
{
    internal class PictureBehavior : Behavior<DesignerCanvas>
    {
        private Point? _pictureDrawingStartPoint = null;
        private string _filename;
        private double _Width;
        private double _Height;
        private SnapAction snapAction;
        public PictureBehavior(string filename, double width, double height)
        {
            _filename = filename;
            _Width = width;
            _Height = height;
            snapAction = new SnapAction();
        }

        protected override void OnAttached()
        {
            this.AssociatedObject.StylusDown += AssociatedObject_StylusDown;
            this.AssociatedObject.StylusMove += AssociatedObject_StylusMove;
            this.AssociatedObject.TouchDown += AssociatedObject_TouchDown;
            this.AssociatedObject.MouseDown += AssociatedObject_MouseDown;
            this.AssociatedObject.MouseMove += AssociatedObject_MouseMove;
            this.AssociatedObject.MouseUp += AssociatedObject_MouseUp;
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            this.AssociatedObject.StylusDown -= AssociatedObject_StylusDown;
            this.AssociatedObject.StylusMove -= AssociatedObject_StylusMove;
            this.AssociatedObject.TouchDown -= AssociatedObject_TouchDown;
            this.AssociatedObject.MouseDown -= AssociatedObject_MouseDown;
            this.AssociatedObject.MouseMove -= AssociatedObject_MouseMove;
            this.AssociatedObject.MouseUp -= AssociatedObject_MouseUp;
            base.OnDetaching();
        }

        private void AssociatedObject_StylusDown(object sender, StylusDownEventArgs e)
        {
            if (e.Source == AssociatedObject)
            {
                _pictureDrawingStartPoint = e.GetPosition(AssociatedObject);
                e.Handled = true;
            }
        }

        private void AssociatedObject_TouchDown(object sender, TouchEventArgs e)
        {
            if (e.Source == AssociatedObject)
            {
                var touchPoint = e.GetTouchPoint(AssociatedObject);
                _pictureDrawingStartPoint = touchPoint.Position;
            }
        }

        private void AssociatedObject_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.StylusDevice != null)
                return;

            var canvas = AssociatedObject as DesignerCanvas;
            Point current = e.GetPosition(canvas);
            snapAction.OnMouseMove(ref current);

            if (e.LeftButton != MouseButtonState.Pressed)
                _pictureDrawingStartPoint = null;

            if (_pictureDrawingStartPoint.HasValue)
            {
                _pictureDrawingStartPoint = current;
                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(canvas);
                if (adornerLayer != null)
                {
                    PictureAdorner adorner = new PictureAdorner(canvas, _pictureDrawingStartPoint, _filename, _Width, _Height);
                    if (adorner != null)
                    {
                        adornerLayer.Add(adorner);
                    }
                }
            }
            e.Handled = true;
        }

        private void AssociatedObject_StylusMove(object sender, StylusEventArgs e)
        {
            var canvas = AssociatedObject as DesignerCanvas;
            Point current = e.GetPosition(canvas);
            snapAction.OnMouseMove(ref current);

            if (e.InAir)
                _pictureDrawingStartPoint = null;

            if (_pictureDrawingStartPoint.HasValue)
            {
                _pictureDrawingStartPoint = current;
                (App.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = "描画";

                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(canvas);
                if (adornerLayer != null)
                {
                    PictureAdorner adorner = new PictureAdorner(canvas, _pictureDrawingStartPoint, _filename, _Width, _Height);
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

        private void AssociatedObject_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (e.Source == AssociatedObject)
                {
                    _pictureDrawingStartPoint = e.GetPosition(AssociatedObject);

                    e.Handled = true;
                }
            }
        }
    }
}
