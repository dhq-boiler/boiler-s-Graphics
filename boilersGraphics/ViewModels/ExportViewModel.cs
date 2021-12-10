using boilersGraphics.Controls;
using boilersGraphics.Dao;
using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.UserControls;
using Microsoft.Win32;
using NLog;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace boilersGraphics.ViewModels
{
    public class ExportViewModel : BindableBase, IDialogAware, IDisposable
    {
        private bool disposedValue;
        private CompositeDisposable _disposables = new CompositeDisposable();

        public ReactivePropertySlim<string> Path { get; set; } = new ReactivePropertySlim<string>();

        public ReactivePropertySlim<int> QualityLevel { get; set; } = new ReactivePropertySlim<int>(100);

        public ReactivePropertySlim<Rect?> SliceRect { get; set; } = new ReactivePropertySlim<Rect?>();

        public ReactivePropertySlim<RenderTargetBitmap> Preview { get; set; } = new ReactivePropertySlim<RenderTargetBitmap>();

        public ReactiveCommand PathFinderCommand { get; set; }

        public ReactiveCommand ExportCommand { get; set; }

        public ReactiveCommand CancelCommand { get; set; }

        public event Action<IDialogResult> RequestClose;

        public ExportViewModel(MainWindowViewModel mainWindowViewModel)
        {
            var mainWindow = App.Current.MainWindow;
            var designerCanvas = mainWindow.GetChildOfType<DesignerCanvas>();
            var diagramViewModel = mainWindow.GetChildOfType<DiagramControl>().DataContext as DiagramViewModel;
            PathFinderCommand = new ReactiveCommand();
            PathFinderCommand.Subscribe(_ => OpenFileDialog())
                             .AddTo(_disposables);
            ExportCommand = Path
                           .Where(x => x != null)
                           .Select(x => x.Length > 0)
                           .ToReactiveCommand();
            ExportCommand.Subscribe(_ => ExportProcess(designerCanvas, mainWindowViewModel, diagramViewModel))
                         .AddTo(_disposables);
            CancelCommand = new ReactiveCommand();
            CancelCommand.Subscribe(_ => CancelClose())
                         .AddTo(_disposables);
            Path.Value = "";

            SliceRect.Subscribe(x =>
            {
                var tempLayoutTransform = designerCanvas.LayoutTransform;
                designerCanvas.LayoutTransform = new MatrixTransform();

                var backgroundItem = diagramViewModel.BackgroundItem.Value;
                RenderTargetBitmap rtb = Render(designerCanvas, diagramViewModel, backgroundItem);
                Preview.Value = rtb;

                //OpenCvSharpHelper.ImShow("preview", rtb);
                
                designerCanvas.LayoutTransform = tempLayoutTransform;
            })
            .AddTo(_disposables);
        }

        private void ExportProcess(DesignerCanvas designerCanvas, MainWindowViewModel mainWindowViewModel, DiagramViewModel diagramViewModel)
        {
            Dictionary<SelectableDesignerItemViewModelBase, bool> tempIsSelected;
            Transform tempLayoutTransform;

            BeforeExport(designerCanvas, diagramViewModel, out tempIsSelected, out tempLayoutTransform);

            Export(mainWindowViewModel, designerCanvas, diagramViewModel);

            AfterExport(designerCanvas, diagramViewModel, tempIsSelected, tempLayoutTransform);

            OkClose();
        }

        private void BeforeExport(DesignerCanvas designerCanvas, DiagramViewModel diagramViewModel, out Dictionary<SelectableDesignerItemViewModelBase, bool> tempIsSelected, out Transform tempLayoutTransform)
        {
            tempIsSelected = new Dictionary<SelectableDesignerItemViewModelBase, bool>();
            var backgroundItem = diagramViewModel.BackgroundItem.Value;

            foreach (var item in diagramViewModel.AllItems.Value.Cast<SelectableDesignerItemViewModelBase>())
            {
                tempIsSelected.Add(item, item.IsSelected.Value);
                //一時的に選択状態を解除する
                item.IsSelected.Value = false;
            }

            foreach (var snapPointVM in diagramViewModel.AllItems.Value.OfType<SnapPointViewModel>())
            {
                //一時的に非表示にする
                snapPointVM.Opacity.Value = 0;
            }

            //一時的に背景の境界線を消す
            backgroundItem.EdgeThickness.Value = 0;

            tempLayoutTransform = designerCanvas.LayoutTransform;
            designerCanvas.LayoutTransform = new MatrixTransform();
        }

        private void Export(MainWindowViewModel mainWindowViewModel, DesignerCanvas designerCanvas, DiagramViewModel diagramViewModel)
        {
            var backgroundItem = diagramViewModel.BackgroundItem.Value;
            RenderTargetBitmap rtb = Render(designerCanvas, diagramViewModel, backgroundItem);

            //OpenCvSharpHelper.ImShow("test", rtb);

            var generator = FileGenerator.Create(System.IO.Path.GetExtension(Path.Value));
            generator.AddFrame(BitmapFrame.Create(rtb));
            generator.SetQualityLevel(QualityLevel.Value);
            generator.Save(mainWindowViewModel, Path.Value);
            LogManager.GetCurrentClassLogger().Info($"Exported:{Path.Value}");

            UpdateStatisticsCount(mainWindowViewModel);
        }

        private RenderTargetBitmap Render(DesignerCanvas designerCanvas, DiagramViewModel diagramViewModel, BackgroundViewModel backgroundItem)
        {
            Size size;
            if (SliceRect.Value.HasValue)
            {
                size = SliceRect.Value.Value.Size;
            }
            else
            {
                size = new Size(diagramViewModel.Width, diagramViewModel.Height);
            }
            var rtb = new RenderTargetBitmap((int)size.Width, (int)size.Height, 96, 96, PixelFormats.Pbgra32);

            DrawingVisual visual = new DrawingVisual();
            using (DrawingContext context = visual.RenderOpen())
            {
                //背景を描画
                RenderBackgroundViewModel(designerCanvas, context, backgroundItem);

                VisualBrush brush = new VisualBrush(designerCanvas);
                brush.Stretch = Stretch.None;
                Rect rect;
                if (SliceRect.Value.HasValue)
                {
                    rect = SliceRect.Value.Value;
                }
                else
                {
                    var bounds = VisualTreeHelper.GetDescendantBounds(designerCanvas);
                    LogManager.GetCurrentClassLogger().Debug($"bounds:{bounds}");
                    rect = bounds;
                }
                context.DrawRectangle(brush, null, rect);
            }

            rtb.Render(visual);
            return rtb;
        }

        private static void UpdateStatisticsCount(MainWindowViewModel mainWindowViewModel)
        {
            var statistics = mainWindowViewModel.Statistics.Value;
            statistics.NumberOfExports++;
            var dao = new StatisticsDao();
            dao.Update(statistics);
        }

        private void AfterExport(DesignerCanvas designerCanvas, DiagramViewModel diagramViewModel, Dictionary<SelectableDesignerItemViewModelBase, bool> tempIsSelected, Transform tempLayoutTransform)
        {
            var backgroundItem = diagramViewModel.BackgroundItem.Value;

            //背景の境界線を復元する
            backgroundItem.EdgeThickness.Value = 1;

            //IsSelectedの復元
            foreach (var item in diagramViewModel.AllItems.Value.Cast<SelectableDesignerItemViewModelBase>())
            {
                bool outIsSelected;
                if (tempIsSelected.TryGetValue(item, out outIsSelected))
                {
                    item.IsSelected.Value = outIsSelected;
                }
            }

            foreach (var snapPointVM in diagramViewModel.AllItems.Value.OfType<SnapPointViewModel>())
            {
                //スナップポイントを半透明に復元する
                snapPointVM.Opacity.Value = 0.5;
            }

            designerCanvas.LayoutTransform = tempLayoutTransform;
        }

        private void RenderBackgroundViewModel(DesignerCanvas designerCanvas, DrawingContext context, BackgroundViewModel background)
        {
            var views = designerCanvas.GetCorrespondingViews<FrameworkElement>(background);
            var view = views.First(x => x.GetType() == background.GetViewType());
            var bounds = VisualTreeHelper.GetDescendantBounds(view);

            Rect rect;
            if (SliceRect.Value.HasValue)
            {
                rect = SliceRect.Value.Value;
            }
            else
            {
                rect = bounds;
            }

            VisualBrush brush = new VisualBrush(view);
            brush.Stretch = Stretch.None;
            view.UpdateLayout();
            if (SliceRect.Value.HasValue)
            {
                context.DrawRectangle(brush, null, rect);
            }
            else
            {
                context.DrawRectangle(brush, null, rect);
            }
        }

        interface IFileGenerator
        {
            void AddFrame(BitmapFrame frame);
            void SetQualityLevel(int level);
            void Save(MainWindowViewModel mainWindowViewModel, string filename);
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

            public void Save(MainWindowViewModel mainWindowViewModel, string filename)
            {
                using (var stream = File.Create(filename))
                {
                    encoder.Save(stream);
                }
                UpdateStatisticsCount(mainWindowViewModel);
            }

            public abstract void UpdateStatisticsCount(MainWindowViewModel mainWindowViewModel);
        }

        class JpegFileGenerator : FileGenerator<JpegBitmapEncoder>
        {
            public override void SetQualityLevel(int level)
            {
                Cast().QualityLevel = level;
            }

            public override void UpdateStatisticsCount(MainWindowViewModel mainWindowViewModel)
            {
                var statistics = mainWindowViewModel.Statistics.Value;
                statistics.NumberOfJpegExports++;
                var dao = new StatisticsDao();
                dao.Update(statistics);
            }
        }

        class PngFileGenerator : FileGenerator<PngBitmapEncoder>
        {
            public override void SetQualityLevel(int level)
            {
            }

            public override void UpdateStatisticsCount(MainWindowViewModel mainWindowViewModel)
            {
                var statistics = mainWindowViewModel.Statistics.Value;
                statistics.NumberOfPngExports++;
                var dao = new StatisticsDao();
                dao.Update(statistics);
            }
        }

        class GifFileGenerator : FileGenerator<GifBitmapEncoder>
        {
            public override void SetQualityLevel(int level)
            {
            }

            public override void UpdateStatisticsCount(MainWindowViewModel mainWindowViewModel)
            {
                var statistics = mainWindowViewModel.Statistics.Value;
                statistics.NumberOfGifExports++;
                var dao = new StatisticsDao();
                dao.Update(statistics);
            }
        }

        class BmpFileGenerator : FileGenerator<BmpBitmapEncoder>
        {
            public override void SetQualityLevel(int level)
            {
            }

            public override void UpdateStatisticsCount(MainWindowViewModel mainWindowViewModel)
            {
                var statistics = mainWindowViewModel.Statistics.Value;
                statistics.NumberOfBmpExports++;
                var dao = new StatisticsDao();
                dao.Update(statistics);
            }
        }

        class TiffFileGenerator : FileGenerator<TiffBitmapEncoder>
        {
            public override void SetQualityLevel(int level)
            {
            }

            public override void UpdateStatisticsCount(MainWindowViewModel mainWindowViewModel)
            {
                var statistics = mainWindowViewModel.Statistics.Value;
                statistics.NumberOfTiffExports++;
                var dao = new StatisticsDao();
                dao.Update(statistics);
            }
        }

        class WmpFileGenerator : FileGenerator<WmpBitmapEncoder>
        {
            public override void SetQualityLevel(int level)
            {
            }

            public override void UpdateStatisticsCount(MainWindowViewModel mainWindowViewModel)
            {
                var statistics = mainWindowViewModel.Statistics.Value;
                statistics.NumberOfWmpExports++;
                var dao = new StatisticsDao();
                dao.Update(statistics);
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
            if (parameters.ContainsKey("sliceRect"))
            {
                SliceRect.Value = parameters.GetValue<Rect>("sliceRect");
            }
        }
    }
}
