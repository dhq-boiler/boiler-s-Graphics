using boilersE2E;
using boilersE2E.NUnit;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Interactions;

namespace boilersGraphics.Test.UITests.PageObjects
{
    public class LoadDialogPO : PageObjectBase
    {
        public LoadDialogPO(WindowsDriver<WindowsElement> session, E2ETestFixture testFixture) : base(session, testFixture)
        { }


        public void Focus_FileName()
        {
            Session.SwitchTo().ActiveElement().SendKeys(Keys.Alt + "N" + Keys.Alt);
        }

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