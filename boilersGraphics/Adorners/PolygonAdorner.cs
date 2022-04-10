﻿using boilersGraphics.Controls;
using boilersGraphics.Dao;
using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
        private string _data;
        private ObservableCollection<Corner> _corners;
        private SnapAction _snapAction;

        public PolygonAdorner(DesignerCanvas designerCanvas, Point? dragStartPoint, ObservableCollection<Corner> corners, string data)
            : base(designerCanvas)
        {
            _designerCanvas = designerCanvas;
            _dragStartPoint = dragStartPoint;
            var parent = (AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel;
            var brush = parent.EdgeBrush.Value.Clone();
            brush.Opacity = 0.5;
            _edgePen = new Pen(brush, parent.EdgeThickness.Value.Value);
            _data = data;
            _corners = corners;
            _snapAction = new SnapAction();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (!this.IsMouseCaptured)
                    this.CaptureMouse();

                //ドラッグ終了座標を更新
                _dragEndPoint = e.GetPosition(this);
                var currentPosition = _dragEndPoint.Value;
                _snapAction.OnMouseMove(ref currentPosition);
                _dragEndPoint = currentPosition;

                (App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel.CurrentPoint = currentPosition;
                (App.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = $"({_dragStartPoint.Value.X}, {_dragStartPoint.Value.Y}) - ({_dragEndPoint.Value.X}, {_dragEndPoint.Value.Y}) (w, h) = ({_dragEndPoint.Value.X - _dragStartPoint.Value.X}, {_dragEndPoint.Value.Y - _dragStartPoint.Value.Y})";

                this.InvalidateVisual();
            }
            else
            {
                if (this.IsMouseCaptured) this.ReleaseMouseCapture();
            }

            e.Handled = true;
        }

        protected override void OnMouseUp(System.Windows.Input.MouseButtonEventArgs e)
        {
            // release mouse capture
            if (this.IsMouseCaptured) this.ReleaseMouseCapture();

            _snapAction.OnMouseUp(this);

            if (_dragStartPoint.HasValue && _dragEndPoint.HasValue)
            {
                var item = new NPolygonViewModel();
                item.Owner = (AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel;
                item.Data.Value = _data;
                item.Left.Value = Math.Min(_dragStartPoint.Value.X, _dragEndPoint.Value.X);
                item.Top.Value = Math.Min(_dragStartPoint.Value.Y, _dragEndPoint.Value.Y);
                item.Width.Value = Math.Max(_dragStartPoint.Value.X - _dragEndPoint.Value.X, _dragEndPoint.Value.X - _dragStartPoint.Value.X);
                item.PathGeometryNoRotate.Value = null;
                item.Height.Value = Math.Max(_dragStartPoint.Value.Y - _dragEndPoint.Value.Y, _dragEndPoint.Value.Y - _dragStartPoint.Value.Y);
                item.UpdatePathGeometryIfEnable();
                item.EdgeBrush.Value = item.Owner.EdgeBrush.Value.Clone();
                item.FillBrush.Value = item.Owner.FillBrush.Value.Clone();
                item.EdgeThickness.Value = item.Owner.EdgeThickness.Value.Value;
                item.ZIndex.Value = item.Owner.Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children).Count();
                item.SnapPoints.Clear();
                _corners.ToList().ForEach(x => LogManager.GetCurrentClassLogger().Debug($"corner:{x.Point.  Value}"));
                item.SnapPoints.AddRange(_corners.Select(x =>
                {
                    var itemPathGeometry = item.PathGeometry.Value;
                    var _x = (-_corners.Select(x => x.Point.Value.X).Min() + x.Point.Value.X) / itemPathGeometry.Bounds.Width * item.Width.Value - 3;
                    var _y = (-_corners.Select(x => x.Point.Value.Y).Min() + x.Point.Value.Y) / itemPathGeometry.Bounds.Height * item.Height.Value - 3;
                    LogManager.GetCurrentClassLogger().Debug($"_x:{_x}, _y:{_y}");
                    var snapPoint = new SnapPoint(_x, _y);
                    snapPoint.Tag = "頂点";
                    snapPoint.Width = 5;
                    snapPoint.Height = 5;
                    snapPoint.SnapPointPosition = SnapPointPosition.Vertex;

                    var controlTemplate = new ControlTemplate(typeof(SnapPoint));
                    var ellipse = new FrameworkElementFactory(typeof(System.Windows.Shapes.Ellipse));
                    ellipse.SetValue(WidthProperty, 5d);
                    ellipse.SetValue(HeightProperty, 5d);
                    ellipse.SetValue(System.Windows.Shapes.Shape.FillProperty, new SolidColorBrush(Colors.Red));
                    ellipse.SetValue(OpacityProperty, 0d);
                    controlTemplate.VisualTree = ellipse;
                    snapPoint.Template = controlTemplate;
                    return snapPoint;
                }));
                item.IsSelected.Value = true;
                item.IsVisible.Value = true;
                item.Owner.DeselectAll();
                ((AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel).AddItemCommand.Execute(item);

                UpdateStatisticsCount();

                _dragStartPoint = null;
                _dragEndPoint = null;
            }

            (App.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = "";
            (App.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = "";

            e.Handled = true;
        }
        private static void UpdateStatisticsCount()
        {
            var statistics = (App.Current.MainWindow.DataContext as MainWindowViewModel).Statistics.Value;
            statistics.NumberOfDrawsOfPolygonTool++;
            var dao = new StatisticsDao();
            dao.Update(statistics);
        }
        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));

            if (_dragStartPoint.HasValue && _dragEndPoint.HasValue)
            {
                var diff = _dragEndPoint.Value - _dragStartPoint.Value;
                var points = new List<Point>();
                points.AddRange(_corners.Select(x => x.Point.Value));
                var width = points.Select(x => x.X).Max() - points.Select(x => x.X).Min();
                var height = points.Select(y => y.Y).Max() - points.Select(y => y.Y).Min();

                var geometry = new StreamGeometry();
                geometry.FillRule = FillRule.EvenOdd;
                using (StreamGeometryContext ctx = geometry.Open())
                {
                    ctx.BeginFigure(_corners.Skip(1).First().Point.Value.Multiple(diff.X / width, diff.Y / height)
                                                                .Shift((_dragStartPoint.Value.X + _dragEndPoint.Value.X) / 2, (_dragStartPoint.Value.Y + _dragEndPoint.Value.Y) / 2), true, true);

                    foreach (var corner in _corners.Skip(1))
                    {
                        ctx.LineTo(corner.Point.Value.Multiple(diff.X / width, diff.Y / height)
                                                     .Shift((_dragStartPoint.Value.X + _dragEndPoint.Value.X) / 2, (_dragStartPoint.Value.Y + _dragEndPoint.Value.Y) / 2), true, false);
                    }
                }
                geometry.Freeze();
                dc.DrawGeometry(((AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel).FillBrush.Value.Clone(), _edgePen, geometry);
            }
        }
    }
}
