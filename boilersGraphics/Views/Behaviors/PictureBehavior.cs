using boilersGraphics.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Input;
using System.Windows.Documents;
using boilersGraphics.Adorners;
using boilersGraphics.Extensions;
using boilersGraphics.ViewModels;
using System.Diagnostics;
using boilersGraphics.Helpers;

namespace boilersGraphics.Views.Behaviors
{
    internal class PictureBehavior : Behavior<DesignerCanvas>
    {
        private Point? _pictureDrawingStartPoint = null;
        private string _filename;
        private double _Width;
        private double _Height;
        private SnapAction _snapAction;

        public PictureBehavior(string filename, double width, double height)
        {
            _filename = filename;
            _Width = width;
            _Height = height;
            _snapAction = new SnapAction();
        }

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

        private void AssociatedObject_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var canvas = AssociatedObject as DesignerCanvas;
            _pictureDrawingStartPoint = e.GetPosition(AssociatedObject);
            _snapAction.OnMouseMove(ref _pictureDrawingStartPoint);

            if (e.LeftButton != MouseButtonState.Pressed)
                _pictureDrawingStartPoint = null;

            if (_pictureDrawingStartPoint.HasValue)
            {
                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(canvas);
                if (adornerLayer != null)
                {
                    PictureAdorner adorner = new PictureAdorner(canvas, _pictureDrawingStartPoint, _filename, _Width, _Height);
                    if (adorner != null)
                    {
                        adornerLayer.Add(adorner);
                    }
                }
            }
            e.Handled = true;
        }

        private void AssociatedObject_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // release mouse capture
            if (AssociatedObject.IsMouseCaptured) AssociatedObject.ReleaseMouseCapture();
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
