using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace boilersGraphics.Adorners
{
    class BezierCurveAdorner : Adorner
    {
        private DesignerCanvas _designerCanvas;
        private Point? _startPoint;
        private Point? _endPoint;
        private Pen _bezierCurvePen;
        private Dictionary<Point, Adorner> _adorners;

        public BezierCurveAdorner(DesignerCanvas designerCanvas, Point? dragStartPoint)
            : base(designerCanvas)
        {
            _designerCanvas = designerCanvas;
            _startPoint = dragStartPoint;
            var parent = (AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel;
            var brush = new SolidColorBrush(parent.EdgeColors.First());
            brush.Opacity = 0.5;
            _bezierCurvePen = new Pen(brush, parent.EdgeThickness.Value.Value);
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
                var points = new List<Point>();
                points.Add(_startPoint.Value);
                points.Add(_endPoint.Value);
                var item = new BezierCurveViewModel(_startPoint.Value, _endPoint.Value, BezierCurve.Evaluate(0.25, points), BezierCurve.Evaluate(0.75, points));
                item.Owner = (AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel;
                item.EdgeColor = item.Owner.EdgeColors.First();
                item.EdgeThickness = item.Owner.EdgeThickness.Value.Value;
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
            {
                var diff = _endPoint.Value - _startPoint.Value;
                var points = new List<Point>();
                points.Add(_startPoint.Value);
                points.Add(_endPoint.Value);
                var width = points.Select(x => x.X).Max() - points.Select(x => x.X).Min();
                var height = points.Select(x => x.Y).Max() - points.Select(x => x.Y).Min();
                var geometry = new StreamGeometry();
                geometry.FillRule = FillRule.EvenOdd;
                using (StreamGeometryContext ctx = geometry.Open())
                {
                    ctx.BeginFigure(_startPoint.Value, true, false);
                    ctx.BezierTo(BezierCurve.Evaluate(0.25, points), BezierCurve.Evaluate(0.75, points), _endPoint.Value, true, false);
                }
                dc.DrawGeometry(Brushes.Transparent, _bezierCurvePen, geometry);
            }
        }

            private Rect ShiftEdgeThickness()
        {
            var parent = (AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel;
            var point1 = _startPoint.Value;
            point1.X += parent.EdgeThickness.Value.Value / 2;
            point1.Y += parent.EdgeThickness.Value.Value / 2;
            var point2 = _endPoint.Value;
            point2.X -= parent.EdgeThickness.Value.Value / 2;
            point2.Y -= parent.EdgeThickness.Value.Value / 2;
            return new Rect(point1, point2);
        }
    }
}