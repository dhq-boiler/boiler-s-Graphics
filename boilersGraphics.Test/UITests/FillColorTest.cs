using boilersGraphics.Test.UITests.PageObjects;
using NUnit.Framework;
using OpenCvSharp;
using System;
using System.IO;
using System.Reflection;
using System.Threading;

namespace boilersGraphics.Test.UITests
{
    [TestFixture]
    public class FillColorTest : E2ETest
    {
        [Test, Apartment(ApartmentState.STA)]
        public void 塗りつぶし色をHueで赤指定して矩形描画()
        {
            var mainwindowPO = new MainWindowPO(Session);
            var selectFillColorDialogPO = mainwindowPO.Click_SelectFillColorButton();
            selectFillColorDialogPO.Click_Solid();
            selectFillColorDialogPO.HueDADA.Initialize();
            selectFillColorDialogPO.HueDADA.MoveTo(35, 10);
            selectFillColorDialogPO.HueDADA.ClickAndHold();
            selectFillColorDialogPO.HueDADA.MoveTo(182, 10);
            selectFillColorDialogPO.HueDADA.Release();
            selectFillColorDialogPO.HueDADA.Perform();
            selectFillColorDialogPO.ColorMapDADA.Initialize();
            selectFillColorDialogPO.ColorMapDADA.MoveTo(0, -1);
            selectFillColorDialogPO.ColorMapDADA.ClickAndHold();
            for (int i = 0; i <= byte.MaxValue; i++)
            {
                selectFillColorDialogPO.ColorMapDADA.MoveByOffset(1, 0);
            }
            selectFillColorDialogPO.ColorMapDADA.MoveByOffset(0, -1);
            selectFillColorDialogPO.ColorMapDADA.Release();
            selectFillColorDialogPO.ColorMapDADA.Perform();
            selectFillColorDialogPO.Click_OK();

            mainwindowPO.Click_RectangleTool();

            mainwindowPO.InitializeActions();
            mainwindowPO.MoveToElement(100, 100);
            mainwindowPO.ClickAndHold();
            for (int i = 1; i < 100; ++i)
                mainwindowPO.MoveByOffset(1, 1);
            mainwindowPO.Release();
            mainwindowPO.Perform();

            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filename = $"{dir}\\Canvas.jpg";
            mainwindowPO.SaveCanvas(filename);

            using (var mat = new Mat(filename))
            {
                PixelIsRed(mat, 150, 150);
            }
        }

        [Test, Apartment(ApartmentState.STA)]
        public void 塗りつぶし色を赤指定して矩形描画()
        {
            var mainwindowPO = new MainWindowPO(Session);
            var selectFillColorDialogPO = mainwindowPO.Click_SelectFillColorButton();
            selectFillColorDialogPO.Click_Solid();
            selectFillColorDialogPO.HueDADA.Initialize();
            selectFillColorDialogPO.HueDADA.MoveTo(35, 10);
            selectFillColorDialogPO.HueDADA.ClickAndHold();
            selectFillColorDialogPO.HueDADA.MoveTo(182, 10);
            selectFillColorDialogPO.HueDADA.Release();
            selectFillColorDialogPO.HueDADA.Perform();
            selectFillColorDialogPO.ColorMapDADA.Initialize();
            selectFillColorDialogPO.ColorMapDADA.MoveTo(1, 1);
            selectFillColorDialogPO.ColorMapDADA.ClickAndHold();
            for (int i = 0; i < byte.MaxValue; i++)
            {
                selectFillColorDialogPO.ColorMapDADA.MoveByOffset(1, 0);
            }
            selectFillColorDialogPO.ColorMapDADA.Release();
            selectFillColorDialogPO.ColorMapDADA.Perform();
            selectFillColorDialogPO.Click_OK();

            mainwindowPO.Click_RectangleTool();

            mainwindowPO.InitializeActions();
            mainwindowPO.MoveToElement(100, 100);
            mainwindowPO.ClickAndHold();
            for (int i = 1; i < 100; ++i)
                mainwindowPO.MoveByOffset(1, 1);
            mainwindowPO.Release();
            mainwindowPO.Perform();

            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filename = $"{dir}\\Canvas.jpg";
            mainwindowPO.SaveCanvas(filename);

            using (var mat = new Mat(filename))
            {
                PixelIsRed(mat, 150, 150);
            }
        }

        private void PixelIsRed(Mat mat, int y, int x)
        {
            Vec3b pic = mat.At<Vec3b>(y, x);
            Console.WriteLine($"(b, g, r) = ({pic.Item0}, {pic.Item1}, {pic.Item2})");
            Assert.That(pic.Item0, Is.EqualTo(0), "{0},{1}", y, x);
            Assert.That(pic.Item1, Is.EqualTo(0), "{0},{1}", y, x);
            Assert.That(pic.Item2, Is.EqualTo(255), "{0},{1}", y, x);
        }
    }
}
