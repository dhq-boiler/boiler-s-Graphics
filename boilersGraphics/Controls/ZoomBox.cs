using boilersGraphics.Extensions;
using boilersGraphics.UserControls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace boilersGraphics.Controls
{
    public class ZoomBox : Control, INotifyPropertyChanged
    {
        private Slider _zoomSlider;
        private ScaleTransform _scaleTransform;

        #region DPs

        #region ScrollViewer
        public ScrollViewer ScrollViewer
        {
            get { return (ScrollViewer)GetValue(ScrollViewerProperty); }
            set { SetValue(ScrollViewerProperty, value); }
        }

        public static readonly DependencyProperty ScrollViewerProperty =
            DependencyProperty.Register("ScrollViewer", typeof(ScrollViewer), typeof(ZoomBox));
        #endregion

        public static readonly DependencyProperty ZoomThumbProperty = DependencyProperty.Register("ZoomThumb", typeof(Thumb), typeof(ZoomBox), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnZoomThumbChanged)));


        public static readonly DependencyProperty ZoomCanvasProperty = DependencyProperty.Register("ZoomCanvas", typeof(Canvas), typeof(ZoomBox), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnZoomCanvasChanged)));


        public static readonly DependencyProperty ScaleProperty = DependencyProperty.Register("Scale", typeof(double), typeof(ZoomBox));

        public static readonly DependencyProperty DesignerCanvasProperty = DependencyProperty.Register("DesignerCanvas", typeof(DesignerCanvas), typeof(ZoomBox), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnDesignerCanvasChanged)));

        public static readonly DependencyProperty TargetRectProperty = DependencyProperty.Register("TargetRect", typeof(Rect), typeof(ZoomBox));

        public event PropertyChangedEventHandler PropertyChanged;

        public DesignerCanvas DesignerCanvas
        {
            get { return (DesignerCanvas)GetValue(DesignerCanvasProperty); }
            set { SetValue(DesignerCanvasProperty, value); }
        }
        public Thumb ZoomThumb
        {
            get { return (Thumb)GetValue(ZoomThumbProperty); }
            set { SetValue(ZoomThumbProperty, value); }
        }

        public Canvas ZoomCanvas
        {
            get { return (Canvas)GetValue(ZoomCanvasProperty); }
            set { SetValue(ZoomCanvasProperty, value); }
        }

        public double Scale
        {
            get { return (double)GetValue(ScaleProperty); }
            set { SetValue(ScaleProperty, value); }
        }

        public Rect TargetRect
        {
            get { return (Rect)GetValue(TargetRectProperty); }
            set { SetValue(TargetRectProperty, value); }
        }

        public DiagramControl DiagramControl
        {
            get { return App.Current.MainWindow.GetChildOfType<DiagramControl>(); }
        }

        #region DesignerCanvas

        private static void OnZoomThumbChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ZoomBox zoomBox = (ZoomBox)d;
            zoomBox.RaisePropertyChanged("ZoomThumb");
            zoomBox.RaisePropertyChanged("");
        }

        private static void OnZoomCanvasChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ZoomBox zoomBox = (ZoomBox)d;
            zoomBox.RaisePropertyChanged("ZoomCanvas");
            zoomBox.RaisePropertyChanged("");
        }

        private static void OnDesignerCanvasChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ZoomBox target = (ZoomBox)d;
            DesignerCanvas oldDesignerCanvas = (DesignerCanvas)e.OldValue;
            DesignerCanvas newDesignerCanvas = target.DesignerCanvas;
            target.OnDesignerCanvasChanged(oldDesignerCanvas, newDesignerCanvas);
            target.RaisePropertyChanged("DesignerCanvas");
            target.RaisePropertyChanged("");
        }


        protected virtual void OnDesignerCanvasChanged(DesignerCanvas oldDesignerCanvas, DesignerCanvas newDesignerCanvas)
        {
            if (oldDesignerCanvas != null)
            {
                newDesignerCanvas.LayoutUpdated -= new EventHandler(this.DesignerCanvas_LayoutUpdated);
                newDesignerCanvas.MouseWheel -= new MouseWheelEventHandler(this.DesignerCanvas_MouseWheel);
            }

            if (newDesignerCanvas != null)
            {
                newDesignerCanvas.LayoutUpdated += new EventHandler(this.DesignerCanvas_LayoutUpdated);
                newDesignerCanvas.MouseWheel += new MouseWheelEventHandler(this.DesignerCanvas_MouseWheel);
                newDesignerCanvas.LayoutTransform = _scaleTransform;
            }
        }

        #endregion

        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (this.ScrollViewer == null)
                return;

            ZoomThumb = Template.FindName("PART_ZoomThumb", this) as Thumb;
            if (ZoomThumb == null)
                throw new Exception("PART_ZoomThumb template is missing!");

            ZoomCanvas = Template.FindName("PART_ZoomCanvas", this) as Canvas;
            if (ZoomCanvas == null)
                throw new Exception("PART_ZoomCanvas template is missing!");

            _zoomSlider = Template.FindName("PART_ZoomSlider", this) as Slider;
            if (_zoomSlider == null)
                throw new Exception("PART_ZoomSlider template is missing!");

            ZoomThumb.DragDelta += new DragDeltaEventHandler(this.Thumb_DragDelta);
            _zoomSlider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(this.ZoomSlider_ValueChanged);
            _scaleTransform = new ScaleTransform();
        }

        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double scale = e.NewValue / e.OldValue;
            double halfViewportHeight = this.ScrollViewer.ViewportHeight / 2;
            double newVerticalOffset = ((this.ScrollViewer.VerticalOffset + halfViewportHeight) * scale - halfViewportHeight);
            double halfViewportWidth = this.ScrollViewer.ViewportWidth / 2;
            double newHorizontalOffset = ((this.ScrollViewer.HorizontalOffset + halfViewportWidth) * scale - halfViewportWidth);
            _scaleTransform.ScaleX *= scale;
            _scaleTransform.ScaleY *= scale;
            Scale = Math.Min(_scaleTransform.ScaleX, _scaleTransform.ScaleY);
            this.ScrollViewer.ScrollToHorizontalOffset(newHorizontalOffset);
            this.ScrollViewer.ScrollToVerticalOffset(newVerticalOffset);
        }

        public void ZoomSliderPlus()
        {
            var ticks = _zoomSlider.Ticks;
            int index = ticks.IndexOf(_zoomSlider.Value);
            index++;
            if (index > ticks.Count() - 1)
                index = ticks.Count() - 1;
            _zoomSlider.Value = ticks[index];
        }

        public void ZoomSliderMinus()
        {
            var ticks = _zoomSlider.Ticks;
            int index = ticks.IndexOf(_zoomSlider.Value);
            index--;
            if (index < 0)
                index = 0;
            _zoomSlider.Value = ticks[index];
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            double scale, xOffset, yOffset;
            this.InvalidateScale(out scale, out xOffset, out yOffset);
            Scale = scale;
            this.ScrollViewer.ScrollToHorizontalOffset(this.ScrollViewer.HorizontalOffset + e.HorizontalChange / scale);
            this.ScrollViewer.ScrollToVerticalOffset(this.ScrollViewer.VerticalOffset + e.VerticalChange / scale);
        }

        private void DesignerCanvas_LayoutUpdated(object sender, EventArgs e)
        {
            double scale, xOffset, yOffset;
            this.InvalidateScale(out scale, out xOffset, out yOffset);
            Scale = scale;
            ZoomThumb.Width = Math.Max(0.0, this.ScrollViewer.ViewportWidth * scale);
            ZoomThumb.Height = Math.Max(0.0, this.ScrollViewer.ViewportHeight * scale);
            Canvas.SetLeft(ZoomThumb, xOffset + this.ScrollViewer.HorizontalOffset * scale);
            Canvas.SetTop(ZoomThumb, yOffset + this.ScrollViewer.VerticalOffset * scale);
        }

        private void DesignerCanvas_MouseWheel(object sender, EventArgs e)
        {
            MouseWheelEventArgs wheel = (MouseWheelEventArgs)e;

            //divide the value by 10 so that it is more smooth
            double value = Math.Max(0, wheel.Delta / 10);
            value = Math.Min(wheel.Delta, 10);
            _zoomSlider.Value += value;
        }

        private void InvalidateScale(out double scale, out double xOffset, out double yOffset)
        {
            double w = DesignerCanvas.ActualWidth * _scaleTransform.ScaleX;
            double h = DesignerCanvas.ActualHeight * _scaleTransform.ScaleY;

            // zoom canvas size
            double x = ZoomCanvas.ActualWidth;
            double y = ZoomCanvas.ActualHeight;
            double scaleX = x / w;
            double scaleY = y / h;
            scale = (scaleX < scaleY) ? scaleX : scaleY;
            xOffset = (x - scale * w) / 2;
            yOffset = (y - scale * h) / 2;
        }
        //
        // 概要:
        //     Checks if a property already matches a desired value. Sets the property and notifies
        //     listeners only when necessary.
        //
        // パラメーター:
        //   storage:
        //     Reference to a property with both getter and setter.
        //
        //   value:
        //     Desired value for the property.
        //
        //   propertyName:
        //     Name of the property used to notify listeners. This value is optional and can
        //     be provided automatically when invoked from compilers that support CallerMemberName.
        //
        // 型パラメーター:
        //   T:
        //     Type of the property.
        //
        // 戻り値:
        //     True if the value was changed, false if the existing value matched the desired
        //     value.
        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }

            storage = value;
            RaisePropertyChanged(propertyName);
            return true;
        }

        //
        // 概要:
        //     Checks if a property already matches a desired value. Sets the property and notifies
        //     listeners only when necessary.
        //
        // パラメーター:
        //   storage:
        //     Reference to a property with both getter and setter.
        //
        //   value:
        //     Desired value for the property.
        //
        //   propertyName:
        //     Name of the property used to notify listeners. This value is optional and can
        //     be provided automatically when invoked from compilers that support CallerMemberName.
        //
        //   onChanged:
        //     Action that is called after the property value has been changed.
        //
        // 型パラメーター:
        //   T:
        //     Type of the property.
        //
        // 戻り値:
        //     True if the value was changed, false if the existing value matched the desired
        //     value.
        protected virtual bool SetProperty<T>(ref T storage, T value, Action onChanged, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }

            storage = value;
            onChanged?.Invoke();
            RaisePropertyChanged(propertyName);
            return true;
        }

        //
        // 概要:
        //     Raises this object's PropertyChanged event.
        //
        // パラメーター:
        //   propertyName:
        //     Name of the property used to notify listeners. This value is optional and can
        //     be provided automatically when invoked from compilers that support System.Runtime.CompilerServices.CallerMemberNameAttribute.
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        //
        // 概要:
        //     Raises this object's PropertyChanged event.
        //
        // パラメーター:
        //   args:
        //     The PropertyChangedEventArgs
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            this.PropertyChanged?.Invoke(this, args);
        }
    }
}
