using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class PolygonAdorner : Adorner
    {
        private Point? _dragStartPoint;
        private Point? _dragEndPoint;
        private Pen _edgePen;
        private DesignerCanvas _designerCanvas;
        private Dictionary<Point, Adorner> _adorners;
        private string _data;
        private readonly Point _startPoint;
        private ObservableCollection<Corner> _corners;

        public PolygonAdorner(DesignerCanvas designerCanvas, Point? dragStartPoint, ObservableCollection<Corner> corners, string data, Point startPoint)
            : base(designerCanvas)
        {
            _designerCanvas = designerCanvas;
            _dragStartPoint = dragStartPoint;
            var parent = (AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel;
            var brush = new SolidColorBrush(parent.EdgeColors.First());
            brush.Opacity = 0.5;
            _edgePen = new Pen(brush, parent.EdgeThickness.Value.Value);
            _adorners = new Dictionary<Point, Adorner>();
            _data = data;
            _startPoint = startPoint;
            _corners = corners;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (!this.IsMouseCaptured)
                    this.CaptureMouse();

                //ドラッグ終了座標を更新
                _dragEndPoint = e.GetPosition(this);

                var mainWindowVM = (App.Current.MainWindow.DataContext as MainWindowViewModel);
                var designerCanvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();

                var diagramVM = mainWindowVM.DiagramViewModel;
                if (diagramVM.EnablePointSnap.Value)
                {
                    var snapPoints = diagramVM.SnapPoints;
                    Point? snapped = null;
                    foreach (var snapPoint in snapPoints)
                    {
                        if (_dragEndPoint.Value.X > snapPoint.X - mainWindowVM.SnapPower.Value
                         && _dragEndPoint.Value.X < snapPoint.X + mainWindowVM.SnapPower.Value
                         && _dragEndPoint.Value.Y > snapPoint.Y - mainWindowVM.SnapPower.Value
                         && _dragEndPoint.Value.Y < snapPoint.Y + mainWindowVM.SnapPower.Value)
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
                        _dragEndPoint = snapped;
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

                (App.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = $"({_dragStartPoint.Value.X}, {_dragStartPoint.Value.Y}) - ({_dragEndPoint.Value.X}, {_dragEndPoint.Value.Y}) (w, h) = ({_dragEndPoint.Value.X - _dragStartPoint.Value.X}, {_dragEndPoint.Value.Y - _dragStartPoint.Value.Y})";

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

            if (_dragStartPoint.HasValue && _dragEndPoint.HasValue)
            {
                var item = new NPolygonViewModel();
                item.Owner = (AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel;
                item.Left.Value = Math.Min(_dragStartPoint.Value.X, _dragEndPoint.Value.X);
                item.Top.Value = Math.Min(_dragStartPoint.Value.Y, _dragEndPoint.Value.Y);
                item.Width.Value = Math.Max(_dragStartPoint.Value.X - _dragEndPoint.Value.X, _dragEndPoint.Value.X - _dragStartPoint.Value.X);
                item.Height.Value = Math.Max(_dragStartPoint.Value.Y - _dragEndPoint.Value.Y, _dragEndPoint.Value.Y - _dragStartPoint.Value.Y);
                item.EdgeColor = item.Owner.EdgeColors.First();
                item.FillColor = item.Owner.FillColors.First();
                item.EdgeThickness = item.Owner.EdgeThickness.Value.Value;
                item.ZIndex.Value = item.Owner.Items.Count;
                item.Data.Value = _data;
                item.IsSelected = true;
                item.Owner.DeselectAll();
                ((AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel).AddItemCommand.Execute(item);

                _dragStartPoint = null;
                _dragEndPoint = null;
            }

            (App.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = "";
            (App.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = "";

            e.Handled = true;
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));

            if (_dragStartPoint.HasValue && _dragEndPoint.HasValue)
            {
                var diff = _dragEndPoint.Value - _dragStartPoint.Value;
                var points = new List<Point>();
                points.Add(_startPoint);
                points.AddRange(_corners.Select(x => x.Point.Value));
                var width = points.Select(x => x.X).Max() - points.Select(x => x.X).Min();
                var height = points.Select(y => y.Y).Max() - points.Select(y => y.Y).Min();

                var geometry = new StreamGeometry();
                geometry.FillRule = FillRule.EvenOdd;
                using (StreamGeometryContext ctx = geometry.Open())
                {
                    ctx.BeginFigure(_startPoint.Multiple(diff.X / width, diff.Y / height)
                                               .Shift((_dragStartPoint.Value.X + _dragEndPoint.Value.X) / 2, (_dragStartPoint.Value.Y + _dragEndPoint.Value.Y) / 2), true, true);

                    foreach (var corner in _corners)
                    {
                        ctx.LineTo(corner.Point.Value.Multiple(diff.X / width, diff.Y / height)
                                                     .Shift((_dragStartPoint.Value.X + _dragEndPoint.Value.X) / 2, (_dragStartPoint.Value.Y + _dragEndPoint.Value.Y) / 2), true, false);
                    }
                }
                geometry.Freeze();
                dc.DrawGeometry(Brushes.Transparent, _edgePen, geometry);
            }
        }
    }
}
