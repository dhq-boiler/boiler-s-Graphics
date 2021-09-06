using boilersGraphics.Controls;
using boilersGraphics.Helpers;
using boilersGraphics.ViewModels;
using Microsoft.Xaml.Behaviors;
using System.Windows.Input;

namespace boilersGraphics.Views.Behaviors
{
    internal class BrushBehavior : Behavior<DesignerCanvas>
    {
        private BrushViewModel currentBrush;

        protected override void OnAttached()
        {
            this.AssociatedObject.StylusDown += AssociatedObject_StylusDown;
            this.AssociatedObject.StylusMove += AssociatedObject_StylusMove;
            this.AssociatedObject.TouchDown += AssociatedObject_TouchDown;
            this.AssociatedObject.MouseDown += AssociatedObject_MouseDown;
            this.AssociatedObject.MouseMove += AssociatedObject_MouseMove;
            this.AssociatedObject.MouseUp += AssociatedObject_MouseUp;
            this.AssociatedObject.TouchUp += AssociatedObject_TouchUp;
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
            this.AssociatedObject.TouchUp -= AssociatedObject_TouchUp;
            base.OnDetaching();
        }

        private void AssociatedObject_StylusDown(object sender, StylusDownEventArgs e)
        {
            if (e.Source == AssociatedObject)
            {
                e.StylusDevice.Capture(AssociatedObject);
                var point = e.GetPosition(AssociatedObject);
                BrushInternal.Down(AssociatedObject, ref currentBrush, e, point);
                e.Handled = true;
            }
        }

        private void AssociatedObject_TouchDown(object sender, TouchEventArgs e)
        {
            if (e.Source == AssociatedObject)
            {
                e.TouchDevice.Capture(AssociatedObject);
                var touchPoint = e.GetTouchPoint(AssociatedObject);
                var point = touchPoint.Position;
                BrushInternal.Down(AssociatedObject, ref currentBrush, e, point);
            }
        }

        private void AssociatedObject_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (e.Source == AssociatedObject)
                {
                    var point = e.GetPosition(AssociatedObject);
                    BrushInternal.Down(AssociatedObject, ref currentBrush, e, point);
                }
            }
        }

        private void AssociatedObject_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (currentBrush == null)
                return;

            if (e.StylusDevice != null)
                return;

            var point = e.GetPosition(AssociatedObject);
            BrushInternal.Draw(ref currentBrush, point);
        }

        private void AssociatedObject_StylusMove(object sender, StylusEventArgs e)
        {
            if (currentBrush == null)
                return;

            var point = e.GetPosition(AssociatedObject);
            BrushInternal.Draw(ref currentBrush, point);
        }

        private void AssociatedObject_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // release mouse capture
            if (AssociatedObject.IsMouseCaptured) AssociatedObject.ReleaseMouseCapture();
            // release stylus capture
            if (AssociatedObject.IsStylusCaptured) AssociatedObject.ReleaseStylusCapture();

            currentBrush = null;
        }

        private void AssociatedObject_TouchUp(object sender, TouchEventArgs e)
        {
            // release touch capture
            if (e.TouchDevice.Captured != null) AssociatedObject.ReleaseTouchCapture(e.TouchDevice);
        }
    }
}
