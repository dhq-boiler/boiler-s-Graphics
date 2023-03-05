using boilersE2E.NUnit;
using OpenQA.Selenium.Appium.Windows;

namespace boilersGraphics.Test.UITests.PageObjects
{
    public class PrivacyPolicyPO : PageObjectBase
    {
        public PrivacyPolicyPO(WindowsDriver<WindowsElement> Session, E2ETestFixture testFixture)
            : base(Session, testFixture)
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
