using boilersGraphics.Test.UITests.PageObjects;
using NUnit.Framework;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace boilersGraphics.Test.UITests
{
    [TestFixture]
    public class EdgeColorTest : E2ETest
    {
        [Test, Apartment(ApartmentState.STA)]
        [Retry(3)]
        public void エッジ色を赤指定して矩形描画()
        {
            var mainwindowPO = new MainWindowPO(Session);
            var selectEdgeColorDialogPO = mainwindowPO.Click_SelectEdgeColorButton();
            selectEdgeColorDialogPO.Click_Solid();
            selectEdgeColorDialogPO.BlueDADA.Initialize();
            selectEdgeColorDialogPO.BlueDADA.MoveTo(-2, 10);
            selectEdgeColorDialogPO.BlueDADA.Click();
            selectEdgeColorDialogPO.BlueDADA.Release();
            selectEdgeColorDialogPO.BlueDADA.Perform();
            selectEdgeColorDialogPO.GreenDADA.Initialize();
            selectEdgeColorDialogPO.GreenDADA.MoveTo(-2, 10);
            selectEdgeColorDialogPO.GreenDADA.Click();
            selectEdgeColorDialogPO.GreenDADA.Release();
            selectEdgeColorDialogPO.GreenDADA.Perform();
            selectEdgeColorDialogPO.RedDADA.Initialize();
            selectEdgeColorDialogPO.RedDADA.MoveTo(184, 10);
            selectEdgeColorDialogPO.RedDADA.Click();
            selectEdgeColorDialogPO.RedDADA.Release();
            selectEdgeColorDialogPO.RedDADA.Perform();
            selectEdgeColorDialogPO.ColorMapDADA.Initialize();
            selectEdgeColorDialogPO.ColorMapDADA.MoveTo(byte.MaxValue, 0); //純色
            selectEdgeColorDialogPO.ColorMapDADA.Click();
            selectEdgeColorDialogPO.ColorMapDADA.Perform();
            TakeScreenShot("カラーダイアログ結果.png");
            selectEdgeColorDialogPO.Click_OK();

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
            TestContext.AddTestAttachment(filename);

            using (var mat = new Mat(filename))
            {
                PixelIs(mat, 100, 100, 191, 191, 255);
                PixelIs(mat, 101, 100, 127, 127, 255);
                PixelIs(mat, 100, 101, 127, 127, 255);
                PixelIs(mat, 101, 101, 64, 64, 255);
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

        private void PixelIs(Mat mat, int y, int x, int b, int g, int r)
        {
            Vec3b pic = mat.At<Vec3b>(y, x);
            Console.WriteLine($"(b, g, r) = ({pic.Item0}, {pic.Item1}, {pic.Item2})");
            Assert.That(pic.Item0, Is.EqualTo(b), "{0},{1}", y, x);
            Assert.That(pic.Item1, Is.EqualTo(g), "{0},{1}", y, x);
            Assert.That(pic.Item2, Is.EqualTo(r), "{0},{1}", y, x);
        }
    }
}
