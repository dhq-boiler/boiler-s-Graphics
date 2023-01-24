using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace boilersGraphics.Helpers
{
    static class OpenCvSharpHelper
    {
        [Conditional("DEBUG")]
        public static void ImShow(string windowTitle, BitmapSource rtb)
        {
            FormatConvertedBitmap newFormatedBitmapSource = new FormatConvertedBitmap();
            newFormatedBitmapSource.BeginInit();
            newFormatedBitmapSource.Source = rtb;
            newFormatedBitmapSource.DestinationFormat = PixelFormats.Bgr24;
            newFormatedBitmapSource.EndInit();

            var mat = OpenCvSharp.WpfExtensions.BitmapSourceConverter.ToMat(newFormatedBitmapSource);
            OpenCvSharp.Cv2.ImShow(windowTitle, mat);
        }

        [Conditional("DEBUG")]
        public static void ImShow(string windowTitle, Visual visual, int width, int height)
        {
            var rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(visual);
            rtb.Freeze();
            ImShow(windowTitle, rtb);
        }

        [Conditional("DEBUG")]
        public static void ImShow(string windowTitle, VisualBrush brush, int width, int height)
        {
            var rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            DrawingVisual visual = new DrawingVisual();
            using (DrawingContext context = visual.RenderOpen())
            {
                context.DrawRectangle(brush, null, new Rect(0, 0, width, height));
            }
            rtb.Render(visual);
            rtb.Freeze();
            ImShow(windowTitle, rtb);
        }

        public static void Save(FrameworkElement element, string filename)
        {
            var rtb = GetImage(element);
            using (var stream = new FileStream(filename, FileMode.OpenOrCreate))
            {
                SaveAsPng(rtb, stream);
            }
        }

        public static RenderTargetBitmap GetImage(FrameworkElement view)
        {
            Size size = new Size(view.ActualWidth, view.ActualHeight);
            if (size.IsEmpty)
                return null;

            RenderTargetBitmap result = new RenderTargetBitmap((int)size.Width, (int)size.Height, 96, 96, PixelFormats.Pbgra32);

            DrawingVisual drawingvisual = new DrawingVisual();
            using (DrawingContext context = drawingvisual.RenderOpen())
            {
                context.DrawRectangle(new VisualBrush(view), null, new Rect(new Point(), size));
                context.Close();
            }

            result.Render(drawingvisual);
            return result;
        }

        public static void SaveAsPng(RenderTargetBitmap src, Stream outputStream)
        {
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(src));

            encoder.Save(outputStream);
        }
    }
}
