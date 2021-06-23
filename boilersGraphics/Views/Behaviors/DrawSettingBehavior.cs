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
    internal class DrawSettingBehavior : Behavior<DesignerCanvas>
    {
        private Point? _settingDrawingStartPoint = null;

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
                    _settingDrawingStartPoint = null;

                if (_settingDrawingStartPoint.HasValue)
                {
                    AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(canvas);
                    if (adornerLayer != null)
                    {
                        SettingAdorner adorner = new SettingAdorner(canvas, _settingDrawingStartPoint);
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
                    _settingDrawingStartPoint = e.GetPosition(AssociatedObject);

                    e.Handled = true;
                }
            }
        }
    }
}
