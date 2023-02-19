using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.Views;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Prism.Unity;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using boilersGraphics.Models;
using boilersGraphics.ViewModels.ColorCorrect;
using Rect = System.Windows.Rect;
using boilersGraphics.Exceptions;

namespace boilersGraphics.ViewModels;

public class ColorCorrectViewModel : EffectViewModel
{
    public ColorCorrectViewModel()
    {
        Initialize();
    }

    public ReactivePropertySlim<ColorCorrectType> CCType { get; } = new(ColorCorrectType.HSV);
    public ReactivePropertySlim<Channel> TargetChannel { get; } = new(Channel.GrayScale);

    public ReactivePropertySlim<WriteableBitmap> Bitmap { get; } = new();

    #region 色相・彩度・輝度
    public ReactivePropertySlim<int> AddHue { get; } = new();
    public ReactivePropertySlim<int> AddSaturation { get; } = new();
    public ReactivePropertySlim<int> AddValue { get; } = new();
    #endregion

    #region トーンカーブ
    public ReactiveCollection<ToneCurveViewModel.Point> Points { get; set; } = new();
    public ReactiveCollection<InOutPair> InOutPairs { get; set; } = new();
    #endregion

    #region 2値化
    public ReactivePropertySlim<double> Threshold { get; } = new();
    public ReactivePropertySlim<double> MaxValue { get; } = new();
    public ReactivePropertySlim<ThresholdTypes> ThresholdTypes { get; } = new(boilersGraphics.ViewModels.ThresholdTypes.Binary);
    public ReactivePropertySlim<bool> OtsuEnabled { get; } = new();
    #endregion


    public override bool SupportsPropertyDialog => true;

    public override void Render()
    {
        if (Width.Value <= 0 || Height.Value <= 0) return;

        Application.Current.Dispatcher.Invoke(() =>
        {
            var mainWindowViewModel = Application.Current.MainWindow.DataContext as MainWindowViewModel;
            var renderer = new EffectRenderer(new WpfVisualTreeHelper());
            var rtb = renderer.Render(Rect.Value, Application.Current.MainWindow.GetChildOfType<DesignerCanvas>(),
                mainWindowViewModel.DiagramViewModel, mainWindowViewModel.DiagramViewModel.BackgroundItem.Value, this, this.ZIndex.Value - 1);
            var newFormattedBitmapSource = new FormatConvertedBitmap();
            newFormattedBitmapSource.BeginInit();
            newFormattedBitmapSource.Source = rtb;
            newFormattedBitmapSource.DestinationFormat = PixelFormats.Bgr24;
            newFormattedBitmapSource.EndInit();

            using (var mat = newFormattedBitmapSource.ToMat())
            using (var hsv = mat.Clone())
            using (var dest = mat.Clone())
            {
                switch (CCType.Value)
                {
                    case Hsv:
                        Cv2.CvtColor(mat, hsv, ColorConversionCodes.BGR2HSV);
                        OperateHSV(hsv);
                        Cv2.CvtColor(hsv, dest, ColorConversionCodes.HSV2BGR);
                        break;
                    case ToneCurve:
                        switch (TargetChannel.Value)
                        {
                            case GrayScaleChannel:
                                Cv2.CvtColor(mat, hsv, ColorConversionCodes.BGR2HSV);
                                var arr4 = Cv2.Split(hsv);
                                OperateToneCurve(arr4[2]);
                                Cv2.Merge(arr4, hsv);
                                Cv2.CvtColor(hsv, dest, ColorConversionCodes.HSV2BGR);
                                break;
                            case BlueChannel:
                                var arr0 = Cv2.Split(mat);
                                OperateToneCurve(arr0[0]);
                                Cv2.Merge(arr0, dest);
                                break;
                            case GreenChannel:
                                var arr1 = Cv2.Split(mat);
                                OperateToneCurve(arr1[1]);
                                Cv2.Merge(arr1, dest);
                                break;
                            case RedChannel:
                                var arr2 = Cv2.Split(mat);
                                OperateToneCurve(arr2[2]);
                                Cv2.Merge(arr2, dest);
                                break;
                        }
                        break;
                    case NegativePositiveConversion:
                        Cv2.BitwiseNot(mat, dest);
                        break;
                    case Binarization:
                        using (var grayscale = new Mat())
                        {
                            Cv2.CvtColor(mat, grayscale, ColorConversionCodes.BGR2GRAY);
                            var flags = ThresholdTypes.Value.ToOpenCvValue();
                            if (OtsuEnabled.Value)
                            {
                                flags |= ViewModels.ThresholdTypes.Otsu.ToOpenCvValue();
                            }
                            Cv2.Threshold(grayscale, dest, Threshold.Value, MaxValue.Value, flags);
                        }
                        break;
                }

                Bitmap.Value = dest.ToWriteableBitmap();
                UpdateLayout();
            }
        });
    }

    private unsafe void OperateToneCurve(Mat singleChannel)
    {
        if (InOutPairs.Count < 256)
            return;
        if (singleChannel.Channels() != 1)
            throw new UnexpectedException("singleChannel.Channels() != 1");

        byte* p = (byte*)singleChannel.Data.ToPointer();
        long step = singleChannel.Step();
        int width = singleChannel.Width;
        int height = singleChannel.Height;
        
        Parallel.For(0, height, y =>
        {
            for (int x = 0; x < width; x++)
            {
                *(p + y * step + x * 1) =
                    (byte)Math.Clamp(InOutPairs.First(z => z.In == *(p + y * step + x * 1)).Out, byte.MinValue,
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