using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Windows;

namespace boilersGraphics.Test.UITests.PageObjects
{
    public class PreviewDialog : PageObjectBase
    {
        public PreviewDialog(WindowsDriver<WindowsElement> session)
            : base(session)
        { }

        public WindowsElement PreviewImage => GetElementBy(By.XPath($"//*/Image[@Name=\"Preview\"][@AutomationId=\"Preview\"]"));

        public void ScreenShot_PreviewImage(string filename)
        {
            PreviewImage.GetScreenshot().SaveAsFile(filename);
        }

        public void Input_FileName(string filename)
        {
            InputText(GetElementByAutomationID("filename"), filename);
        }

        public void Click_PerformExportButton()
        {
            GetElementByAutomationID("PerformExport").Click();
        }
    }
}
