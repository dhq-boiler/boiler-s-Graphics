using boilersGraphics.Controls;
using boilersGraphics.Helpers;
using boilersGraphics.ViewModels;
using Microsoft.Xaml.Behaviors;
using Prism.Services.Dialogs;
using System;
using System.Windows.Input;

namespace boilersGraphics.Views.Behaviors
{
    public class BrushBehavior : Behavior<DesignerCanvas>
    {
        private BrushViewModel currentBrush;
        private IDialogService dlgService;

        public BrushBehavior(IDialogService dlgService)
        {
            this.dlgService = dlgService;
        }

        protected override void OnAttached()
        {
            //this.AssociatedObject.GotFocus += AssociatedObject_GotFocus;
            //this.AssociatedObject.LostFocus += AssociatedObject_LostFocus;
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
            //this.AssociatedObject.GotFocus -= AssociatedObject_GotFocus;
            //this.AssociatedObject.LostFocus -= AssociatedObject_LostFocus;
            this.AssociatedObject.StylusDown -= AssociatedObject_StylusDown;
            this.AssociatedObject.StylusMove -= AssociatedObject_StylusMove;
            this.AssociatedObject.TouchDown -= AssociatedObject_TouchDown;
            this.AssociatedObject.MouseDown -= AssociatedObject_MouseDown;
            this.AssociatedObject.MouseMove -= AssociatedObject_MouseMove;
            this.AssociatedObject.MouseUp -= AssociatedObject_MouseUp;
            this.AssociatedObject.TouchUp -= AssociatedObject_TouchUp;
            base.OnDetaching();
        }

        public event EventHandler ThicknessDialogClose;

        //private void AssociatedObject_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        //{
        //    dlgService.Show(nameof(Thickness), new DialogParameters() { { "Behavior", this } }, null);
        //}

        //private void AssociatedObject_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        //{
        //    ThicknessDialogClose?.Invoke(this, new EventArgs());
        //}

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
