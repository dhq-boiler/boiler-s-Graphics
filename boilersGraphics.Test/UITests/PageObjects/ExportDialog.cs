using OpenQA.Selenium.Appium.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
