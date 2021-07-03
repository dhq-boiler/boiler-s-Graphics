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
        private Slider _zoomSlider;
        private ScaleTransform _scaleTransform;

        public MiniMapViewModel ViewModel { get; private set; }

        public MiniMap()
        {
        }

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
                ViewModel.Scale.Value = _scaleTransform.ScaleX;
            }
        }

        #endregion

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

            _zoomSlider = Template.FindName("PART_ZoomSlider", this) as Slider;
            if (_zoomSlider == null)
                throw new Exception("PART_ZoomSlider template is missing!");

            _zoomThumb.DragDelta += new DragDeltaEventHandler(this.Thumb_DragDelta);
            _zoomSlider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(this.ZoomSlider_ValueChanged);
            _scaleTransform = new ScaleTransform();
            DataContext = ViewModel = new MiniMapViewModel(this, _zoomCanvas);
        }

        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double scale = e.NewValue / e.OldValue;
            ViewModel.Scale.Value *= scale;
            _scaleTransform.ScaleX *= scale;
            _scaleTransform.ScaleY *= scale;
            ViewModel.ViewportWidth.Value = ViewModel.ViewportWidth.Value / ViewModel.Scale.Value;
            ViewModel.ViewportHeight.Value = ViewModel.ViewportHeight.Value / ViewModel.Scale.Value;
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            double scale = ViewModel.Scale.Value;
            ViewModel.ViewportLeft.Value = ViewModel.ViewportLeft.Value + e.HorizontalChange / scale;
            ViewModel.ViewportTop.Value = ViewModel.ViewportTop.Value + e.VerticalChange / scale;
        }

        private void DesignerCanvas_LayoutUpdated(object sender, EventArgs e)
        {
            var diagramControl = App.Current.MainWindow.GetChildOfType<DiagramControl>();
            var border = diagramControl.FindName("CanvasEdge") as Border;
            var diagramVM = ((App.Current.MainWindow.DataContext) as MainWindowViewModel).DiagramViewModel;
            double scale;
            switch (ViewModel.Scale.Value)
            {
                case 1.0:
                    if (diagramVM.WorldWidth >= diagramVM.WorldHeight)
                    {
                        scale = Width / diagramVM.WorldWidth;
                        ViewModel.MiniMapWidth.Value = diagramVM.WorldWidth * scale;
                        ViewModel.MiniMapHeight.Value = diagramVM.WorldHeight * scale;
                        ViewModel.CanvasLeft.Value = diagramVM.Width * scale;
                        ViewModel.CanvasTop.Value = diagramVM.Height * scale;
                        ViewModel.CanvasWidth.Value = diagramVM.Width * scale;
                        ViewModel.CanvasHeight.Value = diagramVM.Height * scale;
                    }
                    else
                    {
                        scale = Height / diagramVM.WorldHeight;
                        ViewModel.MiniMapWidth.Value = diagramVM.WorldWidth * scale;
                        ViewModel.MiniMapHeight.Value = diagramVM.WorldHeight * scale;
                        ViewModel.CanvasLeft.Value = diagramVM.Width * scale;
                        ViewModel.CanvasTop.Value = diagramVM.Height * scale;
                        ViewModel.CanvasWidth.Value = diagramVM.Width * scale;
                        ViewModel.CanvasHeight.Value = diagramVM.Height * scale;
                    }
                    ViewModel.ViewportWidth.Value = this.ScrollViewer.ViewportWidth * scale;
                    ViewModel.ViewportHeight.Value = this.ScrollViewer.ViewportHeight * scale;
                    break;
            }
        }

        //private void DesignerCanvas_MouseWheel(object sender, EventArgs e)
        //{
        //    MouseWheelEventArgs wheel = (MouseWheelEventArgs)e;

        //    //divide the value by 10 so that it is more smooth
        //    double value = Math.Max(0, wheel.Delta / 10);
        //    value = Math.Min(wheel.Delta, 10);
        //    _zoomSlider.Value += value;
        //}
    }
}
