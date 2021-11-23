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
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var loadFilePath = $"{dir}\\XmlFiles\\checker_pattern.xml";
            var exportFilePath = $"{dir}\\ExportTest2.jpg";

            try
            {
                session.FindElementByAccessibilityId("Load").Click();
                session.FindElementByAccessibilityId("1").Click();
                session.Keyboard.SendKeys(Keys.Alt + "N" + Keys.Alt);
                session.Keyboard.SendKeys(Keys.Alt + "N" + Keys.Alt);
                session.FindElementByAccessibilityId("1148").SendKeys(loadFilePath);
                session.FindElementByName("開く(O)").Click();

                session.FindElementByAccessibilityId("Export").Click();
                session.FindElementByAccessibilityId("FileName").SendKeys(exportFilePath);
                session.FindElementByAccessibilityId("PerformExport").Click();
                Thread.Sleep(1000);
                Assert.That(File.Exists(exportFilePath), Is.EqualTo(true));
            }
            finally
            {
                File.Delete(exportFilePath);
            }
        }
    }
}
