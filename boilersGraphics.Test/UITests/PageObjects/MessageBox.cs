using OpenQA.Selenium.Appium.Windows;

namespace boilersGraphics.Test.UITests.PageObjects
{
    /// <summary>
    /// 現在のキャンパスは破棄されますが、よろしいですか？
    /// </summary>
    public class MessageBox : PageObjectBase
    {
        public MessageBox(WindowsDriver<WindowsElement> Session)
            : base(Session)
        { }

        public LoadDialog Click_OKButton()
        {
            GetElementByAutomationID("1").Click();
            return new LoadDialog(Session);
        }
    }
}
