using OpenQA.Selenium.Appium.Windows;

namespace boilersGraphics.Test.UITests.PageObjects
{
    public class PrivacyPolicyPO : PageObjectBase
    {
        public PrivacyPolicyPO(WindowsDriver<WindowsElement> Session)
            : base(Session)
        { }

        public bool Click_AgreeButton_IfExists()
        {
            if (ExistsElementByAutomationID("Agree", 1))
            {
                GetElementByAutomationID("Agree").Click();
                return true;
            }
            return false;
        }

        public bool Click_OKButton_IfExists()
        {
            if (ExistsElementByAutomationID("OK", 1))
            {
                //OKボタンを押下する
                GetElementByAutomationID("OK").Click();
                return true;
            }
            return false;
        }
    }
}
