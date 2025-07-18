using boilersE2E.NUnit;
using OpenQA.Selenium.Appium.Windows;

namespace boilersGraphics.Test.UITests.PageObjects
{
    public class LoadDialogPO : PageObjectBase
    {
        public LoadDialogPO(WindowsDriver session, E2ETestFixture testFixture) : base(session, testFixture)
        { }

        public void Input_FileName(string filename)
        {
            InputText(GetElementByAutomationID("1148"), filename);
        }

        public void Click_OpenButton()
        {
            GetElementByAutomationID("1").Click();
        }
    }
}