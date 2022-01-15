using boilersGraphics.Controls;
using boilersGraphics.Helpers;
using boilersGraphics.ViewModels;
using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace boilersGraphics.Views.Behaviors
{
    public class NDrawStraightLineBehavior : Behavior<DesignerCanvas>
    {
        private Point? _straightLineStartPoint;
        private SnapAction snapAction;
        private StraightConnectorViewModel item;

        public NDrawStraightLineBehavior()
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
                _straightLineStartPoint = e.GetPosition(AssociatedObject);
                var viewModel = AssociatedObject.DataContext as IDiagramViewModel;
                item = new StraightConnectorViewModel(viewModel, _straightLineStartPoint.Value);
                e.Handled = true;
            }
        }

        private void AssociatedObject_TouchDown(object sender, TouchEventArgs e)
        {
            if (e.Source == AssociatedObject)
            {
                var touchPoint = e.GetTouchPoint(AssociatedObject);
                _straightLineStartPoint = touchPoint.Position;
                var viewModel = AssociatedObject.DataContext as IDiagramViewModel;
                item = new StraightConnectorViewModel(viewModel, _straightLineStartPoint.Value);
            }
        }

        private void AssociatedObject_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (e.Source == AssociatedObject)
                {
                    _straightLineStartPoint = e.GetPosition(AssociatedObject);
                    var viewModel = AssociatedObject.DataContext as IDiagramViewModel;
                    item = new StraightConnectorViewModel(viewModel, _straightLineStartPoint.Value);
                    e.Handled = true;
                }
            }
        }

        private void AssociatedObject_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.StylusDevice != null)
                return;

            var canvas = AssociatedObject as DesignerCanvas;
            Point current = e.GetPosition(canvas);
            var ellipses = (AssociatedObject.DataContext as DiagramViewModel).AllItems.Value.OfType<NEllipseViewModel>();
            var pies = (AssociatedObject.DataContext as DiagramViewModel).AllItems.Value.OfType<NPieViewModel>();
            var appendIntersectionPoints = new List<Tuple<Point, object>>();
            Vector vec = new Vector();
            if (_straightLineStartPoint.HasValue)
            {
                vec = Point.Subtract(current, _straightLineStartPoint.Value);
                snapAction.SnapIntersectionOfEllipseAndTangent(ellipses, _straightLineStartPoint.Value, current, appendIntersectionPoints);
                snapAction.SnapIntersectionOfPieAndTangent(pies, _straightLineStartPoint.Value, current, appendIntersectionPoints);
            }
            snapAction.OnMouseMove(ref current, vec, appendIntersectionPoints);

            if (e.LeftButton != MouseButtonState.Pressed)
                _straightLineStartPoint = null;

            if (_straightLineStartPoint.HasValue)
            {
                _straightLineStartPoint = current;
                (App.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = boilersGraphics.Properties.Resources.String_Draw;

                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(canvas);
                if (adornerLayer != null)
                {
                    var adorner = new Adorners.StraightLineAdorner(canvas, _straightLineStartPoint, item);
                    if (adorner != null)
                    {
                        adornerLayer.Add(adorner);
                    }
                }
            }
        }

        private void AssociatedObject_StylusMove(object sender, StylusEventArgs e)
        {
            var canvas = AssociatedObject as DesignerCanvas;
            Point current = e.GetPosition(canvas);
            var ellipses = (AssociatedObject.DataContext as DiagramViewModel).AllItems.Value.OfType<NEllipseViewModel>();
            var pies = (AssociatedObject.DataContext as DiagramViewModel).AllItems.Value.OfType<NPieViewModel>();
            var appendIntersectionPoints = new List<Tuple<Point, object>>();
            Vector vec = new Vector();
            if (_straightLineStartPoint.HasValue)
            {
                vec = Point.Subtract(current, _straightLineStartPoint.Value);
                snapAction.SnapIntersectionOfEllipseAndTangent(ellipses, _straightLineStartPoint.Value, current, appendIntersectionPoints);
                snapAction.SnapIntersectionOfPieAndTangent(pies, _straightLineStartPoint.Value, current, appendIntersectionPoints);
            }
            snapAction.OnMouseMove(ref current, vec, appendIntersectionPoints);

            if (e.InAir)
                _straightLineStartPoint = null;

            if (_straightLineStartPoint.HasValue)
            {
                _straightLineStartPoint = current;
                (App.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = boilersGraphics.Properties.Resources.String_Draw;

                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(canvas);
                if (adornerLayer != null)
                {
                    var adorner = new Adorners.StraightLineAdorner(canvas, _straightLineStartPoint, item);
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
