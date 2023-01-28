using boilersGraphics.Test.UITests.PageObjects;
using NLog;
using NUnit.Framework;
using OpenCvSharp;
using System.IO;
using System.Reflection;
using System.Threading;

namespace boilersGraphics.Test.UITests
{
    [TestFixture]
    public class ExportTest : E2ETest
    {
        [Test, Apartment(ApartmentState.STA)]
        [Retry(3)]
        public void 真っ白なキャンパスをエクスポートする()
        {
            var mainwindowPO = new MainWindowPO(Session);

            TakeScreenShot("SCREENSHOT_A.png");
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var exportFilePath = $"{dir}\\ExportTest.jpg";

            var exportDialogPO = mainwindowPO.Click_ExportButton();
            exportDialogPO.Input_FileName(exportFilePath);
            exportDialogPO.Click_PerformExportButton();
            Thread.Sleep(1000);
            LogManager.GetCurrentClassLogger().Info(exportFilePath);
            Assert.That(exportFilePath, Does.Exist.After(5000, 50));
            TestContext.AddTestAttachment(exportFilePath);
            Assert.That(File.Exists(exportFilePath), Is.EqualTo(true));

            using (var mat = new Mat(exportFilePath))
            {
                TestPixelIsBlack(mat, 0, 0);
                TestPixelIsWhite(mat, 1, 1);
                TestPixelIsWhite(mat, 99, 99);
                TestPixelIsWhite(mat, 998, 998);
                TestPixelIsBlack(mat, 999, 999);
                TestPixelIsBlack(mat, 999, 0);
                TestPixelIsBlack(mat, 0, 999);
            }
        }

        [Test, Apartment(ApartmentState.STA)]
        [Retry(3)]
        public void チェッカーパターンを読み込んでエクスポートする()
        {
            var mainwindowPO = new MainWindowPO(Session);

            TakeScreenShot("SCREENSHOT_A.png");
            LogManager.GetCurrentClassLogger().Info("A");
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var loadFilePath = $"{dir}\\XmlFiles\\checker_pattern.xml";
            var exportFilePath = $"{dir}\\ExportTest2.jpg";

            LogManager.GetCurrentClassLogger().Info("B");
            TakeScreenShot("SCREENSHOT_B.png");
            var msgboxPO = mainwindowPO.Click_LoadButton();
            LogManager.GetCurrentClassLogger().Info("C");
            TakeScreenShot("SCREENSHOT_C.png");
            var loaddialogPO = msgboxPO.Click_OKButton();
            LogManager.GetCurrentClassLogger().Info("D");
            TakeScreenShot("SCREENSHOT_D.png");
            loaddialogPO.InitializeActions();
            loaddialogPO.Focus_FileName();
            loaddialogPO.Input_FileName(loadFilePath);
            loaddialogPO.Click_OpenButton();
            loaddialogPO.Perform();

            LogManager.GetCurrentClassLogger().Info("H");
            TakeScreenShot("SCREENSHOT_H.png");
            var exportdialogPO = mainwindowPO.Click_ExportButton();

            LogManager.GetCurrentClassLogger().Info("I");
            TakeScreenShot("SCREENSHOT_I.png");

            exportdialogPO.Input_FileName(exportFilePath);
                
            LogManager.GetCurrentClassLogger().Info("J");
            TakeScreenShot("SCREENSHOT_J.png");
            exportdialogPO.Click_PerformExportButton();

            LogManager.GetCurrentClassLogger().Info("K");
            TakeScreenShot("SCREENSHOT_K.png");
            Thread.Sleep(1000);
            LogManager.GetCurrentClassLogger().Info("L");
            TakeScreenShot("SCREENSHOT_L.png");
            Assert.That(exportFilePath, Does.Exist.After(5000, 50));
            TestContext.AddTestAttachment(exportFilePath);
            LogManager.GetCurrentClassLogger().Info("M");
            TakeScreenShot("SCREENSHOT_M.png");

            //using (var mat = new Mat(exportFilePath))
            //{
            //    for (int y = 0; y < 100; ++y)
            //    {
            //        for (int x = 0; x < 100; ++x)
            //        {
            //            TestPixelIsBlack(mat, y, x);
            //        }
            //    }

            //    for (int y = 900; y < 1000; ++y)
            //    {
            //        for (int x = 900; x < 1000; ++x)
            //        {
            //            TestPixelIsBlack(mat, y, x);
            //        }
            //    }

            //    for (int y = 900; y < 1000; ++y)
            //    {
            //        for (int x = 0; x < 100; ++x)
            //        {
            //            TestPixelIsWhite(mat, y, x);
            //        }
            //    }

            //    for (int y = 0; y < 100; ++y)
            //    {
            //        for (int x = 900; x < 1000; ++x)
            //        {
            //            TestPixelIsWhite(mat, y, x);
            //        }
            //    }

            //    for (int y = 900; y < 1000; ++y)
            //    {
            //        for (int x = 800; x < 900; ++x)
            //        {
            //            TestPixelIsWhite(mat, y, x);
            //        }
            //    }

            //    for (int y = 800; y < 900; ++y)
            //    {
            //        for (int x = 900; x < 1000; ++x)
            //        {
            //            TestPixelIsWhite(mat, y, x);
            //        }
            //    }
            //}
        }

