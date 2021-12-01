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
                    session.FindElementByAccessibilityId("Load").Click();
                    LogManager.GetCurrentClassLogger().Info("C");
                    //「現在のキャンパスは破棄されますが、よろしいですか？」→OK（"1"）
                    session.FindElementByAccessibilityId("1").Click();
                    LogManager.GetCurrentClassLogger().Info("D");
                    //var action = new Actions(session);
                    //action.SendKeys(Keys.Alt + "N" + Keys.Alt);
                    //action.Perform();
                    //LogManager.GetCurrentClassLogger().Info("E");
                    var action = new Actions(session);
                    action.SendKeys(Keys.Alt + "N" + Keys.Alt);
                    action.Perform();
                    LogManager.GetCurrentClassLogger().Info("F");
                    //ファイル名（コンボボックス、"1148"）に入力
                    GetElementByAutomationID("1148").SendKeys(loadFilePath);
                    LogManager.GetCurrentClassLogger().Info("G");
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
                //action = new Actions(session);
                //action.Click(session.FindElementByAccessibilityId("Export"));
                //action.Perform();
                GetElementByAutomationID("Export").Click();
                GetElementByAutomationID("Export").Click();
                GetElementByAutomationID("Export").Click();
                GetElementByAutomationID("Export").Click();
                GetElementByAutomationID("Export").Click();

                Thread.Sleep(5000);

                LogManager.GetCurrentClassLogger().Info("I");
                //action = new Actions(session);
                //action.SendKeys(GetElementByAutomationID("filename"), exportFilePath);
                //action.Perform();
                GetElementByAutomationID("filename").SendKeys(exportFilePath);
                LogManager.GetCurrentClassLogger().Info("J");
                //action = new Actions(session);
                //action.Click(session.FindElementByAccessibilityId("PerformExport"));
                //action.Perform();
                GetElementByAutomationID("PerformExport").Click();

                LogManager.GetCurrentClassLogger().Info("K");
                Thread.Sleep(1000);
                LogManager.GetCurrentClassLogger().Info("L");
                Assert.That(File.Exists(exportFilePath), Is.EqualTo(true));
                LogManager.GetCurrentClassLogger().Info("M");
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
