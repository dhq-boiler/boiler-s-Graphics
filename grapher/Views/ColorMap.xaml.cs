using System;
using System.Windows;
using System.Windows.Controls;
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

        private void Thumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            var newX = X + e.HorizontalChange;
            var newY = Y + e.VerticalChange;

            if (newX < -(Thumb.Width - 1) / 2)
            {
                newX = -(Thumb.Width - 1) / 2;
            }

            if (newY < -(Thumb.Height - 1) / 2)
            {
                newY = -(Thumb.Height - 1) / 2;
            }

            if (newX > 256 - (Thumb.Width - 1) / 2 - 1)
            {
                newX = 256 - (Thumb.Width - 1) / 2 - 1;
            }

            if (newY > 256 - (Thumb.Height - 1) / 2 - 1)
            {
                newY = 256 - (Thumb.Height - 1) / 2 - 1;
            }

            X = newX;
            Y = newY;
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

        private void Image_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var position = e.GetPosition(sender as IInputElement);
            if (position.X < 0 || position.X > 254 || position.Y < 0 || position.Y > 254)
            {
                return;
            }

            X = position.X - (Thumb.Width / 2);
            Y = position.Y - (Thumb.Height / 2);
        }
    }
}
