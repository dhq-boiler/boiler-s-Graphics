using boilersE2E;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Interactions;

namespace boilersGraphics.Test.UITests.PageObjects
{
    public class LoadDialog : PageObjectBase
    {
        public LoadDialog(WindowsDriver<WindowsElement> session)
            : base(session)
        { }

        private Actions action;

        public void InitializeActions()
        {
            action = new Actions(Session);
        }

        public void Focus_FileName()
        {
            action.SendKeys(Keys.Alt + "N" + Keys.Alt);
        }

        public void Input_FileName(string filename)
        {
            action.InputText(GetElementByAutomationID("1148"), filename);
        }

        public void Click_OpenButton()
        {
            action.Click(GetElementByAutomationID("1"));
        }

        public void Perform()
        {
            action.Perform();
        }
    }
}