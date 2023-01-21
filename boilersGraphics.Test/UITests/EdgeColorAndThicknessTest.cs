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
    public class EdgeColorAndThicknessTest : E2ETest
    {
        [Test, Apartment(ApartmentState.STA)]
        [Retry(3)]
        public void エッジを赤太さ25ptを選択して描画()
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

            mainwindowPO.Click_EdgeThicknessComboBox();
            mainwindowPO.Click_EdgeThicknessComboBoxItem(10); //25pt

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
                for (int i = 105; i <= 123; i++)
                {
                    PixelIs(mat, i, i, 0, 0, 255);
                }
            }
        }

        private void PixelIsBlack(Mat mat, int y, int x)
        {
            Vec3b pic = mat.At<Vec3b>(y, x);

            Assert.That(pic.Item0, Is.EqualTo(0), "{0},{1}", y, x);
            Assert.That(pic.Item1, Is.EqualTo(0), "{0},{1}", y, x);
            Assert.That(pic.Item2, Is.EqualTo(0), "{0},{1}", y, x);
        }

        private void PixelIsWhite(Mat mat, int y, int x)
        {
            Vec3b pic = mat.At<Vec3b>(y, x);

            Assert.That(pic.Item0, Is.EqualTo(255), "{0},{1}", y, x);
            Assert.That(pic.Item1, Is.EqualTo(255), "{0},{1}", y, x);
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
