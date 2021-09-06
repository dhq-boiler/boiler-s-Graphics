﻿using boilersGraphics.Controls;
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

        public BrushViewModel CurrentBrush { get { return currentBrush; } }

        public BrushBehavior(IDialogService dlgService)
        {
            this.dlgService = dlgService;
            currentBrush = new BrushViewModel();
        }

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

        public event EventHandler ThicknessDialogClose;

        private bool downFlag = false;

        private void AssociatedObject_StylusDown(object sender, StylusDownEventArgs e)
        {
            if (downFlag)
                return;

            if (e.Source == AssociatedObject)
            {
                if (!(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
                {
                    currentBrush.CloseThicknessDialog();
                    currentBrush = null;
                }
                e.StylusDevice.Capture(AssociatedObject);
                var point = e.GetPosition(AssociatedObject);
                if (currentBrush == null)
                {
                    currentBrush = new BrushViewModel();
                    currentBrush.Thickness.Value = new System.Windows.Thickness(1);
                    currentBrush.OpenThicknessDialog();
                }
                BrushInternal.Down(AssociatedObject, ref currentBrush, e, point);
                downFlag = true;
                e.Handled = true;
            }
        }

        private void AssociatedObject_TouchDown(object sender, TouchEventArgs e)
        {
            if (downFlag)
                return;

            if (e.Source == AssociatedObject)
            {
                if (!(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
                {
                    currentBrush.CloseThicknessDialog();
                    currentBrush = null;
                }
                e.TouchDevice.Capture(AssociatedObject);
                var touchPoint = e.GetTouchPoint(AssociatedObject);
                var point = touchPoint.Position;
                if (currentBrush == null)
                {
                    currentBrush = new BrushViewModel();
                    currentBrush.Thickness.Value = new System.Windows.Thickness(1);
                    currentBrush.OpenThicknessDialog();
                }
                BrushInternal.Down(AssociatedObject, ref currentBrush, e, point);
                downFlag = true;
            }
        }

        private void AssociatedObject_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (downFlag)
                return;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (e.Source == AssociatedObject)
                {
                    if (!(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
                    {
                        currentBrush.CloseThicknessDialog();
                        currentBrush = null;
                    }
                    var point = e.GetPosition(AssociatedObject);
                    if (currentBrush == null)
                    {
                        currentBrush = new BrushViewModel();
                        currentBrush.Thickness.Value = new System.Windows.Thickness(1);
                        currentBrush.OpenThicknessDialog();
                    }
                    BrushInternal.Down(AssociatedObject, ref currentBrush, e, point);
                    downFlag = true;
                }
            }
        }

        private void AssociatedObject_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!downFlag)
                return;

            if (e.StylusDevice != null)
                return;

            var point = e.GetPosition(AssociatedObject);
            BrushInternal.Draw(ref currentBrush, point);
        }

        private void AssociatedObject_StylusMove(object sender, StylusEventArgs e)
        {
            if (!downFlag)
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

            downFlag = false;
        }

        private void AssociatedObject_TouchUp(object sender, TouchEventArgs e)
        {
            // release touch capture
            if (e.TouchDevice.Captured != null) AssociatedObject.ReleaseTouchCapture(e.TouchDevice);
        }
    }
}
