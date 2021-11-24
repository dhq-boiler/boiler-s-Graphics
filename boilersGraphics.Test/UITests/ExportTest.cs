using NLog;
using NUnit.Framework;
using OpenQA.Selenium;
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
                session.FindElementByAccessibilityId("Export").Click();
                session.FindElementByAccessibilityId("FileName").SendKeys(exportFilePath);
                session.FindElementByAccessibilityId("PerformExport").Click();
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
                LogManager.GetCurrentClassLogger().Info("B");
                session.FindElementByAccessibilityId("Load").Click();
                LogManager.GetCurrentClassLogger().Info("C");
                session.FindElementByAccessibilityId("1").Click();
                LogManager.GetCurrentClassLogger().Info("D");
                session.Keyboard.SendKeys(Keys.Alt + "N" + Keys.Alt);
                LogManager.GetCurrentClassLogger().Info("E");
                session.Keyboard.SendKeys(Keys.Alt + "N" + Keys.Alt);
                LogManager.GetCurrentClassLogger().Info("F");
                session.FindElementByAccessibilityId("1148").SendKeys(loadFilePath);
                LogManager.GetCurrentClassLogger().Info("G");
                session.FindElementByAccessibilityId("1").Click();

                LogManager.GetCurrentClassLogger().Info("H");
                Thread.Sleep(1000);


                session.FindElementByAccessibilityId("Export").Click();
                LogManager.GetCurrentClassLogger().Info("I");
                session.FindElementByAccessibilityId("FileName").SendKeys(exportFilePath);
                LogManager.GetCurrentClassLogger().Info("J");
                session.FindElementByAccessibilityId("PerformExport").Click();
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
