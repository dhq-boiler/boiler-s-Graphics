using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace boilersGraphics.Controls.Effects
{
    public class MosaicEffect : ShaderEffect
    {
        public MosaicEffect()
        {
            PixelShader ps = new PixelShader();
            string path = System.IO.Path.GetFullPath(@".\Controls\Effects\fx\MosaicEffect.ps");
            ps.UriSource = new Uri(path);

            this.PixelShader = ps;
            UpdateShaderValue(InputProperty);
            //UpdateShaderValue(WidthProperty);
            //UpdateShaderValue(HeightProperty);
            //UpdateShaderValue(BytecodeProperty);
        }

        public Brush Input
        {
            get { return (Brush)GetValue(InputProperty); }
            set { SetValue(InputProperty, value); }
        }

        public static readonly DependencyProperty InputProperty =
            ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(MosaicEffect), 0);


        public double Width
        {
            get { return (double)GetValue(WidthProperty); }
            set { SetValue(WidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Width.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WidthProperty =
            DependencyProperty.Register("Width",
                typeof(double),
                typeof(MosaicEffect),
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
                typeof(MosaicEffect),
                new PropertyMetadata(0.0, PixelShaderConstantCallback(1)));

        public double ColumnPixels
        {
            get { return (double)GetValue(ColumnPixelsProperty); }
            set { SetValue(ColumnPixelsProperty, value); }
        }

        public static readonly DependencyProperty ColumnPixelsProperty =
            DependencyProperty.Register(
                "ColumnPixels",
                typeof(double),
                typeof(MosaicEffect),
                new PropertyMetadata(0.0, PixelShaderConstantCallback(2)));
        public double RowPixels
        {
            get { return (double)GetValue(RowPixelsProperty); }
            set { SetValue(RowPixelsProperty, value); }
        }

        public static readonly DependencyProperty RowPixelsProperty =
            DependencyProperty.Register(
                "RowPixels",
                typeof(double),
                typeof(MosaicEffect),
                new PropertyMetadata(0.0, PixelShaderConstantCallback(3)));


        //public double Time
        //{
        //    get { return (double)GetValue(TimeProperty); }
        //    set { SetValue(TimeProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for Time.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty TimeProperty =
        //    DependencyProperty.Register(
        //        "Time",
        //        typeof(double),
        //        typeof(MosaicEffect),
        //        new PropertyMetadata(0.0, PixelShaderConstantCallback(2)));

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
                typeof(MosaicEffect),
                new PropertyMetadata(
                    null,
                    (d, e) =>
                    {
                        var t = d as MosaicEffect;
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
