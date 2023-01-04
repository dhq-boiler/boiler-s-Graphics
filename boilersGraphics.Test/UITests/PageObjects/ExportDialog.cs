using OpenQA.Selenium.Appium.Windows;

namespace boilersGraphics.Test.UITests.PageObjects
{
    public class ExportDialog : PageObjectBase
    {
        public ExportDialog(WindowsDriver<WindowsElement> session)
            : base(session)
        { }

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
