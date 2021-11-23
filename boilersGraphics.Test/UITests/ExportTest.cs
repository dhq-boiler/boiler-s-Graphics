using NUnit.Framework;
using System.IO;
using System.Reflection;

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
            session.FindElementByAccessibilityId("Export").Click();
            session.FindElementByAccessibilityId("FileName").SendKeys(exportFilePath);
            session.FindElementByAccessibilityId("PerformExport").Click();
            Assert.That(File.Exists(exportFilePath), Is.EqualTo(true));
        }
        
        [TearDown]
        public void RemoveExportedFile()
        {
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var exportFilePath = $"{dir}\\ExportTest.jpg";
            File.Delete(exportFilePath);
        }
    }
}
