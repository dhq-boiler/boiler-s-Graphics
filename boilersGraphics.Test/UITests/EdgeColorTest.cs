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
        public void エッジ色を赤指定して矩形描画()
        {
            var mainwindowPO = new MainWindow(Session);
            var selectEdgeColorDialogPO = mainwindowPO.Click_SelectEdgeColorButton();
            selectEdgeColorDialogPO.Click_Solid();
            selectEdgeColorDialogPO.ColorMapDADA.Initialize();
            selectEdgeColorDialogPO.ColorMapDADA.MoveTo(254, 1); //Red
            selectEdgeColorDialogPO.ColorMapDADA.Click();
            selectEdgeColorDialogPO.ColorMapDADA.Perform();
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

            using (var mat = new Mat(filename))
            {
                PixelIsRed(mat, 100, 100);
            }
        }

        private void PixelIsRed(Mat mat, int y, int x)
        {
            Vec3b pic = mat.At<Vec3b>(y, x);
            Console.WriteLine($"(b, g, r) = ({pic.Item0}, {pic.Item1}, {pic.Item2})");
            Assert.That(pic.Item0, Is.EqualTo(65), "{0},{1}", y, x);
            Assert.That(pic.Item1, Is.EqualTo(65), "{0},{1}", y, x);
            Assert.That(pic.Item2, Is.EqualTo(255), "{0},{1}", y, x);
        }
    }
}
