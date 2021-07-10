using boilersGraphics.Adorners;
using boilersGraphics.Controls;
using boilersGraphics.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace boilersGraphics.Views.Behaviors
{
    internal class RubberbandBehavior : Behavior<DesignerCanvas>
    {
        private Point? _rubberbandSelectionStartPoint = null;

        protected override void OnAttached()
        {
            this.AssociatedObject.MouseDown += AssociatedObject_MouseDown;
            this.AssociatedObject.MouseMove += AssociatedObject_MouseMove;
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            this.AssociatedObject.MouseDown -= AssociatedObject_MouseDown;
            this.AssociatedObject.MouseMove -= AssociatedObject_MouseMove;
            base.OnDetaching();
        }

        private void AssociatedObject_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var canvas = AssociatedObject as DesignerCanvas;
            // if mouse button is not pressed we have no drag operation, ...
            if (e.LeftButton != MouseButtonState.Pressed)
                _rubberbandSelectionStartPoint = null;

            // ... but if mouse button is pressed and start
            // point value is set we do have one
            if (_rubberbandSelectionStartPoint.HasValue)
            {
                // create rubberband adorner
                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(canvas);
                if (adornerLayer != null)
                {
                    RubberbandAdorner adorner = new RubberbandAdorner(canvas, _rubberbandSelectionStartPoint);
                    if (adorner != null)
                    {
                        adornerLayer.Add(adorner);
                    }
                }
            }
            e.Handled = true;
        }

        private void AssociatedObject_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                //if we are source of event, we are rubberband selecting
                if (e.Source == AssociatedObject)
                {
                    // in case that this click is the start for a 
                    // drag operation we cache the start point
                    _rubberbandSelectionStartPoint = e.GetPosition(AssociatedObject);

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
