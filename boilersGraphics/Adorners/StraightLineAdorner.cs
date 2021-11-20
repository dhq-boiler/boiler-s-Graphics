using boilersGraphics.Controls;
using boilersGraphics.Dao;
using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace boilersGraphics.Adorners
{
    internal class StraightLineAdorner : Adorner
    {
        private DesignerCanvas _designerCanvas;
        private Point? _startPoint;
        private Point? _endPoint;
        private Pen _straightLinePen;
        private SnapAction _snapAction;
        private StraightConnectorViewModel item;

        public StraightLineAdorner(DesignerCanvas designerCanvas, Point? dragStartPoint, StraightConnectorViewModel item)
            : base(designerCanvas)
        {
            this.item = item;
            _designerCanvas = designerCanvas;
            _startPoint = dragStartPoint;
            var parent = (AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel;
            var brush = new SolidColorBrush(parent.EdgeColors.First());
            brush.Opacity = 0.5;
            _straightLinePen = new Pen(brush, parent.EdgeThickness.Value.Value);
            _snapAction = new SnapAction();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (!this.IsMouseCaptured)
                    this.CaptureMouse();

                var ellipses = (_designerCanvas.DataContext as DiagramViewModel).AllItems.Value.OfType<NEllipseViewModel>();

                //ドラッグ終了座標を更新
                _endPoint = e.GetPosition(this);

                var appendIntersectionPoints = new List<Tuple<Point, NEllipseViewModel>>();
                _snapAction.SnapIntersectionOfEllipseAndTangent(ellipses, _startPoint.Value, _endPoint.Value, appendIntersectionPoints);

                var currentPosition = _endPoint.Value;
                _snapAction.OnMouseMove(ref currentPosition, appendIntersectionPoints);
                _endPoint = currentPosition;

                (App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel.CurrentPoint = currentPosition;
                (App.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = $"({_startPoint.Value.X}, {_startPoint.Value.Y}) - ({_endPoint.Value.X}, {_endPoint.Value.Y}) (w, h) = ({_endPoint.Value.X - _startPoint.Value.X}, {_endPoint.Value.Y - _startPoint.Value.Y})";

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

            if (_startPoint.HasValue && _endPoint.HasValue)
            {
                var viewModel = (AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel;
                item.Owner = viewModel;
                item.EdgeColor.Value = item.Owner.EdgeColors.First();
                item.EdgeThickness.Value = item.Owner.EdgeThickness.Value.Value;
                item.ZIndex.Value = item.Owner.Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children).Count();
                item.IsSelected.Value = true;
                item.IsVisible.Value = true;
                item.AddPointP2(viewModel, _endPoint.Value);
                item.PathGeometry.Value = GeometryCreator.CreateLine(item);
                item.SnapPoint0VM.Value.IsSelected.Value = true;
                item.SnapPoint1VM.Value.IsSelected.Value = true;
                item.SnapPoint0VM.Value.IsHitTestVisible.Value = true;
                item.SnapPoint1VM.Value.IsHitTestVisible.Value = true;
                item.Owner.DeselectAll();
                ((AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel).AddItemCommand.Execute(item);

                _snapAction.PostProcess(SnapPointPosition.EndEdge, item);

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
            statistics.NumberOfDrawsOfTheStraightLineTool++;
            var dao = new StatisticsDao();
            dao.Update(statistics);
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));

            if (_startPoint.HasValue && _endPoint.HasValue)
                dc.DrawLine(_straightLinePen, _startPoint.Value, _endPoint.Value);
        }
    }
}
