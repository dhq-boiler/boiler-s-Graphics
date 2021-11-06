﻿using boilersGraphics.Controls;
using boilersGraphics.Dao;
using boilersGraphics.Helpers;
using boilersGraphics.ViewModels;
using Microsoft.Xaml.Behaviors;
using Prism.Services.Dialogs;
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
            currentBrush = BrushViewModel.CreateInstance();
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

        private bool downFlag = false;

        private void AssociatedObject_StylusDown(object sender, StylusDownEventArgs e)
        {
            if (downFlag)
                return;

            if (e.Source == AssociatedObject)
            {
                (AssociatedObject.DataContext as DiagramViewModel).MainWindowVM.Recorder.BeginRecode();

                if (!(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
                {
                    currentBrush.CloseThicknessDialog();
                    currentBrush.Dispose();
                    currentBrush = null;
                }
                e.StylusDevice.Capture(AssociatedObject);
                var point = e.GetPosition(AssociatedObject);
                if (currentBrush == null)
                {
                    currentBrush = BrushViewModel.CreateInstance();
                    currentBrush.OpenThicknessDialog();
                }
                BrushInternal.Down((AssociatedObject.DataContext as DiagramViewModel).MainWindowVM, AssociatedObject, ref currentBrush, () => e.StylusDevice.Capture(AssociatedObject), e, point);
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
                (AssociatedObject.DataContext as DiagramViewModel).MainWindowVM.Recorder.BeginRecode();

                if (!(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
                {
                    currentBrush.CloseThicknessDialog();
                    currentBrush.Dispose();
                    currentBrush = null;
                }
                e.TouchDevice.Capture(AssociatedObject);
                var touchPoint = e.GetTouchPoint(AssociatedObject);
                var point = touchPoint.Position;
                if (currentBrush == null)
                {
                    currentBrush = BrushViewModel.CreateInstance();
                    currentBrush.OpenThicknessDialog();
                }
                BrushInternal.Down((AssociatedObject.DataContext as DiagramViewModel).MainWindowVM, AssociatedObject, ref currentBrush, () => e.TouchDevice.Capture(AssociatedObject), e, point);
                downFlag = true;
            }
        }

        private void AssociatedObject_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (downFlag)
                return;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                (AssociatedObject.DataContext as DiagramViewModel).MainWindowVM.Recorder.BeginRecode();

                if (e.Source == AssociatedObject)
                {
                    if (!(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
                    {
                        currentBrush.CloseThicknessDialog();
                        currentBrush.Dispose();
                        currentBrush = null;
                    }
                    e.MouseDevice.Capture(AssociatedObject);
                    var point = e.GetPosition(AssociatedObject);
                    if (currentBrush == null)
                    {
                        currentBrush = BrushViewModel.CreateInstance();
                        currentBrush.OpenThicknessDialog();
                    }
                    BrushInternal.Down((AssociatedObject.DataContext as DiagramViewModel).MainWindowVM, AssociatedObject, ref currentBrush, () => e.MouseDevice.Capture(AssociatedObject), e, point);
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
            BrushInternal.Draw((AssociatedObject.DataContext as DiagramViewModel).MainWindowVM, ref currentBrush, point);
        }

        private void AssociatedObject_StylusMove(object sender, StylusEventArgs e)
        {
            if (!downFlag)
                return;

            var point = e.GetPosition(AssociatedObject);
            BrushInternal.Draw((AssociatedObject.DataContext as DiagramViewModel).MainWindowVM, ref currentBrush, point);
        }

        private void AssociatedObject_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!downFlag)
                return;

            (AssociatedObject.DataContext as DiagramViewModel).MainWindowVM.Recorder.EndRecode();

            UpdateStatisticsCount();

            // release mouse capture
            if (AssociatedObject.IsMouseCaptured) AssociatedObject.ReleaseMouseCapture();
            // release stylus capture
            if (AssociatedObject.IsStylusCaptured) AssociatedObject.ReleaseStylusCapture();

            downFlag = false;
        }

        private static void UpdateStatisticsCount()
        {
            var statistics = (App.Current.MainWindow.DataContext as MainWindowViewModel).Statistics.Value;
            statistics.BrushToolDrawCount++;
            var dao = new StatisticsDao();
            dao.Update(statistics);
        }

        private void AssociatedObject_TouchUp(object sender, TouchEventArgs e)
        {
            if (!downFlag)
                return;

            (AssociatedObject.DataContext as DiagramViewModel).MainWindowVM.Recorder.EndRecode();

            // release touch capture
            if (e.TouchDevice.Captured != null) AssociatedObject.ReleaseTouchCapture(e.TouchDevice);

            downFlag = false;
        }
    }
}
