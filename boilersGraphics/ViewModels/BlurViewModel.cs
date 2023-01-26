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
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace boilersGraphics.ViewModels
{
    public class BlurEffectViewModel : DesignerItemViewModelBase
    {
        public ReactivePropertySlim<WriteableBitmap> Bitmap { get; } = new ReactivePropertySlim<WriteableBitmap>();
        public ReactivePropertySlim<double> KernelWidth { get; } = new ReactivePropertySlim<double>(111d);
        public ReactivePropertySlim<double> KernelHeight { get; } = new ReactivePropertySlim<double>(111d);
        public ReactivePropertySlim<double> Sigma { get; } = new ReactivePropertySlim<double>(16d);

        public BlurEffectViewModel()
        {
            (Application.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel.AllItems.Subscribe(items =>
            {
                foreach (var item in items)
                {
                    item.BeginMonitor(() =>
                    {
                        Render();
                    }).AddTo(_CompositeDisposable);
                }
            }).AddTo(_CompositeDisposable);

            KernelWidth.Subscribe(_ =>
            {
                Render();
            }).AddTo(_CompositeDisposable);
            KernelHeight.Subscribe(_ =>
            {
                Render();
            }).AddTo(_CompositeDisposable);
        }

        public void Render()
        {
            if (this.Width.Value <= 0 || this.Height.Value <= 0)
            {
                return;
            }

            RenderTargetBitmap rtb = Renderer.Render(new System.Windows.Rect(Left.Value, Top.Value, Width.Value, Height.Value), App.Current.MainWindow.GetChildOfType<DesignerCanvas>(), (App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel, (App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel.BackgroundItem.Value, this);
            FormatConvertedBitmap newFormatedBitmapSource = new FormatConvertedBitmap();
            newFormatedBitmapSource.BeginInit();
            newFormatedBitmapSource.Source = rtb;
            newFormatedBitmapSource.DestinationFormat = PixelFormats.Bgr24;
            newFormatedBitmapSource.EndInit();

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

            using (var mat = BitmapSourceConverter.ToMat(newFormatedBitmapSource))
                using (var dest = new Mat())
            {
                Cv2.GaussianBlur(mat, dest, new OpenCvSharp.Size(KernelWidth.Value, KernelHeight.Value), Sigma.Value);
                Bitmap.Value = dest.ToWriteableBitmap();
            }
        }

        public override async Task OnRectChanged(System.Windows.Rect rect)
        {
            Render();
        }

        public ReactivePropertySlim<string> Source
        {
            get;
        }

        public ReadOnlyReactivePropertySlim<IList<byte>> Bytecode
        {
            get;
        }

        public ReadOnlyReactivePropertySlim<string> ErrorMessage
        {
            get;
        }

        public override bool SupportsPropertyDialog => true;

        public override object Clone()
        {
            var clone = new BlurEffectViewModel();
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
            clone.KernelWidth.Value = KernelWidth.Value;
            clone.KernelHeight.Value = KernelHeight.Value;
            clone.Bitmap.Value = Bitmap.Value;
            return clone;
        }

        public override PathGeometry CreateGeometry(bool flag = false)
        {
            return GeometryCreator.CreateRectangle(this, 0, 0, flag);
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
            var dialogService = new DialogService((App.Current as PrismApplication).Container as IContainerExtension);
            IDialogResult result = null;
            dialogService.Show(nameof(DetailBlur), new DialogParameters() { { "ViewModel", this } }, ret => result = ret);
        }
    }
}
