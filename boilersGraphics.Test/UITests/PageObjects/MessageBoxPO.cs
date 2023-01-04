using OpenQA.Selenium.Appium.Windows;

namespace boilersGraphics.Test.UITests.PageObjects
{
    /// <summary>
    /// 現在のキャンパスは破棄されますが、よろしいですか？
    /// </summary>
    public class MessageBoxPO : PageObjectBase
    {
        public MessageBoxPO(WindowsDriver<WindowsElement> Session)
            : base(Session)
        { }

        public LoadDialogPO Click_OKButton()
        {
            GetElementByAutomationID("1").Click();
            return new LoadDialogPO(Session);
        }
    }
}
