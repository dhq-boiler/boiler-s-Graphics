using boilersE2E.NUnit;
using OpenQA.Selenium.Appium.Windows;

namespace boilersGraphics.Test.UITests.PageObjects
{
    /// <summary>
    /// 現在のキャンパスは破棄されますが、よろしいですか？
    /// </summary>
    public class MessageBoxPO : PageObjectBase
    {
        public MessageBoxPO(WindowsDriver<WindowsElement> Session, E2ETestFixture testFixture)
            : base(Session, testFixture)
        { }

        public LoadDialogPO Click_OKButton()
        {
            GetElementByAutomationID("1").Click();
            return new LoadDialogPO(Session, TestFixture);
        }
    }
}
