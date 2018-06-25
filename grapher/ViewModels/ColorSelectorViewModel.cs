using grapher.Helpers;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Prism.Interactivity.InteractionRequest;
using Prism.Mvvm;
using Reactive.Bindings;
using System;
using System.Reactive.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace grapher.ViewModels
{
    internal class ColorSelectorViewModel : BindableBase, IInteractionRequestAware
    {
        private byte _A;
        private WriteableBitmap _WhiteBlackColumnMap;
        private WriteableBitmap _HueSelector;
        private WriteableBitmap _RedSelector;
        private WriteableBitmap _GreenSelector;
        private WriteableBitmap _BlueSelector;
        private bool _hsv2bgr;
        private bool _bgr2hsv;
        private INotification _Notification;

        public ColorSelectorViewModel()
        {
            OkCommand = Color
                .Where(x => x != null)
                .Select(_ => true)
                .ToReactiveCommand();

            OkCommand.Subscribe(_ =>
            {
                EditTarget.New = Color.Value;
                FinishInteraction();
            });

            Hue.Subscribe(_ =>
            {
                GenerateSaturationValueMat();
                if (!_bgr2hsv)
                {
                    _hsv2bgr = true;
                    SetRGB();
                    _hsv2bgr = false;
                }
            });

            R.Subscribe(_ =>
            {
                if (!_hsv2bgr)
                {
                    _bgr2hsv = true;
                    RecalcHue();
                    RecalcSaturation();
                    RecalcValue();
                    _bgr2hsv = false;
                }
            });

            G.Subscribe(_ =>
            {
                if (!_hsv2bgr)
                {
                    _bgr2hsv = true;
                    RecalcHue();
                    RecalcSaturation();
                    RecalcValue();
                    _bgr2hsv = false;
                }
            });

            B.Subscribe(_ =>
            {
                if (!_hsv2bgr)
                {
                    _bgr2hsv = true;
                    RecalcHue();
                    RecalcSaturation();
                    RecalcValue();
                    _bgr2hsv = false;
                }
            });

            Color.Subscribe(_ =>
            {
                _hsv2bgr = true;
                _bgr2hsv = true;
                SetRGB();
                _hsv2bgr = false;
                _bgr2hsv = false;
            });

            GenerateHueSelectorMat();
            GenerateSaturationValueMat();
            GenerateRedSelectorMat();
            GenerateGreenSelectorMat();
            GenerateBlueSelectorMat();
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
            R.Value = Color.Value.R;
            G.Value = Color.Value.G;
            B.Value = Color.Value.B;
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

        /// <summary>
        /// 0 ≦ Hue ≦ 180
        /// </summary>
        public ReactiveProperty<byte> Hue { get; } = new ReactiveProperty<byte>();

        public ReactiveProperty<byte> Saturation { get; } = new ReactiveProperty<byte>();

        public ReactiveProperty<byte> Value { get; } = new ReactiveProperty<byte>();

        public byte A
        {
            get { return _A; }
            set { SetProperty(ref _A, value); }
        }

        public ReactiveProperty<byte> R { get; } = new ReactiveProperty<byte>();

        public ReactiveProperty<byte> G { get; } = new ReactiveProperty<byte>();

        public ReactiveProperty<byte> B { get; } = new ReactiveProperty<byte>();

        public ReactiveProperty<Color> Color { get; } = new ReactiveProperty<Color>();

        public ReactiveCommand OkCommand { get; }

        public ColorExchange EditTarget { get; set; }

        public INotification Notification
        {
            get { return _Notification; }
            set
            {
                _Notification = value;
                EditTarget = value.Content as ColorExchange;
                Color.Value = EditTarget.Old;
            }
        }

        public Action FinishInteraction { get; set; }
    }
}
