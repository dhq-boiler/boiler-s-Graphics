using boilersGraphics.Adorners;
using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace boilersGraphics.Views.Behaviors
{
    internal class NDrawPolygonBehavior : Behavior<DesignerCanvas>
    {
        private Point? _polygonDrawingStartPoint = null;
        private SnapAction _snapAction;

        public NDrawPolygonBehavior(ObservableCollection<Corner> corners, string data, Point startPoint)
        {
            Corners = corners;
            Data = data;
            StartPoint = startPoint;
            _snapAction = new SnapAction();
        }

        public ObservableCollection<Corner> Corners { get; }
        public string Data { get; }
        public Point StartPoint { get; }

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
                    _polygonDrawingStartPoint = e.GetPosition(AssociatedObject);

                    e.Handled = true;
                }
            }
        }

        private void AssociatedObject_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var canvas = AssociatedObject as DesignerCanvas;
            _polygonDrawingStartPoint = e.GetPosition(AssociatedObject);
            _snapAction.OnMouseMove(ref _polygonDrawingStartPoint);

            if (e.LeftButton != MouseButtonState.Pressed)
                _polygonDrawingStartPoint = null;

            if (_polygonDrawingStartPoint.HasValue)
            {
                (App.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = "描画";

                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(canvas);
                if (adornerLayer != null)
                {
                    var adorner = new Adorners.PolygonAdorner(canvas, _polygonDrawingStartPoint, Corners, Data, StartPoint);
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
