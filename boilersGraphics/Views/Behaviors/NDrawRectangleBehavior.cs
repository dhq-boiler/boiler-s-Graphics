using boilersGraphics.Controls;
using boilersGraphics.ViewModels;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;
using boilersGraphics.Extensions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace boilersGraphics.Views.Behaviors
{
    internal class NDrawRectangleBehavior : Behavior<DesignerCanvas>
    {
        private Point? _rectangleStartPoint;
        private Dictionary<Point, Adorner> _adorners;

        public NDrawRectangleBehavior()
        {
            _adorners = new Dictionary<Point, Adorner>();
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

        private void AssociatedObject_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (e.Source == AssociatedObject)
                {
                    _rectangleStartPoint = e.GetPosition(AssociatedObject);

                    e.Handled = true;
                }
            }
        }

        private void AssociatedObject_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var canvas = AssociatedObject as DesignerCanvas;
            var mainWindowVM = (App.Current.MainWindow.DataContext as MainWindowViewModel);
            var designerCanvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();
            var diagramVM = mainWindowVM.DiagramViewModel;
            var snapPoints = diagramVM.SnapPoints;
            Point? snapped = null;
            var currentPoint = e.GetPosition(AssociatedObject);
            foreach (var snapPoint in snapPoints)
            {
                if (currentPoint.X > snapPoint.X - mainWindowVM.SnapPower.Value
                    && currentPoint.X < snapPoint.X + mainWindowVM.SnapPower.Value
                    && currentPoint.Y > snapPoint.Y - mainWindowVM.SnapPower.Value
                    && currentPoint.Y < snapPoint.Y + mainWindowVM.SnapPower.Value)
                {
                    //スナップする座標を一時変数へ保存
                    snapped = snapPoint;
                }
            }

            if (snapped != null)
            {
                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(designerCanvas);
                RemoveFromAdornerLayerAndDictionary(snapped, adornerLayer);

                //ドラッグ終了座標を一時変数で上書きしてスナップ
                _rectangleStartPoint = snapped;
                if (adornerLayer != null)
                {
                    Trace.WriteLine($"Snap={snapped.Value}");
                    if (!_adorners.ContainsKey(snapped.Value))
                    {
                        var adorner = new Adorners.SnapPointAdorner(designerCanvas, snapped.Value);
                        if (adorner != null)
                        {
                            adornerLayer.Add(adorner);

                            //ディクショナリに記憶する
                            _adorners.Add(snapped.Value, adorner);
                        }
                    }
                }
            }
            else //スナップしなかった場合
            {
                RemoveAllAdornerFromAdornerLayerAndDictionary(designerCanvas);
            }

            if (e.LeftButton != MouseButtonState.Pressed)
                _rectangleStartPoint = null;

            if (_rectangleStartPoint.HasValue)
            {
                (App.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = "描画";

                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(canvas);
                if (adornerLayer != null)
                {
                    var adorner = new Adorners.RectangleAdorner(canvas, _rectangleStartPoint);
                    if (adorner != null)
                    {
                        adornerLayer.Add(adorner);
                    }
                }
            }
        }

        private void AssociatedObject_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // release mouse capture
            if (AssociatedObject.IsMouseCaptured) AssociatedObject.ReleaseMouseCapture();
        }

        private void RemoveAllAdornerFromAdornerLayerAndDictionary(DesignerCanvas designerCanvas)
        {
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(designerCanvas);
            var removes = _adorners.ToList();

            removes.ForEach(x =>
            {
                if (adornerLayer != null)
                {
                    adornerLayer.Remove(x.Value);
                }
                _adorners.Remove(x.Key);
            });
        }

        private void RemoveFromAdornerLayerAndDictionary(Point? snapped, AdornerLayer adornerLayer)
        {
            var removes = _adorners.Where(x => x.Key != snapped)
                                                       .ToList();
            removes.ForEach(x =>
            {
                if (adornerLayer != null)
                {
                    adornerLayer.Remove(x.Value);
                }
                _adorners.Remove(x.Key);
            });
        }
    }
}
