using grapher.Adorners;
using grapher.Controls;
using grapher.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace grapher.Views.Behaviors
{
    class DrawSettingBehavior : Behavior<DesignerCanvas>
    {
        private Point? settingDrawingStartPoint = null;

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
            if (canvas.SourceConnector == null)
            {
                if (e.LeftButton != MouseButtonState.Pressed)
                    settingDrawingStartPoint = null;

                if (this.settingDrawingStartPoint.HasValue)
                {
                    AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(canvas);
                    if (adornerLayer != null)
                    {
                        SettingAdorner adorner = new SettingAdorner(canvas, settingDrawingStartPoint);
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
                    settingDrawingStartPoint = e.GetPosition(AssociatedObject);

                    e.Handled = true;
                }
            }
        }
    }
}