        [Test, Apartment(ApartmentState.STA)]
        [Retry(3)]
        public void スライス()
        {
            TakeScreenShot("SCREENSHOT_A.png");
            var mainwindowPO = new MainWindowPO(Session);
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var loadFilePath = $"{dir}\\XmlFiles\\checker_pattern.xml";

            var msgboxPO = mainwindowPO.Click_LoadButton();
            var loaddialogPO = msgboxPO.Click_OKButton();

            loaddialogPO.InitializeActions();
            loaddialogPO.Focus_FileName();
            loaddialogPO.Input_FileName(loadFilePath);
            loaddialogPO.Click_OpenButton();
            loaddialogPO.Perform();

            TakeScreenShot("SCREENSHOT_GetDesignerCanvas.png");
            mainwindowPO.Click_SliceTool();
            mainwindowPO.InitializeActions();
            mainwindowPO.MoveToElement(100, 100);
            mainwindowPO.ClickAndHold();
            for (int i = 1; i < 200; ++i)
                mainwindowPO.MoveByOffset(1, 1);
            mainwindowPO.Release();
            mainwindowPO.Perform();

            var previewFilePath = $"{dir}\\ExportTest3.jpg";

            Thread.Sleep(1000);

            TakeScreenShot("SCREENSHOT_PREVIEW.png");

            var previewdialogPO = new PreviewDialogPO(Session);

            previewdialogPO.ScreenShot_PreviewImage(previewFilePath);
            Assert.That(previewFilePath, Does.Exist.After(5000, 50));
            TestContext.AddTestAttachment(previewFilePath);

            var exportFilePath = $"{dir}\\ExportTest4.jpg";

            previewdialogPO.Input_FileName(exportFilePath);
            previewdialogPO.Click_PerformExportButton();
            Assert.That(exportFilePath, Does.Exist.After(5000, 50));
            TestContext.AddTestAttachment(exportFilePath);

            using (var mat = new Mat(exportFilePath))
            {
                Assert.That(mat.Rows, Is.EqualTo(200));
                Assert.That(mat.Cols, Is.EqualTo(200));
                //for (int y = 0; y < 100; ++y)
                //{
                //    for (int x = 0; x < 100; ++x)
                //    {
                //        TestPixelIsBlack(mat, y, x);
                //    }
                //}
                //for (int y = 100; y < 200; ++y)
                //{
                //    for (int x = 100; x < 200; ++x)
                //    {
                //        TestPixelIsBlack(mat, y, x);
                //    }
                //}
                //for (int y = 0; y < 100; ++y)
                //{
                //    for (int x = 100; x < 200; ++x)
                //    {
                //        TestPixelIsWhite(mat, y, x);
                //    }
                //}
                //for (int y = 100; y < 200; ++y)
                //{
                //    for (int x = 0; x < 100; ++x)
                //    {
                //        TestPixelIsWhite(mat, y, x);
                //    }
                //}
            }
        }

        private void TestPixelIsBlack(Mat mat, int y, int x)
        {
            Vec3b pic = mat.At<Vec3b>(y, x);

            Assert.That(pic.Item0, Is.EqualTo(0), "{0},{1}", y, x);
            Assert.That(pic.Item1, Is.EqualTo(0), "{0},{1}", y, x);
            Assert.That(pic.Item2, Is.EqualTo(0), "{0},{1}", y, x);
        }

        private void TestPixelIsWhite(Mat mat, int y, int x)
        {
            Vec3b pic = mat.At<Vec3b>(y, x);

            Assert.That(pic.Item0, Is.EqualTo(255), "{0},{1}", y, x);
            Assert.That(pic.Item1, Is.EqualTo(255), "{0},{1}", y, x);
            Assert.That(pic.Item2, Is.EqualTo(255), "{0},{1}", y, x);
        }
    }
}
