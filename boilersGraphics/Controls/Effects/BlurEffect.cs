using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace boilersGraphics.Controls.Effects;

public class BlurEffect : ShaderEffect
{
    public static readonly DependencyProperty InputProperty =
        RegisterPixelShaderSamplerProperty("Input", typeof(BlurEffect), 0);

    // Using a DependencyProperty as the backing store for Width.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty WidthProperty =
        DependencyProperty.Register("Width",
            typeof(double),
            typeof(BlurEffect),
            new PropertyMetadata(0.0, PixelShaderConstantCallback(0)));

    // Using a DependencyProperty as the backing store for Height.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty HeightProperty =
        DependencyProperty.Register(
            "Height",
            typeof(double),
            typeof(BlurEffect),
            new PropertyMetadata(0.0, PixelShaderConstantCallback(1)));

    public static readonly DependencyProperty KernelWidthProperty =
        DependencyProperty.Register(
            "KernelWidth",
            typeof(double),
            typeof(BlurEffect),
            new PropertyMetadata(0.0, PixelShaderConstantCallback(2)));

    public static readonly DependencyProperty KernelHeightProperty =
        DependencyProperty.Register(
            "KernelHeight",
            typeof(double),
            typeof(BlurEffect),
            new PropertyMetadata(0.0, PixelShaderConstantCallback(3)));

    public static readonly DependencyProperty SigmaProperty =
        DependencyProperty.Register(
            "Sigma",
            typeof(double),
            typeof(BlurEffect),
            new PropertyMetadata(0.0, PixelShaderConstantCallback(4)));

    // Using a DependencyProperty as the backing store for Bytecode.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty BytecodeProperty =
        DependencyProperty.Register(
            "Bytecode",
            typeof(IList<byte>),
            typeof(BlurEffect),
            new PropertyMetadata(
                null,
                (d, e) =>
                {
                    var t = d as BlurEffect;
                    if (t != null) t.OnBytecodePropertyChanged(e);
                }));

    public BlurEffect()
    {
        var ps = new PixelShader();
        PixelShader = ps;
        UpdateShaderValue(InputProperty);
    }

    public Brush Input
    {
        get => (Brush)GetValue(InputProperty);
        set => SetValue(InputProperty, value);
    }


    public double Width
    {
        get => (double)GetValue(WidthProperty);
        set => SetValue(WidthProperty, value);
    }

    public double Height
    {
        get => (double)GetValue(HeightProperty);
        set => SetValue(HeightProperty, value);
    }

    public double KernelWidth
    {
        get => (double)GetValue(KernelWidthProperty);
        set => SetValue(KernelWidthProperty, value);
    }

    public double KernelHeight
    {
        get => (double)GetValue(KernelHeightProperty);
        set => SetValue(KernelHeightProperty, value);
    }

    public double Sigma
    {
        get => (double)GetValue(SigmaProperty);
        set => SetValue(SigmaProperty, value);
    }

    public IList<byte> Bytecode
    {
        get => (IList<byte>)GetValue(BytecodeProperty);
        set => SetValue(BytecodeProperty, value);
    }


    private void OnBytecodePropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        var v = e.NewValue as byte[];
        if (v != null)
            using (var ms = new MemoryStream(v))
            {
                PixelShader.SetStreamSource(ms);
            }
    }
}