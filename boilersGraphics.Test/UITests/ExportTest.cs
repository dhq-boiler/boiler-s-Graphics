using NLog;
using NUnit.Framework;
using OpenCvSharp;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace boilersGraphics.Test.UITests
{
    [TestFixture]
    public class ExportTest : AppSession
    {
        [Test]
        public void 真っ白なキャンパスをエクスポートする()
        {
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var exportFilePath = $"{dir}\\ExportTest.jpg";

            GetElementByAutomationID("Export").Click();
            GetWindowByTitle("エクスポート").SwitchTo();
            GetElementByAutomationID("filename").SendKeys(exportFilePath);
            GetElementByAutomationID("PerformExport").Click();
            Thread.Sleep(1000);
            LogManager.GetCurrentClassLogger().Info(exportFilePath);
            TestContext.AddTestAttachment(exportFilePath);
            Assert.That(File.Exists(exportFilePath), Is.EqualTo(true));

            using (var mat = new Mat(exportFilePath))
            {
                TestPixelIsWhite(mat, 0, 0);
                TestPixelIsWhite(mat, 99, 99);
                TestPixelIsWhite(mat, 999, 999);
                TestPixelIsWhite(mat, 999, 0);
                TestPixelIsWhite(mat, 0, 999);
            }
        }

        [Test]
        public void チェッカーパターンを読み込んでエクスポートする()
        {
            LogManager.GetCurrentClassLogger().Info("A");
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var loadFilePath = $"{dir}\\XmlFiles\\checker_pattern.xml";
            var exportFilePath = $"{dir}\\ExportTest2.jpg";

            try
            {
                LogManager.GetCurrentClassLogger().Info("B");
                TakeScreenShot("SCREENSHOT_B.png");
                session.FindElementByAccessibilityId("Load").Click();
                LogManager.GetCurrentClassLogger().Info("C");
                TakeScreenShot("SCREENSHOT_C.png");
                //「現在のキャンパスは破棄されますが、よろしいですか？」→OK（"1"）
                session.FindElementByAccessibilityId("1").Click();
                LogManager.GetCurrentClassLogger().Info("D");
                TakeScreenShot("SCREENSHOT_D.png");
                var action = new Actions(session);
                action.SendKeys(Keys.Alt + "N" + Keys.Alt);
                action.Perform();
                LogManager.GetCurrentClassLogger().Info("F");
                TakeScreenShot("SCREENSHOT_F.png");
                //ファイル名（コンボボックス、"1148"）に入力
                GetElementByAutomationID("1148").SendKeys(loadFilePath);
                LogManager.GetCurrentClassLogger().Info("G");
                TakeScreenShot("SCREENSHOT_G.png");
                //開く（O)ボタン（"1")をクリック
                GetElementByAutomationID("1").Click();
                //開くボタンが押されない場合があるので、"1"がまだ存在していたら再度押し直す
                if (ExistsElementByAutomationID("1"))
                {
                    LogManager.GetCurrentClassLogger().Info("GG");
                    GetElementByAutomationID("1").Click();
                }
            }
            finally
            {
                if (IsElementPresent(By.Name("開く")))
                {
                    session.FindElementByName("開く").FindElementByAccessibilityId("2").Click();
                }
                if (IsElementPresent(By.Name("Open")))
                {
                    session.FindElementByName("Open").FindElementByAccessibilityId("2").Click();
                }
            }

            LogManager.GetCurrentClassLogger().Info("H");
            TakeScreenShot("SCREENSHOT_H.png");
            GetElementByAutomationID("Export").Click();

            LogManager.GetCurrentClassLogger().Info("I");
            TakeScreenShot("SCREENSHOT_I.png");

            GetWindowByTitle("エクスポート").SwitchTo();

            GetElementByAutomationID("filename").SendKeys(exportFilePath);
                
            LogManager.GetCurrentClassLogger().Info("J");
            TakeScreenShot("SCREENSHOT_J.png");
            GetElementByAutomationID("PerformExport").Click();

            session.WindowHandles.Select(x => session.SwitchTo().Window(x)).First(x => x.Title.StartsWith("boiler's Graphics")).SwitchTo();

            LogManager.GetCurrentClassLogger().Info("K");
            TakeScreenShot("SCREENSHOT_K.png");
            Thread.Sleep(1000);
            LogManager.GetCurrentClassLogger().Info("L");
            TakeScreenShot("SCREENSHOT_L.png");
            TestContext.AddTestAttachment(exportFilePath);
            Assert.That(File.Exists(exportFilePath), Is.EqualTo(true));
            LogManager.GetCurrentClassLogger().Info("M");
            TakeScreenShot("SCREENSHOT_M.png");

            using (var mat = new Mat(exportFilePath))
            {
                for (int y = 0; y < 100; ++y)
                {
                    for (int x = 0; x < 100; ++x)
                    {
                        TestPixelIsBlack(mat, y, x);
                    }
                }

                for (int y = 900; y < 1000; ++y)
                {
                    for (int x = 900; x < 1000; ++x)
                    {
                        TestPixelIsBlack(mat, y, x);
                    }
                }

                for (int y = 900; y < 1000; ++y)
                {
                    for (int x = 0; x < 100; ++x)
                    {
                        TestPixelIsWhite(mat, y, x);
                    }
                }

                for (int y = 0; y < 100; ++y)
                {
                    for (int x = 900; x < 1000; ++x)
                    {
                        TestPixelIsWhite(mat, y, x);
                    }
                }

                for (int y = 900; y < 1000; ++y)
                {
                    for (int x = 800; x < 900; ++x)
                    {
                        TestPixelIsWhite(mat, y, x);
                    }
                }

                for (int y = 800; y < 900; ++y)
                {
                    for (int x = 900; x < 1000; ++x)
                    {
                        TestPixelIsWhite(mat, y, x);
                    }
                }
            }
        }

        [Test]
        public void スライス()
        {
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var loadFilePath = $"{dir}\\XmlFiles\\checker_pattern.xml";

            session.WindowHandles.Select(x => session.SwitchTo().Window(x)).First(x => x.Title.StartsWith("boiler's Graphics")).SwitchTo();

            session.FindElementByAccessibilityId("Load").Click();
            //「現在のキャンパスは破棄されますが、よろしいですか？」→OK（"1"）
            session.FindElementByAccessibilityId("1").Click();

            Thread.Sleep(1000);

            var action = new Actions(session);
            action.SendKeys(Keys.Alt + "N" + Keys.Alt);
            //ファイル名（コンボボックス、"1148"）に入力
            action.SendKeys(GetElementByAutomationID("1148"), loadFilePath);
            //開く（O)ボタン（"1")をクリック
            action.Click(GetElementByAutomationID("1"));
            action.Perform();
            //開くボタンが押されない場合があるので、"1"がまだ存在していたら再度押し直す
            if (ExistsElementByAutomationID("1"))
            {
                GetElementByAutomationID("1").Click();
            }

            GetElementByName("slice").Click();
            action = new Actions(session);
            var designerCanvas = GetElementBy(By.XPath("//Pane[@Name=\"DesignerScrollViewer\"][@AutomationId=\"DesignerScrollViewer\"]"));
            action.MoveToElement(designerCanvas, 525, 100);
            action.ClickAndHold();
            for (int i = 1; i < 200; ++i)
                action.MoveByOffset(1, 1);
            action.Release();
            action.Perform();

            var previewFilePath = $"{dir}\\ExportTest3.jpg";

            Thread.Sleep(1000);

            TakeScreenShot("SCREENSHOT_PREVIEW.png");

            GetWindowByTitle("エクスポート").SwitchTo();

            GetElementBy(By.XPath("//Window[@ClassName=\"Window\"][@Name=\"エクスポート\"]/Custom[@ClassName=\"Export\"]/Image[@Name=\"Preview\"][@AutomationId=\"Preview\"]")).GetScreenshot().SaveAsFile(previewFilePath);
            TestContext.AddTestAttachment(previewFilePath);

            var exportFilePath = $"{dir}\\ExportTest4.jpg";

            GetElementByAutomationID("filename").SendKeys(exportFilePath);
            GetElementByAutomationID("PerformExport").Click();
            TestContext.AddTestAttachment(exportFilePath);

            using (var mat = new Mat(exportFilePath))
            {
                Assert.That(mat.Rows, Is.EqualTo(200));
                Assert.That(mat.Cols, Is.EqualTo(200));
                for (int y = 0; y < 100; ++y)
                {
                    for (int x = 0; x < 100; ++x)
                    {
                        TestPixelIsBlack(mat, y, x);
                    }
                }
                for (int y = 100; y < 200; ++y)
                {
                    for (int x = 100; x < 200; ++x)
                    {
                        TestPixelIsBlack(mat, y, x);
                    }
                }
                for (int y = 0; y < 100; ++y)
                {
                    for (int x = 100; x < 200; ++x)
                    {
                        TestPixelIsWhite(mat, y, x);
                    }
                }
                for (int y = 100; y < 200; ++y)
                {
                    for (int x = 0; x < 100; ++x)
                    {
                        TestPixelIsWhite(mat, y, x);
                    }
                }
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
