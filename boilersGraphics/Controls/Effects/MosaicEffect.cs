using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace boilersGraphics.Controls.Effects;

public class MosaicEffect : ShaderEffect
{
    public static readonly DependencyProperty InputProperty =
        RegisterPixelShaderSamplerProperty("Input", typeof(MosaicEffect), 0);

    // Using a DependencyProperty as the backing store for Width.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty WidthProperty =
        DependencyProperty.Register("Width",
            typeof(double),
            typeof(MosaicEffect),
            new PropertyMetadata(0.0, PixelShaderConstantCallback(0)));

    // Using a DependencyProperty as the backing store for Height.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty HeightProperty =
        DependencyProperty.Register(
            "Height",
            typeof(double),
            typeof(MosaicEffect),
            new PropertyMetadata(0.0, PixelShaderConstantCallback(1)));

    public static readonly DependencyProperty CpProperty =
        DependencyProperty.Register(
            "Cp",
            typeof(double),
            typeof(MosaicEffect),
            new PropertyMetadata(0.0, PixelShaderConstantCallback(2)));

    public static readonly DependencyProperty RpProperty =
        DependencyProperty.Register(
            "Rp",
            typeof(double),
            typeof(MosaicEffect),
            new PropertyMetadata(0.0, PixelShaderConstantCallback(3)));

    // Using a DependencyProperty as the backing store for Bytecode.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty BytecodeProperty =
        DependencyProperty.Register(
            "Bytecode",
            typeof(IList<byte>),
            typeof(MosaicEffect),
            new PropertyMetadata(
                null,
                (d, e) =>
                {
                    var t = d as MosaicEffect;
                    if (t != null) t.OnBytecodePropertyChanged(e);
                }));

    public MosaicEffect()
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

    public double Cp
    {
        get => (double)GetValue(CpProperty);
        set => SetValue(CpProperty, value);
    }

    public double Rp
    {
        get => (double)GetValue(RpProperty);
        set => SetValue(RpProperty, value);
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