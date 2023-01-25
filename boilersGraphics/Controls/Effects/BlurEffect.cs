using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace boilersGraphics.Controls.Effects
{
    public class BlurEffect : ShaderEffect
    {
        public BlurEffect()
        {
            PixelShader ps = new PixelShader();
            this.PixelShader = ps;
            UpdateShaderValue(InputProperty);
        }

        public Brush Input
        {
            get { return (Brush)GetValue(InputProperty); }
            set { SetValue(InputProperty, value); }
        }

        public static readonly DependencyProperty InputProperty =
            ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(BlurEffect), 0);


        public double Width
        {
            get { return (double)GetValue(WidthProperty); }
            set { SetValue(WidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Width.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WidthProperty =
            DependencyProperty.Register("Width",
                typeof(double),
                typeof(BlurEffect),
                new PropertyMetadata(0.0, PixelShaderConstantCallback(0)));

        public double Height
        {
            get { return (double)GetValue(HeightProperty); }
            set { SetValue(HeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Height.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeightProperty =
            DependencyProperty.Register(
                "Height",
                typeof(double),
                typeof(BlurEffect),
                new PropertyMetadata(0.0, PixelShaderConstantCallback(1)));

        public double KernelWidth
        {
            get { return (double)GetValue(KernelWidthProperty); }
            set { SetValue(KernelWidthProperty, value); }
        }

        public static readonly DependencyProperty KernelWidthProperty =
            DependencyProperty.Register(
                "KernelWidth",
                typeof(double),
                typeof(BlurEffect),
                new PropertyMetadata(0.0, PixelShaderConstantCallback(2)));

        public double KernelHeight
        {
            get { return (double)GetValue(KernelHeightProperty); }
            set { SetValue(KernelHeightProperty, value); }
        }

        public static readonly DependencyProperty KernelHeightProperty =
            DependencyProperty.Register(
                "KernelHeight",
                typeof(double),
                typeof(BlurEffect),
                new PropertyMetadata(0.0, PixelShaderConstantCallback(3)));

        public double Sigma
        {
            get { return (double)GetValue(SigmaProperty); }
            set { SetValue(SigmaProperty, value); }
        }

        public static readonly DependencyProperty SigmaProperty =
            DependencyProperty.Register(
                "Sigma",
                typeof(double),
                typeof(BlurEffect),
                new PropertyMetadata(0.0, PixelShaderConstantCallback(4)));

        public IList<byte> Bytecode
        {
            get { return (IList<byte>)GetValue(BytecodeProperty); }
            set { SetValue(BytecodeProperty, value); }
        }

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
                        if (t != null)
                        {
                            t.OnBytecodePropertyChanged(e);
                        }
                    }));


        private void OnBytecodePropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            var v = e.NewValue as byte[];
            if (v != null)
            {
                using (var ms = new MemoryStream(v))
                {
                    PixelShader.SetStreamSource(ms);
                }
            }
        }
    }
}
