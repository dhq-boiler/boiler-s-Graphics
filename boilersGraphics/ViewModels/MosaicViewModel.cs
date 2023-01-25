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
    public class MosaicViewModel : DesignerItemViewModelBase
    {
        public ReactivePropertySlim<WriteableBitmap> Bitmap { get; } = new ReactivePropertySlim<WriteableBitmap>();
        public ReactivePropertySlim<double> ColumnPixels { get; } = new ReactivePropertySlim<double>(30d);
        public ReactivePropertySlim<double> RowPixels { get; } = new ReactivePropertySlim<double>(30d);

        public MosaicViewModel()
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

            ColumnPixels.Subscribe(_ =>
            {
                Render();
            }).AddTo(_CompositeDisposable);
            RowPixels.Subscribe(_ =>
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

            App.Current.Dispatcher.Invoke(() =>
            {
                RenderTargetBitmap rtb = Renderer.Render(new System.Windows.Rect(Left.Value, Top.Value, Width.Value, Height.Value), App.Current.MainWindow.GetChildOfType<DesignerCanvas>(), (App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel, (App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel.BackgroundItem.Value, this);
                FormatConvertedBitmap newFormatedBitmapSource = new FormatConvertedBitmap();
                newFormatedBitmapSource.BeginInit();
                newFormatedBitmapSource.Source = rtb;
                newFormatedBitmapSource.DestinationFormat = PixelFormats.Bgr24;
                newFormatedBitmapSource.EndInit();

                using (var mat = BitmapSourceConverter.ToMat(newFormatedBitmapSource))
                using (var dest = mat.Clone())
                {
                    Mosaic(mat, dest, ColumnPixels.Value, RowPixels.Value);
                    Bitmap.Value = dest.ToWriteableBitmap();
                }
            });
        }

        private unsafe void Mosaic(Mat mat, Mat dest, double columnPixels, double rowPixels)
        {
            var column = columnPixels;
            var row = rowPixels;
            if (column <= 0 || row <= 0)
            {
                byte* _p_src = (byte*)mat.Data.ToPointer();
                byte* _p_dst = (byte*)dest.Data.ToPointer();
                int _channels = mat.Channels();
                long _destStep = dest.Step();
                long _srcStep = mat.Step();

                Parallel.For(0, mat.Height, y =>
                {
                    for (int x = 0; x < mat.Width; x++)
                    {
                        for (int c = 0; c < _channels; c++)
                        {
                            *(_p_dst + y * _destStep + x * _channels + c) = *(_p_src + y * _srcStep + x * _channels + c);
                        }
                    }
                });
                return;
            }
            if (mat.Width > mat.Height)
            {
                column = column * mat.Width / mat.Height;
            }
            else
            {
                row = row * mat.Height / mat.Width;
            }

            byte* p_src = (byte*)mat.Data.ToPointer();
            byte* p_dst = (byte*)dest.Data.ToPointer();
            int channels = mat.Channels();
            long destStep = dest.Step();
            long srcStep = mat.Step();

            Parallel.For(0, mat.Height, y =>
            {
                var yy = Y(y, row);
                for (int x = 0; x < mat.Width; x++)
                {
                    var xx = X(x, column);
                    for (int c = 0; c < channels; c++)
                    {
                        if (mat.Cols <= xx || mat.Rows <= yy)
                        {
                            continue;
                        }
                        *(p_dst + y * destStep + x * channels + c) = *(p_src + yy * srcStep + xx * channels + c);
                    }
                }
            });
        }

        private long Y(int y, double row)
        {
            return (long)(0.5 * (Math.Floor((double)y / row) + Math.Ceiling((double)y / row)) * row);
        }
        private long X(int x, double column)
        {
            return (long)(0.5 * (Math.Floor((double)x / column) + Math.Ceiling((double)x / column)) * column);
        }

        public override void OnRectChanged(System.Windows.Rect rect)
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
            dialogService.Show(nameof(DetailMosaic), new DialogParameters() { { "ViewModel", this } }, ret => result = ret);
        }
    }
}
