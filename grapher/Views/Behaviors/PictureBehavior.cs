using grapher.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Input;
using System.Windows.Documents;
using grapher.Adorners;

namespace grapher.Views.Behaviors
{
    internal class PictureBehavior : Behavior<DesignerCanvas>
    {
        private Point? _pictureDrawingStartPoint = null;
        private string _filename;
        private bool _LeftShiftKeyIsPressed;
        private bool _RightShiftKeyIsPressed;
        public PictureBehavior(string filename)
        {
            _filename = filename;
        }

        protected override void OnAttached()
        {
            this.AssociatedObject.MouseDown += AssociatedObject_MouseDown;
            this.AssociatedObject.MouseMove += AssociatedObject_MouseMove;
            this.AssociatedObject.PreviewKeyDown += AssociatedObject_PreviewKeyDown;
            this.AssociatedObject.PreviewKeyUp += AssociatedObject_PreviewKeyUp;
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            this.AssociatedObject.MouseDown -= AssociatedObject_MouseDown;
            this.AssociatedObject.MouseMove -= AssociatedObject_MouseMove;
            this.AssociatedObject.PreviewKeyDown -= AssociatedObject_PreviewKeyDown;
            this.AssociatedObject.PreviewKeyUp -= AssociatedObject_PreviewKeyUp;
            base.OnDetaching();
        }

        private void AssociatedObject_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            //set Breakpoint but not fire this event
            if (e.Key == Key.LeftShift)
            {
                _LeftShiftKeyIsPressed = false;
            }
            if (e.Key == Key.RightShift)
            {
                _RightShiftKeyIsPressed = false;
            }
        }

        private void AssociatedObject_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            //set Breakpoint but not fire this event
            if (e.Key == Key.LeftShift)
            {
                _LeftShiftKeyIsPressed = true;
            }
            if (e.Key == Key.RightShift)
            {
                _RightShiftKeyIsPressed = true;
            }
        }

        private void AssociatedObject_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var canvas = AssociatedObject as DesignerCanvas;
            if (canvas.SourceConnector == null)
            {
                if (e.LeftButton != MouseButtonState.Pressed)
                    _pictureDrawingStartPoint = null;

                if (_pictureDrawingStartPoint.HasValue)
                {
                    AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(canvas);
                    if (adornerLayer != null)
                    {
                        PictureAdorner adorner = new PictureAdorner(canvas, _pictureDrawingStartPoint, _filename, _LeftShiftKeyIsPressed, _RightShiftKeyIsPressed);
                        if (adorner != null)
                        {
                            adornerLayer.Add(adorner);
                        }
                    }
                }
            }
            e.Handled = true;
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
