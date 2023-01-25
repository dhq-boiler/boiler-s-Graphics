using boilersGraphics.Controls;
using boilersGraphics.Helpers;
using boilersGraphics.ViewModels;
using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace boilersGraphics.Views.Behaviors
{
    public class BlurBehavior : Behavior<DesignerCanvas>
    {
        private Point? _rectangleStartPoint;
        private SnapAction snapAction;
        private BlurEffectViewModel item;

        public BlurBehavior()
        {
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
                _rectangleStartPoint = e.GetPosition(AssociatedObject);
                var viewModel = AssociatedObject.DataContext as IDiagramViewModel;
                item = new BlurEffectViewModel();
                item.Owner = viewModel;
                e.Handled = true;
            }
        }

        private void AssociatedObject_TouchDown(object sender, TouchEventArgs e)
        {
            if (e.Source == AssociatedObject)
            {
                var touchPoint = e.GetTouchPoint(AssociatedObject);
                _rectangleStartPoint = touchPoint.Position;
                var viewModel = AssociatedObject.DataContext as IDiagramViewModel;
                item = new  BlurEffectViewModel();
                item.Owner = viewModel;
            }
        }

        private void AssociatedObject_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.StylusDevice != null)
                return;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (e.Source == AssociatedObject)
                {
                    _rectangleStartPoint = e.GetPosition(AssociatedObject);
                    var viewModel = AssociatedObject.DataContext as IDiagramViewModel;
                    item = new BlurEffectViewModel();
                    item.Owner = viewModel;
                }
            }
        }

        private void AssociatedObject_StylusMove(object sender, StylusEventArgs e)
        {
            var canvas = AssociatedObject as DesignerCanvas;
            Point current = e.GetPosition(canvas);
            snapAction.OnMouseMove(ref current);

            if (e.InAir)
                _rectangleStartPoint = null;

            if (_rectangleStartPoint.HasValue)
            {
                if (_rectangleStartPoint.Value.X < current.X && _rectangleStartPoint.Value.Y <= current.Y)
                {
                    snapAction.PostProcess(SnapPointPosition.LeftTop, item);
                }
                else if (_rectangleStartPoint.Value.X < current.X && current.Y < _rectangleStartPoint.Value.Y)
                {
                    snapAction.PostProcess(SnapPointPosition.LeftBottom, item);
                }
                else if (current.X <= _rectangleStartPoint.Value.X && _rectangleStartPoint.Value.Y <= current.Y)
                {
                    snapAction.PostProcess(SnapPointPosition.RightTop, item);
                }
                else if (current.X <= _rectangleStartPoint.Value.X && current.Y < _rectangleStartPoint.Value.Y)
                {
                    snapAction.PostProcess(SnapPointPosition.RightBottom, item);
                }
                _rectangleStartPoint = current;
                (App.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = boilersGraphics.Properties.Resources.String_Draw;

                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(canvas);
                if (adornerLayer != null)
                {
                    var adorner = new Adorners.BlurAdorner(canvas, _rectangleStartPoint, item);
                    if (adorner != null)
                    {
                        adornerLayer.Add(adorner);
                    }
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
                if (_rectangleStartPoint.Value.X < current.X && _rectangleStartPoint.Value.Y <= current.Y)
                {
                    snapAction.PostProcess(SnapPointPosition.LeftTop, item);
                }
                else if (_rectangleStartPoint.Value.X < current.X && current.Y < _rectangleStartPoint.Value.Y)
                {
                    snapAction.PostProcess(SnapPointPosition.LeftBottom, item);
                }
                else if (current.X <= _rectangleStartPoint.Value.X && _rectangleStartPoint.Value.Y <= current.Y)
                {
                    snapAction.PostProcess(SnapPointPosition.RightTop, item);
                }
                else if (current.X <= _rectangleStartPoint.Value.X && current.Y < _rectangleStartPoint.Value.Y)
                {
                    snapAction.PostProcess(SnapPointPosition.RightBottom, item);
                }
                _rectangleStartPoint = current;
                (App.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = boilersGraphics.Properties.Resources.String_Draw;

                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(canvas);
                if (adornerLayer != null)
                {
                    var adorner = new Adorners.BlurAdorner(canvas, _rectangleStartPoint, item);
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
            if (AssociatedObject.IsMouseCaptured)
            {
                AssociatedObject.ReleaseMouseCapture();
            }
        }
    }
}
