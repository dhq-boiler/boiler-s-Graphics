using boilersGraphics.Helpers;
using NUnit.Framework;
using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace boilersGraphics.Test
{
    [TestFixture]
    public class OpenCvSharpHelperTest
    {
        // 本クラスの本番上のバグ防止意図:
        // - GetImage は ActualWidth/ActualHeight が 0 の View に対して null を返し、
        //   呼び出し側が「サイズ未確定 View をレンダリングする」誤動作 (NRE / 真っ黒画像)
        //   を起こさないこと。
        // - SaveAsPng は書き出した PNG が正しく PNG として読み戻せること
        //   (ヘッダ破損/ストリーム閉じ忘れ等を catch)。
        // - ToMat は Bgr24 形式の Mat を返し、OpenCV 連携で色チャネルが化けないこと。

        private static Border BuildLayoutedView(int width, int height, Brush fill = null)
        {
            var border = new Border
            {
                Width = width,
                Height = height,
                Background = fill ?? new SolidColorBrush(Color.FromRgb(255, 0, 0)),
            };
            border.Measure(new Size(width, height));
            border.Arrange(new Rect(0, 0, width, height));
            return border;
        }

        // ---- GetImage ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void GetImage_サイズが空のViewはnullを返す()
        {
            // ActualWidth/ActualHeight が 0 のとき null を返す契約
            var view = new Border();
            // Measure/Arrange しない → Size empty
            var result = OpenCvSharpHelper.GetImage(view);
            Assert.That(result, Is.Null);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void GetImage_有効なViewは指定サイズのRenderTargetBitmapを返す()
        {
            var view = BuildLayoutedView(80, 60);
            var rtb = OpenCvSharpHelper.GetImage(view);

            Assert.That(rtb, Is.Not.Null);
            Assert.That(rtb.PixelWidth, Is.EqualTo(80));
            Assert.That(rtb.PixelHeight, Is.EqualTo(60));
        }

        // ---- SaveAsPng (Stream版) ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void SaveAsPng_書き出したPNGはPNGデコーダで読み戻せる()
        {
            var view = BuildLayoutedView(40, 30);
            var rtb = OpenCvSharpHelper.GetImage(view);
            using var ms = new MemoryStream();

            OpenCvSharpHelper.SaveAsPng(rtb, ms);

            Assert.That(ms.Length, Is.GreaterThan(0));
            // PNG ヘッダ確認 (89 50 4E 47 = ‰PNG)
            ms.Position = 0;
            var header = new byte[4];
            ms.Read(header, 0, 4);
            Assert.That(header[0], Is.EqualTo(0x89));
            Assert.That(header[1], Is.EqualTo(0x50));
            Assert.That(header[2], Is.EqualTo(0x4E));
            Assert.That(header[3], Is.EqualTo(0x47));

            // PngBitmapDecoder で読み戻して、Pixel サイズが一致することを確認
            ms.Position = 0;
            var decoder = new PngBitmapDecoder(ms, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
            var frame = decoder.Frames[0];
            Assert.That(frame.PixelWidth, Is.EqualTo(40));
            Assert.That(frame.PixelHeight, Is.EqualTo(30));
        }

        // ---- SaveAsPng (file版) ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void SaveAsPng_ファイル版でファイルが作られて中身がPNG()
        {
            var view = BuildLayoutedView(20, 20);
            var rtb = OpenCvSharpHelper.GetImage(view);
            var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"opencv_test_{Guid.NewGuid():N}.png");
            try
            {
                OpenCvSharpHelper.SaveAsPng(rtb, path);
                Assert.That(File.Exists(path), Is.True);
                var size = new FileInfo(path).Length;
                Assert.That(size, Is.GreaterThan(0));

                // 読み戻して PNG として認識される
                using var fs = File.OpenRead(path);
                var decoder = new PngBitmapDecoder(fs, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                Assert.That(decoder.Frames[0].PixelWidth, Is.EqualTo(20));
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        // ---- Save (FrameworkElement → file) ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void Save_FrameworkElement版がPNGを出力する()
        {
            var view = BuildLayoutedView(15, 15);
            var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"opencv_save_{Guid.NewGuid():N}.png");
            try
            {
                OpenCvSharpHelper.Save(view, path);
                Assert.That(File.Exists(path), Is.True);
                using var fs = File.OpenRead(path);
                var decoder = new PngBitmapDecoder(fs, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                Assert.That(decoder.Frames[0].PixelWidth, Is.EqualTo(15));
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        // ---- ToMat: Bgr24 形式に変換されることを保証 ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void ToMat_OpenCV連携用にBgr24チャネルのMatを返す()
        {
            // 単色 50x40 Bgra32 RTB を作る
            var view = BuildLayoutedView(50, 40);
            var rtb = OpenCvSharpHelper.GetImage(view);

            using var mat = OpenCvSharpHelper.ToMat(rtb);

            // OpenCV の Mat は H=Rows, W=Cols
            Assert.That(mat.Rows, Is.EqualTo(40));
            Assert.That(mat.Cols, Is.EqualTo(50));
            // Bgr24 = 3 channel, 8-bit
            Assert.That(mat.Channels(), Is.EqualTo(3));
            Assert.That(mat.Type(), Is.EqualTo(OpenCvSharp.MatType.CV_8UC3));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void ToMat_赤背景BorderはBGR順で青0_緑0_赤255()
        {
            var view = BuildLayoutedView(10, 10, new SolidColorBrush(Color.FromRgb(255, 0, 0)));
            var rtb = OpenCvSharpHelper.GetImage(view);

            using var mat = OpenCvSharpHelper.ToMat(rtb);

            // OpenCV は BGR 順で格納される
            // Mat.At<Vec3b>(row, col): [B, G, R]
            var pixel = mat.At<OpenCvSharp.Vec3b>(5, 5);
            Assert.That(pixel.Item0, Is.EqualTo(0));   // B
            Assert.That(pixel.Item1, Is.EqualTo(0));   // G
            Assert.That(pixel.Item2, Is.EqualTo(255)); // R
        }
    }
}
