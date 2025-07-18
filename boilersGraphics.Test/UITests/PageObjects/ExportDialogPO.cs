using boilersE2E;
using boilersE2E.NUnit;
using NUnit.Framework;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Interactions;

namespace boilersGraphics.Test.UITests.PageObjects
{
    public class ExportDialogPO : PageObjectBase
    {
        public ExportDialogPO(WindowsDriver session, E2ETestFixture testFixture)
            : base(session, testFixture)
        { }

        private Actions action;



        public void Input_FileName(string filename)
        {
            TestFixture.InputText(GetElementByAutomationID("filename"), filename);
        }

        public void Click_PerformExportButton()
        {
            GetElementByAutomationID("PerformExport").Click();
        }
    }
}
