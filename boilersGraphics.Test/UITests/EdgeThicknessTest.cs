using boilersGraphics.Test.UITests.PageObjects;
using NUnit.Framework;
using OpenCvSharp;
using System.IO;
using System.Reflection;
using System.Threading;

namespace boilersGraphics.Test.UITests
{
    [TestFixture]
    public class EdgeThicknessTest : E2ETest
    {
        [Test, Apartment(ApartmentState.STA)]
        public void エッジ太さ5ptを選択して描画()
        {
            var mainwindowPO = new MainWindowPO(Session);

            mainwindowPO.Click_EdgeThicknessComboBox();
            mainwindowPO.Click_EdgeThicknessComboBoxItem(6);

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
                PixelIsBlack(mat, 98, 98);
                PixelIsBlack(mat, 100, 98);
                PixelIsBlack(mat, 98, 100);
                PixelIsBlack(mat, 100, 100);
                PixelIsBlack(mat, 102, 100);
                PixelIsBlack(mat, 100, 102);
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
    }
}
