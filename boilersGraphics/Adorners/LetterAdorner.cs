using boilersGraphics.Controls;
using boilersGraphics.Dao;
using boilersGraphics.Extensions;
using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace boilersGraphics.Adorners
{
    public class LetterAdorner : Adorner
    {
        private Point? _startPoint;
        private Point? _endPoint;
        private Pen _rectanglePen;

        private DesignerCanvas _designerCanvas;

        public LetterAdorner(DesignerCanvas designerCanvas, Point? dragStartPoint)
            : base(designerCanvas)
        {
            _designerCanvas = designerCanvas;
            _startPoint = dragStartPoint;
            var brush = new SolidColorBrush(Colors.Black);
            brush.Opacity = 0.5;
            _rectanglePen = new Pen(brush, 1);
        }

        protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (!this.IsMouseCaptured)
                    this.CaptureMouse();

                _endPoint = e.GetPosition(this);
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
                LetterDesignerItemViewModel itemBase = new LetterDesignerItemViewModel();
                itemBase.Owner = (AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel;
                itemBase.Left.Value = Math.Max(0, _startPoint.Value.X);
                itemBase.Top.Value = Math.Max(0, _startPoint.Value.Y);
                itemBase.Width.Value = Math.Abs(_endPoint.Value.X - _startPoint.Value.X);
                itemBase.Height.Value = Math.Abs(_endPoint.Value.Y - _startPoint.Value.Y);
                itemBase.EdgeBrush.Value = itemBase.Owner.EdgeBrush.Value.Clone();
                itemBase.EdgeThickness.Value = itemBase.Owner.EdgeThickness.Value.Value;
                itemBase.FillBrush.Value = itemBase.Owner.FillBrush.Value.Clone();
                itemBase.IsSelected.Value = true;
                itemBase.IsVisible.Value = true;
                itemBase.Owner.DeselectAll();
                itemBase.ZIndex.Value = itemBase.Owner.Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children.Value).Count();
                ((AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel).AddItemCommand.Execute(itemBase);

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
            statistics.NumberOfDrawsOfTheLetterTool++;
            var dao = new StatisticsDao();
            dao.Update(statistics);
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
