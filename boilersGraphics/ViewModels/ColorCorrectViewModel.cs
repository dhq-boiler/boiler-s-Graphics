using boilersGraphics.Controls;
using boilersGraphics.Exceptions;
using boilersGraphics.Helpers;
using boilersGraphics.ViewModels.ColorCorrect;
using boilersGraphics.Views;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Prism.Unity;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ZLinq;
using Rect = System.Windows.Rect;

namespace boilersGraphics.ViewModels;

public class ColorCorrectViewModel : EffectViewModel
{
    public ColorCorrectViewModel()
    {
        Initialize();
    }

    public ReactivePropertySlim<ColorCorrectType> CCType { get; } = new(ColorCorrectType.HSV);
    public ReactivePropertySlim<Channel> TargetChannel { get; } = new(Channel.RGB);

    public ReactivePropertySlim<WriteableBitmap> Bitmap { get; } = new();

    #region 色相・彩度・輝度
    public ReactivePropertySlim<int> AddHue { get; } = new();
    public ReactivePropertySlim<int> AddSaturation { get; } = new();
    public ReactivePropertySlim<int> AddValue { get; } = new();
    #endregion

    #region トーンカーブ
    public ReactiveCollection<ToneCurveViewModel.Curve> Curves { get; set; } = new();
    public ReactivePropertySlim<ToneCurveViewModel.Curve> TargetCurve { get; set; } = new();
    #endregion

    #region 2値化
    public ReactivePropertySlim<double> Threshold { get; } = new();
    public ReactivePropertySlim<double> MaxValue { get; } = new(255);
    public ReactivePropertySlim<ThresholdTypes> ThresholdTypes { get; } = new(boilersGraphics.ViewModels.ThresholdTypes.Binary);
    public ReactivePropertySlim<bool> OtsuEnabled { get; } = new();
    #endregion


    public override bool SupportsPropertyDialog => true;

    public override void Render()
    {
        if (Width.Value <= 0 || Height.Value <= 0) return;

        Application.Current.Dispatcher.Invoke(() =>
        {
            var renderer = new EffectRenderer(new WpfVisualTreeHelper());
            var rtb = renderer.Render(Rect.Value, DesignerCanvas.GetInstance(), 
                MainWindowViewModel.Instance.DiagramViewModel, MainWindowViewModel.Instance.DiagramViewModel.BackgroundItem.Value, this, 0, this.ZIndex.Value - 1);
            var newFormattedBitmapSource = new FormatConvertedBitmap();
            newFormattedBitmapSource.BeginInit();
            newFormattedBitmapSource.Source = rtb;
            newFormattedBitmapSource.DestinationFormat = PixelFormats.Bgr24;
            newFormattedBitmapSource.EndInit();

            if (TargetCurve.Value is not null && TargetCurve.Value.InOutPairs is null)
            {
                TargetCurve.Value.InOutPairs = new();
            }

            using (var a = newFormattedBitmapSource.ToMat())
            using (var b = new Mat())
            using (var c = new Mat())
            using (var d = new Mat())
            using (var z = a.Clone())
            {
                switch (CCType.Value)
                {
                    case Hsv:
                        Cv2.CvtColor(a, b, ColorConversionCodes.BGR2HSV);
                        OperateHSV(b);
                        Cv2.CvtColor(b, z, ColorConversionCodes.HSV2BGR);
                        break;
                    case ToneCurve:
                        if (Curves.AsValueEnumerable().Count() != 4)
                            break;
                        var arr3 = Cv2.Split(a);
                        OperateToneCurve(arr3[0], Curves[1]); //B
                        OperateToneCurve(arr3[1], Curves[2]); //G
                        OperateToneCurve(arr3[2], Curves[3]); //R
                        Cv2.Merge(arr3, b);
                        Cv2.CvtColor(b, c, ColorConversionCodes.BGR2HSV);
                        var arr4 = Cv2.Split(c);
                        OperateToneCurve(arr4[2], Curves[0]); //Value
                        Cv2.Merge(arr4, d);
                        Cv2.CvtColor(d, z, ColorConversionCodes.HSV2BGR);
                        break;
                    case NegativePositiveConversion:
                        Cv2.BitwiseNot(a, z);
                        break;
                    case Binarization:
                        using (var grayscale = new Mat())
                        {
                            Cv2.CvtColor(a, grayscale, ColorConversionCodes.BGR2GRAY);
                            var flags = ThresholdTypes.Value.ToOpenCvValue();
                            if ((flags & OpenCvSharp.ThresholdTypes.Triangle) != OpenCvSharp.ThresholdTypes.Triangle 
                              && OtsuEnabled.Value)
                            {
                                flags |= ViewModels.ThresholdTypes.Otsu.ToOpenCvValue();
                            }
                            Cv2.Threshold(grayscale, z, Threshold.Value, MaxValue.Value, flags);
                        }
                        break;
                }

                Bitmap.Value = z.ToWriteableBitmap();
                UpdateLayout();
            }
        });
    }

