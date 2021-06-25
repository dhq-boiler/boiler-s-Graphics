using boilersGraphics.Extensions;
using boilersGraphics.UserControls;
using boilersGraphics.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace boilersGraphics.Controls
{
    public class MiniMap : Control
    {
        private Thumb _zoomThumb;
        private Canvas _zoomCanvas;
        private ScaleTransform _scaleTransform;

        #region DPs

        #region ScrollViewer
        public ScrollViewer ScrollViewer
        {
            get { return (ScrollViewer)GetValue(ScrollViewerProperty); }
            set { SetValue(ScrollViewerProperty, value); }
        }

        public static readonly DependencyProperty ScrollViewerProperty =
            DependencyProperty.Register("ScrollViewer", typeof(ScrollViewer), typeof(MiniMap));
        #endregion

        #region DesignerCanvas


        public static readonly DependencyProperty DesignerCanvasProperty =
            DependencyProperty.Register("DesignerCanvas", typeof(DesignerCanvas), typeof(MiniMap),
                new FrameworkPropertyMetadata(null,
                    new PropertyChangedCallback(OnDesignerCanvasChanged)));


        public DesignerCanvas DesignerCanvas
        {
            get { return (DesignerCanvas)GetValue(DesignerCanvasProperty); }
            set { SetValue(DesignerCanvasProperty, value); }
        }

        private static void OnDesignerCanvasChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MiniMap target = (MiniMap)d;
            DesignerCanvas oldDesignerCanvas = (DesignerCanvas)e.OldValue;
            DesignerCanvas newDesignerCanvas = target.DesignerCanvas;
            target.OnDesignerCanvasChanged(oldDesignerCanvas, newDesignerCanvas);
        }


        protected virtual void OnDesignerCanvasChanged(DesignerCanvas oldDesignerCanvas, DesignerCanvas newDesignerCanvas)
        {
            if (oldDesignerCanvas != null)
            {
                newDesignerCanvas.LayoutUpdated -= new EventHandler(this.DesignerCanvas_LayoutUpdated);
                //newDesignerCanvas.MouseWheel -= new MouseWheelEventHandler(this.DesignerCanvas_MouseWheel);
            }

            if (newDesignerCanvas != null)
            {
                newDesignerCanvas.LayoutUpdated += new EventHandler(this.DesignerCanvas_LayoutUpdated);
                //newDesignerCanvas.MouseWheel += new MouseWheelEventHandler(this.DesignerCanvas_MouseWheel);
                newDesignerCanvas.LayoutTransform = _scaleTransform;
            }
        }

        #endregion

        #endregion

        #region CanvasLeft

        public static readonly DependencyProperty CanvasLeftProperty =
            DependencyProperty.Register("CanvasLeft", typeof(double), typeof(MiniMap));

        public double CanvasLeft
        {
            get { return (double)GetValue(CanvasLeftProperty); }
            set { SetValue(CanvasLeftProperty, value); }
        }

        #endregion

        #region CanvasTop

        public static readonly DependencyProperty CanvasTopProperty =
            DependencyProperty.Register("CanvasTop", typeof(double), typeof(MiniMap));

        public double CanvasTop
        {
            get { return (double)GetValue(CanvasTopProperty); }
            set { SetValue(CanvasTopProperty, value); }
        }

        #endregion

        #region CanvasWidth

        public static readonly DependencyProperty CanvasWidthProperty =
            DependencyProperty.Register("CanvasWidth", typeof(double), typeof(MiniMap));

        public double CanvasWidth
        {
            get { return (double)GetValue(CanvasWidthProperty); }
            set { SetValue(CanvasWidthProperty, value); }
        }

        #endregion

        #region CanvasHeight

        public static readonly DependencyProperty CanvasHeightProperty =
            DependencyProperty.Register("CanvasHeight", typeof(double), typeof(MiniMap));

        public double CanvasHeight
        {
            get { return (double)GetValue(CanvasHeightProperty); }
            set { SetValue(CanvasHeightProperty, value); }
        }

        #endregion

        #region MiniMapWidth

        public static readonly DependencyProperty MiniMapWidthProperty =
            DependencyProperty.Register("MiniMapWidth", typeof(double), typeof(MiniMap));

        public double MiniMapWidth
        {
            get { return (double)GetValue(MiniMapWidthProperty); }
            set { SetValue(MiniMapWidthProperty, value); }
        }

        #endregion

        #region MiniMapHeight

        public static readonly DependencyProperty MiniMapHeightProperty =
            DependencyProperty.Register("MiniMapHeight", typeof(double), typeof(MiniMap));

        public double MiniMapHeight
        {
            get { return (double)GetValue(MiniMapHeightProperty); }
            set { SetValue(MiniMapHeightProperty, value); }
        }

        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (this.ScrollViewer == null)
                return;

            _zoomThumb = Template.FindName("PART_ZoomThumb", this) as Thumb;
            if (_zoomThumb == null)
                throw new Exception("PART_ZoomThumb template is missing!");

            _zoomCanvas = Template.FindName("PART_ZoomCanvas", this) as Canvas;
            if (_zoomCanvas == null)
                throw new Exception("PART_ZoomCanvas template is missing!");

            //_zoomSlider = Template.FindName("PART_ZoomSlider", this) as Slider;
            //if (_zoomSlider == null)
            //    throw new Exception("PART_ZoomSlider template is missing!");

            _zoomThumb.DragDelta += new DragDeltaEventHandler(this.Thumb_DragDelta);
            //_zoomSlider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(this.ZoomSlider_ValueChanged);
            _scaleTransform = new ScaleTransform();
        }

        //private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        //{
        //    double scale = e.NewValue / e.OldValue;
        //    double halfViewportHeight = this.ScrollViewer.ViewportHeight / 2;
        //    double newVerticalOffset = ((this.ScrollViewer.VerticalOffset + halfViewportHeight) * scale - halfViewportHeight);
        //    double halfViewportWidth = this.ScrollViewer.ViewportWidth / 2;
        //    double newHorizontalOffset = ((this.ScrollViewer.HorizontalOffset + halfViewportWidth) * scale - halfViewportWidth);
        //    _scaleTransform.ScaleX *= scale;
        //    _scaleTransform.ScaleY *= scale;
        //    this.ScrollViewer.ScrollToHorizontalOffset(newHorizontalOffset);
        //    this.ScrollViewer.ScrollToVerticalOffset(newVerticalOffset);
        //}

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            double scale, xOffset, yOffset;
            this.InvalidateScale(out scale, out xOffset, out yOffset);
            this.ScrollViewer.ScrollToHorizontalOffset(this.ScrollViewer.HorizontalOffset + e.HorizontalChange / scale);
            this.ScrollViewer.ScrollToVerticalOffset(this.ScrollViewer.VerticalOffset + e.VerticalChange / scale);
        }

        private void DesignerCanvas_LayoutUpdated(object sender, EventArgs e)
        {
            var diagramControl = App.Current.MainWindow.GetChildOfType<DiagramControl>();
            var border = diagramControl.FindName("CanvasEdge") as Border;
            var diagramVM = ((App.Current.MainWindow.DataContext) as MainWindowViewModel).DiagramViewModel;
            double scale;
            if (diagramVM.WorldWidth >= diagramVM.WorldHeight)
            {
                scale = Width / diagramVM.WorldWidth;
                MiniMapWidth = diagramVM.WorldWidth * scale;
                MiniMapHeight = diagramVM.WorldHeight * scale;
                CanvasLeft = diagramVM.Width * scale;
                CanvasTop = diagramVM.Height * scale;
                CanvasWidth = diagramVM.Width * scale;
                CanvasHeight = diagramVM.Height * scale;
            }
            else
            {
                scale = Height / diagramVM.WorldHeight;
                MiniMapWidth = diagramVM.WorldWidth * scale;
                MiniMapHeight = diagramVM.WorldHeight * scale;
                CanvasLeft = diagramVM.Width * scale;
                CanvasTop = diagramVM.Height * scale;
                CanvasWidth = diagramVM.Width * scale;
                CanvasHeight = diagramVM.Height * scale;
            }
            Canvas.SetLeft(_zoomThumb, this.ScrollViewer.HorizontalOffset * scale);
            Canvas.SetTop(_zoomThumb, this.ScrollViewer.VerticalOffset * scale);
            _zoomThumb.Width = this.ScrollViewer.ViewportWidth * scale;
            _zoomThumb.Height = this.ScrollViewer.ViewportHeight * scale;
        }

        //private void DesignerCanvas_MouseWheel(object sender, EventArgs e)
        //{
        //    MouseWheelEventArgs wheel = (MouseWheelEventArgs)e;

        //    //divide the value by 10 so that it is more smooth
        //    double value = Math.Max(0, wheel.Delta / 10);
        //    value = Math.Min(wheel.Delta, 10);
        //    _zoomSlider.Value += value;
        //}

        private void InvalidateScale(out double scale, out double xOffset, out double yOffset)
        {
            double w = DesignerCanvas.ActualWidth * _scaleTransform.ScaleX;
            double h = DesignerCanvas.ActualHeight * _scaleTransform.ScaleY;

            // zoom canvas size
            double x = _zoomCanvas.ActualWidth;
            double y = _zoomCanvas.ActualHeight;
            double scaleX = x / w;
            double scaleY = y / h;
            scale = (scaleX < scaleY) ? scaleX : scaleY;
            xOffset = (x - scale * w) / 2;
            yOffset = (y - scale * h) / 2;
        }
    }
}
