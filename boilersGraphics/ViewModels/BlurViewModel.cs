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
using Size = OpenCvSharp.Size;

namespace boilersGraphics.ViewModels;

public class BlurEffectViewModel : EffectViewModel
{
    public BlurEffectViewModel()
    {
        Initialize();
    }

    public override void Initialize()
    {
        base.Initialize();
        KernelWidth.Subscribe(_ => { Render(); }).AddTo(_CompositeDisposable);
        KernelHeight.Subscribe(_ => { Render(); }).AddTo(_CompositeDisposable);
    }

    public ReactivePropertySlim<WriteableBitmap> Bitmap { get; } = new();
    public ReactivePropertySlim<double> KernelWidth { get; } = new(111d);
    public ReactivePropertySlim<double> KernelHeight { get; } = new(111d);
    public ReactivePropertySlim<double> Sigma { get; } = new(16d);

    public ReactivePropertySlim<string> Source { get; }

    public ReadOnlyReactivePropertySlim<IList<byte>> Bytecode { get; }

    public ReadOnlyReactivePropertySlim<string> ErrorMessage { get; }

    public override bool SupportsPropertyDialog => true;

    public override void Render()
    {
        if (Width.Value <= 0 || Height.Value <= 0) return;

        var renderer = new EffectRenderer(new WpfVisualTreeHelper());
        var rtb = renderer.Render(new Rect(Left.Value, Top.Value, Width.Value, Height.Value),
            Application.Current.MainWindow.GetChildOfType<DesignerCanvas>(),
            (Application.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel,
            (Application.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel.BackgroundItem.Value, this, 
            0,
            this.ZIndex.Value - 1);
        var newFormattedBitmapSource = new FormatConvertedBitmap();
        newFormattedBitmapSource.BeginInit();
        newFormattedBitmapSource.Source = rtb;
        newFormattedBitmapSource.DestinationFormat = PixelFormats.Bgr24;
        newFormattedBitmapSource.EndInit();

        if (!(KernelWidth.Value > 0 && KernelWidth.Value % 2 == 1))
        {
            MessageBox.Show("!(KernelWidth > 0 && KernelWidth% 2 == 1)");
            return;
        }

        if (!(KernelHeight.Value > 0 && KernelHeight.Value % 2 == 1))
        {
            MessageBox.Show("!(KernelHeight > 0 && KernelHeight% 2 == 1)");
            return;
        }

        using var mat = newFormattedBitmapSource.ToMat();
        using var dest = new Mat();
        Cv2.GaussianBlur(mat, dest, new Size(KernelWidth.Value, KernelHeight.Value), Sigma.Value);
        Bitmap.Value = dest.ToWriteableBitmap();
        UpdateLayout();
    }

    public override async Task OnRectChanged(Rect rect)
    {
        Render();
    }

    public override object Clone()
    {
        var clone = new BlurEffectViewModel
        {
            Owner = Owner
        };
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
        clone.KernelWidth.Value = KernelWidth.Value;
        clone.KernelHeight.Value = KernelHeight.Value;
        clone.Bitmap.Value = Bitmap.Value;
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
        dialogService.Show(nameof(DetailBlur), new DialogParameters { { "ViewModel", this } }, ret => result = ret);
    }
    public override IDisposable BeginMonitor(Action action)
    {
        var compositeDisposable = new CompositeDisposable();
        base.BeginMonitor(action).AddTo(compositeDisposable);
        this.ObserveProperty(x => x.Bitmap).Subscribe(_ => action()).AddTo(compositeDisposable);
        return compositeDisposable;
    }
}