    private unsafe void OperateToneCurve(Mat singleChannel, ViewModels.ColorCorrect.ToneCurveViewModel.Curve targetCurve)
    {
        if (targetCurve is null)
            return;
        if (targetCurve.InOutPairs is null)
            return;
        if (targetCurve.InOutPairs.Count < 256)
            return;
        if (singleChannel.Channels() != 1)
            throw new UnexpectedException("singleChannel.Channels() != 1");

        byte* p = (byte*)singleChannel.Data.ToPointer();
        long step = singleChannel.Step();
        int width = singleChannel.Width;
        int height = singleChannel.Height;

        var inOutPairs = targetCurve.InOutPairs;
        
        Parallel.For(0, height, y =>
        {
            for (int x = 0; x < width; x++)
            {
                *(p + y * step + x * 1) =
                    (byte)Math.Clamp(inOutPairs.AsValueEnumerable().First(z => z.In == *(p + y * step + x * 1)).Out, byte.MinValue,
                        byte.MaxValue);
            }
        });
    }

    private unsafe void OperateHSV(Mat hsv)
    {
        byte* p = (byte*)hsv.Data.ToPointer();

        for (int y = 0; y < hsv.Height; y++)
        {
            long step = hsv.Step();
            for (int x = 0; x < hsv.Width; x++)
            {
                //H
                *(p + y * step + x * 3) = CirculalyClamp(*(p + y * step + x * 3) + AddHue.Value, 0, 180);

                //S
                *(p + y * step + x * 3 + 1) = (byte)Math.Clamp(*(p + y * step + x * 3 + 1) + AddSaturation.Value, byte.MinValue, byte.MaxValue);

                //V
                *(p + y * step + x * 3 + 2) = (byte)Math.Clamp(*(p + y * step + x * 3 + 2) + AddValue.Value, byte.MinValue, byte.MaxValue);
            }
        }
    }

    private static byte CirculalyClamp(int value, byte min, byte max)
    {
        return (byte)(value > max ? value - Math.Abs(min) : (value < min ? value + max : value));
    }

    public override async Task OnRectChanged(Rect rect)
    {
        Render();
    }

    public override object Clone()
    {
        var clone = new ColorCorrectViewModel();
        clone.Owner = Owner;
        clone.Left.Value = Left.Value;
        clone.Top.Value = Top.Value;
        clone.Width.Value = Width.Value;
        clone.Height.Value = Height.Value;
        clone.EdgeBrush.Value = EdgeBrush.Value;
        clone.FillBrush.Value = FillBrush.Value;
        clone.EdgeThickness.Value = EdgeThickness.Value;
        clone.RotationAngle.Value = RotationAngle.Value;
        clone.StrokeLineJoin.Value = StrokeLineJoin.Value;
        clone.StrokeDashArray.Value = StrokeDashArray.Value;
        clone.StrokeMiterLimit.Value = StrokeMiterLimit.Value;
        clone.Bitmap.Value = Bitmap.Value;
        clone.AddHue.Value = AddHue.Value;
        clone.AddSaturation.Value = AddSaturation.Value;
        clone.AddValue.Value = AddValue.Value;
        clone.Curves = Curves;
        clone.TargetCurve = TargetCurve;
        clone.Threshold.Value = Threshold.Value;
        clone.MaxValue.Value = MaxValue.Value;
        clone.ThresholdTypes.Value = ThresholdTypes.Value;
        clone.OtsuEnabled.Value = OtsuEnabled.Value;
        clone.PathGeometryNoRotate.Value = PathGeometryNoRotate.Value;
        clone.PathGeometryRotate.Value = PathGeometryRotate.Value;
        return clone;
    }


    public override Type GetViewType()
    {
        return typeof(Image);
    }

    public override void OpenPropertyDialog()
    {
        var dialogService =
            new DialogService((Application.Current as PrismApplication).Container as IContainerExtension);
        IDialogResult result = null;
        dialogService.Show(nameof(DetailColorCorrect), new DialogParameters { { "ViewModel", this } }, ret => result = ret);
    }

    public override void OpenInstructionDialog()
    {
        var dialogService =
            new DialogService((Application.Current as PrismApplication).Container as IContainerExtension);
        IDialogResult result = null;
        dialogService.Show(nameof(ColorCorrectInstruction), new DialogParameters { { "ViewModel", this } }, ret => result = ret);
    }

    public override IDisposable BeginMonitor(Action action)
    {
        var compositeDisposable = new CompositeDisposable();
        base.BeginMonitor(action).AddTo(compositeDisposable);
        this.ObserveProperty(x => x.Bitmap).Subscribe(_ => action()).AddTo(compositeDisposable);
        return compositeDisposable;
    }
}