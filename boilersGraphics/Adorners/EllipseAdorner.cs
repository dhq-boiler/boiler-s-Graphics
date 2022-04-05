using boilersGraphics.Controls;
using boilersGraphics.Dao;
using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace boilersGraphics.Adorners
{
    internal class EllipseAdorner : Adorner
    {
        private DesignerCanvas _designerCanvas;
        private Point? _startPoint;
        private Point? _endPoint;
        private Pen _ellipsePen;

        public EllipseAdorner(DesignerCanvas designerCanvas, Point? dragStartPoint)
            : base(designerCanvas)
        {
            _designerCanvas = designerCanvas;
            _startPoint = dragStartPoint;
            var parent = (AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel;
            var brush = parent.EdgeBrush.Value.Clone();
            brush.Opacity = 0.5;
            _ellipsePen = new Pen(brush, parent.EdgeThickness.Value.Value);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (!this.IsMouseCaptured)
                    this.CaptureMouse();

                _endPoint = e.GetPosition(this);

                if ((Keyboard.GetKeyStates(Key.LeftShift) & KeyStates.Down) == KeyStates.Down ||
                    (Keyboard.GetKeyStates(Key.RightShift) & KeyStates.Down) == KeyStates.Down)
                {
                    var diff = _endPoint.Value - _startPoint.Value;
                    var x = Math.Max(diff.X, diff.Y);
                    var y = Math.Max(diff.X, diff.Y);
                    _endPoint = new Point(_startPoint.Value.X + x, _startPoint.Value.Y + y);

                    (App.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = $"({_startPoint.Value.X}, {_startPoint.Value.Y}) - ({_endPoint.Value.X}, {_endPoint.Value.Y}) (w, h) = ({_endPoint.Value.X - _startPoint.Value.X}, {_endPoint.Value.Y - _startPoint.Value.Y})";
                }
                else if ((Keyboard.GetKeyStates(Key.LeftAlt) & KeyStates.Down) == KeyStates.Down ||
                         (Keyboard.GetKeyStates(Key.RightAlt) & KeyStates.Down) == KeyStates.Down)
                {
                    var radiusX = (_endPoint.Value.X - _startPoint.Value.X) / 2;
                    var radiusY = (_endPoint.Value.Y - _startPoint.Value.Y) / 2;
                    var center = _startPoint.Value;
                    var leftTop = new Point(center.X - radiusX, center.Y - radiusY);
                    (App.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = $"({leftTop.X}, {leftTop.Y}) - ({_endPoint.Value.X}, {_endPoint.Value.Y}) (w, h) = ({Math.Max(_startPoint.Value.X - _endPoint.Value.X, _endPoint.Value.X - _startPoint.Value.X)}, {Math.Max(_startPoint.Value.Y - _endPoint.Value.Y, _endPoint.Value.Y - _startPoint.Value.Y)})";
                }
                else
                {
                    (App.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = $"({_startPoint.Value.X}, {_startPoint.Value.Y}) - ({_endPoint.Value.X}, {_endPoint.Value.Y}) (w, h) = ({_endPoint.Value.X - _startPoint.Value.X}, {_endPoint.Value.Y - _startPoint.Value.Y})";
                }

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

            // remove this adorner from adorner layer
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(_designerCanvas);
            if (adornerLayer != null)
                adornerLayer.Remove(this);

            if (_startPoint.HasValue && _endPoint.HasValue)
            {
                var item = new NEllipseViewModel();
                item.Owner = (AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel;

                if ((Keyboard.GetKeyStates(Key.LeftShift) & KeyStates.Down) == KeyStates.Down ||
                    (Keyboard.GetKeyStates(Key.RightShift) & KeyStates.Down) == KeyStates.Down)
                {
                    item.Left.Value = Math.Min(_startPoint.Value.X, _endPoint.Value.X);
                    item.Top.Value = Math.Min(_startPoint.Value.Y, _endPoint.Value.Y);
                    item.Width.Value = Math.Max(_startPoint.Value.X - _endPoint.Value.X, _endPoint.Value.X - _startPoint.Value.X);
                    item.Height.Value = Math.Max(_startPoint.Value.Y - _endPoint.Value.Y, _endPoint.Value.Y - _startPoint.Value.Y);
                }
                else if ((Keyboard.GetKeyStates(Key.LeftAlt) & KeyStates.Down) == KeyStates.Down ||
                         (Keyboard.GetKeyStates(Key.RightAlt) & KeyStates.Down) == KeyStates.Down)
                {
                    var radiusX = (_endPoint.Value.X - _startPoint.Value.X) / 2;
                    var radiusY = (_endPoint.Value.Y - _startPoint.Value.Y) / 2;
                    var center = _startPoint.Value;
                    var leftTop = new Point(center.X - radiusX, center.Y - radiusY);
                    item.Left.Value = leftTop.X;
                    item.Top.Value = leftTop.Y;
                    item.Width.Value = Math.Max(_startPoint.Value.X - _endPoint.Value.X, _endPoint.Value.X - _startPoint.Value.X);
                    item.Height.Value = Math.Max(_startPoint.Value.Y - _endPoint.Value.Y, _endPoint.Value.Y - _startPoint.Value.Y);
                }
                else
                {
                    item.Left.Value = Math.Min(_startPoint.Value.X, _endPoint.Value.X);
                    item.Top.Value = Math.Min(_startPoint.Value.Y, _endPoint.Value.Y);
                    item.Width.Value = Math.Max(_startPoint.Value.X - _endPoint.Value.X, _endPoint.Value.X - _startPoint.Value.X);
                    item.Height.Value = Math.Max(_startPoint.Value.Y - _endPoint.Value.Y, _endPoint.Value.Y - _startPoint.Value.Y);
                }

                Dilate(item);

                item.EdgeBrush.Value = item.Owner.EdgeBrush.Value.Clone();
                item.EdgeThickness.Value = item.Owner.EdgeThickness.Value.Value;
                item.FillBrush.Value = item.Owner.FillBrush.Value.Clone();
                item.ZIndex.Value = item.Owner.Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children).Count();
                item.PathGeometryNoRotate.Value = GeometryCreator.CreateEllipse(item);
                item.IsSelected.Value = true;
                item.IsVisible.Value = true;
                item.Owner.DeselectAll();
                ((AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel).AddItemCommand.Execute(item);

                UpdateStatisticsCount();

                _startPoint = null;
                _endPoint = null;
            }

            (App.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = "";
            (App.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = "";

            e.Handled = true;
        }

        private static void UpdateStatisticsCount()
        {
            var statistics = (App.Current.MainWindow.DataContext as MainWindowViewModel).Statistics.Value;
            statistics.NumberOfDrawsOfTheEllipseTool++;
            var dao = new StatisticsDao();
            dao.Update(statistics);
        }

        //EdgeThickness分だけitemを拡張することでElipseAdornerの見た目と描画を一致させる
        private void Dilate(NEllipseViewModel item)
        {
            var parent = (AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel;
            item.Left.Value -= parent.EdgeThickness.Value.Value / 2;
            item.Top.Value -= parent.EdgeThickness.Value.Value / 2;
            item.Width.Value += parent.EdgeThickness.Value.Value;
            item.Height.Value += parent.EdgeThickness.Value.Value;
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));

            if (_startPoint.HasValue && _endPoint.HasValue)
            {
                if ((Keyboard.GetKeyStates(Key.LeftShift) & KeyStates.Down) == KeyStates.Down ||
                    (Keyboard.GetKeyStates(Key.RightShift) & KeyStates.Down) == KeyStates.Down)
                {
                    var diff = _endPoint.Value - _startPoint.Value;

                    var x = Math.Max(diff.X, diff.Y);
                    var y = Math.Max(diff.X, diff.Y);
                    _endPoint = new Point(_startPoint.Value.X + x, _startPoint.Value.Y + y);

                    var center = new Point((_endPoint.Value.X + _startPoint.Value.X) / 2, (_endPoint.Value.Y + _startPoint.Value.Y) / 2);
                    var radiusX = (_endPoint.Value.X - _startPoint.Value.X) / 2;
                    var radiusY = (_endPoint.Value.Y - _startPoint.Value.Y) / 2;
                    dc.DrawEllipse(((AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel).FillBrush.Value.Clone(), _ellipsePen, center, radiusX, radiusY);
                }
                else if ((Keyboard.GetKeyStates(Key.LeftAlt) & KeyStates.Down) == KeyStates.Down ||
                         (Keyboard.GetKeyStates(Key.RightAlt) & KeyStates.Down) == KeyStates.Down)
                {
                    var diff = _endPoint.Value - _startPoint.Value;

                    var x = Math.Max(diff.X, diff.Y);
                    var y = Math.Max(diff.X, diff.Y);
                    _endPoint = new Point(_startPoint.Value.X + x, _startPoint.Value.Y + y);

                    //var center = new Point((_endPoint.Value.X + _startPoint.Value.X) / 2, (_endPoint.Value.Y + _startPoint.Value.Y) / 2);
                    var radiusX = (_endPoint.Value.X - _startPoint.Value.X) / 2;
                    var radiusY = (_endPoint.Value.Y - _startPoint.Value.Y) / 2;
                    dc.DrawEllipse(((AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel).FillBrush.Value.Clone(), _ellipsePen, _startPoint.Value, radiusX, radiusY);
                }
                else
                {
                    var center = new Point((_endPoint.Value.X + _startPoint.Value.X) / 2, (_endPoint.Value.Y + _startPoint.Value.Y) / 2);
                    var radiusX = (_endPoint.Value.X - _startPoint.Value.X) / 2;
                    var radiusY = (_endPoint.Value.Y - _startPoint.Value.Y) / 2;
                    dc.DrawEllipse(((AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel).FillBrush.Value.Clone(), _ellipsePen, center, radiusX, radiusY);
                }
            }
        }
    }
}
