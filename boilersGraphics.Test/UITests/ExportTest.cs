using NLog;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System;
using System.IO;
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

            try
            {
                GetElementByAutomationID("Export").Click();
                GetElementByAutomationID("filename").SendKeys(exportFilePath);
                GetElementByAutomationID("PerformExport").Click();
                Thread.Sleep(1000);
                LogManager.GetCurrentClassLogger().Info(exportFilePath);
                Assert.That(File.Exists(exportFilePath), Is.EqualTo(true));
            }
            finally
            {
                File.Delete(exportFilePath);
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
                    session.FindElementByAccessibilityId("1").Click(); //ファイル名が全選択状態になる
                    session.FindElementByAccessibilityId("1").Click(); //もう一度クリックが必要
                                                                       //キャンセルボタン("2")をクリック
                                                                       //session.FindElementByAccessibilityId("2").Click();
                }
                finally
                {
                    if (IsElementPresent(By.Name("開く")))
                    {
                        session.FindElementByName("開く").FindElementByAccessibilityId("Close").Click();
                    }
                }

                LogManager.GetCurrentClassLogger().Info("H");
                TakeScreenShot("SCREENSHOT_H.png");
                GetElementByAutomationID("Export").Click();

                Thread.Sleep(5000);

                LogManager.GetCurrentClassLogger().Info("I");
                TakeScreenShot("SCREENSHOT_I.png");
                
                GetElementByAutomationID("filename").SendKeys(exportFilePath);
                
                LogManager.GetCurrentClassLogger().Info("J");
                TakeScreenShot("SCREENSHOT_J.png");
                GetElementByAutomationID("PerformExport").Click();

                LogManager.GetCurrentClassLogger().Info("K");
                TakeScreenShot("SCREENSHOT_K.png");
                Thread.Sleep(1000);
                LogManager.GetCurrentClassLogger().Info("L");
                TakeScreenShot("SCREENSHOT_L.png");
                Assert.That(File.Exists(exportFilePath), Is.EqualTo(true));
                LogManager.GetCurrentClassLogger().Info("M");
                TakeScreenShot("SCREENSHOT_M.png");
            }
            finally
            {
                LogManager.GetCurrentClassLogger().Info("N");
                File.Delete(exportFilePath);
                LogManager.GetCurrentClassLogger().Info("O");
            }
        }
    }
}
