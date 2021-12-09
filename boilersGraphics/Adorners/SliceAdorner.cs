using boilersGraphics.Controls;
using boilersGraphics.Helpers;
using boilersGraphics.ViewModels;
using boilersGraphics.Views;
using Prism.Services.Dialogs;
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
    class SliceAdorner : Adorner
    {
        private DesignerCanvas _designerCanvas;
        private Point? _startPoint;
        private readonly IDialogService dialogService;
        private Point? _endPoint;
        private Pen _rectanglePen;
        private SnapAction _snapAction;

        public SliceAdorner(DesignerCanvas designerCanvas, Point? dragStartPoint, Prism.Services.Dialogs.IDialogService dialogService)
            : base(designerCanvas)
        {
            _designerCanvas = designerCanvas;
            _startPoint = dragStartPoint;
            this.dialogService = dialogService;
            var brush = new SolidColorBrush(Colors.Black);
            brush.Opacity = 0.5;
            _rectanglePen = new Pen(brush, 1);
            _snapAction = new SnapAction();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (!this.IsMouseCaptured)
                    this.CaptureMouse();

                //ドラッグ終了座標を更新
                _endPoint = e.GetPosition(this);
                var currentPosition = _endPoint.Value;
                _snapAction.OnMouseMove(ref currentPosition);
                _endPoint = currentPosition;

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

            if (_startPoint.HasValue && _endPoint.HasValue)
            {
                _snapAction.OnMouseUp(this);

                var sliceRect = new Rect(_startPoint.Value, _endPoint.Value);

                IDialogResult result = null;
                var dialogParameters = new DialogParameters() { { "sliceRect", sliceRect } };
                this.dialogService.ShowDialog(nameof(Export), dialogParameters, ret => result = ret);
                if (result != null)
                {

                }

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
                dc.DrawRectangle(Brushes.Transparent, _rectanglePen, ShiftEdgeThickness());
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
