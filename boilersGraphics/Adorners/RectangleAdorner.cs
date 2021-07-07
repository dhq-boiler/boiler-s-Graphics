using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace boilersGraphics.Adorners
{
    internal class RectangleAdorner : Adorner
    {
        private DesignerCanvas _designerCanvas;
        private Point? _startPoint;
        private Point? _endPoint;
        private Pen _rectanglePen;
        private Dictionary<Point, Adorner> _adorners;

        public RectangleAdorner(DesignerCanvas designerCanvas, Point? dragStartPoint)
            : base(designerCanvas)
        {
            _designerCanvas = designerCanvas;
            _startPoint = dragStartPoint;
            var parent = (AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel;
            var brush = new SolidColorBrush(parent.EdgeColors.First());
            brush.Opacity = 0.5;
            _rectanglePen = new Pen(brush, 1);
            _adorners = new Dictionary<Point, Adorner>();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (!this.IsMouseCaptured)
                    this.CaptureMouse();

                //ドラッグ終了座標を更新
                _endPoint = e.GetPosition(this);

                var mainWindowVM = (App.Current.MainWindow.DataContext as MainWindowViewModel);
                var designerCanvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();

                var diagramVM = mainWindowVM.DiagramViewModel;
                if (diagramVM.EnablePointSnap.Value)
                {
                    var snapPoints = diagramVM.SnapPoints;
                    Point? snapped = null;
                    foreach (var snapPoint in snapPoints)
                    {
                        if (_endPoint.Value.X > snapPoint.X - mainWindowVM.SnapPower.Value
                         && _endPoint.Value.X < snapPoint.X + mainWindowVM.SnapPower.Value
                         && _endPoint.Value.Y > snapPoint.Y - mainWindowVM.SnapPower.Value
                         && _endPoint.Value.Y < snapPoint.Y + mainWindowVM.SnapPower.Value)
                        {
                            //スナップする座標を一時変数へ保存
                            snapped = snapPoint;
                        }
                    }

                    //スナップした場合
                    if (snapped != null)
                    {
                        AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(designerCanvas);
                        RemoveFromAdornerLayerAndDictionary(snapped, adornerLayer);

                        //ドラッグ終了座標を一時変数で上書きしてスナップ
                        _endPoint = snapped;
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
                }

                (App.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = $"({_startPoint.Value.X}, {_startPoint.Value.Y}) - ({_endPoint.Value.X}, {_endPoint.Value.Y}) (w, h) = ({_endPoint.Value.X - _startPoint.Value.X}, {_endPoint.Value.Y - _startPoint.Value.Y})";

                this.InvalidateVisual();
            }
            else
            {
                if (this.IsMouseCaptured) this.ReleaseMouseCapture();
            }

            e.Handled = true;
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

        protected override void OnMouseUp(System.Windows.Input.MouseButtonEventArgs e)
        {
            // release mouse capture
            if (this.IsMouseCaptured) this.ReleaseMouseCapture();

            // remove this adorner from adorner layer
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(_designerCanvas);
            if (adornerLayer != null)
            {
                adornerLayer.Remove(this);

                foreach (var adorner in _adorners)
                    adornerLayer.Remove(adorner.Value);

                _adorners.Clear();
            }

            if (_startPoint.HasValue && _endPoint.HasValue)
            {
                var rand = new Random();
                var item = new NRectangleViewModel();
                item.Owner = (AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel;
                item.Left.Value = Math.Min(_startPoint.Value.X, _endPoint.Value.X);
                item.Top.Value = Math.Min(_startPoint.Value.Y, _endPoint.Value.Y);
                item.Width.Value = Math.Max(_startPoint.Value.X - _endPoint.Value.X, _endPoint.Value.X - _startPoint.Value.X);
                item.Height.Value = Math.Max(_startPoint.Value.Y - _endPoint.Value.Y, _endPoint.Value.Y - _startPoint.Value.Y);
                item.EdgeColor = item.Owner.EdgeColors.First();
                item.FillColor = item.Owner.FillColors.First();
                item.ZIndex.Value = item.Owner.Items.Count;
                item.IsSelected = true;
                item.Owner.DeselectAll();
                ((AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel).AddItemCommand.Execute(item);

                _startPoint = null;
                _endPoint = null;
            }

            (App.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = "";
            (App.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = "";

            e.Handled = true;
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));

            if (_startPoint.HasValue && _endPoint.HasValue)
                dc.DrawRectangle(Brushes.Transparent, _rectanglePen, new Rect(_startPoint.Value, _endPoint.Value));
        }
    }
}
