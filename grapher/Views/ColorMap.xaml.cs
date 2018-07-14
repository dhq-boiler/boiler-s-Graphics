using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace grapher.Views
{
    /// <summary>
    /// ColorMap.xaml の相互作用ロジック
    /// </summary>
    public partial class ColorMap : UserControl
    {
        public ColorMap()
        {
            InitializeComponent();

            var tooltip = (ToolTip)Thumb.ToolTip;
            if (tooltip.PlacementTarget == null)
            {
                tooltip.PlacementTarget = this;
            }
            tooltip.PlacementRectangle = new Rect(0, 0, 0, 0);
            tooltip.Placement = System.Windows.Controls.Primitives.PlacementMode.Relative;

            tooltip.HorizontalOffset = 0 + 10;
            tooltip.VerticalOffset = 0 + 10;

            X = -(Thumb.Width / 2);
            Y = -(Thumb.Height / 2);
        }

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(WriteableBitmap), typeof(ColorMap), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnSourceChanged)));

        public WriteableBitmap Source
        {
            get { return (WriteableBitmap)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as ColorMap;
            ctrl.PickupColor();
        }

        public static readonly DependencyProperty XProperty = DependencyProperty.Register("X", typeof(double), typeof(ColorMap), new FrameworkPropertyMetadata(0d, new PropertyChangedCallback(OnXChanged)));

        public double X
        {
            get { return (double)GetValue(XProperty); }
            set { SetValue(XProperty, value); }
        }

        private static void OnXChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as ColorMap;
            ctrl.PickupColor();
        }

        public static readonly DependencyProperty YProperty = DependencyProperty.Register("Y", typeof(double), typeof(ColorMap), new FrameworkPropertyMetadata(0d, new PropertyChangedCallback(OnYChanged)));

        public double Y
        {
            get { return (double)GetValue(YProperty); }
            set { SetValue(YProperty, value); }
        }

        private static void OnYChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as ColorMap;
            ctrl.PickupColor();
        }

        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register("Color", typeof(Color), typeof(ColorMap));

        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        public static readonly DependencyProperty SaturationProperty = DependencyProperty.Register("Saturation", typeof(byte), typeof(ColorMap), new FrameworkPropertyMetadata(default(byte), new PropertyChangedCallback(OnSaturationChanged)));

        public byte Saturation
        {
            get { return (byte)GetValue(SaturationProperty); }
            set { SetValue(SaturationProperty, value); }
        }

        private static void OnSaturationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as ColorMap;
            ctrl.X = (byte)e.NewValue - (ctrl.Thumb.Width - 1) / 2;
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(byte), typeof(ColorMap), new FrameworkPropertyMetadata(default(byte), new PropertyChangedCallback(OnValueChanged)));

        public byte Value
        {
            get { return (byte)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as ColorMap;
            ctrl.Y = 255 - (byte)e.NewValue - (ctrl.Thumb.Height - 1) / 2;
        }

        private void Thumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            var newSaturation = X + e.HorizontalChange + (Thumb.Width - 1) / 2;
            var newValue = Y + e.VerticalChange + (Thumb.Height - 1) / 2;

            if (newSaturation < 0)
            {
                newSaturation = 0;
            }

            if (newValue < 0)
            {
                newValue = 0;
            }

            if (newSaturation > 255)
            {
                newSaturation = 255;
            }

            if (newValue > 255)
            {
                newValue = 255;
            }

            Saturation = (byte)Math.Round(newSaturation);
            Value = (byte)Math.Round(255 - newValue);

            SetToolTipCoordinate();
        }

        private void SetToolTipCoordinate()
        {
            var tooltip = (ToolTip)Thumb.ToolTip;

            var currentPosition = Mouse.GetPosition((IInputElement)Image);
            var locationFromScreen = Image.PointToScreen(currentPosition);
            var source = PresentationSource.FromVisual(this);
            var targetPoint = source.CompositionTarget.TransformFromDevice.Transform(locationFromScreen);

            tooltip.HorizontalOffset = currentPosition.X + 10;
            tooltip.VerticalOffset = currentPosition.Y + 10;
        }

        private void PickupColor()
        {
            if (Source == null)
            {
                return;
            }

            unsafe
            {
                byte* p = (byte*)Source.BackBuffer.ToPointer();
                int step = Source.BackBufferStride;

                int x = (int)Math.Round(X + (Thumb.Width - 1) / 2);
                int y = (int)Math.Round(Y + (Thumb.Height - 1) / 2);

                byte b = *(p + y * step + x * 3);
                byte g = *(p + y * step + x * 3 + 1);
                byte r = *(p + y * step + x * 3 + 2);

                Color = new Color() { A = 255, B = b, G = g, R = r };
            }
        }

        public bool IsPressed { get; private set; }

        private void Image_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var position = e.GetPosition(sender as IInputElement);
            if (position.X < 0 || position.X > 254 || position.Y < 0 || position.Y > 254)
            {
                return;
            }

            Saturation = (byte)Math.Round(position.X);
            Value = (byte)Math.Round(255 - position.Y);

            SetToolTipCoordinate();

            IsPressed = true;

            var tooltip = (ToolTip)Thumb.ToolTip;
            tooltip.IsOpen = true;

            Image.CaptureMouse();
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsPressed && e.LeftButton == MouseButtonState.Pressed)
            {
                var position = e.GetPosition(sender as IInputElement);
                if (position.X < 0 || position.X > 254 || position.Y < 0 || position.Y > 254)
                {
                    return;
                }

                Saturation = (byte)Math.Round(position.X);
                Value = (byte)Math.Round(255 - position.Y);

                SetToolTipCoordinate();
            }
            else
            {
                IsPressed = false;
                Image.ReleaseMouseCapture();
            }
        }

        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (IsPressed)
            {
                IsPressed = false;
            }

            var tooltip = (ToolTip)Thumb.ToolTip;
            tooltip.IsOpen = false;

            Image.ReleaseMouseCapture();
        }

        private void Thumb_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var tooltip = (ToolTip)Thumb.ToolTip;
            if (tooltip.PlacementTarget == null)
            {
                tooltip.PlacementTarget = this;
            }

            tooltip.HorizontalOffset = X + 13;
            tooltip.VerticalOffset = Y + 14;

            tooltip.IsOpen = true;
        }

        private void Thumb_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var tooltip = (ToolTip)Thumb.ToolTip;
            tooltip.IsOpen = false;
        }

        private void Thumb_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (IsPressed)
            {
                IsPressed = false;
            }

            var tooltip = (ToolTip)Thumb.ToolTip;
            tooltip.IsOpen = false;

            Image.ReleaseMouseCapture();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var tooltip = (ToolTip)Thumb.ToolTip;

            var locationFromScreen = Thumb.PointToScreen(new Point(0, 0));
            var source = PresentationSource.FromVisual(this);
            var targetPoint = source.CompositionTarget.TransformFromDevice.Transform(locationFromScreen);

            tooltip.HorizontalOffset = 0 + 10;
            tooltip.VerticalOffset = 0 + 10;
        }

        private void Thumb_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            var tooltip = (ToolTip)Thumb.ToolTip;

            var locationFromScreen = Thumb.PointToScreen(new Point(0, 0));
            var source = PresentationSource.FromVisual(this);
            var targetPoint = source.CompositionTarget.TransformFromDevice.Transform(locationFromScreen);

            tooltip.HorizontalOffset = 0 + 10;
            tooltip.VerticalOffset = 0 + 10;
        }

        private void Thumb_ToolTipClosing(object sender, ToolTipEventArgs e)
        {
            var tooltip = (ToolTip)Thumb.ToolTip;

            tooltip.HorizontalOffset = 0 + 10;
            tooltip.VerticalOffset = 0 + 10;

            e.Handled = true;
        }
    }
}
