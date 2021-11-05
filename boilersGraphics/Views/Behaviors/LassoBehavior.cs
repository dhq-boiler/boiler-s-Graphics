using boilersGraphics.Adorners;
using boilersGraphics.Controls;
using boilersGraphics.ViewModels;
using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace boilersGraphics.Views.Behaviors
{
    /// <summary>
    /// なげなわツールのビヘイビア
    /// </summary>
    public class LassoBehavior : Behavior<DesignerCanvas>
    {
        private Point? _lassoSelectionStartPoint = null;

        protected override void OnAttached()
        {
            this.AssociatedObject.StylusDown += AssociatedObject_StylusDown;
            this.AssociatedObject.StylusMove += AssociatedObject_StylusMove;
            this.AssociatedObject.TouchDown += AssociatedObject_TouchDown;
            this.AssociatedObject.MouseDown += AssociatedObject_MouseDown;
            this.AssociatedObject.MouseMove += AssociatedObject_MouseMove;
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            this.AssociatedObject.StylusDown -= AssociatedObject_StylusDown;
            this.AssociatedObject.StylusMove -= AssociatedObject_StylusMove;
            this.AssociatedObject.TouchDown -= AssociatedObject_TouchDown;
            this.AssociatedObject.MouseDown -= AssociatedObject_MouseDown;
            this.AssociatedObject.MouseMove -= AssociatedObject_MouseMove;
            base.OnDetaching();
        }

        private void AssociatedObject_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var canvas = AssociatedObject as DesignerCanvas;
            // if mouse button is not pressed we have no drag operation, ...
            if (e.LeftButton != MouseButtonState.Pressed)
                _lassoSelectionStartPoint = null;

            // ... but if mouse button is pressed and start
            // point value is set we do have one
            if (_lassoSelectionStartPoint.HasValue)
            {
                // create rubberband adorner
                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(canvas);
                if (adornerLayer != null)
                {
                    RubberbandAdorner adorner = new RubberbandAdorner(canvas, _lassoSelectionStartPoint);
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

            if (e.InAir)
                _lassoSelectionStartPoint = null;

            if (_lassoSelectionStartPoint.HasValue)
            {
                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(canvas);
                if (adornerLayer != null)
                {
                    RubberbandAdorner adorner = new RubberbandAdorner(canvas, _lassoSelectionStartPoint);
                    if (adorner != null)
                    {
                        adornerLayer.Add(adorner);
                    }
                }
            }
        }

        private void AssociatedObject_StylusDown(object sender, StylusDownEventArgs e)
        {
            if (e.Source == AssociatedObject)
            {
                _lassoSelectionStartPoint = e.GetPosition(AssociatedObject);

                (App.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = "範囲選択";

                IDiagramViewModel vm = (AssociatedObject.DataContext as IDiagramViewModel);
                if (!(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
                {
                    vm.ClearSelectedItemsCommand.Execute(null);
                }
                e.Handled = true;
            }
        }

        private void AssociatedObject_TouchDown(object sender, TouchEventArgs e)
        {
            if (e.Source == AssociatedObject)
            {
                var touchPoint = e.GetTouchPoint(AssociatedObject);
                _lassoSelectionStartPoint = touchPoint.Position;

                (App.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = "範囲選択";

                IDiagramViewModel vm = (AssociatedObject.DataContext as IDiagramViewModel);
                if (!(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
                {
                    vm.ClearSelectedItemsCommand.Execute(null);
                }
            }
        }

        private void AssociatedObject_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.StylusDevice != null)
                return;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                //if we are source of event, we are rubberband selecting
                if (e.Source == AssociatedObject)
                {
                    // in case that this click is the start for a 
                    // drag operation we cache the start point
                    _lassoSelectionStartPoint = e.GetPosition(AssociatedObject);

                    (App.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = "範囲選択";

                    IDiagramViewModel vm = (AssociatedObject.DataContext as IDiagramViewModel);
                    if (!(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
                    {
                        vm.ClearSelectedItemsCommand.Execute(null);
                    }
                    e.Handled = true;
                }
            }
        }
    }
}
