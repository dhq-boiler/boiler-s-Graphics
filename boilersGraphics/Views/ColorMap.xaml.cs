using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace boilersGraphics.Views;

/// <summary>
///     ColorMap.xaml の相互作用ロジック
/// </summary>
public partial class ColorMap : UserControl
{
    public static readonly DependencyProperty AProperty = DependencyProperty.Register("A", typeof(byte),
        typeof(ColorMap), new FrameworkPropertyMetadata((byte)0, OnAChanged));

    public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source",
        typeof(WriteableBitmap), typeof(ColorMap), new FrameworkPropertyMetadata(null, OnSourceChanged));

    public static readonly DependencyProperty XProperty = DependencyProperty.Register("X", typeof(double),
        typeof(ColorMap), new FrameworkPropertyMetadata(0d, OnXChanged));

    public static readonly DependencyProperty YProperty = DependencyProperty.Register("Y", typeof(double),
        typeof(ColorMap), new FrameworkPropertyMetadata(0d, OnYChanged));

    public static readonly DependencyProperty ColorProperty =
        DependencyProperty.Register("Color", typeof(Color), typeof(ColorMap));

    public static readonly DependencyProperty SaturationProperty = DependencyProperty.Register("Saturation",
        typeof(byte), typeof(ColorMap), new FrameworkPropertyMetadata(default(byte), OnSaturationChanged));

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(byte),
        typeof(ColorMap), new FrameworkPropertyMetadata(default(byte), OnValueChanged));

    public ColorMap()
    {
        InitializeComponent();

        var tooltip = (ToolTip)Thumb.ToolTip;
        if (tooltip.PlacementTarget == null) tooltip.PlacementTarget = this;
        tooltip.PlacementRectangle = new Rect(0, 0, 0, 0);
        tooltip.Placement = PlacementMode.Relative;

        tooltip.HorizontalOffset = 0 + 10;
        tooltip.VerticalOffset = 0 + 10;

        X = -(Thumb.Width / 2);
        Y = -(Thumb.Height / 2);
        Image.Opacity = A / 255d;
    }

    public byte A
    {
        get => (byte)GetValue(AProperty);
        set => SetValue(AProperty, value);
    }

    public WriteableBitmap Source
    {
        get => (WriteableBitmap)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public double X
    {
        get => (double)GetValue(XProperty);
        set => SetValue(XProperty, value);
    }

    public double Y
    {
        get => (double)GetValue(YProperty);
        set => SetValue(YProperty, value);
    }

    public Color Color
    {
        get => (Color)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    public byte Saturation
    {
        get => (byte)GetValue(SaturationProperty);
        set => SetValue(SaturationProperty, value);
    }

    public byte Value
    {
        get => (byte)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public bool IsPressed { get; private set; }

    private static void OnAChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ctrl = d as ColorMap;
        ctrl.Image.Opacity = (byte)e.NewValue / 255d;
        ctrl.PickupColor();
    }

    private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ctrl = d as ColorMap;
        ctrl.PickupColor();
    }

    private static void OnXChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ctrl = d as ColorMap;
        ctrl.PickupColor();
    }

    private static void OnYChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ctrl = d as ColorMap;
        ctrl.PickupColor();
    }

    private static void OnSaturationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ctrl = d as ColorMap;
        ctrl.X = (byte)e.NewValue - (ctrl.Thumb.Width - 1) / 2;
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ctrl = d as ColorMap;
        ctrl.Y = 255 - (byte)e.NewValue - (ctrl.Thumb.Height - 1) / 2;
    }

    private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
        var newSaturation = X + e.HorizontalChange + (Thumb.Width - 1) / 2;
        var newValue = Y + e.VerticalChange + (Thumb.Height - 1) / 2;

        if (newSaturation < 0) newSaturation = 0;

        if (newValue < 0) newValue = 0;

        if (newSaturation > 255) newSaturation = 255;

        if (newValue > 255) newValue = 255;

        Saturation = (byte)Math.Round(newSaturation);
        Value = (byte)Math.Round(255 - newValue);

        SetToolTipCoordinate();
    }

    private void SetToolTipCoordinate()
    {
        var tooltip = (ToolTip)Thumb.ToolTip;

        var currentPosition = Mouse.GetPosition(Image);
        var locationFromScreen = Image.PointToScreen(currentPosition);
        var source = PresentationSource.FromVisual(this);
        var targetPoint = source.CompositionTarget.TransformFromDevice.Transform(locationFromScreen);

        tooltip.HorizontalOffset = currentPosition.X + 10;
        tooltip.VerticalOffset = currentPosition.Y + 10;
    }

    private void PickupColor()
    {
        if (Source == null) return;

        unsafe
        {
            var p = (byte*)Source.BackBuffer.ToPointer();
            var step = Source.BackBufferStride;

            var x = (int)Math.Round(X + (Thumb.Width - 1) / 2);
            var y = (int)Math.Round(Y + (Thumb.Height - 1) / 2);

            var b = *(p + y * step + x * 3);
            var g = *(p + y * step + x * 3 + 1);
            var r = *(p + y * step + x * 3 + 2);

            Color = new Color { A = A, B = b, G = g, R = r };
        }
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        var tooltip = (ToolTip)Thumb.ToolTip;

        var locationFromScreen = Thumb.PointToScreen(new Point(0, 0));
        var source = PresentationSource.FromVisual(this);
        var targetPoint = source.CompositionTarget.TransformFromDevice.Transform(locationFromScreen);

        tooltip.HorizontalOffset = 0 + 10;
        tooltip.VerticalOffset = 0 + 10;

        //ロードした彩度、輝度を元にThumbの座標を初期設定
        X = -(Thumb.Width / 2) + Saturation;
        Y = -(Thumb.Height / 2) + (ActualHeight - Value);
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

    private void Thumb_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        var tooltip = (ToolTip)Thumb.ToolTip;
        if (tooltip.PlacementTarget == null) tooltip.PlacementTarget = this;

        tooltip.HorizontalOffset = X + 13;
        tooltip.VerticalOffset = Y + 14;

        tooltip.IsOpen = true;
        Thumb.CaptureMouse();
    }

    private void Thumb_PreviewStylusDown(object sender, StylusDownEventArgs e)
    {
        var tooltip = (ToolTip)Thumb.ToolTip;
        if (tooltip.PlacementTarget == null) tooltip.PlacementTarget = this;

        tooltip.HorizontalOffset = X + 13;
        tooltip.VerticalOffset = Y + 14;

        tooltip.IsOpen = true;
        Thumb.CaptureStylus();
    }

    private void Thumb_PreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
        var tooltip = (ToolTip)Thumb.ToolTip;
        tooltip.IsOpen = false;
        Thumb.ReleaseMouseCapture();
    }

    private void Thumb_PreviewStylusUp(object sender, StylusEventArgs e)
    {
        var tooltip = (ToolTip)Thumb.ToolTip;
        tooltip.IsOpen = false;
        Thumb.ReleaseStylusCapture();
    }

    private void Thumb_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (IsPressed) IsPressed = false;

        var tooltip = (ToolTip)Thumb.ToolTip;
        tooltip.IsOpen = false;

        Thumb.ReleaseMouseCapture();
    }

    private void Thumb_StylusUp(object sender, StylusEventArgs e)
    {
        if (IsPressed) IsPressed = false;

        var tooltip = (ToolTip)Thumb.ToolTip;
        tooltip.IsOpen = false;

        Thumb.ReleaseStylusCapture();
    }

    private void Image_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        var position = e.GetPosition(sender as IInputElement);
        if (position.X < 0 || position.X > 254 || position.Y < 0 || position.Y > 254) return;

        Saturation = (byte)Math.Round(position.X);
        Value = (byte)Math.Round(255 - position.Y);

        SetToolTipCoordinate();

        IsPressed = true;

        var tooltip = (ToolTip)Thumb.ToolTip;
        tooltip.IsOpen = true;

        Thumb.CaptureMouse();
    }

    private void Image_MouseDown(object sender, MouseButtonEventArgs e)
    {
        var position = e.GetPosition(sender as IInputElement);
        if (position.X < 0 || position.X > 254 || position.Y < 0 || position.Y > 254) return;

        Saturation = (byte)Math.Round(position.X);
        Value = (byte)Math.Round(255 - position.Y);

        SetToolTipCoordinate();

        IsPressed = true;

        var tooltip = (ToolTip)Thumb.ToolTip;
        tooltip.IsOpen = true;

        Thumb.CaptureMouse();
    }

    private void Image_MouseMove(object sender, MouseEventArgs e)
    {
        if (Image.IsMouseCaptured || Image.IsStylusCaptured || Thumb.IsMouseCaptured || Thumb.IsStylusCaptured)
        {
            var position = e.GetPosition(sender as IInputElement);
            if (position.X < 0 || position.X > 254 || position.Y < 0 || position.Y > 254) return;

            Saturation = (byte)Math.Round(position.X);
            Value = (byte)Math.Round(255 - position.Y);

            SetToolTipCoordinate();
        }
    }

    private void Image_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (IsPressed) IsPressed = false;

        var tooltip = (ToolTip)Thumb.ToolTip;
        tooltip.IsOpen = false;

        Thumb.ReleaseMouseCapture();
    }

    private void Image_StylusDown(object sender, StylusDownEventArgs e)
    {
        var position = e.GetPosition(sender as IInputElement);
        if (position.X < 0 || position.X > 254 || position.Y < 0 || position.Y > 254) return;

        Saturation = (byte)Math.Round(position.X);
        Value = (byte)Math.Round(255 - position.Y);

        SetToolTipCoordinate();

        IsPressed = true;

        var tooltip = (ToolTip)Thumb.ToolTip;
        tooltip.IsOpen = true;

        Thumb.CaptureStylus();
    }

    private void Image_StylusUp(object sender, StylusEventArgs e)
    {
        if (IsPressed) IsPressed = false;

        var tooltip = (ToolTip)Thumb.ToolTip;
        tooltip.IsOpen = false;

        Thumb.ReleaseStylusCapture();
    }

    private void Thumb_MouseMove(object sender, MouseEventArgs e)
    {
        if (Image.IsMouseCaptured || Image.IsStylusCaptured || Thumb.IsMouseCaptured || Thumb.IsStylusCaptured)
        {
            var position = e.GetPosition(Image);
            if (position.X < 0 || position.X > 254 || position.Y < 0 || position.Y > 254) return;

            Saturation = (byte)Math.Round(position.X);
            Value = (byte)Math.Round(255 - position.Y);

            SetToolTipCoordinate();
        }
    }
}