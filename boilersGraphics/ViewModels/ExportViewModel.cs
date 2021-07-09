using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.UserControls;
using Microsoft.Win32;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace boilersGraphics.ViewModels
{
    class ExportViewModel : BindableBase, IDialogAware, IDisposable
    {
        private bool disposedValue;
        private CompositeDisposable _disposables = new CompositeDisposable();

        public ReactiveProperty<string> Path { get; set; } = new ReactiveProperty<string>();

        public ReactiveProperty<int> QualityLevel { get; set; } = new ReactiveProperty<int>(100);

        public ReactiveCommand PathFinderCommand { get; set; }

        public ReactiveCommand ExportCommand { get; set; }

        public ReactiveCommand CancelCommand { get; set; }

        public event Action<IDialogResult> RequestClose;

        public ExportViewModel()
        {
            PathFinderCommand = new ReactiveCommand();
            PathFinderCommand.Subscribe(_ => OpenFileDialog())
                             .AddTo(_disposables);
            ExportCommand = Path
                           .Where(x => x != null)
                           .Select(x => x.Length > 0)
                           .ToReactiveCommand();
            ExportCommand.Subscribe(_ => Export())
                         .AddTo(_disposables);
            CancelCommand = new ReactiveCommand();
            CancelCommand.Subscribe(_ => CancelClose())
                         .AddTo(_disposables);
            Path.Value = "";
        }

        private void Export()
        {
            var diagramControl = App.Current.MainWindow.GetChildOfType<DiagramControl>();
            var itemsControl = diagramControl.GetChildOfType<ItemsControl>();
            var designerCanvas = diagramControl.GetChildOfType<DesignerCanvas>();

            var tempIsSelected = new Dictionary<SelectableDesignerItemViewModelBase, bool>();
            foreach (var item in itemsControl.Items.Cast<SelectableDesignerItemViewModelBase>())
            {
                tempIsSelected.Add(item, item.IsSelected);
                item.IsSelected = false;
            }

            var rtb = new RenderTargetBitmap((int)designerCanvas.ActualWidth, (int)designerCanvas.ActualHeight, 96, 96, PixelFormats.Pbgra32);

            DrawingVisual visual = new DrawingVisual();
            using (DrawingContext context = visual.RenderOpen())
            {
                VisualBrush brush = new VisualBrush(designerCanvas);
                context.DrawRectangle(brush, null, new Rect(new Point(), new Size(designerCanvas.Width, designerCanvas.Height)));
            }

            rtb.Render(visual);

            //IsSelectedの復元
            foreach (var item in itemsControl.Items.Cast<SelectableDesignerItemViewModelBase>())
            {
                bool outIsSelected;
                if (tempIsSelected.TryGetValue(item, out outIsSelected))
                {
                    item.IsSelected = outIsSelected;
                }
            }

            var generator = FileGenerator.Create(System.IO.Path.GetExtension(Path.Value));
            generator.AddFrame(BitmapFrame.Create(rtb));
            generator.SetQualityLevel(QualityLevel.Value);
            generator.Save(Path.Value);

            OkClose();
        }

        interface IFileGenerator
        {
            void AddFrame(BitmapFrame frame);
            void SetQualityLevel(int level);
            void Save(string filename);
        }

        abstract class FileGenerator
        {
            public static IFileGenerator Create(string extension)
            {
                switch (extension)
                {
                    case ".jpg":
                    case ".jpeg":
                        return new JpegFileGenerator();
                    case ".png":
                        return new PngFileGenerator();
                    case ".gif":
                        return new GifFileGenerator();
                    case ".bmp":
                        return new BmpFileGenerator();
                    case ".tiff":
                        return new TiffFileGenerator();
                    default:
                        throw new NotSupportedException($"{extension} is not supported");
                }
            }
        }

        abstract class FileGenerator<T> : IFileGenerator where T : BitmapEncoder

        {
            protected BitmapEncoder encoder;
            public FileGenerator()
            {
                encoder = CreateEncoder();
            }

            public abstract BitmapEncoder CreateEncoder();

            public abstract void SetQualityLevel(int level);

            public void AddFrame(BitmapFrame frame)
            {
                encoder.Frames.Add(frame);
            }

            public void Save(string filename)
            {
                using (var stream = File.Create(filename))
                {
                    encoder.Save(stream);
                }
            }
        }

        class JpegFileGenerator : FileGenerator<JpegBitmapEncoder>
        {
            public override BitmapEncoder CreateEncoder()
            {
                return new JpegBitmapEncoder();
            }

            public override void SetQualityLevel(int level)
            {
                (encoder as JpegBitmapEncoder).QualityLevel = level;
            }
        }

        class PngFileGenerator : FileGenerator<PngBitmapEncoder>
        {
            public override BitmapEncoder CreateEncoder()
            {
                return new PngBitmapEncoder();
            }

            public override void SetQualityLevel(int level)
            {
            }
        }

        class GifFileGenerator : FileGenerator<GifBitmapEncoder>
        {
            public override BitmapEncoder CreateEncoder()
            {
                return new GifBitmapEncoder();
            }

            public override void SetQualityLevel(int level)
            {
            }
        }

        class BmpFileGenerator : FileGenerator<BmpBitmapEncoder>
        {
            public override BitmapEncoder CreateEncoder()
            {
                return new BmpBitmapEncoder();
            }

            public override void SetQualityLevel(int level)
            {
            }
        }

        class TiffFileGenerator : FileGenerator<TiffBitmapEncoder>
        {
            public override BitmapEncoder CreateEncoder()
            {
                return new TiffBitmapEncoder();
            }

            public override void SetQualityLevel(int level)
            {
            }
        }

        private void OkClose()
        {
            var result = new DialogResult(ButtonResult.OK);
            RequestClose.Invoke(result);
        }

        private void CancelClose()
        {
            var result = new DialogResult(ButtonResult.Cancel);
            RequestClose.Invoke(result);
        }

        private void OpenFileDialog()
        {
            var fileDialog = new SaveFileDialog();
            fileDialog.Filter = "JPEG file|*.jpg;*.jpeg|PNG file|*.png|GIF file|*.gif|BMP file|*.bmp|TIFF file|*.tiff";
            if (fileDialog.ShowDialog() == true)
            {
                Path.Value = fileDialog.FileName;
            }
        }

        public string Title => "エクスポート";

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _disposables.Dispose();
                }

                _disposables = null;
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
        }
    }
}
