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
            var diagramViewModel = diagramControl.DataContext as DiagramViewModel;

            var tempIsSelected = new Dictionary<SelectableDesignerItemViewModelBase, bool>();
            foreach (var item in itemsControl.Items.Cast<SelectableDesignerItemViewModelBase>())
            {
                tempIsSelected.Add(item, item.IsSelected.Value);
                //一時的に選択状態を解除する
                item.IsSelected.Value = false;
            }

            foreach (var snapPointVM in itemsControl.Items.OfType<SnapPointViewModel>())
            {
                //一時的に非表示にする
                snapPointVM.Opacity.Value = 0;
            }

            var rtb = new RenderTargetBitmap((int)diagramViewModel.Width, (int)diagramViewModel.Height, 96, 96, PixelFormats.Pbgra32);

            DrawingVisual visual = new DrawingVisual();
            using (DrawingContext context = visual.RenderOpen())
            {
                //背景を描画
                RenderDesignerItemViewModelBase(context, backgroundItem);

                foreach (var item in diagramViewModel.AllItems.Value.Except(new SelectableDesignerItemViewModelBase[] { diagramViewModel.BackgroundItem.Value }).OrderBy(x => x.ZIndex.Value))
                {
                    if (item is DesignerItemViewModelBase designerItem)
                    {
                        RenderDesignerItemViewModelBase(context, designerItem);
                    }
                    else if (item is ConnectorBaseViewModel connector)
                    {
                        RenderConnectorBaseViewModel(context, connector);
                    }
                }
            }

            rtb.Render(visual);

            //IsSelectedの復元
            foreach (var item in itemsControl.Items.Cast<SelectableDesignerItemViewModelBase>())
            {
                bool outIsSelected;
                if (tempIsSelected.TryGetValue(item, out outIsSelected))
                {
                    item.IsSelected.Value = outIsSelected;
                }
            }

            foreach (var snapPointVM in itemsControl.Items.OfType<SnapPointViewModel>())
            {
                //スナップポイントを半透明に復元する
                snapPointVM.Opacity.Value = 0.5;
            }

            var generator = FileGenerator.Create(System.IO.Path.GetExtension(Path.Value));
            generator.AddFrame(BitmapFrame.Create(rtb));
            generator.SetQualityLevel(QualityLevel.Value);
            generator.Save(Path.Value);

            OkClose();
        }

        private static void RenderConnectorBaseViewModel(DrawingContext context, ConnectorBaseViewModel connector)
        {
            var diagramControl = App.Current.MainWindow.GetChildOfType<DiagramControl>();
            var view = diagramControl.GetCorrespondingViews<FrameworkElement>(connector).First(x => x.GetType() == connector.GetViewType());
            VisualBrush brush = new VisualBrush(view);
            context.DrawRectangle(brush, null, new Rect(connector.LeftTop.Value, new Size(connector.Width.Value, connector.Height.Value)));
        }

        private static void RenderDesignerItemViewModelBase(DrawingContext context, DesignerItemViewModelBase designerItem)
        {
            var diagramControl = App.Current.MainWindow.GetChildOfType<DiagramControl>();
            var view = diagramControl.GetCorrespondingViews<FrameworkElement>(designerItem).First(x => x.GetType() == designerItem.GetViewType());
            VisualBrush brush = new VisualBrush(view);
            context.DrawRectangle(brush, null, new Rect(new Point(designerItem.Left.Value, designerItem.Top.Value), new Size(designerItem.Width.Value, designerItem.Height.Value)));
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
                    case ".wmp":
                        return new WmpFileGenerator();
                    default:
                        throw new NotSupportedException($"{extension} is not supported");
                }
            }
        }

        abstract class FileGenerator<T> : IFileGenerator where T : BitmapEncoder, new()

        {
            protected BitmapEncoder encoder;
            public FileGenerator()
            {
                encoder = CreateEncoder();
            }

            public T Cast()
            {
                return encoder as T;
            }

            public T CreateEncoder()
            {
                return new T();
            }

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
            public override void SetQualityLevel(int level)
            {
                Cast().QualityLevel = level;
            }
        }

        class PngFileGenerator : FileGenerator<PngBitmapEncoder>
        {
            public override void SetQualityLevel(int level)
            {
            }
        }

        class GifFileGenerator : FileGenerator<GifBitmapEncoder>
        {
            public override void SetQualityLevel(int level)
            {
            }
        }

        class BmpFileGenerator : FileGenerator<BmpBitmapEncoder>
        {
            public override void SetQualityLevel(int level)
            {
            }
        }

        class TiffFileGenerator : FileGenerator<TiffBitmapEncoder>
        {
            public override void SetQualityLevel(int level)
            {
            }
        }

        class WmpFileGenerator : FileGenerator<WmpBitmapEncoder>
        {
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
            fileDialog.Filter = "JPEG file|*.jpg;*.jpeg|PNG file|*.png|GIF file|*.gif|BMP file|*.bmp|TIFF file|*.tiff|WMP file|*.wmp";
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
