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
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Rect = System.Windows.Rect;

namespace boilersGraphics.ViewModels;

public class MosaicViewModel : EffectViewModel
{
    public MosaicViewModel()
    {
        Initialize();
    }

    public override void Initialize()
    {
        base.Initialize();
        ColumnPixels.Subscribe(_ => { Render(); }).AddTo(_CompositeDisposable);
        RowPixels.Subscribe(_ => { Render(); }).AddTo(_CompositeDisposable);
    }

    public ReactivePropertySlim<WriteableBitmap> Bitmap { get; } = new();
    public ReactivePropertySlim<double> ColumnPixels { get; } = new(30d);
    public ReactivePropertySlim<double> RowPixels { get; } = new(30d);

    public ReactivePropertySlim<string> Source { get; }

    public ReadOnlyReactivePropertySlim<IList<byte>> Bytecode { get; }

    public ReadOnlyReactivePropertySlim<string> ErrorMessage { get; }

    public override bool SupportsPropertyDialog => true;

    public override void Render()
    {
        if (Width.Value <= 0 || Height.Value <= 0) return;

        Application.Current.Dispatcher.Invoke(() =>
        {
            var mainWindowViewModel = Application.Current.MainWindow.DataContext as MainWindowViewModel;
            var renderer = new Renderer(new WpfVisualTreeHelper());
            var rtb = renderer.Render(Rect.Value, Application.Current.MainWindow.GetChildOfType<DesignerCanvas>(),
                mainWindowViewModel.DiagramViewModel, mainWindowViewModel.DiagramViewModel.BackgroundItem.Value, this.ZIndex.Value - 1);
            var newFormattedBitmapSource = new FormatConvertedBitmap();
            newFormattedBitmapSource.BeginInit();
            newFormattedBitmapSource.Source = rtb;
            newFormattedBitmapSource.DestinationFormat = PixelFormats.Bgr24;
            newFormattedBitmapSource.EndInit();

            using (var mat = newFormattedBitmapSource.ToMat())
            using (var dest = mat.Clone())
            {
                Mosaic(mat, dest, ColumnPixels.Value, RowPixels.Value);
                Bitmap.Value = dest.ToWriteableBitmap();
                UpdateLayout();
            }
        });
    }

    private unsafe void Mosaic(Mat mat, Mat dest, double columnPixels, double rowPixels)
    {
        var column = columnPixels;
        var row = rowPixels;
        if (column <= 0 || row <= 0)
        {
            var _p_src = (byte*)mat.Data.ToPointer();
            var _p_dst = (byte*)dest.Data.ToPointer();
            var _channels = mat.Channels();
            var _destStep = dest.Step();
            var _srcStep = mat.Step();

            Parallel.For(0, mat.Height, y =>
            {
                for (var x = 0; x < mat.Width; x++)
                for (var c = 0; c < _channels; c++)
                    *(_p_dst + y * _destStep + x * _channels + c) = *(_p_src + y * _srcStep + x * _channels + c);
            });
            return;
        }

        if (mat.Width > mat.Height)
            column = column * mat.Width / mat.Height;
        else
            row = row * mat.Height / mat.Width;

        var p_src = (byte*)mat.Data.ToPointer();
        var p_dst = (byte*)dest.Data.ToPointer();
        var channels = mat.Channels();
        var destStep = dest.Step();
        var srcStep = mat.Step();
        var srcCols = mat.Cols;
        var srcRows = mat.Rows;

        Parallel.For(0, srcRows,
            y => { ProcessRow(y, column, row, p_src, p_dst, channels, destStep, srcStep, srcCols, srcRows); });
    }

    private unsafe void ProcessRow(int y, double column, double row, byte* p_src, byte* p_dst, int channels,
        long destStep, long srcStep, int srcCols, int srcRows)
    {
        var yy = GetMosaicPixelIndex(y, row);
        if (srcRows <= yy) return;
        for (var x = 0; x < srcCols; x++)
        {
            var xx = GetMosaicPixelIndex(x, column);
            if (srcCols <= xx) continue;
            ProcessColumn(y, column, p_src, p_dst, channels, destStep, srcStep, srcCols, srcRows, yy, x, xx);
        }
    }

    private unsafe void ProcessColumn(int y, double column, byte* p_src, byte* p_dst, int channels, long destStep,
        long srcStep, int srcCols, int srcRows, long yy, int x, long xx)
    {
        for (var c = 0; c < channels; c++)
            *(p_dst + y * destStep + x * channels + c) = *(p_src + yy * srcStep + xx * channels + c);
    }

    private static long GetMosaicPixelIndex(int a, double b)
    {
        var aDivideByB = a / b;
        var mod = aDivideByB % 1;
        double ceiling;
        double floor;
        if (mod < 0.5)
        {
            //mod = 0.4 -> 1.4 -> (long)1.4 -> 1
            floor = (long)aDivideByB + (long)(1d + mod);
            //mod = 0.4 -> -0.1 -> (long)-0.1 -> 0
            ceiling = (long)aDivideByB + (long)(mod - 0.5);
        }
        else
        {
            //mod = 0.6 -> 1.1 -> (long)1.1 -> 1
            floor = (long)aDivideByB + (long)(0.5 + mod);
            //mod = 0.6 -> -0.4 -> (long)-0.4 -> 0
            ceiling = (long)aDivideByB + (long)(mod - 1);
        }

        return (long)(0.5 * (floor + ceiling) * b);
    }

    public override async Task OnRectChanged(Rect rect)
    {
        Render();
    }

    public override object Clone()
    {
        var clone = new MosaicViewModel();
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
        clone.ColumnPixels.Value = ColumnPixels.Value;
        clone.RowPixels.Value = RowPixels.Value;
        clone.Bitmap.Value = Bitmap.Value;
        return clone;
    }

    public override PathGeometry CreateGeometry(double angle)
    {
        return GeometryCreator.CreateRectangleWithAngle(this, 0, 0, RotationAngle.Value);
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
        dialogService.Show(nameof(DetailMosaic), new DialogParameters { { "ViewModel", this } }, ret => result = ret);
    }
    public override IDisposable BeginMonitor(Action action)
    {
        var compositeDisposable = new CompositeDisposable();
        base.BeginMonitor(action).AddTo(compositeDisposable);
        this.ObserveProperty(x => x.Bitmap).Subscribe(_ => action()).AddTo(compositeDisposable);
        return compositeDisposable;
    }
}