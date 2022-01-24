using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.Models;
using boilersGraphics.Properties;
using boilersGraphics.Views;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace boilersGraphics.ViewModels
{
    public class ColorPickerViewModel : BindableBase, IDialogAware, IDisposable
    {
        private WriteableBitmap _WhiteBlackColumnMap;
        private WriteableBitmap _HueSelector;
        private WriteableBitmap _RedSelector;
        private WriteableBitmap _GreenSelector;
        private WriteableBitmap _BlueSelector;
        private WriteableBitmap _ASelector;
        private bool _hsv2bgr;
        private bool _bgr2hsv;
        private CompositeDisposable _disposables = new CompositeDisposable();
        private ColorPicker _colorPicker;
        private IEnumerable<ColorSpot> _spots;
        private bool _flag = true;

        public event Action<IDialogResult> RequestClose;

        public ColorPickerViewModel()
        {
            OkCommand = Color
                .Where(x => x != null)
                .Select(_ => true)
                .ToReactiveCommand();

            OkCommand
                .Subscribe(_ =>
                {
                    var colorSpots = new ColorSpots();
                    colorSpots.ColorSpot0 = ColorSpot0.Value;
                    colorSpots.ColorSpot1 = ColorSpot1.Value;
                    colorSpots.ColorSpot2 = ColorSpot2.Value;
                    colorSpots.ColorSpot3 = ColorSpot3.Value;
                    colorSpots.ColorSpot4 = ColorSpot4.Value;
                    colorSpots.ColorSpot5 = ColorSpot5.Value;
                    colorSpots.ColorSpot6 = ColorSpot6.Value;
                    colorSpots.ColorSpot7 = ColorSpot7.Value;
                    colorSpots.ColorSpot8 = ColorSpot8.Value;
                    colorSpots.ColorSpot9 = ColorSpot9.Value;
                    colorSpots.ColorSpot10 = ColorSpot10.Value;
                    colorSpots.ColorSpot11 = ColorSpot11.Value;
                    colorSpots.ColorSpot12 = ColorSpot12.Value;
                    colorSpots.ColorSpot13 = ColorSpot13.Value;
                    colorSpots.ColorSpot14 = ColorSpot14.Value;
                    colorSpots.ColorSpot15 = ColorSpot15.Value;
                    colorSpots.ColorSpot16 = ColorSpot16.Value;
                    colorSpots.ColorSpot17 = ColorSpot17.Value;
                    colorSpots.ColorSpot18 = ColorSpot18.Value;
                    colorSpots.ColorSpot19 = ColorSpot19.Value;
                    colorSpots.ColorSpot20 = ColorSpot20.Value;
                    colorSpots.ColorSpot21 = ColorSpot21.Value;
                    colorSpots.ColorSpot22 = ColorSpot22.Value;
                    colorSpots.ColorSpot23 = ColorSpot23.Value;
                    colorSpots.ColorSpot24 = ColorSpot24.Value;
                    colorSpots.ColorSpot25 = ColorSpot25.Value;
                    colorSpots.ColorSpot26 = ColorSpot26.Value;
                    colorSpots.ColorSpot27 = ColorSpot27.Value;
                    colorSpots.ColorSpot28 = ColorSpot28.Value;
                    colorSpots.ColorSpot29 = ColorSpot29.Value;
                    colorSpots.ColorSpot30 = ColorSpot30.Value;
                    colorSpots.ColorSpot31 = ColorSpot31.Value;
                    colorSpots.ColorSpot32 = ColorSpot32.Value;
                    colorSpots.ColorSpot33 = ColorSpot33.Value;
                    colorSpots.ColorSpot34 = ColorSpot34.Value;
                    colorSpots.ColorSpot35 = ColorSpot35.Value;
                    colorSpots.ColorSpot36 = ColorSpot36.Value;
                    colorSpots.ColorSpot37 = ColorSpot37.Value;
                    colorSpots.ColorSpot38 = ColorSpot38.Value;
                    colorSpots.ColorSpot39 = ColorSpot39.Value;
                    colorSpots.ColorSpot40 = ColorSpot40.Value;
                    colorSpots.ColorSpot41 = ColorSpot41.Value;
                    colorSpots.ColorSpot42 = ColorSpot42.Value;
                    colorSpots.ColorSpot43 = ColorSpot43.Value;
                    colorSpots.ColorSpot44 = ColorSpot44.Value;
                    colorSpots.ColorSpot45 = ColorSpot45.Value;
                    colorSpots.ColorSpot46 = ColorSpot46.Value;
                    colorSpots.ColorSpot47 = ColorSpot47.Value;
                    colorSpots.ColorSpot48 = ColorSpot48.Value;
                    colorSpots.ColorSpot49 = ColorSpot49.Value;
                    colorSpots.ColorSpot50 = ColorSpot50.Value;
                    colorSpots.ColorSpot51 = ColorSpot51.Value;
                    colorSpots.ColorSpot52 = ColorSpot52.Value;
                    colorSpots.ColorSpot53 = ColorSpot53.Value;
                    colorSpots.ColorSpot54 = ColorSpot54.Value;
                    colorSpots.ColorSpot55 = ColorSpot55.Value;
                    colorSpots.ColorSpot56 = ColorSpot56.Value;
                    colorSpots.ColorSpot57 = ColorSpot57.Value;
                    colorSpots.ColorSpot58 = ColorSpot58.Value;
                    colorSpots.ColorSpot59 = ColorSpot59.Value;
                    colorSpots.ColorSpot60 = ColorSpot60.Value;
                    colorSpots.ColorSpot61 = ColorSpot61.Value;
                    colorSpots.ColorSpot62 = ColorSpot62.Value;
                    colorSpots.ColorSpot63 = ColorSpot63.Value;
                    colorSpots.ColorSpot64 = ColorSpot64.Value;
                    colorSpots.ColorSpot65 = ColorSpot65.Value;
                    colorSpots.ColorSpot66 = ColorSpot66.Value;
                    colorSpots.ColorSpot67 = ColorSpot67.Value;
                    colorSpots.ColorSpot68 = ColorSpot68.Value;
                    colorSpots.ColorSpot69 = ColorSpot69.Value;
                    colorSpots.ColorSpot70 = ColorSpot70.Value;
                    colorSpots.ColorSpot71 = ColorSpot71.Value;
                    colorSpots.ColorSpot72 = ColorSpot72.Value;
                    colorSpots.ColorSpot73 = ColorSpot73.Value;
                    colorSpots.ColorSpot74 = ColorSpot74.Value;
                    colorSpots.ColorSpot75 = ColorSpot75.Value;
                    colorSpots.ColorSpot76 = ColorSpot76.Value;
                    colorSpots.ColorSpot77 = ColorSpot77.Value;
                    colorSpots.ColorSpot78 = ColorSpot78.Value;
                    colorSpots.ColorSpot79 = ColorSpot79.Value;
                    colorSpots.ColorSpot80 = ColorSpot80.Value;
                    colorSpots.ColorSpot81 = ColorSpot81.Value;
                    colorSpots.ColorSpot82 = ColorSpot82.Value;
                    colorSpots.ColorSpot83 = ColorSpot83.Value;
                    colorSpots.ColorSpot84 = ColorSpot84.Value;
                    colorSpots.ColorSpot85 = ColorSpot85.Value;
                    colorSpots.ColorSpot86 = ColorSpot86.Value;
                    colorSpots.ColorSpot87 = ColorSpot87.Value;
                    colorSpots.ColorSpot88 = ColorSpot88.Value;
                    colorSpots.ColorSpot89 = ColorSpot89.Value;
                    colorSpots.ColorSpot90 = ColorSpot90.Value;
                    colorSpots.ColorSpot91 = ColorSpot91.Value;
                    colorSpots.ColorSpot92 = ColorSpot92.Value;
                    colorSpots.ColorSpot93 = ColorSpot93.Value;
                    colorSpots.ColorSpot94 = ColorSpot94.Value;
                    colorSpots.ColorSpot95 = ColorSpot95.Value;
                    colorSpots.ColorSpot96 = ColorSpot96.Value;
                    colorSpots.ColorSpot97 = ColorSpot97.Value;
                    colorSpots.ColorSpot98 = ColorSpot98.Value;
                    colorSpots.ColorSpot99 = ColorSpot99.Value;

                    EditTarget.New = Output.Value;
                    var parameters = new DialogParameters()
                    {
                        { "ColorExchange", EditTarget },
                        { "ColorSpots",  colorSpots }
                    };
                    var ret = new DialogResult(ButtonResult.OK, parameters);
                    RequestClose.Invoke(ret);
                })
                .AddTo(_disposables);

            Hue
                .Subscribe(_ =>
                {
                    GenerateSaturationValueMat();
                    if (!_bgr2hsv)
                    {
                        _hsv2bgr = true;
                        SetRGB();
                        _hsv2bgr = false;
                    }
                    SetColorToSpot();
                })
                .AddTo(_disposables);

            Saturation
                .Subscribe(_ =>
                {
                    if (!_bgr2hsv)
                    {
                        _hsv2bgr = true;
                        HSV2RGB();
                        _hsv2bgr = false;
                    }
                    SetColorToSpot();
                })
                .AddTo(_disposables);

            Value
                .Subscribe(_ =>
                {
                    if (!_bgr2hsv)
                    {
                        _hsv2bgr = true;
                        HSV2RGB();
                        _hsv2bgr = false;
                    }
                    SetColorToSpot();
                })
                .AddTo(_disposables);

            A
                .Subscribe(_ =>
                {
                    SetColorToSpot();
                })
                .AddTo(_disposables);

            R
                .Subscribe(_ =>
                {
                    GenerateASelectorMat();
                    if (!_hsv2bgr)
                    {
                        _bgr2hsv = true;
                        RecalcHue();
                        RecalcValue();
                        RecalcSaturation();
                    }
                    SetColorToSpot();
                })
                .AddTo(_disposables);

            G
                .Subscribe(_ =>
                {
                    GenerateASelectorMat();
                    if (!_hsv2bgr)
                    {
                        _bgr2hsv = true;
                        RecalcHue();
                        RecalcValue();
                        RecalcSaturation();
                        _bgr2hsv = false;
                    }
                    SetColorToSpot();
                })
                .AddTo(_disposables);

            B
                .Subscribe(_ =>
                {
                    GenerateASelectorMat();
                    if (!_hsv2bgr)
                    {
                        _bgr2hsv = true;
                        RecalcHue();
                        RecalcValue();
                        RecalcSaturation();
                        _bgr2hsv = false;
                    }
                    SetColorToSpot();
                })
                .AddTo(_disposables);

            Color
                .Subscribe(newColor =>
                {
                    if (!_hsv2bgr && !_bgr2hsv)
                    {
                        _hsv2bgr = true;
                        _bgr2hsv = true;
                        SetRGB();
                        _hsv2bgr = false;
                        _bgr2hsv = false;
                    }
                })
                .AddTo(_disposables);

            GenerateHueSelectorMat();
            GenerateSaturationValueMat();
            GenerateRedSelectorMat();
            GenerateGreenSelectorMat();
            GenerateBlueSelectorMat();
            GenerateASelectorMat();

            OpenCloseColorPalleteCommand.Subscribe(_ =>
            {
                if (ColorPalleteVisibility.Value == Visibility.Collapsed)
                    ColorPalleteVisibility.Value = Visibility.Visible;
                else if (ColorPalleteVisibility.Value == Visibility.Visible)
                    ColorPalleteVisibility.Value = Visibility.Collapsed;
            })
            .AddTo(_disposables);
            SpotSelectCommand.Subscribe(x =>
            {
                var colorSpot = x as ColorSpot;
                if (colorSpot.IsSelected.Value == true)
                    colorSpot.IsSelected.Value = false;
                else
                {
                    if (!colorSpot.IsSelected.Value)
                    {
                        _spots.ToList().ForEach(x => x.IsSelected.Value = false);
                        colorSpot.IsSelected.Value = true;
                        var a = colorSpot.Color.A;
                        var r = colorSpot.Color.R;
                        var g = colorSpot.Color.G;
                        var b = colorSpot.Color.B;
                        _flag = false;
                        A.Value = a;
                        R.Value = r;
                        G.Value = g;
                        B.Value = b;
                        _flag = true;
                    }
                }
            })
            .AddTo(_disposables);
            LoadedCommand.Subscribe(x =>
            {
                var source = x.Source;
                _colorPicker = source as ColorPicker;
                _spots = _colorPicker.FindVisualChildren<ColorSpot>();
            })
            .AddTo(_disposables);
        }

        private void SetColorToSpot()
        {
            if (_spots != null && _flag)
            {
                _spots.Where(x => x.IsSelected.Value)
                      .ToList()
                      .ForEach(x =>
                      {
                          x.Color = Output.Value;
                      });
            }
        }

        private void GenerateRedSelectorMat()
        {
            using (var bgrMat = new Mat(10, 255, MatType.CV_8UC3))
            {
                unsafe
                {
                    byte* p = (byte*)bgrMat.Data.ToPointer();

                    for (int y = 0; y < 10; ++y)
                    {
                        byte* py = p + y * bgrMat.Step();

                        for (int x = 0; x < 255; ++x)
                        {
                            *(py + x * 3) = 0;
                            *(py + x * 3 + 1) = 0;
                            *(py + x * 3 + 2) = (byte)x;
                        }
                    }
                }

                RedSelector = WriteableBitmapConverter.ToWriteableBitmap(bgrMat);
            }
        }

        private void GenerateGreenSelectorMat()
        {
            using (var bgrMat = new Mat(10, 255, MatType.CV_8UC3))
            {
                unsafe
                {
                    byte* p = (byte*)bgrMat.Data.ToPointer();

                    for (int y = 0; y < 10; ++y)
                    {
                        byte* py = p + y * bgrMat.Step();

                        for (int x = 0; x < 255; ++x)
                        {
                            *(py + x * 3) = 0;
                            *(py + x * 3 + 1) = (byte)x;
                            *(py + x * 3 + 2) = 0;
                        }
                    }
                }

                GreenSelector = WriteableBitmapConverter.ToWriteableBitmap(bgrMat);
            }
        }

        private void GenerateBlueSelectorMat()
        {
            using (var bgrMat = new Mat(10, 255, MatType.CV_8UC3))
            {
                unsafe
                {
                    byte* p = (byte*)bgrMat.Data.ToPointer();

                    for (int y = 0; y < 10; ++y)
                    {
                        byte* py = p + y * bgrMat.Step();

                        for (int x = 0; x < 255; ++x)
                        {
                            *(py + x * 3) = (byte)x;
                            *(py + x * 3 + 1) = 0;
                            *(py + x * 3 + 2) = 0;
                        }
                    }
                }

                BlueSelector = WriteableBitmapConverter.ToWriteableBitmap(bgrMat);
            }
        }

        private void GenerateASelectorMat()
        {
            using (var bgrMat = new Mat(10, 255, MatType.CV_8UC4))
            {
                unsafe
                {
                    byte* p = (byte*)bgrMat.Data.ToPointer();

                    for (int y = 0; y < 10; ++y)
                    {
                        byte* py = p + y * bgrMat.Step();

                        for (int x = 0; x < 255; ++x)
                        {
                            if ((y + x) % 2 == 0)
                            {
                                *(py + x * 4) = B.Value;
                                *(py + x * 4 + 1) = G.Value;
                                *(py + x * 4 + 2) = R.Value;
                                *(py + x * 4 + 3) = (byte)x;
                            }
                            else
                            {
                                *(py + x * 4) = 255 / 2;
                                *(py + x * 4 + 1) = 255 / 2;
                                *(py + x * 4 + 2) = 255 / 2;
                                *(py + x * 4 + 3) = (byte)x;
                            }
                        }
                    }
                }

                ASelector = WriteableBitmapConverter.ToWriteableBitmap(bgrMat);
            }
        }

        private void GenerateHueSelectorMat()
        {
            using (var hsvMat = new Mat(10, 180, MatType.CV_8UC3))
            {
                unsafe
                {
                    byte* p = (byte*)hsvMat.Data.ToPointer();

                    for (int y = 0; y < 10; ++y)
                    {
                        byte* py = p + y * hsvMat.Step();

                        for (int x = 0; x < 180; ++x)
                        {
                            *(py + x * 3) = (byte)x; //hue
                            *(py + x * 3 + 1) = 255;
                            *(py + x * 3 + 2) = 255;
                        }
                    }
                }

                using (var bgrMat = new Mat())
                {
                    Cv2.CvtColor(hsvMat, bgrMat, ColorConversionCodes.HSV2BGR);
                    HueSelector = WriteableBitmapConverter.ToWriteableBitmap(bgrMat);
                }
            }
        }

        private void GenerateSaturationValueMat()
        {
            using (var hsvMat = new Mat(256, 256, MatType.CV_8UC3))
            {
                unsafe
                {
                    byte* p = (byte*)hsvMat.Data.ToPointer();

                    for (int y = 0; y < 256; ++y)
                    {
                        byte* py = p + y * hsvMat.Step();

                        for (int x = 0; x < 256; ++x)
                        {
                            *(py + x * 3) = Hue.Value;
                            *(py + x * 3 + 1) = (byte)x; //saturation
                            *(py + x * 3 + 2) = (byte)(255 - y); //value
                        }
                    }
                }

                using (var bgrMat = new Mat())
                {
                    Cv2.CvtColor(hsvMat, bgrMat, ColorConversionCodes.HSV2BGR);
                    WhiteBlackColorMap = WriteableBitmapConverter.ToWriteableBitmap(bgrMat);
                }
            }
        }

        private void RecalcHue()
        {
            var r = R.Value / 255d;
            var g = G.Value / 255d;
            var b = B.Value / 255d;

            var max = Math.Max(r, Math.Max(g, b));
            var min = Math.Min(r, Math.Min(g, b));

            if (max == 0 || max == min)
            {
                return;
            }

            double hue = 0d;

            if (r == max)
            {
                hue = (g - b) / (max - min);
            }
            else if (g == max)
            {
                hue = 2.0 + (b - r) / (max - min);
            }
            else if (b == max)
            {
                hue = 4.0 + (r - g) / (max - min);
            }

            hue *= 60d;
            if (hue < 0d)
            {
                hue = hue + 360d;
            }

            hue /= 2d;

            Hue.Value = (byte)Math.Round(hue);
        }

        private void RecalcSaturation()
        {
            if (Value.Value == 0)
            {
                Saturation.Value = 0;
            }
            else
            {
                var r = R.Value / 255d;
                var g = G.Value / 255d;
                var b = B.Value / 255d;
                var v = Value.Value / 255d;

                var max = Math.Max(r, Math.Max(g, b));
                var min = Math.Min(r, Math.Min(g, b));

                var saturation = (max - min) / v;

                Saturation.Value = (byte)Math.Round(saturation * 255d);
            }
        }

        private void RecalcValue()
        {
            Value.Value = Math.Max(R.Value, Math.Max(G.Value, B.Value));
        }

        private void SetRGB()
        {
            HSV2RGB();
        }

        private void HSV2RGB()
        {
            var hue = Hue.Value * 2d;
            var saturation = Saturation.Value / 255d;
            var value = Value.Value / 255d;

            var C = value * saturation;
            var X = C * (1 - Math.Abs((hue / 60d) % 2d - 1d));
            var m = value - C;

            double _r = 0, _g = 0, _b = 0;
            if ((0 <= hue && hue < 60) || hue == 360)
            {
                _r = C;
                _g = X;
                _b = 0;
            }
            else if (60 <= hue && hue < 120)
            {
                _r = X;
                _g = C;
                _b = 0;
            }
            else if (120 <= hue && hue < 180)
            {
                _r = 0;
                _g = C;
                _b = X;
            }
            else if (180 <= hue && hue < 240)
            {
                _r = 0;
                _g = X;
                _b = C;
            }
            else if (240 <= hue && hue < 300)
            {
                _r = X;
                _g = 0;
                _b = C;
            }
            else if (300 <= hue && hue < 360)
            {
                _r = C;
                _g = 0;
                _b = X;
            }

            R.Value = (byte)((_r + m) * 255d);
            G.Value = (byte)((_g + m) * 255d);
            B.Value = (byte)((_b + m) * 255d);
        }

        public WriteableBitmap WhiteBlackColorMap
        {
            get { return _WhiteBlackColumnMap; }
            set { SetProperty(ref _WhiteBlackColumnMap, value); }
        }

        public WriteableBitmap HueSelector
        {
            get { return _HueSelector; }
            set { SetProperty(ref _HueSelector, value); }
        }

        public WriteableBitmap RedSelector
        {
            get { return _RedSelector; }
            set { SetProperty(ref _RedSelector, value); }
        }

        public WriteableBitmap GreenSelector
        {
            get { return _GreenSelector; }
            set { SetProperty(ref _GreenSelector, value); }
        }

        public WriteableBitmap BlueSelector
        {
            get { return _BlueSelector; }
            set { SetProperty(ref _BlueSelector, value); }
        }

        public WriteableBitmap ASelector
        {
            get { return _ASelector; }
            set { SetProperty(ref _ASelector, value); }
        }

        /// <summary>
        /// 0 ≦ Hue ≦ 180
        /// </summary>
        public ReactivePropertySlim<byte> Hue { get; } = new ReactivePropertySlim<byte>();

        public ReactivePropertySlim<byte> Saturation { get; } = new ReactivePropertySlim<byte>();

        public ReactivePropertySlim<byte> Value { get; } = new ReactivePropertySlim<byte>();

        public ReactivePropertySlim<byte> A { get; } = new ReactivePropertySlim<byte>();

        public ReactivePropertySlim<byte> R { get; } = new ReactivePropertySlim<byte>();

        public ReactivePropertySlim<byte> G { get; } = new ReactivePropertySlim<byte>();

        public ReactivePropertySlim<byte> B { get; } = new ReactivePropertySlim<byte>();

        public ReactivePropertySlim<Color> Color { get; } = new ReactivePropertySlim<Color>();

        public ReactivePropertySlim<Color> Output { get; } = new ReactivePropertySlim<Color>();

        public ReactivePropertySlim<Visibility> ColorPalleteVisibility { get; } = new ReactivePropertySlim<Visibility>(Visibility.Collapsed);

        public ReactivePropertySlim<Color> ColorSpot0 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot1 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot2 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot3 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot4 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot5 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot6 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot7 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot8 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot9 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot10 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot11 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot12 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot13 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot14 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot15 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot16 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot17 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot18 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot19 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot20 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot21 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot22 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot23 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot24 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot25 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot26 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot27 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot28 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot29 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot30 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot31 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot32 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot33 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot34 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot35 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot36 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot37 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot38 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot39 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot40 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot41 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot42 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot43 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot44 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot45 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot46 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot47 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot48 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot49 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot50 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot51 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot52 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot53 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot54 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot55 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot56 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot57 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot58 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot59 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot60 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot61 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot62 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot63 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot64 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot65 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot66 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot67 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot68 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot69 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot70 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot71 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot72 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot73 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot74 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot75 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot76 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot77 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot78 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot79 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot80 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot81 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot82 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot83 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot84 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot85 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot86 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot87 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot88 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot89 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot90 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot91 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot92 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot93 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot94 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot95 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot96 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot97 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot98 { get; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<Color> ColorSpot99 { get; } = new ReactivePropertySlim<Color>();


        public ReactiveCommand OkCommand { get; }

        public ReactiveCommand OpenCloseColorPalleteCommand { get; } = new ReactiveCommand();

        public ReactiveCommand SpotSelectCommand { get; } = new ReactiveCommand();

        public ReactiveCommand<RoutedEventArgs> LoadedCommand { get; } = new ReactiveCommand<RoutedEventArgs>();

        public ColorExchange EditTarget { get; set; }

        public string Title => Resources.Title_ColorPicker;

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            EditTarget = parameters.GetValue<ColorExchange>("ColorExchange");
            A.Value = EditTarget.Old.A;
            R.Value = EditTarget.Old.R;
            G.Value = EditTarget.Old.G;
            B.Value = EditTarget.Old.B;
            var colorspots = parameters.GetValue<ColorSpots>("ColorSpots");
            this.ColorSpot0.Value = colorspots.ColorSpot0;
            this.ColorSpot1.Value = colorspots.ColorSpot1;
            this.ColorSpot2.Value = colorspots.ColorSpot2;
            this.ColorSpot3.Value = colorspots.ColorSpot3;
            this.ColorSpot4.Value = colorspots.ColorSpot4;
            this.ColorSpot5.Value = colorspots.ColorSpot5;
            this.ColorSpot6.Value = colorspots.ColorSpot6;
            this.ColorSpot7.Value = colorspots.ColorSpot7;
            this.ColorSpot8.Value = colorspots.ColorSpot8;
            this.ColorSpot9.Value = colorspots.ColorSpot9;
            this.ColorSpot10.Value = colorspots.ColorSpot10;
            this.ColorSpot11.Value = colorspots.ColorSpot11;
            this.ColorSpot12.Value = colorspots.ColorSpot12;
            this.ColorSpot13.Value = colorspots.ColorSpot13;
            this.ColorSpot14.Value = colorspots.ColorSpot14;
            this.ColorSpot15.Value = colorspots.ColorSpot15;
            this.ColorSpot16.Value = colorspots.ColorSpot16;
            this.ColorSpot17.Value = colorspots.ColorSpot17;
            this.ColorSpot18.Value = colorspots.ColorSpot18;
            this.ColorSpot19.Value = colorspots.ColorSpot19;
            this.ColorSpot20.Value = colorspots.ColorSpot20;
            this.ColorSpot21.Value = colorspots.ColorSpot21;
            this.ColorSpot22.Value = colorspots.ColorSpot22;
            this.ColorSpot23.Value = colorspots.ColorSpot23;
            this.ColorSpot24.Value = colorspots.ColorSpot24;
            this.ColorSpot25.Value = colorspots.ColorSpot25;
            this.ColorSpot26.Value = colorspots.ColorSpot26;
            this.ColorSpot27.Value = colorspots.ColorSpot27;
            this.ColorSpot28.Value = colorspots.ColorSpot28;
            this.ColorSpot29.Value = colorspots.ColorSpot29;
            this.ColorSpot30.Value = colorspots.ColorSpot30;
            this.ColorSpot31.Value = colorspots.ColorSpot31;
            this.ColorSpot32.Value = colorspots.ColorSpot32;
            this.ColorSpot33.Value = colorspots.ColorSpot33;
            this.ColorSpot34.Value = colorspots.ColorSpot34;
            this.ColorSpot35.Value = colorspots.ColorSpot35;
            this.ColorSpot36.Value = colorspots.ColorSpot36;
            this.ColorSpot37.Value = colorspots.ColorSpot37;
            this.ColorSpot38.Value = colorspots.ColorSpot38;
            this.ColorSpot39.Value = colorspots.ColorSpot39;
            this.ColorSpot40.Value = colorspots.ColorSpot40;
            this.ColorSpot41.Value = colorspots.ColorSpot41;
            this.ColorSpot42.Value = colorspots.ColorSpot42;
            this.ColorSpot43.Value = colorspots.ColorSpot43;
            this.ColorSpot44.Value = colorspots.ColorSpot44;
            this.ColorSpot45.Value = colorspots.ColorSpot45;
            this.ColorSpot46.Value = colorspots.ColorSpot46;
            this.ColorSpot47.Value = colorspots.ColorSpot47;
            this.ColorSpot48.Value = colorspots.ColorSpot48;
            this.ColorSpot49.Value = colorspots.ColorSpot49;
            this.ColorSpot50.Value = colorspots.ColorSpot50;
            this.ColorSpot51.Value = colorspots.ColorSpot51;
            this.ColorSpot52.Value = colorspots.ColorSpot52;
            this.ColorSpot53.Value = colorspots.ColorSpot53;
            this.ColorSpot54.Value = colorspots.ColorSpot54;
            this.ColorSpot55.Value = colorspots.ColorSpot55;
            this.ColorSpot56.Value = colorspots.ColorSpot56;
            this.ColorSpot57.Value = colorspots.ColorSpot57;
            this.ColorSpot58.Value = colorspots.ColorSpot58;
            this.ColorSpot59.Value = colorspots.ColorSpot59;
            this.ColorSpot60.Value = colorspots.ColorSpot60;
            this.ColorSpot61.Value = colorspots.ColorSpot61;
            this.ColorSpot62.Value = colorspots.ColorSpot62;
            this.ColorSpot63.Value = colorspots.ColorSpot63;
            this.ColorSpot64.Value = colorspots.ColorSpot64;
            this.ColorSpot65.Value = colorspots.ColorSpot65;
            this.ColorSpot66.Value = colorspots.ColorSpot66;
            this.ColorSpot67.Value = colorspots.ColorSpot67;
            this.ColorSpot68.Value = colorspots.ColorSpot68;
            this.ColorSpot69.Value = colorspots.ColorSpot69;
            this.ColorSpot70.Value = colorspots.ColorSpot70;
            this.ColorSpot71.Value = colorspots.ColorSpot71;
            this.ColorSpot72.Value = colorspots.ColorSpot72;
            this.ColorSpot73.Value = colorspots.ColorSpot73;
            this.ColorSpot74.Value = colorspots.ColorSpot74;
            this.ColorSpot75.Value = colorspots.ColorSpot75;
            this.ColorSpot76.Value = colorspots.ColorSpot76;
            this.ColorSpot77.Value = colorspots.ColorSpot77;
            this.ColorSpot78.Value = colorspots.ColorSpot78;
            this.ColorSpot79.Value = colorspots.ColorSpot79;
            this.ColorSpot80.Value = colorspots.ColorSpot80;
            this.ColorSpot81.Value = colorspots.ColorSpot81;
            this.ColorSpot82.Value = colorspots.ColorSpot82;
            this.ColorSpot83.Value = colorspots.ColorSpot83;
            this.ColorSpot84.Value = colorspots.ColorSpot84;
            this.ColorSpot85.Value = colorspots.ColorSpot85;
            this.ColorSpot86.Value = colorspots.ColorSpot86;
            this.ColorSpot87.Value = colorspots.ColorSpot87;
            this.ColorSpot88.Value = colorspots.ColorSpot88;
            this.ColorSpot89.Value = colorspots.ColorSpot89;
            this.ColorSpot90.Value = colorspots.ColorSpot90;
            this.ColorSpot91.Value = colorspots.ColorSpot91;
            this.ColorSpot92.Value = colorspots.ColorSpot92;
            this.ColorSpot93.Value = colorspots.ColorSpot93;
            this.ColorSpot94.Value = colorspots.ColorSpot94;
            this.ColorSpot95.Value = colorspots.ColorSpot95;
            this.ColorSpot96.Value = colorspots.ColorSpot96;
            this.ColorSpot97.Value = colorspots.ColorSpot97;
            this.ColorSpot98.Value = colorspots.ColorSpot98;
            this.ColorSpot99.Value = colorspots.ColorSpot99;
        }

        #region IDisposable

        public void Dispose()
        {
            _disposables.Dispose();
        }

        #endregion //IDisposable
    }
}
