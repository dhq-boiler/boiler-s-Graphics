using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.ViewModels;
using Microsoft.Xaml.Behaviors;
using NLog;
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
                e.Handled = true;
            }
        }

        private void AssociatedObject_TouchDown(object sender, TouchEventArgs e)
        {
            if (e.Source == AssociatedObject)
            {
                var touchPoint = e.GetTouchPoint(AssociatedObject);
                _straightLineStartPoint = touchPoint.Position;
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
                    snapAction.PostProcess(SnapPointPosition.BeginEdge, item);
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
            var appendIntersectionPoints = new List<Point>();
            if (_straightLineStartPoint.HasValue)
            {
                foreach (var ellipse in ellipses)
                {
                    var snapPower = (App.Current.MainWindow.DataContext as MainWindowViewModel).SnapPower.Value;
                    var array = new List<Tuple<Point[], double>>();
                    for (double y = -snapPower; y < snapPower; y++)
                    {
                        for (double x = -snapPower; x < snapPower; x++)
                        {
                            var tuple = Intersection.FindEllipseSegmentIntersections(ellipse, _straightLineStartPoint.Value, new Point(current.X + x, current.Y + y), false);
                            array.Add(tuple);
                        }
                    }
                    var minDiscriminant = array.FirstOrDefault(x => Math.Abs(x.Item2) == array.Min(x => Math.Abs(x.Item2)));
                    if (minDiscriminant != null && minDiscriminant.Item1.Count() == 1)
                    {
                        appendIntersectionPoints.AddRange(minDiscriminant.Item1);
                    }
                }
            }
            snapAction.OnMouseMove(ref current, appendIntersectionPoints);

            if (e.LeftButton != MouseButtonState.Pressed)
                _straightLineStartPoint = null;

            if (_straightLineStartPoint.HasValue)
            {
                _straightLineStartPoint = current;
                (App.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = "描画";

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
            snapAction.OnMouseMove(ref current);

            if (e.InAir)
                _straightLineStartPoint = null;

            if (_straightLineStartPoint.HasValue)
            {
                _straightLineStartPoint = current;
                (App.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = "描画";

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
